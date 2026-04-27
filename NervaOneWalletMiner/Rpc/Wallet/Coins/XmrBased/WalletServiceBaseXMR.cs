using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    public abstract class WalletServiceBaseXMR : IWalletService
    {
        protected abstract string CoinPrefix { get; }
        protected virtual decimal CoinAtomicUnits => 1_000_000_000_000m;

        protected decimal AmountFromAtomicUnits(ulong value, int decimalPlaces) =>
            Math.Round(Convert.ToDecimal(value / (double)CoinAtomicUnits), decimalPlaces);

        protected ulong AtomicUnitsFromAmount(decimal amount) =>
            (ulong)(amount * CoinAtomicUnits);

        protected static string GetCallerName([CallerMemberName] string name = "") => name;

        protected ServiceError GetServiceError(string source, JToken error)
        {
            ServiceError serviceError = new();

            try
            {
                serviceError.IsError = true;
                serviceError.Code = error["code"]?.ToString() ?? string.Empty;
                serviceError.Message = error["message"]?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".CGSE", ex);
            }

            return serviceError;
        }

        protected static uint GetPriority(string stringPriority)
        {
            uint priority = 0;

            switch (stringPriority)
            {
                case SendPriority.Low:
                    priority = 1;
                    break;
                case SendPriority.Medium:
                    priority = 2;
                    break;
                case SendPriority.High:
                    priority = 3;
                    break;
                default:
                    priority = 0;
                    break;
            }

            return priority;
        }

        #region Open Wallet
        /* RPC request params:
         *  std::string filename;
         *  std::string password;
         *  bool autosave_current;
         */
        public async Task<OpenWalletResponse> OpenWallet(RpcBase rpc, OpenWalletRequest requestObj)
        {
            OpenWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["filename"] = requestObj.WalletName,
                    ["password"] = new string(requestObj.Password)
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "open_wallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Nothing expected back so just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WOPW", ex);
            }
            finally
            {
                Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
            }

            return responseObj;
        }
        #endregion // Open Wallet

        #region Close Wallet
        /* RPC request params:
         *  bool autosave_current;
         */
        public async Task<CloseWalletResponse> CloseWallet(RpcBase rpc, CloseWalletRequest requestObj)
        {
            CloseWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "close_wallet"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Nothing expected back so just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WCLW", ex);
            }

            return responseObj;
        }
        #endregion // Close Wallet

        #region Create Wallet
        /* RPC request params:
         *  std::string filename;
         *  std::string password;
         *  std::string language;
         */
        public async Task<CreateWalletResponse> CreateWallet(RpcBase rpc, CreateWalletRequest requestObj)
        {
            CreateWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["filename"] = requestObj.WalletName,
                    ["password"] = new string(requestObj.Password),
                    ["language"] = requestObj.Language
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "create_wallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Nothing expected back so just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WCRW", ex);
            }
            finally
            {
                Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
            }

            return responseObj;
        }
        #endregion // Create Wallet

        #region Create Account
        /* RPC request params:
         *  std::string label;
         */
        public async Task<CreateAccountResponse> CreateAccount(RpcBase rpc, CreateAccountRequest requestObj)
        {
            CreateAccountResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["label"] = requestObj.Label
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "create_account",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Returns account_index and address but don't really need it
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WCRA", ex);
            }

            return responseObj;
        }
        #endregion // Create Account

        #region Label Account
        /* RPC request params:
         *  uint32_t account_index;
         *  std::string label;
         */
        public async Task<LabelAccountResponse> LabelAccount(RpcBase rpc, LabelAccountRequest requestObj)
        {
            LabelAccountResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["account_index"] = requestObj.AccountIndex,
                    ["label"] = requestObj.Label
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "label_account",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WLAT", ex);
            }

            return responseObj;
        }
        #endregion // Label Account

        #region Save Wallet
        public async Task<SaveWalletResponse> SaveWallet(RpcBase rpc, SaveWalletRequest requestObj)
        {
            SaveWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "store"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WSWT", ex);
            }

            return responseObj;
        }
        #endregion // Save Wallet

        #region Restore from Seed
        /* RPC request params:
         *  uint64_t restore_height;                OPT
         *  std::string filename;
         *  std::string seed;
         *  std::string seed_offset;
         *  std::string password;
         *  std::string language;
         *  bool autosave_current;                  OPT
         *  bool enable_multisig_experimental;      OPT
         */
        public virtual async Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            RestoreFromSeedResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["restore_height"] = requestObj.RestoreHeight,
                    ["filename"] = requestObj.WalletName,
                    ["seed"] = new string(requestObj.Seed),
                    ["seed_offset"] = requestObj.SeedOffset,
                    ["password"] = new string(requestObj.Password),
                    ["language"] = requestObj.Language,
                    ["autosave_current"] = requestObj.AutoSave
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "restore_deterministic_wallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            ResRestoreFromSeed createWalletResponse = JsonConvert.DeserializeObject<ResRestoreFromSeed>(resultToken.ToString())!;
                            responseObj.Address = createWalletResponse.address;
                            responseObj.Seed = createWalletResponse.seed;
                            responseObj.Info = createWalletResponse.info;
                            responseObj.WasDeprecated = createWalletResponse.was_deprecated;

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WRFS", ex);
            }
            finally
            {
                Array.Clear(requestObj.Seed, 0, requestObj.Seed.Length);
                Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
            }

            return responseObj;
        }

        private class ResRestoreFromSeed
        {
            public string address { get; set; } = string.Empty;
            public string seed { get; set; } = string.Empty;
            public string info { get; set; } = string.Empty;
            public bool was_deprecated { get; set; }
        }
        #endregion // Restore from Seed

        #region Restore from Keys
        /* RPC request params:
         *  uint64_t restore_height;                OPT
         *  std::string filename;
         *  std::string address;
         *  std::string spendkey;
         *  std::string viewkey;
         *  std::string password;
         *  bool autosave_current;                  OPT
         *  std::string language;
         */
        public virtual async Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
        {
            RestoreFromKeysResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["restore_height"] = requestObj.RestoreHeight,
                    ["filename"] = requestObj.WalletName,
                    ["address"] = requestObj.WalletAddress,
                    ["spendkey"] = new string(requestObj.SpendKey),
                    ["viewkey"] = new string(requestObj.ViewKey),
                    ["password"] = new string(requestObj.Password),
                    ["language"] = requestObj.Language,
                    ["autosave_current"] = requestObj.AutoSave
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "generate_from_keys",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            ResRestoreFromKeys createWalletResponse = JsonConvert.DeserializeObject<ResRestoreFromKeys>(resultToken.ToString())!;
                            responseObj.Address = createWalletResponse.address;
                            responseObj.Info = createWalletResponse.info;

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WRFK", ex);
            }
            finally
            {
                Array.Clear(requestObj.ViewKey, 0, requestObj.ViewKey.Length);
                Array.Clear(requestObj.SpendKey, 0, requestObj.SpendKey.Length);
                Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
            }

            return responseObj;
        }

        private class ResRestoreFromKeys
        {
            public string address { get; set; } = string.Empty;
            public string info { get; set; } = string.Empty;
        }
        #endregion // Restore from Keys

        #region Transfer
        /* RPC request params:
         *  std::list<transfer_destination> destinations;
         *  uint32_t account_index;
         *  std::set<uint32_t> subaddr_indices;
         *  std::set<uint32_t> subtract_fee_from_outputs;       OPT
         *  uint32_t priority;
         *  uint64_t ring_size;                                 OPT
         *  uint64_t unlock_time;
         *  std::string payment_id;
         *  bool get_tx_key;
         *  bool do_not_relay;                                  OPT
         *  bool get_tx_hex;                                    OPT
         *  bool get_tx_metadata;                               OPT
         */
        public async Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj)
        {
            TransferResponse responseObj = new();

            try
            {
                // Build request content json
                JArray destinationsJson = new JArray();
                foreach (Common.TransferDestination destination in requestObj.Destinations)
                {
                    destinationsJson.Add(new JObject
                    {
                        ["amount"] = AtomicUnitsFromAmount(destination.Amount),
                        ["address"] = destination.Address
                    });
                }

                JObject paramsJson = new JObject
                {
                    ["destinations"] = destinationsJson,
                    ["account_index"] = requestObj.AccountIndex,
                    ["subaddr_indices"] = new JArray(requestObj.SubAddressIndices),
                    ["priority"] = GetPriority(requestObj.Priority),
                    ["unlock_time"] = requestObj.UnlockTime,
                    ["payment_id"] = requestObj.PaymentId is null ? "" : requestObj.PaymentId,
                    ["get_tx_key"] = requestObj.GetTxKey,
                    ["do_not_relay"] = requestObj.DoNotRelay,
                    ["get_tx_hex"] = requestObj.GetTxHex,
                    ["get_tx_metadata"] = requestObj.GetTxMetadata
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "transfer",
                    ["params"] = paramsJson
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            // We don't use response values
                            ResTransfer transferResponse = JsonConvert.DeserializeObject<ResTransfer>(resultToken.ToString())!;

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WTRA", ex);
            }

            return responseObj;
        }

        private class ResTransfer
        {
            public string tx_hash { get; set; } = string.Empty;
            public string tx_key { get; set; } = string.Empty;
            public ulong amount { get; set; } = 0;
            public AmountsList amounts_by_dest { get; set; } = new();
            public ulong fee { get; set; } = 0;
            public ulong weight { get; set; } = 0;
            public string tx_blob { get; set; } = string.Empty;
            public string tx_metadata { get; set; } = string.Empty;
            public string multisig_txset { get; set; } = string.Empty;
            public string unsigned_txset { get; set; } = string.Empty;
            public KeyImageList spent_key_images { get; set; } = new();
        }

        private class AmountsList
        {
            public List<ulong> amounts { get; set; } = [];
        }

        private class KeyImageList
        {
            public List<string> key_images { get; set; } = [];
        }
        #endregion // Transfer

        #region Transfer Split
        /* RPC request params:
         *  std::list<transfer_destination> destinations;
         *  uint32_t account_index;
         *  std::set<uint32_t> subaddr_indices;
         *  uint32_t priority;
         *  uint64_t ring_size;                                 OPT
         *  uint64_t unlock_time;
         *  std::string payment_id;
         *  bool get_tx_key;
         *  bool do_not_relay;                                  OPT
         *  bool get_tx_hex;                                    OPT
         *  bool get_tx_metadata;                               OPT
         */
        public async Task<TransferResponse> TransferSplit(RpcBase rpc, TransferRequest requestObj)
        {
            TransferResponse responseObj = new();

            try
            {
                // Build request content json
                JArray destinationsJson = new JArray();
                foreach (Common.TransferDestination destination in requestObj.Destinations)
                {
                    destinationsJson.Add(new JObject
                    {
                        ["amount"] = AtomicUnitsFromAmount(destination.Amount),
                        ["address"] = destination.Address
                    });
                }

                JObject paramsJson = new JObject
                {
                    ["destinations"] = destinationsJson,
                    ["account_index"] = requestObj.AccountIndex,
                    ["subaddr_indices"] = new JArray(requestObj.SubAddressIndices),
                    ["priority"] = GetPriority(requestObj.Priority),
                    ["unlock_time"] = requestObj.UnlockTime,
                    ["payment_id"] = requestObj.PaymentId is null ? "" : requestObj.PaymentId,
                    ["get_tx_key"] = requestObj.GetTxKey,
                    ["do_not_relay"] = requestObj.DoNotRelay,
                    ["get_tx_hex"] = requestObj.GetTxHex,
                    ["get_tx_metadata"] = requestObj.GetTxMetadata
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "transfer_split",
                    ["params"] = paramsJson
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {                        
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            // We don't use response values
                            ResTransferSplit transferResponse = JsonConvert.DeserializeObject<ResTransferSplit>(resultToken.ToString())!;

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WTRS", ex);
            }

            return responseObj;
        }

        private class ResTransferSplit
        {
            public List<string> tx_hash_list { get; set; } = [];
            public List<string> tx_key_list { get; set; } = [];
            public List<ulong> amount_list { get; set; } = [];
            public List<AmountsList> amounts_by_dest_list { get; set; } = [];
            public List<ulong> fee_list { get; set; } = [];
            public List<ulong> weight_list { get; set; } = [];
            public List<string> tx_blob_list { get; set; } = [];
            public List<string> tx_metadata_list { get; set; } = [];
            public string multisig_txset { get; set; } = string.Empty;
            public string unsigned_txset { get; set; } = string.Empty;
            public List<KeyImageList> spent_key_images_list { get; set; } = [];
        }
        #endregion // Transfer Split

        #region Rescan Spent
        public async Task<RescanSpentResponse> RescanSpent(RpcBase rpc, RescanSpentRequest requestObj)
        {
            RescanSpentResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "rescan_spent"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WRES", ex);
            }

            return responseObj;
        }
        #endregion // Rescan Spent

        #region Rescan Blockchain
        /* RPC request params:
         *  bool hard;                          OPT
         */

        // Rescan the blockchain from scratch. If 'hard' is specified, you will lose any information which can not be recovered from the blockchain itself.
        public async Task<RescanBlockchainResponse> RescanBlockchain(RpcBase rpc, RescanBlockchainRequest requestObj)
        {
            RescanBlockchainResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "rescan_blockchain"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // Just set error to false
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WRBN", ex);
            }

            return responseObj;
        }
        #endregion // Rescan Blockchain

        #region Make Integrated Address
        /* RPC request params:
         *  std::string standard_address;
         *  std::string payment_id;
         */
        public async Task<MakeIntegratedAddressResponse> MakeIntegratedAddress(RpcBase rpc, MakeIntegratedAddressRequest requestObj)
        {
            MakeIntegratedAddressResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["standard_address"] = requestObj.StandardAddress,
                    ["payment_id"] = requestObj.PaymentId
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "make_integrated_address",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            ResMakeIntegratedAddress createWalletResponse = JsonConvert.DeserializeObject<ResMakeIntegratedAddress>(resultToken.ToString())!;
                            responseObj.IntegratedAddress = createWalletResponse.integrated_address;
                            responseObj.PaymentId = createWalletResponse.payment_id;

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WMIA", ex);
            }

            return responseObj;
        }

        private class ResMakeIntegratedAddress
        {
            public string integrated_address { get; set; } = string.Empty;
            public string payment_id { get; set; } = string.Empty;
        }
        #endregion // Make Integrated Address

        #region Get Accounts
        /* RPC request params:
         *  std::string tag;      // all accounts if empty, otherwise those accounts with this tag
         *  bool strict_balances;
         *  bool regexp; // allow regular expression filters if set to true
         */
        public async Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj)
        {
            GetAccountsResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_accounts"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            ResGetAccounts getAccountsResponse = JsonConvert.DeserializeObject<ResGetAccounts>(resultToken.ToString())!;
                            responseObj.BalanceUnlocked = AmountFromAtomicUnits(getAccountsResponse.total_unlocked_balance, 4);
                            responseObj.BalanceTotal = AmountFromAtomicUnits(getAccountsResponse.total_balance, 4);

                            foreach (WalletAccount account in getAccountsResponse.subaddress_accounts)
                            {
                                Account newAccount = new()
                                {
                                    Index = account.account_index,
                                    Label = account.label,
                                    AddressFull = account.base_address,
                                    AddressShort = GlobalMethods.GetShorterString(account.base_address, 12),
                                    BalanceTotal = AmountFromAtomicUnits(account.balance, 4),
                                    BalanceUnlocked = AmountFromAtomicUnits(account.unlocked_balance, 4)
                                };

                                responseObj.SubAccounts.Add(newAccount);
                            }

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGTA", ex);
            }

            return responseObj;
        }

        // Internal helper objects used to interact with service
        private class ResGetAccounts
        {
            public ulong total_balance { get; set; }
            public ulong total_unlocked_balance { get; set; }
            public List<WalletAccount> subaddress_accounts { get; set; } = [];
        }

        private class WalletAccount
        {
            public uint account_index { get; set; }
            public string base_address { get; set; } = string.Empty;
            public ulong balance { get; set; }
            public ulong unlocked_balance { get; set; }
            public string label { get; set; } = string.Empty;
            public string tag { get; set; } = string.Empty;
        }
        #endregion // Get Accounts

        #region Get Transfers
        /* RPC request params:
         *  bool in;
         *  bool out;
         *  bool pending;
         *  bool failed;
         *  bool pool;
         *
         *  bool filter_by_height;
         *  uint64_t min_height;
         *  uint64_t max_height;
         *  uint32_t account_index;
         *  std::set<uint32_t> subaddr_indices;
         *  bool all_accounts;
         */
        public async Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj)
        {
            GetTransfersResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["in"] = requestObj.IncludeIn,
                    ["out"] = requestObj.IncludeOut,
                    ["pending"] = requestObj.IncludePending,
                    ["failed"] = requestObj.IncludeFailed,
                    ["pool"] = requestObj.IncludePool,
                    ["filter_by_height"] = requestObj.IsFilterByHeight,
                    ["min_height"] = requestObj.MinHeight,
                    ["account_index"] = requestObj.AccountIndex,
                    ["subaddr_indices"] = new JArray(requestObj.SubaddressIndices),
                    ["all_accounts"] = requestObj.IsAllAccounts
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_transfers",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {                        
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            // Create success response object
                            ResGetTransfers getTransfersResponse = JsonConvert.DeserializeObject<ResGetTransfers>(resultToken.ToString())!;
                            foreach (TransferEntry entry in getTransfersResponse.In)
                            {
                                Transfer newTransfer = new()
                                {
                                    AccountIndex = entry.subaddr_index.major,
                                    TransactionId = entry.txid,
                                    AddressShort = GlobalMethods.GetShorterString(entry.address, 12),
                                    Height = entry.height,
                                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp).ToLocalTime(),
                                    Amount = AmountFromAtomicUnits(entry.amount, 4),
                                    Type = entry.type
                                };

                                responseObj.Transfers.Add(newTransfer);
                            }

                            foreach (TransferEntry entry in getTransfersResponse.Out)
                            {
                                Transfer newTransfer = new()
                                {
                                    AccountIndex = entry.subaddr_index.major,
                                    TransactionId = entry.txid,
                                    AddressShort = GlobalMethods.GetShorterString(entry.address, 12),
                                    Height = entry.height,
                                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp).ToLocalTime(),
                                    Amount = AmountFromAtomicUnits(entry.amount, 4),
                                    Type = entry.type
                                };

                                responseObj.Transfers.Add(newTransfer);
                            }

                            foreach (TransferEntry entry in getTransfersResponse.pending)
                            {
                                Transfer newTransfer = new()
                                {
                                    AccountIndex = entry.subaddr_index.major,
                                    TransactionId = entry.txid,
                                    AddressShort = GlobalMethods.GetShorterString(entry.address, 12),
                                    Height = entry.height,
                                    Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp).ToLocalTime(),
                                    Amount = AmountFromAtomicUnits(entry.amount, 4),
                                    Type = entry.type
                                };

                                responseObj.Transfers.Add(newTransfer);
                            }

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGTF", ex);
            }

            return responseObj;
        }

        // Internal helper objects used to interact with service
        private class ResGetTransfers
        {
            public List<TransferEntry> In { get; set; } = [];
            public List<TransferEntry> Out { get; set; } = [];
            public List<TransferEntry> pending { get; set; } = [];
            public List<TransferEntry> failed { get; set; } = [];
            public List<TransferEntry> pool { get; set; } = [];
        }
        #endregion // Get Transfers

        #region Get Transfer By TxId
        /* RPC request params:
         *  std::string txid;
         *  uint32_t account_index;
         */
        public async Task<GetTransferByTxIdResponse> GetTransferByTxId(RpcBase rpc, GetTranserByTxIdRequest requestObj)
        {
            GetTransferByTxIdResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["txid"] = requestObj.TransactionId,
                    ["account_index"] = requestObj.AccountIndex
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_transfer_by_txid",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            // Create success response object
                            ResGetTransferById getTransfByTxIdResponse = JsonConvert.DeserializeObject<ResGetTransferById>(resultToken.ToString())!;
                            responseObj.Address = getTransfByTxIdResponse.transfer.address;
                            responseObj.TransactionId = getTransfByTxIdResponse.transfer.txid;
                            responseObj.PaymentId = getTransfByTxIdResponse.transfer.payment_id;
                            responseObj.Type = getTransfByTxIdResponse.transfer.type;
                            responseObj.Height = getTransfByTxIdResponse.transfer.height;
                            responseObj.Timestamp = GlobalMethods.UnixTimeStampToDateTime(getTransfByTxIdResponse.transfer.timestamp).ToLocalTime();
                            responseObj.Amount = AmountFromAtomicUnits(getTransfByTxIdResponse.transfer.amount, 6);
                            responseObj.Fee = AmountFromAtomicUnits(getTransfByTxIdResponse.transfer.fee, 6);
                            responseObj.Note = getTransfByTxIdResponse.transfer.note;
                            responseObj.Confirmations = getTransfByTxIdResponse.transfer.confirmations;

                            foreach (TransferDestination destination in getTransfByTxIdResponse.transfer.destinations)
                            {
                                responseObj.Destinations.Add(destination.address + " | " + AmountFromAtomicUnits(destination.amount, 6));
                            }

                            // There is also transfers but it seems to have the same info. Can you have more than 1 transfer for given transaction id?
                            //foreach (TransferEntry entry in getTransfByTxIdResponse.transfers)
                            //{

                            //}

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGTI", ex);
            }

            return responseObj;
        }

        // Internal helper objects used to interact with service
        private class ResGetTransferById
        {
            public TransferEntry transfer { get; set; } = new();
            public List<TransferEntry> transfers { get; set; } = [];
        }
        #endregion // Get Transfer By TxId

        #region Get Height
        public async Task<GetHeightResponse> GetHeight(RpcBase rpc, GetHeightRequest requestObj)
        {
            GetHeightResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_height"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            ResGetHeight getHeightResponse = JsonConvert.DeserializeObject<ResGetHeight>(resultToken.ToString())!;
                            responseObj.Height = getHeightResponse.height;

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGHT", ex);
            }

            return responseObj;
        }

        private class ResGetHeight
        {
            public ulong height { get; set; }
        }
        #endregion // Get Height

        #region Query Key
        /* RPC request params:
         *  std::string key_type;
         */
        public virtual async Task<GetPrivateKeysResponse> GetPrivateKeys(RpcBase rpc, GetPrivateKeysRequest requestObj)
        {
            // Default: two separate query_key calls for view_key and spend_key (XMR/WOW pattern)

            GetPrivateKeysResponse responseObj = new();

            try
            {
                // Build request content json
                JObject requestJson = [];

                if (requestObj.KeyType == KeyType.Mnemonic)
                {
                    requestJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = "0",
                        ["method"] = "query_key",
                        ["params"] = new JObject() { ["key_type"] = "mnemonic" }
                    };
                }
                else if (requestObj.KeyType == KeyType.AllViewSpend)
                {
                    requestJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = "0",
                        ["method"] = "query_key",
                        ["params"] = new JObject() { ["key_type"] = "view_key" }
                    };
                }

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            ResQueryKey queryKeyResponse = JsonConvert.DeserializeObject<ResQueryKey>(resultToken.ToString())!;
                            if (requestObj.KeyType == KeyType.Mnemonic)
                            {
                                responseObj.Mnemonic = queryKeyResponse.key.ToCharArray();

                                responseObj.Error.IsError = false;
                            }
                            else if (requestObj.KeyType == KeyType.AllViewSpend)
                            {
                                responseObj.PrivateViewKey = queryKeyResponse.key.ToCharArray();

                                // Call again to get spend key
                                requestJson = new JObject
                                {
                                    ["jsonrpc"] = "2.0",
                                    ["id"] = "0",
                                    ["method"] = "query_key",
                                    ["params"] = new JObject() { ["key_type"] = "spend_key" }
                                };

                                httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                                if (httpResponse.IsSuccessStatusCode)
                                {
                                    jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                                    error = jsonObject["error"];
                                    if (error != null)
                                    {
                                        // Set Service error
                                        responseObj.Error = GetServiceError(GetCallerName(), error);
                                    }
                                    else
                                    {
                                        var spendKeyToken = jsonObject.SelectToken("result");
                                        if (spendKeyToken == null)
                                        {
                                            responseObj.Error.IsError = true;
                                            responseObj.Error.Message = "Response missing 'result' field";
                                        }
                                        else
                                        {
                                            queryKeyResponse = JsonConvert.DeserializeObject<ResQueryKey>(spendKeyToken.ToString())!;
                                            responseObj.PrivateSpendKey = queryKeyResponse.key.ToCharArray();

                                            responseObj.Error.IsError = false;
                                        }
                                    }
                                }
                                else
                                {
                                    // Set HTTP error
                                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGPK", ex);
            }

            return responseObj;
        }

        private class ResQueryKey
        {
            public string key { get; set; } = string.Empty;
        }
        #endregion // Query Key

        #region Get Transfers Export
        public async Task<GetTransfersExportResponse> GetTransfersExport(RpcBase rpc, GetTransfersExportRequest requestObj)
        {
            GetTransfersExportResponse responseObj = new();
            StringBuilder exportBuilder = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["in"] = true,
                    ["out"] = true,
                    ["pending"] = false,
                    ["failed"] = false,
                    ["pool"] = false,
                    ["filter_by_height"] = true,
                    ["min_height"] = 0,
                    ["account_index"] = requestObj.AccountIndex,
                    ["subaddr_indices"] = new JArray(requestObj.SubaddressIndices),
                    ["all_accounts"] = requestObj.IsAllAccounts
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_transfers",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        List<ExportObject> exportList = [];

                        var resultToken = jsonObject.SelectToken("result");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result' field";
                        }
                        else
                        {
                            // Create success response object
                            ResGetTransfers getTransfersResponse = JsonConvert.DeserializeObject<ResGetTransfers>(resultToken.ToString())!;
                            foreach (TransferEntry entry in getTransfersResponse.In)
                            {
                                ExportObject newTransfer = new()
                                {
                                    Address = entry.address,
                                    Height = entry.height.ToString(),
                                    Type = entry.type,
                                    TimeStamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp),
                                    Amount = AmountFromAtomicUnits(entry.amount, 12),
                                    TransactionId = entry.txid,
                                    PaymentId = entry.payment_id,
                                    Fee = AmountFromAtomicUnits(entry.fee, 12),
                                    Note = entry.note
                                };

                                string destinations = string.Empty;
                                foreach (TransferDestination destination in entry.destinations)
                                {
                                    if (!string.IsNullOrEmpty(destinations))
                                    {
                                        destinations += ",";
                                    }
                                    destinations += destination.address;
                                }

                                newTransfer.Destination = destinations;
                                exportList.Add(newTransfer);
                            }

                            foreach (TransferEntry entry in getTransfersResponse.Out)
                            {
                                ExportObject newTransfer = new()
                                {
                                    Address = entry.address,
                                    Height = entry.height.ToString(),
                                    Type = entry.type,
                                    TimeStamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp),
                                    Amount = AmountFromAtomicUnits(entry.amount, 12),
                                    TransactionId = entry.txid,
                                    PaymentId = entry.payment_id,
                                    Fee = AmountFromAtomicUnits(entry.fee, 12),
                                    Note = entry.note
                                };

                                string destinations = string.Empty;
                                foreach (TransferDestination destination in entry.destinations)
                                {
                                    if (!string.IsNullOrEmpty(destinations))
                                    {
                                        destinations += ",";
                                    }
                                    destinations += destination.address;
                                }

                                newTransfer.Destination = destinations;
                                exportList.Add(newTransfer);
                            }

                            // Header row
                            exportBuilder.AppendLine("height,type,timestamp,amount,fee,running balance,address,transaction id,payment id,destination,note");

                            // Transactions
                            decimal runningBalance = 0;
                            foreach (ExportObject transfer in exportList.OrderBy(x => x.TimeStamp))
                            {
                                runningBalance += (transfer.Type == TransferType.Out ? -1 * (transfer.Amount + transfer.Fee) : transfer.Amount);

                                exportBuilder.AppendLine(
                                    transfer.Height + "," +
                                    transfer.Type + "," +
                                    transfer.TimeStamp.ToString() + "," +
                                    transfer.Amount.ToString("F12") + "," +
                                    transfer.Fee.ToString("F12") + "," +
                                    runningBalance.ToString("F12") + "," +
                                    transfer.Address + "," +
                                    transfer.TransactionId + "," +
                                    transfer.PaymentId + "," +
                                    "\"" + transfer.Destination + "\"," +
                                    "\"" + transfer.Note + "\""
                                );
                            }

                            responseObj.ExportString = exportBuilder.ToString();
                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGEX", ex);
            }

            return responseObj;
        }

        private class ExportObject
        {
            public string Address { get; set; } = string.Empty;
            public string Height { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public DateTime TimeStamp { get; set; }
            public decimal Amount { get; set; }
            public string TransactionId { get; set; } = string.Empty;
            public string PaymentId { get; set; } = string.Empty;
            public decimal Fee { get; set; }
            public string Destination { get; set; } = string.Empty;
            public string Note { get; set; } = string.Empty;
        }
        #endregion // Get Transfers Export

        #region Sweep Below
        /* RPC request params (sweep_all):
         *  string address;
         *  uint32_t account_index;
         *  uint64_t below_amount;          OPT - only outputs below this amount will be swept
         *  bool get_tx_keys;               OPT
         *  bool do_not_relay;              OPT
         *  bool get_tx_hex;                OPT
         *  bool get_tx_metadata;           OPT
         */
        public async Task<SweepBelowResponse> SweepBelow(RpcBase rpc, SweepBelowRequest requestObj)
        {
            SweepBelowResponse responseObj = new();

            try
            {
                JObject paramsJson = new JObject
                {
                    ["address"] = requestObj.WalletAddress,
                    ["account_index"] = 0,
                    ["below_amount"] = AtomicUnitsFromAmount(Convert.ToDecimal(requestObj.Amount))
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "sweep_all",
                    ["params"] = paramsJson
                };

                Logger.LogDebug(CoinPrefix + ".SWBL", "Calling sweep_all with below_amount: " + requestObj.Amount);
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString(), TimeSpan.FromMinutes(60));
                Logger.LogDebug(CoinPrefix + ".SWBL", "sweep_all returned.");
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".SWBL", ex);
            }

            return responseObj;
        }
        #endregion // Sweep Below

        #region Common Internal Helper Objects
        private class TransferEntry
        {
            public string txid { get; set; } = string.Empty;
            public string payment_id { get; set; } = string.Empty;
            public ulong height { get; set; }
            public ulong timestamp { get; set; }
            public ulong amount { get; set; }
            public List<ulong> amounts { get; set; } = [];
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
            public long confirmations { get; set; }
            public ulong suggested_confirmations_threshold { get; set; }
        }

        private class TransferDestination
        {
            public ulong amount { get; set; }
            public string address { get; set; } = string.Empty;
        }

        private class SubaddressIndex
        {
            public int major { get; set; }
            public int minor { get; set; }
        }
        #endregion // Common Internal Helper Objects

        #region Unsupported Methods
        public Task<UnlockWithPassResponse> UnlockWithPass(RpcBase rpc, UnlockWithPassRequest requestObj)
        {
            throw new NotImplementedException();
        }
        #endregion // Unsupported Methods
    }
}
