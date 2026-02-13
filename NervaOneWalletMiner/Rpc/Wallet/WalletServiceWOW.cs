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
using System.Text;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    public class WalletServiceWOW : IWalletService
    {
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
                    ["password"] = requestObj.Password
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WOPW", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WCLW", ex);
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
                    ["password"] = requestObj.Password,
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WCRW", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // Monero returns account_index and address but don't really need it

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WCRA", ex);
            }

            return responseObj;
        }

        private class ResCreateAccount
        {
            public uint account_index { get; set; } = 0;
            public string address { get; set; } = string.Empty;
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WLAT", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WSWT", ex);
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
        public async Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            RestoreFromSeedResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["restore_height"] = requestObj.RestoreHeight,
                    ["filename"] = requestObj.WalletName,
                    ["seed"] = requestObj.Seed,
                    ["seed_offset"] = requestObj.SeedOffset,
                    ["password"] = requestObj.Password,
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResRestoreFromSeed createWalletResponse = JsonConvert.DeserializeObject<ResRestoreFromSeed>(jsonObject.SelectToken("result").ToString());
                        responseObj.Address = createWalletResponse.address;
                        responseObj.Seed = createWalletResponse.seed;
                        responseObj.Info = createWalletResponse.info;
                        responseObj.WasDeprecated = createWalletResponse.was_deprecated;

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WRFS", ex);
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
        public async Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
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
                    ["spendkey"] = requestObj.SpendKey,
                    ["viewkey"] = requestObj.ViewKey,
                    ["password"] = requestObj.Password,
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResRestoreFromKeys createWalletResponse = JsonConvert.DeserializeObject<ResRestoreFromKeys>(jsonObject.SelectToken("result").ToString());
                        responseObj.Address = createWalletResponse.address;
                        responseObj.Info = createWalletResponse.info;

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WRFK", ex);
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
                dynamic paramsJson = new JObject();
                dynamic destinationsJson = new JArray();
                foreach (Common.TransferDestination destination in requestObj.Destinations)
                {
                    dynamic newDest = new JObject();
                    newDest.amount = CommonWOW.AtomicUnitsFromDoubleAmount(destination.Amount);
                    newDest.address = destination.Address;
                    destinationsJson.Add(newDest);
                }
                paramsJson.destinations = destinationsJson;

                paramsJson.account_index = requestObj.AccountIndex;
                paramsJson.subaddr_indices = new JArray(requestObj.SubAddressIndices);
                paramsJson.priority = CommonWOW.GetPriority(requestObj.Priority);
                paramsJson.unlock_time = requestObj.UnlockTime;
                paramsJson.payment_id = requestObj.PaymentId is null ? "" : requestObj.PaymentId;
                paramsJson.get_tx_key = requestObj.GetTxKey;
                paramsJson.do_not_relay = requestObj.DoNotRelay;
                paramsJson.get_tx_hex = requestObj.GetTxHex;
                paramsJson.get_tx_metadata = requestObj.GetTxMetadata;

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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // We don't use response values
                        ResTransfer transferResponse = JsonConvert.DeserializeObject<ResTransfer>(jsonObject.SelectToken("result").ToString());

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WTRA", ex);
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
                dynamic paramsJson = new JObject();
                dynamic destinationsJson = new JArray();
                foreach (Common.TransferDestination destination in requestObj.Destinations)
                {
                    dynamic newDest = new JObject();
                    newDest.amount = CommonWOW.AtomicUnitsFromDoubleAmount(destination.Amount);
                    newDest.address = destination.Address;
                    destinationsJson.Add(newDest);
                }
                paramsJson.destinations = destinationsJson;

                paramsJson.account_index = requestObj.AccountIndex;
                paramsJson.subaddr_indices = new JArray(requestObj.SubAddressIndices);
                paramsJson.priority = CommonWOW.GetPriority(requestObj.Priority);
                paramsJson.unlock_time = requestObj.UnlockTime;
                paramsJson.payment_id = requestObj.PaymentId is null ? "" : requestObj.PaymentId;
                paramsJson.get_tx_key = requestObj.GetTxKey;
                paramsJson.do_not_relay = requestObj.DoNotRelay;
                paramsJson.get_tx_hex = requestObj.GetTxHex;
                paramsJson.get_tx_metadata = requestObj.GetTxMetadata;

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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // We don't use response values
                        ResTransferSplit transferResponse = JsonConvert.DeserializeObject<ResTransferSplit>(jsonObject.SelectToken("result").ToString());

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WTRS", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WRES", ex);
            }

            return responseObj;
        }
        #endregion // Rescan Spent

        #region Rescan Blockchain
        /* RPC request params:
         *  bool hard;                          OPT
         */

        //Rescan the blockchain from scratch. If 'hard' is specified, you will lose any information which can not be recovered from the blockchain itself.
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WRBN", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResMakeIntegratedAddress createWalletResponse = JsonConvert.DeserializeObject<ResMakeIntegratedAddress>(jsonObject.SelectToken("result").ToString());
                        responseObj.IntegratedAddress = createWalletResponse.integrated_address;
                        responseObj.PaymentId = createWalletResponse.payment_id;

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WMIA", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResGetAccounts getAccountsResponse = JsonConvert.DeserializeObject<ResGetAccounts>(jsonObject.SelectToken("result").ToString());
                        responseObj.BalanceUnlocked = CommonWOW.DoubleAmountFromAtomicUnits(getAccountsResponse.total_unlocked_balance, 4);
                        responseObj.BalanceTotal = CommonWOW.DoubleAmountFromAtomicUnits(getAccountsResponse.total_balance, 4);

                        foreach (WalletAccount account in getAccountsResponse.subaddress_accounts)
                        {
                            Account newAccount = new()
                            {
                                Index = account.account_index,
                                Label = account.label,
                                AddressFull = account.base_address,
                                AddressShort = GlobalMethods.GetShorterString(account.base_address, 12),
                                BalanceTotal = CommonWOW.DoubleAmountFromAtomicUnits(account.balance, 4),
                                BalanceUnlocked = CommonWOW.DoubleAmountFromAtomicUnits(account.unlocked_balance, 4)
                            };

                            responseObj.SubAccounts.Add(newAccount);
                        }

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WGTA", ex);
            }

            return responseObj;
        }

        // Internal helper obejcts used to interact with service
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // Create success response object
                        ResGetTransfers getTransfersResponse = JsonConvert.DeserializeObject<ResGetTransfers>(jsonObject.SelectToken("result").ToString());
                        foreach (TransferEntry entry in getTransfersResponse.In)
                        {
                            Transfer newTransfer = new()
                            {
                                AccountIndex = entry.subaddr_index.major,
                                TransactionId = entry.txid,
                                AddressShort = GlobalMethods.GetShorterString(entry.address, 12),
                                Height = entry.height,
                                Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp).ToLocalTime(),
                                Amount = CommonWOW.DoubleAmountFromAtomicUnits(entry.amount, 4),
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
                                Amount = CommonWOW.DoubleAmountFromAtomicUnits(entry.amount, 4),
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
                                Amount = CommonWOW.DoubleAmountFromAtomicUnits(entry.amount, 4),
                                Type = entry.type
                            };

                            responseObj.Transfers.Add(newTransfer);
                        }

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WGTF", ex);
            }

            return responseObj;
        }

        // Internal helper obejcts used to interact with service
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // Create success response object
                        ResGetTransferById getTransfByTxIdResponse = JsonConvert.DeserializeObject<ResGetTransferById>(jsonObject.SelectToken("result").ToString());
                        responseObj.Address = getTransfByTxIdResponse.transfer.address;
                        responseObj.TransactionId = getTransfByTxIdResponse.transfer.txid;
                        responseObj.PaymentId = getTransfByTxIdResponse.transfer.payment_id;
                        responseObj.Type = getTransfByTxIdResponse.transfer.type;
                        responseObj.Height = getTransfByTxIdResponse.transfer.height;
                        responseObj.Timestamp = GlobalMethods.UnixTimeStampToDateTime(getTransfByTxIdResponse.transfer.timestamp).ToLocalTime();
                        responseObj.Amount = CommonWOW.DoubleAmountFromAtomicUnits(getTransfByTxIdResponse.transfer.amount, 6);
                        responseObj.Fee = CommonWOW.DoubleAmountFromAtomicUnits(getTransfByTxIdResponse.transfer.fee, 6);
                        responseObj.Note = getTransfByTxIdResponse.transfer.note;
                        responseObj.Confirmations = getTransfByTxIdResponse.transfer.confirmations;

                        foreach (TransferDestination destination in getTransfByTxIdResponse.transfer.destinations)
                        {
                            responseObj.Destinations.Add(destination.address + " | " + CommonWOW.DoubleAmountFromAtomicUnits(destination.amount, 6));
                        }

                        // There is also transfers but it seems to have the same info. Can you have more than 1 transfer for given transactioin id?
                        //foreach (TransferEntry entry in getTransfersResponse.transfers)
                        //{

                        //}                       

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WGTI", ex);
            }

            return responseObj;
        }

        // Internal helper obejcts used to interact with service
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResGetHeight getHeightResponse = JsonConvert.DeserializeObject<ResGetHeight>(jsonObject.SelectToken("result").ToString());
                        responseObj.Height = getHeightResponse.height;

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WGHT", ex);
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
        public async Task<GetPrivateKeysResponse> GetPrivateKeys(RpcBase rpc, GetPrivateKeysRequest requestObj)
        {
            // TODO: I do not like how this is done. Change it!

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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResQueryKey getHeightResponse = JsonConvert.DeserializeObject<ResQueryKey>(jsonObject.SelectToken("result").ToString());
                        if (requestObj.KeyType == KeyType.Mnemonic)
                        {
                            responseObj.Mnemonic = getHeightResponse.key;

                            responseObj.Error.IsError = false;
                        }
                        else if (requestObj.KeyType == KeyType.AllViewSpend)
                        {
                            responseObj.PrivateViewKey = getHeightResponse.key;

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
                                jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                                error = JObject.Parse(jsonObject.ToString())["error"];
                                if (error != null)
                                {
                                    // Set Service error
                                    responseObj.Error = CommonWOW.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                                }
                                else
                                {
                                    getHeightResponse = JsonConvert.DeserializeObject<ResQueryKey>(jsonObject.SelectToken("result").ToString());
                                    responseObj.PrivateSpendKey = getHeightResponse.key;

                                    responseObj.Error.IsError = false;
                                }
                            }
                            else
                            {
                                // Set HTTP error
                                responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                            }
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WGPK", ex);
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
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonXNV.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        List<ExportObject> exportList = [];

                        // Create success response object
                        ResGetTransfers getTransfersResponse = JsonConvert.DeserializeObject<ResGetTransfers>(jsonObject.SelectToken("result").ToString());
                        foreach (TransferEntry entry in getTransfersResponse.In)
                        {
                            ExportObject newTransfer = new()
                            {
                                Address = entry.address,
                                Height = entry.height.ToString(),
                                Type = entry.type,
                                TimeStamp = GlobalMethods.UnixTimeStampToDateTime(entry.timestamp),
                                Amount = CommonXNV.DecimalAmountFromAtomicUnits(entry.amount, 12),
                                TransactionId = entry.txid,
                                PaymentId = entry.payment_id,
                                Fee = CommonXNV.DecimalAmountFromAtomicUnits(entry.fee, 12),
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
                                Amount = CommonXNV.DecimalAmountFromAtomicUnits(entry.amount, 12),
                                TransactionId = entry.txid,
                                PaymentId = entry.payment_id,
                                Fee = CommonXNV.DecimalAmountFromAtomicUnits(entry.fee, 12),
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
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WOW.WGEX", ex);
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

        public Task<SweepBelowResponse> SweepBelow(RpcBase rpc, SweepBelowRequest requestObj)
        {
            throw new NotImplementedException();
        }
        #endregion // Unsupported Methods
    }
}
