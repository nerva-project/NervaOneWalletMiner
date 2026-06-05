using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Litecoin implementation as of 5/29/26: https://github.com/litecoin-project/litecoin

    internal class WalletServiceLTC : WalletServiceBaseBTC
    {
        protected override string CoinPrefix => "LTC";
        protected override string CoinName => "Litecoin";
        protected override uint CoinType => 2;
        protected override bool SupportMultipleScriptTypes => true;
        protected override bool SupportTaproot => false;
        protected override bool UseDescriptorWallet => false;

        private static string CurrentWalletPath => "wallet/" + GlobalData.OpenedWalletName;
        private static int _ltcId = 0;

        // MWEB receives are invisible to listsinceblock; populate via listunspent + gettransaction
        private readonly Dictionary<string, Transfer> _mwebReceiveCache = [];
        // Full MWEB addresses of current UTXOs; used to identify change outputs
        private readonly HashSet<string> _mwebOwnedAddressCache = [];
        // listsinceblock may return the same MWEB kernel with a different txid on a later tick
        private readonly HashSet<string> _seenMwebTransferKeys = [];
        private string _mwebCacheWalletName = string.Empty;

        public override async Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj)
        {
            GetTransfersResponse response = await base.GetTransfers(rpc, requestObj);
            if (response.Error.IsError)
            {
                return response;
            }

            try
            {
                if (_mwebCacheWalletName != GlobalData.OpenedWalletName)
                {
                    _mwebReceiveCache.Clear();
                    _mwebOwnedAddressCache.Clear();
                    _seenMwebTransferKeys.Clear();
                    _mwebCacheWalletName = GlobalData.OpenedWalletName;
                }

                // Deduplicate MWEB entries - same kernel may appear with a different txid on a later tick
                response.Transfers.RemoveAll(t =>
                {
                    if (!t.Address.StartsWith("ltcmweb", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    return !_seenMwebTransferKeys.Add($"{t.Height}|{t.Address}|{t.Amount}|{t.Type}");
                });

                HashSet<string> knownTxIds = [.. response.Transfers.Select(t => t.TransactionId)];

                // MWEB receives don't appear in listsinceblock; detect them via listunspent
                var listUnspentJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _ltcId++,
                    ["method"] = "listunspent",
                    ["params"] = new JObject()
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, CurrentWalletPath), listUnspentJson.ToString(), rpc.UserName, rpc.Password);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    return response;
                }

                JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
                var error = jsonObject["error"];
                if (error != null && error.Type != JTokenType.Null)
                {
                    return response;
                }

                var utxos = jsonObject["result"] as JArray;
                if (utxos == null)
                {
                    return response;
                }

                // Block duplicate kernel IDs for the same receive address appearing across ticks
                var cachedReceiveAddresses = new HashSet<string>(_mwebReceiveCache.Values.Select(t => t.Address));

                foreach (JToken utxo in utxos)
                {
                    string address = utxo["address"]?.ToString() ?? string.Empty;
                    if (!address.StartsWith("ltcmweb", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    _mwebOwnedAddressCache.Add(address);

                    string txid = utxo["txid"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(txid) || knownTxIds.Contains(txid))
                    {
                        continue;
                    }

                    int utxoConfirms = utxo["confirmations"]?.Value<int>() ?? 0;
                    if (utxoConfirms < 1)
                    {
                        continue;
                    }

                    if (_mwebReceiveCache.ContainsKey(txid) || cachedReceiveAddresses.Contains(address))
                    {
                        continue;
                    }

                    // New MWEB receive - call gettransaction once to get height and timestamp
                    decimal amount = utxo["amount"]?.Value<decimal>() ?? 0;
                    Transfer? mwebTransfer = await FetchMwebReceive(rpc, txid, amount, address);
                    if (mwebTransfer != null)
                    {
                        _mwebReceiveCache[txid] = mwebTransfer;
                        cachedReceiveAddresses.Add(address);
                    }
                }

                // Add all cached MWEB receives to the response
                foreach (Transfer cached in _mwebReceiveCache.Values)
                {
                    response.Transfers.Add(cached);
                }

                // MWEB change goes back to a wallet-owned one-time address and shows as a send; remove it
                if (_mwebOwnedAddressCache.Count > 0)
                {
                    // Case 1: MWEB send to external - the same txid also has an Out to an external MWEB address
                    var mwebSendTxIds = new HashSet<string>(
                        response.Transfers
                            .Where(t => t.Type == TransferType.Out &&
                                        t.Address.StartsWith("ltcmweb", StringComparison.OrdinalIgnoreCase) &&
                                        !_mwebOwnedAddressCache.Contains(t.Address))
                            .Select(t => t.TransactionId)
                    );

                    if (mwebSendTxIds.Count > 0)
                    {
                        response.Transfers.RemoveAll(t =>
                            t.Type == TransferType.Out &&
                            _mwebOwnedAddressCache.Contains(t.Address) &&
                            mwebSendTxIds.Contains(t.TransactionId)
                        );
                    }

                    // Case 2: MWEB self-send - multiple own-MWEB Outs in same txid; keep largest, remove change
                    var ownMwebOutsByTxId = response.Transfers
                        .Where(t => t.Type == TransferType.Out && _mwebOwnedAddressCache.Contains(t.Address))
                        .GroupBy(t => t.TransactionId)
                        .Where(g => g.Count() > 1)
                        .ToList();

                    foreach (var group in ownMwebOutsByTxId)
                    {
                        decimal maxAmount = group.Max(t => t.Amount);
                        response.Transfers.RemoveAll(t =>
                            t.Type == TransferType.Out &&
                            t.TransactionId == group.Key &&
                            _mwebOwnedAddressCache.Contains(t.Address) &&
                            t.Amount < maxAmount
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("LTC.WGTF", ex);
            }

            return response;
        }

        private async Task<Transfer?> FetchMwebReceive(RpcBase rpc, string txid, decimal amount, string address)
        {
            try
            {
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _ltcId++,
                    ["method"] = "gettransaction",
                    ["params"] = new JObject
                    {
                        ["txid"] = txid,
                        ["include_watchonly"] = true,
                        ["verbose"] = false
                    }
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, CurrentWalletPath), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    return null;
                }

                JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
                var error = jsonObject["error"];
                if (error != null && error.Type != JTokenType.Null)
                {
                    return null;
                }

                var result = jsonObject["result"];
                if (result == null)
                {
                    return null;
                }

                // Negative fee means this wallet was the sender - it's change, not an external receive
                decimal fee = result["fee"]?.Value<decimal>() ?? 0;
                if (fee < 0)
                {
                    return null;
                }

                string blockheight = result["blockheight"]?.ToString() ?? string.Empty;
                ulong timereceived = result["timereceived"]?.Value<ulong>() ?? 0;
                long confirmations = result["confirmations"]?.Value<long>() ?? 0;
                string blockhash = result["blockhash"]?.ToString() ?? string.Empty;

                return new Transfer
                {
                    TransactionId = txid,
                    Address = address,
                    AddressShort = GlobalMethods.GetShorterString(address, 12),
                    BlockHash = blockhash,
                    Height = string.IsNullOrEmpty(blockheight) ? 0 : Convert.ToUInt32(blockheight),
                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(timereceived).ToLocalTime(),
                    Amount = amount,
                    Type = TransferType.In,
                    Confirmations = confirmations
                };
            }
            catch (Exception ex)
            {
                Logger.LogException("LTC.FMWR", ex);
                return null;
            }
        }
    }
}
