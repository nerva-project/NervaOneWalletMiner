using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    internal class WalletServiceDASH : IWalletService
    {
        private static int _id = 0;

        #region Open Wallet
        public async Task<OpenWalletResponse> OpenWallet(RpcBase rpc, OpenWalletRequest requestObj)
        {
            OpenWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["filename"] = requestObj.WalletName,
                    ["load_on_startup"] = false
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "loadwallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                Logger.LogException("DAS.WOPW", ex);
            }

            return responseObj;
        }
        #endregion // Open Wallet

        #region Close Wallet
        public async Task<CloseWalletResponse> CloseWallet(RpcBase rpc, CloseWalletRequest requestObj)
        {
            CloseWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["wallet_name"] = requestObj.WalletName,
                    ["load_on_startup"] = false

                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "unloadwallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                Logger.LogException("DAS.WCLW", ex);
            }

            return responseObj;
        }
        #endregion // Close Wallet

        #region Create Wallet
        public async Task<CreateWalletResponse> CreateWallet(RpcBase rpc, CreateWalletRequest requestObj)
        {
            CreateWalletResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["wallet_name"] = requestObj.WalletName,
                    ["passphrase"] = requestObj.Password
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "createwallet",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                Logger.LogException("DAS.WCRW", ex);
            }

            return responseObj;
        }
        #endregion // Create Wallet

        #region Create Account
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
                    ["method"] = "getnewaddress",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                Logger.LogException("DAS.WCRA", ex);
            }

            return responseObj;
        }
        #endregion // Create Account

        public Task<LabelAccountResponse> LabelAccount(RpcBase rpc, LabelAccountRequest requestObj)
        {
            // setlabel
            throw new NotImplementedException();
        }

        public Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            // TODO: Figure out how restoring works and make it work or change it so it's coin specific
            throw new NotImplementedException();
        }

        public Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
        {
            // importelectrumwallet
            // importwalle
            throw new NotImplementedException();
        }

        public Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj)
        {
            // sendtoaddress
            throw new NotImplementedException();
        }

        public Task<TransferResponse> TransferSplit(RpcBase rpc, TransferRequest requestObj)
        {
            // Not supported
            throw new NotImplementedException();
        }

        public Task<RescanSpentResponse> RescanSpent(RpcBase rpc, RescanSpentRequest requestObj)
        {
            // scantxoutset
            throw new NotImplementedException();
        }

        public Task<RescanBlockchainResponse> RescanBlockchain(RpcBase rpc, RescanBlockchainRequest requestObj)
        {
            // rescanblockchain
            throw new NotImplementedException();
        }

        public Task<MakeIntegratedAddressResponse> MakeIntegratedAddress(RpcBase rpc, MakeIntegratedAddressRequest requestObj)
        {
            // TODO: Add coin specific flag and show this option, only if coin supports it
            throw new NotImplementedException();
        }

        #region Get Accounts
        public async Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj)
        {
            GetAccountsResponse responseObj = new();

            try
            {
                bool isSuccess = false;


                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "getwalletinfo"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                        isSuccess = true;
                        ResGetAccounts getAccountsResponse = JsonConvert.DeserializeObject<ResGetAccounts>(jsonObject.SelectToken("result").ToString());
                        responseObj.BalanceUnlocked = getAccountsResponse.balance - getAccountsResponse.unconfirmed_balance;
                        responseObj.BalanceTotal = getAccountsResponse.balance;

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }

                if(isSuccess)
                {
                    // Build request content json
                    var requestParams = new JObject
                    {
                        ["addlocked"] = true,
                        ["include_empty"] = true,
                        ["include_watchonly"] = true,
                    };
                    requestJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "listreceivedbyaddress",
                        ["params"] = requestParams
                    };

                    // Call service and process response
                    httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                            isSuccess = true;
                            List<WalletAccount> getAccountsResponse = JsonConvert.DeserializeObject<List<WalletAccount>>(jsonObject.SelectToken("result").ToString());

                            uint index = 0;

                            foreach (WalletAccount account in getAccountsResponse)
                            {
                                Account newAccount = new()
                                {
                                    Index = index++,
                                    Label = account.label,
                                    AddressFull = account.address,
                                    AddressShort = GlobalMethods.GetShorterString(account.address, 12),
                                    BalanceTotal = account.amount,
                                    // TODO: Need to do this another way. Also, rename those balances so they make more sense
                                    BalanceUnlocked = account.confirmations > 10 ? account.amount : 0
                                };

                                responseObj.SubAccounts.Add(newAccount);
                            }
                        }
                    }
                    else
                    {
                        // Set HTTP error
                        responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.WGTA", ex);
            }

            return responseObj;
        }

        private class ResGetAccounts
        {
            public decimal balance { get; set; }
            public decimal unconfirmed_balance { get; set; }
        }

        private class WalletAccount
        {
            public string address { get; set; } = string.Empty;
            public decimal amount { get; set; }
            public int confirmations { get; set; }
            public string label { get; set; } = string.Empty;
        }
        #endregion // Get Accounts

        #region Get Transfers
        public async Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj)
        {
            GetTransfersResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["blockhash"] = requestObj.SinceBlockHash,
                    ["target_confirmations"] = 1,
                    ["include_watchonly"] = true,
                    ["include_removed"] = true
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "listsinceblock",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                        // Create success response object
                        List<TransferEntry> getTransfersResponse = JsonConvert.DeserializeObject<List<TransferEntry>>(jsonObject.SelectToken("result.transactions").ToString());
                        foreach (TransferEntry entry in getTransfersResponse)
                        {
                            Transfer newTransfer = new()
                            {
                                AccountIndex = -1,
                                TransactionId = entry.txid,
                                AddressShort = GlobalMethods.GetShorterString(entry.address, 12),
                                Height = Convert.ToUInt32(entry.blockheight),
                                Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timereceived),
                                Amount = Convert.ToDecimal(entry.amount),
                                Type = CommonDASH.GetTransactionType(entry.category)
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
                Logger.LogException("DAS.WGTF", ex);
            }

            return responseObj;
        }

        private class TransferEntry
        {
            public string address { get; set; } = string.Empty;
            public string amount { get; set; } = string.Empty;
            public string fee { get; set; } = string.Empty;
            public string blockheight { get; set; } = string.Empty;
            public string txid { get; set; } = string.Empty;
            public string category { get; set; } = string.Empty;
            public string label { get; set; } = string.Empty;
            public string blockhash { get; set; } = string.Empty;
            public ulong timereceived { get; set; }
            public long confirmations { get; set; }
            public string comment { get; set; } = string.Empty;
            public List<TransferDetails> Details { get; set; } = [];
        }

        private class TransferDetails
        {
            public string address { get; set; } = string.Empty;
            public string category { get; set; } = string.Empty;

        }
        #endregion // Get Transfers

        public async Task<GetTransferByTxIdResponse> GetTransferByTxId(RpcBase rpc, GetTranserByTxIdRequest requestObj)
        {
            GetTransferByTxIdResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["txid"] = requestObj.TransactionId,
                    ["include_watchonly"] = true,
                    ["verbose"] = false
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "gettransaction",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
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
                        // Create success response object
                        TransferEntry getTransfByTxIdResponse = JsonConvert.DeserializeObject<TransferEntry>(jsonObject.SelectToken("result").ToString());
                        
                        responseObj.TransactionId = getTransfByTxIdResponse.txid;                        
                        responseObj.Height = Convert.ToUInt32(getTransfByTxIdResponse.blockheight);
                        responseObj.Timestamp = GlobalMethods.UnixTimeStampToDateTime(getTransfByTxIdResponse.timereceived);
                        responseObj.Amount = string.IsNullOrEmpty(getTransfByTxIdResponse.amount) ? 0 : Convert.ToDecimal(getTransfByTxIdResponse.amount);
                        responseObj.Fee = string.IsNullOrEmpty(getTransfByTxIdResponse.fee) ? 0 : Convert.ToDecimal(getTransfByTxIdResponse.fee);
                        responseObj.Confirmations = getTransfByTxIdResponse.confirmations;
                        responseObj.Note = getTransfByTxIdResponse.comment;

                        foreach(TransferDetails details in getTransfByTxIdResponse.Details)
                        {
                            // Grab first one. Adjust if need be
                            responseObj.Address = details.address;
                            responseObj.Type = details.category;
                            break;
                        }

                        // TODO: If you want responseObj.Destinations, need to send true for ["verbose"] and try to pick it up that way
                        //responseObj.Destinations.Add(destination.address + " | " + destination.amount);

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
                Logger.LogException("DAS.WGTI", ex);
            }

            return responseObj;
        }

        private class ResGetTransferById
        {
            public TransferEntry transfer { get; set; } = new();
            public List<TransferEntry> transfers { get; set; } = [];
        }

        public Task<QueryKeyResponse> QueryKey(RpcBase rpc, QueryKeyRequest requestObj)
        {
            // DumpWallet
            throw new NotImplementedException();
        }


        #region Unsupported Methods
        public Task<SaveWalletResponse> SaveWallet(RpcBase rpc, SaveWalletRequest requestObj)
        {
            // Not used. ICoinSettings.IsSavingWalletSupported
            throw new NotImplementedException();
        }

        public Task<GetHeightResponse> GetHeight(RpcBase rpc, GetHeightRequest requestObj)
        {
            // Not used. ICoinSettings.IsWalletHeightSupported
            throw new NotImplementedException();
        }
        #endregion // Unsupported Methods
    }
}