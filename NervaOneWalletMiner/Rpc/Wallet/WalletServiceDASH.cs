using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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

        #region Unlock with Passphrase
        public async Task<UnlockWithPassResponse> UnlockWithPass(RpcBase rpc, UnlockWithPassRequest requestObj)
        {
            UnlockWithPassResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["passphrase"] = requestObj.Password,
                    ["timeout"] = requestObj.TimeoutInSeconds
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "walletpassphrase",
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
        #endregion // Unlock wiht Passphrase

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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    ["id"] = _id++,
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
            // TODO: setlabel
            throw new NotImplementedException();
        }

        public Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            // TODO: Figure out how restoring works and make it work or change it so it's coin specific
            throw new NotImplementedException();
        }

        public Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
        {
            // TODO: importelectrumwallet
            // importwalle
            throw new NotImplementedException();
        }

        #region Transfer
        public async Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj)
        {
            TransferResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["address"] = requestObj.Destinations.FirstOrDefault()!.Address,
                    ["amount"] = requestObj.Destinations.FirstOrDefault()!.Amount,
                    ["comment"] = requestObj.Comment,
                    ["comment_to"] = requestObj.CommentTo,
                    ["subtractfeefromamount"] = requestObj.SubtractFeeFromAmount
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "sendtoaddress",
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // We don't use response values
                        //ResTransfer transferResponse = JsonConvert.DeserializeObject<ResTransfer>(jsonObject.SelectToken("result").ToString());

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
                Logger.LogException("DAS.WTRA", ex);
            }

            return responseObj;
        }
        #endregion // Transfer

        public Task<RescanSpentResponse> RescanSpent(RpcBase rpc, RescanSpentRequest requestObj)
        {
            // TODO: scantxoutset
            throw new NotImplementedException();
        }

        public Task<RescanBlockchainResponse> RescanBlockchain(RpcBase rpc, RescanBlockchainRequest requestObj)
        {
            // TODO: rescanblockchain
            throw new NotImplementedException();
        }

        #region Get Accounts
        public async Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj)
        {
            // TODO: This is not really DASH way of doing things but should work for now.
            // Do not need to show 0 balance addresses and can generate new address each time user wants to receive funds
            GetAccountsResponse responseObj = new();

            try
            {
                bool isSuccess = false;
                uint index = 0;
                Dictionary<string, Account> accountsDictionary = [];

                // Build request content json
                var requestParams = new JObject
                {
                    ["addlocked"] = true,
                    ["include_empty"] = true,
                    ["include_watchonly"] = true,
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "listreceivedbyaddress",
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {                        
                        List<WalletAccount> getAccountsResponse = JsonConvert.DeserializeObject<List<WalletAccount>>(jsonObject.SelectToken("result").ToString());
                                               
                        foreach (WalletAccount account in getAccountsResponse)
                        {
                            Account newAccount = new()
                            {
                                Index = index++,
                                AddressFull = account.address,
                                AddressShort = GlobalMethods.GetShorterString(account.address, 12),
                                Label = account.label
                            };

                            accountsDictionary.Add(newAccount.AddressFull, newAccount);
                        }

                        isSuccess = true;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }

                if (isSuccess)
                {
                    // Build request content json
                    requestJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "listunspent",
                        ["params"] = new JObject()
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
                            responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                        }
                        else
                        {
                            List<WalletAccount> getAccountsResponse = JsonConvert.DeserializeObject<List<WalletAccount>>(jsonObject.SelectToken("result").ToString());

                            foreach (WalletAccount account in getAccountsResponse)
                            {
                                if(accountsDictionary.ContainsKey(account.address))
                                {
                                    accountsDictionary[account.address].BalanceTotal = account.amount;
                                    accountsDictionary[account.address].BalanceUnlocked = account.amount;                                    
                                }
                                else
                                {
                                    Account newAccount = new()
                                    {
                                        Index = index++,
                                        AddressFull = account.address,
                                        AddressShort = GlobalMethods.GetShorterString(account.address, 12),
                                        BalanceTotal = account.amount,
                                        BalanceUnlocked = account.amount,                                        
                                        Label = account.label
                                    };

                                    accountsDictionary.Add(newAccount.AddressFull, newAccount);
                                }
                            }

                            responseObj.SubAccounts = accountsDictionary.Values.ToList();

                            // Add up total balances
                            foreach(Account account in responseObj.SubAccounts)
                            {
                                responseObj.BalanceTotal += account.BalanceTotal;
                                responseObj.BalanceUnlocked += account.BalanceUnlocked;
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
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.WGTA", ex);
            }

            return responseObj;
        }

        private class WalletAccount
        {
            public string address { get; set; } = string.Empty;
            public decimal amount { get; set; }
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                                Height = string.IsNullOrEmpty(entry.blockheight) ? 0 : Convert.ToUInt32(entry.blockheight),
                                Timestamp = GlobalMethods.UnixTimeStampToDateTime(entry.timereceived).ToLocalTime(),
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
        #endregion // Get Transfers

        #region Get Transfer By TxId
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
                    ["id"] = _id++,
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        // Create success response object
                        TransferEntry getTransfByTxIdResponse = JsonConvert.DeserializeObject<TransferEntry>(jsonObject.SelectToken("result").ToString());
                        
                        responseObj.TransactionId = getTransfByTxIdResponse.txid;                        
                        responseObj.Height = string.IsNullOrEmpty(getTransfByTxIdResponse.blockheight)? 0 : Convert.ToUInt32(getTransfByTxIdResponse.blockheight);
                        responseObj.Timestamp = GlobalMethods.UnixTimeStampToDateTime(getTransfByTxIdResponse.timereceived).ToLocalTime();
                        responseObj.Confirmations = getTransfByTxIdResponse.confirmations;
                        responseObj.Note = getTransfByTxIdResponse.comment;

                        foreach(TransferDetails details in getTransfByTxIdResponse.Details)
                        {
                            if(decimal.Parse(details.amount, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign) == requestObj.Amount)
                            {
                                responseObj.Address = details.address;
                                responseObj.Type = details.category;
                                responseObj.Amount = string.IsNullOrEmpty(details.amount) ? 0 : decimal.Parse(details.amount, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
                                responseObj.Fee = string.IsNullOrEmpty(details.fee) ? 0 : decimal.Parse(details.fee, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
                                break;
                            }
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
        #endregion // Get Transfer By TxId

        public async Task<GetPrivateKeysResponse> GetPrivateKeys(RpcBase rpc, GetPrivateKeysRequest requestObj)
        {
            // TODO: DumpWallet
            GetPrivateKeysResponse responseObj = new();

            try
            {
                // Build request content json
                var requestParams = new JObject
                {
                    ["filename"] = requestObj.DumpFileWithPath
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "dumpwallet",
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
                        responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                Logger.LogException("DAS.WGPK", ex);
            }

            return responseObj;
        }

        public Task<GetTransfersExportResponse> GetTransfersExport(RpcBase rpc, GetTransfersExportRequest requestObj)
        {
            throw new NotImplementedException();
        }

        #region Common Internal Helper Objects
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
            public string amount { get; set; } = string.Empty;
            public string fee { get; set; } = string.Empty;

        }
        #endregion // Common Internal Helper Objects

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

        public Task<MakeIntegratedAddressResponse> MakeIntegratedAddress(RpcBase rpc, MakeIntegratedAddressRequest requestObj)
        {
            // Not used. ICoinSettings.AreIntegratedAddressesSupported
            throw new NotImplementedException();
        }

        public Task<TransferResponse> TransferSplit(RpcBase rpc, TransferRequest requestObj)
        {
            // Not supported
            throw new NotImplementedException();
        }
        #endregion // Unsupported Methods
    }
}