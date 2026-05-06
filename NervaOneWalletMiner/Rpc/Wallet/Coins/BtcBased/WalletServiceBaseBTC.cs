using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    public abstract class WalletServiceBaseBTC : IWalletService
    {
        protected abstract string CoinPrefix { get; }

        private static int _id = 0;

        private static string GetCallerName([System.Runtime.CompilerServices.CallerMemberName] string name = "") => name;

        protected ServiceError GetServiceError(string source, JToken error)
        {
            ServiceError serviceError = new();

            try
            {
                serviceError.IsError = true;
                if (error is JObject errorObj)
                {
                    serviceError.Code = errorObj["code"]?.ToString() ?? string.Empty;
                    serviceError.Message = errorObj["message"]?.ToString() ?? string.Empty;
                }
                else
                {
                    serviceError.Message = error.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".CGSE", ex);
            }

            return serviceError;
        }

        protected string GetTransactionType(string type)
        {
            string returnType = TransferType.Unknown;

            switch (type.ToLower())
            {
                case "receive":
                    returnType = TransferType.In;
                    break;
                case "send":
                    returnType = TransferType.Out;
                    break;
                case "coinjoin":
                    returnType = TransferType.Out;
                    break;
                case "generate":
                    returnType = TransferType.Block;
                    break;
                case "immature":
                    returnType = TransferType.Block;
                    break;
                default:
                    returnType = TransferType.Unknown;
                    break;
            }

            return returnType;
        }

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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    JObject? errorJson = null;
                    try { errorJson = JObject.Parse(errorContent); } catch { }

                    string? errorCode = errorJson?.SelectToken("error.code")?.ToString();
                    if (errorCode == "-35")
                    {
                        // Wallet was auto-loaded by daemon on startup — already in the desired state
                        Logger.LogDebug(CoinPrefix + ".WOPW", "Wallet already loaded: " + requestObj.WalletName);
                        responseObj.Error.IsError = false;
                    }
                    else
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WOPW", ex);
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
                    ["passphrase"] = new string(requestObj.Password),
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                Logger.LogException(CoinPrefix + ".WOPW", ex);
            }
            finally
            {
                Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
            }

            return responseObj;
        }
        #endregion // Unlock with Passphrase

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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                Logger.LogException(CoinPrefix + ".WCLW", ex);
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
                    ["passphrase"] = new string(requestObj.Password)
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        responseObj.Error.IsError = false;

                        // Generate the first address so GetAccounts has something to display immediately after wallet creation.
                        // Bitcoin Core does not auto-create an address on createwallet; without this, listreceivedbyaddress returns empty.
                        var firstAddrJson = new JObject
                        {
                            ["jsonrpc"] = "2.0",
                            ["id"] = _id++,
                            ["method"] = "getnewaddress",
                            ["params"] = new JObject { ["label"] = "" }
                        };

                        HttpResponseMessage firstAddrResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), firstAddrJson.ToString(), rpc.UserName, rpc.Password);
                        if (firstAddrResponse.IsSuccessStatusCode)
                        {
                            JObject firstAddrResult = JObject.Parse(await firstAddrResponse.Content.ReadAsStringAsync());
                            var firstAddrError = firstAddrResult["error"];
                            if (firstAddrError != null && firstAddrError.Type != JTokenType.Null)
                            {
                                Logger.LogError(CoinPrefix + ".WCRW", "Wallet created but failed to generate first address: " + firstAddrError.ToString());
                            }
                        }
                        else
                        {
                            Logger.LogError(CoinPrefix + ".WCRW", "Wallet created but getnewaddress HTTP call failed");
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                Logger.LogException(CoinPrefix + ".WCRA", ex);
            }

            return responseObj;
        }
        #endregion // Create Account

        public Task<LabelAccountResponse> LabelAccount(RpcBase rpc, LabelAccountRequest requestObj)
        {
            // TODO: setlabel
            throw new NotImplementedException();
        }

        #region Import Wallet
        public async Task<ImportWalletResponse> ImportWallet(RpcBase rpc, ImportWalletRequest requestObj)
        {
            try
            {
                bool isDescriptorDump = IsDescriptorDumpFile(requestObj.DumpFileWithPath);
                Logger.LogDebug(CoinPrefix + ".WIMW", "Restoring " + requestObj.WalletName + " from " + (isDescriptorDump ? "descriptor" : "legacy") + " dump");

                if (isDescriptorDump)
                {
                    return await ImportDescriptorWallet(rpc, requestObj);
                }
                else
                {
                    return await ImportLegacyWallet(rpc, requestObj);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WIMW", ex);
            }
            finally
            {
                if (requestObj.Password.Length > 0)
                {
                    Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
                }
            }

            return new ImportWalletResponse();
        }

        private static bool IsDescriptorDumpFile(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                for (int i = 0; i < 10; i++)
                {
                    string? line = reader.ReadLine();
                    if (line == null) { break; }
                    if (line.Contains("importdescriptors")) { return true; }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("BTC.IDDF", ex);
            }
            return false;
        }

        private async Task<ImportWalletResponse> ImportLegacyWallet(RpcBase rpc, ImportWalletRequest requestObj)
        {
            ImportWalletResponse responseObj = new();

            // Step 1: Create a new empty legacy wallet (descriptors:false required for importwallet)
            var createParams = new JObject
            {
                ["wallet_name"] = requestObj.WalletName,
                ["passphrase"] = requestObj.Password.Length > 0 ? new string(requestObj.Password) : string.Empty,
                ["descriptors"] = false
            };

            var createJson = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = _id++,
                ["method"] = "createwallet",
                ["params"] = createParams
            };

            HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), createJson.ToString(), rpc.UserName, rpc.Password);
            if (!httpResponse.IsSuccessStatusCode)
            {
                responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                return responseObj;
            }

            JObject createResult = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
            var createError = createResult["error"];
            if (createError != null && createError.Type != JTokenType.Null)
            {
                responseObj.Error = GetServiceError(GetCallerName(), createError);
                return responseObj;
            }

            // Step 2: If password was set, unlock the wallet before importing
            string walletUrl = HttpHelper.GetServiceUrl(rpc, "wallet/" + requestObj.WalletName);
            if (requestObj.Password.Length > 0)
            {
                var unlockParams = new JObject
                {
                    ["passphrase"] = new string(requestObj.Password),
                    ["timeout"] = 7200
                };

                var unlockJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "walletpassphrase",
                    ["params"] = unlockParams
                };

                HttpResponseMessage unlockResponse = await HttpHelper.GetPostFromService(walletUrl, unlockJson.ToString(), rpc.UserName, rpc.Password);
                if (!unlockResponse.IsSuccessStatusCode)
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), unlockResponse);
                    return responseObj;
                }

                JObject unlockResult = JObject.Parse(await unlockResponse.Content.ReadAsStringAsync());
                var unlockError = unlockResult["error"];
                if (unlockError != null && unlockError.Type != JTokenType.Null)
                {
                    responseObj.Error = GetServiceError(GetCallerName(), unlockError);
                    return responseObj;
                }
            }

            // Step 3: Import all keys. importwallet triggers a full blockchain rescan — use a long timeout.
            var importJson = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = _id++,
                ["method"] = "importwallet",
                ["params"] = new JObject { ["filename"] = requestObj.DumpFileWithPath }
            };

            httpResponse = await HttpHelper.GetPostFromService(walletUrl, importJson.ToString(), rpc.UserName, rpc.Password, TimeSpan.FromMinutes(20));
            if (httpResponse.IsSuccessStatusCode)
            {
                string content = await httpResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Empty response from importwallet";
                }
                else
                {
                    JObject jsonObject = JObject.Parse(content);
                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
                    {
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        responseObj.Error.IsError = false;
                    }
                }
            }
            else
            {
                responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
            }

            return responseObj;
        }

        private async Task<ImportWalletResponse> ImportDescriptorWallet(RpcBase rpc, ImportWalletRequest requestObj)
        {
            ImportWalletResponse responseObj = new();

            // Step 1: Create a new descriptor wallet (default — no descriptors:false)
            var createParams = new JObject
            {
                ["wallet_name"] = requestObj.WalletName,
                ["passphrase"] = requestObj.Password.Length > 0 ? new string(requestObj.Password) : string.Empty
            };

            var createJson = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = _id++,
                ["method"] = "createwallet",
                ["params"] = createParams
            };

            HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), createJson.ToString(), rpc.UserName, rpc.Password);
            if (!httpResponse.IsSuccessStatusCode)
            {
                responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                return responseObj;
            }

            JObject createResult = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
            var createError = createResult["error"];
            if (createError != null && createError.Type != JTokenType.Null)
            {
                responseObj.Error = GetServiceError(GetCallerName(), createError);
                return responseObj;
            }

            // Step 2: If password was set, unlock the wallet before importing
            string walletUrl = HttpHelper.GetServiceUrl(rpc, "wallet/" + requestObj.WalletName);
            if (requestObj.Password.Length > 0)
            {
                var unlockParams = new JObject
                {
                    ["passphrase"] = new string(requestObj.Password),
                    ["timeout"] = 7200
                };

                var unlockJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "walletpassphrase",
                    ["params"] = unlockParams
                };

                HttpResponseMessage unlockResponse = await HttpHelper.GetPostFromService(walletUrl, unlockJson.ToString(), rpc.UserName, rpc.Password);
                if (!unlockResponse.IsSuccessStatusCode)
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), unlockResponse);
                    return responseObj;
                }

                JObject unlockResult = JObject.Parse(await unlockResponse.Content.ReadAsStringAsync());
                var unlockError = unlockResult["error"];
                if (unlockError != null && unlockError.Type != JTokenType.Null)
                {
                    responseObj.Error = GetServiceError(GetCallerName(), unlockError);
                    return responseObj;
                }
            }

            // Step 3: Parse descriptors from the dump file (skip comment lines, parse the JSON body)
            string fileContent = File.ReadAllText(requestObj.DumpFileWithPath);
            string jsonContent = string.Join("\n", fileContent.Split('\n')
                .Where(l => !l.TrimStart().StartsWith("#") && !string.IsNullOrWhiteSpace(l)));

            JObject dumpJson;
            try
            {
                dumpJson = JObject.Parse(jsonContent);
            }
            catch (JsonReaderException)
            {
                responseObj.Error.IsError = true;
                responseObj.Error.Message = "Failed to parse descriptor dump file — file may be corrupt or wrong format";
                return responseObj;
            }

            JArray? descriptors = dumpJson["descriptors"] as JArray;
            if (descriptors == null || descriptors.Count == 0)
            {
                responseObj.Error.IsError = true;
                responseObj.Error.Message = "No descriptors found in dump file";
                return responseObj;
            }

            // Step 4: Import descriptors. Timestamps in each descriptor tell Bitcoin Core how far back to scan,
            // so this is much faster than importwallet's full rescan from genesis.
            var importJson = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = _id++,
                ["method"] = "importdescriptors",
                ["params"] = new JArray { descriptors }
            };

            httpResponse = await HttpHelper.GetPostFromService(walletUrl, importJson.ToString(), rpc.UserName, rpc.Password, TimeSpan.FromMinutes(30));
            if (httpResponse.IsSuccessStatusCode)
            {
                string content = await httpResponse.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Empty response from importdescriptors";
                }
                else
                {
                    JObject jsonObject = JObject.Parse(content);
                    var topError = jsonObject["error"];
                    if (topError != null && topError.Type != JTokenType.Null)
                    {
                        responseObj.Error = GetServiceError(GetCallerName(), topError);
                    }
                    else
                    {
                        // importdescriptors returns an array of per-descriptor results
                        var results = jsonObject["result"] as JArray;
                        var failed = results?.FirstOrDefault(r => r["success"]?.Value<bool>() == false);
                        if (failed != null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Failed to import one or more descriptors: " + (failed["error"]?["message"]?.ToString() ?? "Unknown error");
                        }
                        else
                        {
                            responseObj.Error.IsError = false;
                        }
                    }
                }
            }
            else
            {
                responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
            }

            return responseObj;
        }
        #endregion // Import Wallet

        public Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            // TODO: Figure out how restoring works and make it work or change it so it's coin specific
            throw new NotImplementedException();
        }

        public Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
        {
            // TODO: importelectrumwallet
            // importwallet
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        // We don't use response values
                        //ResTransfer transferResponse = JsonConvert.DeserializeObject<ResTransfer>(jsonObject.SelectToken("result")!.ToString())!;

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
                Logger.LogException(CoinPrefix + ".WTRA", ex);
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
            // TODO: This is not really DASH/BTC way of doing things but should work for now.
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                            List<WalletAccount> getAccountsResponse = JsonConvert.DeserializeObject<List<WalletAccount>>(resultToken.ToString())!;
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
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
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
                        JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                        var error = jsonObject["error"];
                        if (error != null && error.Type != JTokenType.Null)
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
                                List<WalletAccount> getAccountsResponse = JsonConvert.DeserializeObject<List<WalletAccount>>(resultToken.ToString())!;
                                foreach (WalletAccount account in getAccountsResponse)
                                {
                                    if (accountsDictionary.TryGetValue(account.address, out Account? existing))
                                    {
                                        // Accumulate UTXOs — an address can have multiple unspent outputs
                                        existing.BalanceTotal += account.amount;
                                        existing.BalanceUnlocked += account.amount;
                                    }
                                    else
                                    {
                                        // Change address not in listreceivedbyaddress — add it
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
                    }
                    else
                    {
                        // Set HTTP error
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGTA", ex);
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(GetCallerName(), error);
                    }
                    else
                    {
                        var resultToken = jsonObject.SelectToken("result.transactions");
                        if (resultToken == null)
                        {
                            responseObj.Error.IsError = true;
                            responseObj.Error.Message = "Response missing 'result.transactions' field";
                        }
                        else
                        {
                            // Create success response object
                            List<TransferEntry> getTransfersResponse = JsonConvert.DeserializeObject<List<TransferEntry>>(resultToken.ToString())!;
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
                                    Type = GetTransactionType(entry.category)
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
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                            TransferEntry getTransfByTxIdResponse = JsonConvert.DeserializeObject<TransferEntry>(resultToken.ToString())!;
                            responseObj.TransactionId = getTransfByTxIdResponse.txid;
                            responseObj.Height = string.IsNullOrEmpty(getTransfByTxIdResponse.blockheight) ? 0 : Convert.ToUInt32(getTransfByTxIdResponse.blockheight);
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

        private class ResGetTransferById
        {
            public TransferEntry transfer { get; set; } = new();
            public List<TransferEntry> transfers { get; set; } = [];
        }
        #endregion // Get Transfer By TxId

        #region Query Keys        
        public async Task<GetPrivateKeysResponse> GetPrivateKeys(RpcBase rpc, GetPrivateKeysRequest requestObj)
        {
            GetPrivateKeysResponse responseObj = new();

            try
            {
                // listdescriptors replaces dumpwallet for descriptor wallets (Bitcoin Core 22+)
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "listdescriptors",
                    ["params"] = new JObject { ["private"] = true }
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
                    {
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
                            string fileContent =
                                "# Bitcoin Wallet Descriptor Export\r\n" +
                                "# Wallet: " + GlobalData.OpenedWalletName + "\r\n" +
                                "# Exported: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                                "# WARNING: Keep this file secure. Anyone with access can steal your funds.\r\n" +
                                "# To restore: use Bitcoin Core importdescriptors with the descriptors below.\r\n\r\n" +
                                resultToken.ToString(Formatting.Indented);

                            File.WriteAllText(requestObj.DumpFileWithPath, fileContent);
                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    JObject? errorJson = null;
                    try { errorJson = JObject.Parse(errorContent); } catch { }

                    if (errorJson?.SelectToken("error.code")?.ToString() == "-4")
                    {
                        Logger.LogDebug(CoinPrefix + ".WGPK", "listdescriptors not available, falling back to dumpwallet for legacy wallet");
                        return await DumpLegacyWallet(rpc, requestObj);
                    }

                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WGPK", ex);
            }

            return responseObj;
        }
        
        private async Task<GetPrivateKeysResponse> DumpLegacyWallet(RpcBase rpc, GetPrivateKeysRequest requestObj)
        {
            GetPrivateKeysResponse responseObj = new();

            try
            {
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "dumpwallet",
                    ["params"] = new JObject { ["filename"] = requestObj.DumpFileWithPath }
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());

                    var error = jsonObject["error"];
                    if (error != null && error.Type != JTokenType.Null)
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
                Logger.LogException(CoinPrefix + ".WDLW", ex);
            }

            return responseObj;
        }
        #endregion // Query Keys

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

        public Task<GetTransfersExportResponse> GetTransfersExport(RpcBase rpc, GetTransfersExportRequest requestObj)
        {
            throw new NotImplementedException();
        }

        public Task<TransferResponse> TransferSplit(RpcBase rpc, TransferRequest requestObj)
        {
            // Not supported
            throw new NotImplementedException();
        }

        public Task<SweepBelowResponse> SweepBelow(RpcBase rpc, SweepBelowRequest requestObj)
        {
            throw new NotImplementedException();
        }
        #endregion // Unsupported Methods
    }
}
