using NervaWalletMiner.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using NervaWalletMiner.Objects.DataGrid;
using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class GetTransfers
    {
        // TODO: Make Wallet methods interfaces. Implement here for given coin.
        // Change Request and Response objects to generic, UI specific objects and build them here
        public static async Task<GetTransfersResponse> CallAsync(RpcSettings rpc, GetTransfersRequest transfersRequest)
        {
            GetTransfersResponse resp = new();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string serviceUrl = GlobalMethods.GetServiceUrl(rpc);

                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);

                    var requestParams = new JObject
                    {
                        ["in"] = transfersRequest.IncludeIn,
                        ["out"] = transfersRequest.IncludeOut,
                        ["pending"] = transfersRequest.IncludePending,
                        ["failed"] = transfersRequest.IncludeFailed,
                        ["pool"] = transfersRequest.IncludePool,
                        ["filter_by_height"] = transfersRequest.IsFilterByHeight,
                        ["min_height"] = transfersRequest.MinHeight,
                        ["account_index"] = transfersRequest.AccountIndex,
                        ["subaddr_indices"] = new JArray(transfersRequest.SubaddressIndices),
                        ["all_accounts"] = transfersRequest.IsAllAccounts
                    };

                    var requestJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = "0",
                        ["method"] = "get_transfers",
                        ["params"] = requestParams
                    };


                    request.Content = new StringContent(requestJson.ToString());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("GetTransfers.CA", "Calling POST: " + serviceUrl);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("GetTransfers.CA", "Call returned: " + serviceUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // Check for errors
                        var error = JObject.Parse(jsonObject.ToString())["error"];
                        if (error != null)
                        {
                            resp.Error.IsError = true;
                            resp.Error.Code = error["code"].ToString();
                            resp.Error.Message = error["message"].ToString();
                            Logger.LogError("RWGA.CSA", "Error from service. Code: " + resp.Error.Code + ", Message: " + resp.Error.Message);
                        }
                        else
                        {
                            GetTransfersRpc getTransfersResponse = JsonConvert.DeserializeObject<GetTransfersRpc>(jsonObject.SelectToken("result").ToString());
                            foreach(TransferEntry entry in getTransfersResponse.In)
                            {
                                Transfer newTransfer = new()
                                {
                                    TransactionId = entry.txid,
                                    TransactionIdShort = GlobalMethods.GetShorterString(entry.txid, 12),
                                    PaymentId = entry.payment_id,
                                    Height = entry.height,
                                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp),
                                    Amount = GlobalMethods.XnvFromAtomicUnits(entry.amount, 2),
                                    Type = entry.type
                                };

                                resp.Transfers.Add(newTransfer);
                            }

                            foreach (TransferEntry entry in getTransfersResponse.Out)
                            {
                                Transfer newTransfer = new()
                                {
                                    TransactionId = entry.txid,
                                    TransactionIdShort = GlobalMethods.GetShorterString(entry.txid, 12),
                                    PaymentId = entry.payment_id,
                                    Height = entry.height,
                                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp),
                                    Amount = GlobalMethods.XnvFromAtomicUnits(entry.amount, 2),
                                    Type = entry.type
                                };

                                resp.Transfers.Add(newTransfer);
                            }

                            foreach (TransferEntry entry in getTransfersResponse.pending)
                            {
                                Transfer newTransfer = new()
                                {
                                    TransactionId = entry.txid,
                                    TransactionIdShort = GlobalMethods.GetShorterString(entry.txid, 12),
                                    PaymentId = entry.payment_id,
                                    Height = entry.height,
                                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp),
                                    Amount = GlobalMethods.XnvFromAtomicUnits(entry.amount, 2),
                                    Type = entry.type
                                };

                                resp.Transfers.Add(newTransfer);
                            }


                            resp.Error.IsError = false;
                        }
                    }
                    else
                    {
                        resp.Error.IsError = true;
                        resp.Error.Code = response.StatusCode.ToString();
                        resp.Error.Message = response.ReasonPhrase;

                        Logger.LogError("RWGA.CSA", "Response failed. Code: " + response.StatusCode + ", Phrase: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RWGA.CSA", ex);
            }

            return resp;
        }

        // Internal structures
        private class GetTransfersRpc
        {
            public List<TransferEntry> In { get; set; } = [];
            public List<TransferEntry> Out { get; set; } = [];
            public List<TransferEntry> pending { get; set; } = [];
            public List<TransferEntry> failed { get; set; } = [];
            public List<TransferEntry> pool { get; set; } = [];
        }

        private class TransferEntry
        {
            public string txid { get; set; } = string.Empty;
            public string payment_id { get; set; } = string.Empty;
            public ulong height { get; set; }
            public ulong timestamp { get; set; }
            public ulong amount { get; set; }
            public ulong fee { get; set; }
            public string note { get; set; } = string.Empty;
            public List<TransferDestination> destinations { get; set; } = [];
            public string type { get; set; } = string.Empty;
            public ulong unlock_time { get; set; }
            public bool locked { get; set; }
            public SubaddressIndex subaddr_index { get; set; } = new();
            public List<SubaddressIndex> subaddr_indices { get; set; } = [];
            public string address { get; set; } = string.Empty;
            public bool double_spend_seen { get; set; }
            public ulong confirmations { get; set; }
            public ulong suggested_confirmations_threshold { get; set; }
        }

        private class TransferDestination
        {
            public ulong amount { get; set; }
            public string address { get; set; } = string.Empty;
        }

        private class SubaddressIndex
        {
            public uint major { get; set; }
            public uint minor { get; set; }
        }
    }
}