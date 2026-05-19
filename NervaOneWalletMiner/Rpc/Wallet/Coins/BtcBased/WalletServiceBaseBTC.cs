using NBitcoin;
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
        protected abstract string CoinName { get; }
        protected abstract uint CoinType { get; }
        protected abstract bool SupportMultipleScriptTypes { get; }

        private static int _id = 0;

        // importdescriptors/importwallet and rescanblockchain are synchronous and block until the chain scan completes
        private static readonly TimeSpan _blockchainScanTimeout = TimeSpan.FromHours(6);
        private static readonly int _blockchainScanTimeoutSeconds = (int)_blockchainScanTimeout.TotalSeconds;
        // timestamp:"now" imports — no chain scan, just key loading
        private const int _fastImportTimeoutSeconds = 300; 

        private static string GetCallerName([System.Runtime.CompilerServices.CallerMemberName] string name = "") => name;


        #region Helper Methods
        
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
        #endregion // Helper Methods

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

        #region Create Wallet From Seed
        public async Task<CreateWalletResponse> CreateWalletFromSeed(RpcBase rpc, CreateWalletRequest requestObj)
        {
            CreateWalletResponse responseObj = new();

            try
            {
                // Step 1: Parse mnemonic and derive root key
                string mnemonicPhrase = new string(requestObj.Seed);
                Mnemonic mnemonic;
                try
                {
                    mnemonic = new Mnemonic(mnemonicPhrase, Wordlist.English);
                }
                catch (Exception)
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Invalid seed phrase";
                    return responseObj;
                }

                ExtKey masterKey = mnemonic.DeriveExtKey();
                string fingerprintHex = masterKey.GetPublicKey().GetHDFingerPrint().ToString();

                Logger.LogDebug(CoinPrefix + ".WCFS", "Creating " + requestObj.WalletName + " from seed (fingerprint: " + fingerprintHex + ")");

                // Step 2: Get descriptor checksums from node
                string baseUrl = HttpHelper.GetServiceUrl(rpc, string.Empty);
                JArray importItems = new();

                foreach (var (rawDescriptor, isInternal) in BuildSeedDescriptors(masterKey, fingerprintHex))
                {
                    var infoJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "getdescriptorinfo",
                        ["params"] = new JObject { ["descriptor"] = rawDescriptor }
                    };

                    HttpResponseMessage infoResponse = await HttpHelper.GetPostFromService(baseUrl, infoJson.ToString(), rpc.UserName, rpc.Password);
                    if (!infoResponse.IsSuccessStatusCode)
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), infoResponse);
                        return responseObj;
                    }

                    JObject infoResult = JObject.Parse(await infoResponse.Content.ReadAsStringAsync());
                    var infoError = infoResult["error"];
                    if (infoError != null && infoError.Type != JTokenType.Null)
                    {
                        responseObj.Error = GetServiceError(GetCallerName(), infoError);
                        return responseObj;
                    }

                    // Use checksum of our private descriptor, not the returned "descriptor" field
                    // (getdescriptorinfo strips private keys from "descriptor" but "checksum" is for the input)
                    string? checksum = infoResult["result"]?["checksum"]?.ToString();
                    if (string.IsNullOrEmpty(checksum))
                    {
                        responseObj.Error.IsError = true;
                        responseObj.Error.Message = "getdescriptorinfo returned no checksum for: " + rawDescriptor;
                        return responseObj;
                    }

                    importItems.Add(new JObject
                    {
                        ["desc"] = rawDescriptor + "#" + checksum,
                        ["timestamp"] = "now",
                        ["active"] = true,
                        ["range"] = new JArray { 0, 1000 },
                        ["internal"] = isInternal
                    });
                }

                // Step 3: Create blank descriptor wallet
                var createParams = new JObject
                {
                    ["wallet_name"] = requestObj.WalletName,
                    ["blank"] = true,
                    ["descriptors"] = true,
                    ["load_on_startup"] = false,
                    ["passphrase"] = requestObj.Password.Length > 0 ? new string(requestObj.Password) : string.Empty
                };

                var createJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "createwallet",
                    ["params"] = createParams
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(baseUrl, createJson.ToString(), rpc.UserName, rpc.Password);
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

                // Step 4: Unlock wallet before importing if it has a password
                string walletUrl = HttpHelper.GetServiceUrl(rpc, "wallet/" + requestObj.WalletName);
                if (requestObj.Password.Length > 0)
                {
                    var unlockJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "walletpassphrase",
                        ["params"] = new JObject
                        {
                            ["passphrase"] = new string(requestObj.Password),
                            ["timeout"] = _fastImportTimeoutSeconds
                        }
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

                // Step 5: Import seed-derived descriptors (timestamp:"now" — no history scan needed)
                var importJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "importdescriptors",
                    ["params"] = new JArray { importItems }
                };

                httpResponse = await HttpHelper.GetPostFromService(walletUrl, importJson.ToString(), rpc.UserName, rpc.Password);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    return responseObj;
                }

                JObject importResult = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
                var importTopError = importResult["error"];
                if (importTopError != null && importTopError.Type != JTokenType.Null)
                {
                    responseObj.Error = GetServiceError(GetCallerName(), importTopError);
                    return responseObj;
                }

                var results = importResult["result"] as JArray;
                var failed = results?.FirstOrDefault(r => r["success"]?.Value<bool>() == false);
                if (failed != null)
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Failed to import one or more descriptors: " + (failed["error"]?["message"]?.ToString() ?? "Unknown error");
                    return responseObj;
                }

                // Step 6: Generate first receiving address so the wallet screen is not empty
                var firstAddrJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "getnewaddress",
                    ["params"] = new JObject { ["label"] = "" }
                };

                HttpResponseMessage firstAddrResponse = await HttpHelper.GetPostFromService(walletUrl, firstAddrJson.ToString(), rpc.UserName, rpc.Password);
                if (firstAddrResponse.IsSuccessStatusCode)
                {
                    JObject firstAddrResult = JObject.Parse(await firstAddrResponse.Content.ReadAsStringAsync());
                    var firstAddrError = firstAddrResult["error"];
                    if (firstAddrError != null && firstAddrError.Type != JTokenType.Null)
                    {
                        Logger.LogError(CoinPrefix + ".WCFS", "Wallet created but failed to generate first address: " + firstAddrError.ToString());
                    }
                }

                responseObj.Error.IsError = false;
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WCFS", ex);
            }
            finally
            {
                Array.Clear(requestObj.Seed, 0, requestObj.Seed.Length);
                if (requestObj.Password.Length > 0)
                {
                    Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
                }
            }

            return responseObj;
        }
        #endregion // Create Wallet From Seed

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

        #region Import Wallet
        public async Task<ImportWalletResponse> ImportWallet(RpcBase rpc, ImportWalletRequest requestObj)
        {
            try
            {
                if (!File.Exists(Path.GetFullPath(requestObj.DumpFileWithPath)))
                {
                    ImportWalletResponse errorResponse = new();
                    errorResponse.Error.IsError = true;
                    errorResponse.Error.Message = "Dump file not found";
                    return errorResponse;
                }

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
                    ["timeout"] = _blockchainScanTimeoutSeconds
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

            httpResponse = await HttpHelper.GetPostFromService(walletUrl, importJson.ToString(), rpc.UserName, rpc.Password, _blockchainScanTimeout);
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

            // Step 1: Create a new blank descriptor wallet
            var createParams = new JObject
            {
                ["wallet_name"] = requestObj.WalletName,
                ["blank"] = true,
                ["descriptors"] = true,
                ["load_on_startup"] = false,
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
                    ["timeout"] = _blockchainScanTimeoutSeconds
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

            httpResponse = await HttpHelper.GetPostFromService(walletUrl, importJson.ToString(), rpc.UserName, rpc.Password, _blockchainScanTimeout);
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

        #region Restore From Seed
        public async Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
        {
            RestoreFromSeedResponse responseObj = new();

            try
            {
                // Step 1: Parse mnemonic and derive root key
                string mnemonicPhrase = new string(requestObj.Seed);
                Mnemonic mnemonic;
                try
                {
                    mnemonic = new Mnemonic(mnemonicPhrase, Wordlist.English);
                }
                catch (Exception)
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Invalid seed phrase — must be 12, 15, 18, 21, or 24 BIP39 English words";
                    return responseObj;
                }

                // SeedOffset is the optional BIP39 passphrase (25th word)
                string bip39Passphrase = requestObj.SeedOffset ?? string.Empty;
                ExtKey masterKey = mnemonic.DeriveExtKey(bip39Passphrase);
                string fingerprintHex = masterKey.GetPublicKey().GetHDFingerPrint().ToString();

                Logger.LogDebug(CoinPrefix + ".WRFS", "Restoring " + requestObj.WalletName + " from seed (fingerprint: " + fingerprintHex + ")");

                // Step 2: Build raw descriptor strings for each address type
                List<(string rawDescriptor, bool isInternal)> rawDescriptors = BuildSeedDescriptors(masterKey, fingerprintHex);

                // Step 3: Get checksummed descriptors from the node and build importdescriptors items
                string baseUrl = HttpHelper.GetServiceUrl(rpc, string.Empty);
                long birthdayTimestamp = requestObj.WalletBirthday.HasValue
                    ? new DateTimeOffset(requestObj.WalletBirthday.Value).ToUnixTimeSeconds()
                    : 0;
                JArray importItems = new();

                foreach (var (rawDescriptor, isInternal) in rawDescriptors)
                {
                    var infoJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "getdescriptorinfo",
                        ["params"] = new JObject { ["descriptor"] = rawDescriptor }
                    };

                    HttpResponseMessage infoResponse = await HttpHelper.GetPostFromService(baseUrl, infoJson.ToString(), rpc.UserName, rpc.Password);
                    if (!infoResponse.IsSuccessStatusCode)
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), infoResponse);
                        return responseObj;
                    }

                    JObject infoResult = JObject.Parse(await infoResponse.Content.ReadAsStringAsync());
                    var infoError = infoResult["error"];
                    if (infoError != null && infoError.Type != JTokenType.Null)
                    {
                        responseObj.Error = GetServiceError(GetCallerName(), infoError);
                        return responseObj;
                    }

                    // Use checksum of our private descriptor, not the returned "descriptor" field
                    // (getdescriptorinfo strips private keys from "descriptor" but "checksum" is for the input)
                    string? checksum = infoResult["result"]?["checksum"]?.ToString();
                    if (string.IsNullOrEmpty(checksum))
                    {
                        responseObj.Error.IsError = true;
                        responseObj.Error.Message = "getdescriptorinfo returned no checksum for: " + rawDescriptor;
                        return responseObj;
                    }

                    importItems.Add(new JObject
                    {
                        ["desc"] = rawDescriptor + "#" + checksum,
                        ["timestamp"] = birthdayTimestamp,
                        ["active"] = true,
                        ["range"] = new JArray { 0, 1000 },
                        ["internal"] = isInternal
                    });
                }

                // Step 4: Create a blank wallet (no auto-generated keys — we will import our seed-derived ones)
                var createParams = new JObject
                {
                    ["wallet_name"] = requestObj.WalletName,
                    ["blank"] = true,
                    ["descriptors"] = true,
                    ["load_on_startup"] = false,
                    ["passphrase"] = requestObj.Password.Length > 0 ? new string(requestObj.Password) : string.Empty
                };

                var createJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "createwallet",
                    ["params"] = createParams
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(baseUrl, createJson.ToString(), rpc.UserName, rpc.Password);
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

                // Step 5: Unlock wallet before importing if it has a password
                string walletUrl = HttpHelper.GetServiceUrl(rpc, "wallet/" + requestObj.WalletName);
                if (requestObj.Password.Length > 0)
                {
                    var unlockJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "walletpassphrase",
                        ["params"] = new JObject
                        {
                            ["passphrase"] = new string(requestObj.Password),
                            ["timeout"] = _blockchainScanTimeoutSeconds
                        }
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

                // Step 6: Import all descriptors — timestamp:0 triggers a full rescan from genesis
                var importJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "importdescriptors",
                    ["params"] = new JArray { importItems }
                };

                httpResponse = await HttpHelper.GetPostFromService(walletUrl, importJson.ToString(), rpc.UserName, rpc.Password, _blockchainScanTimeout);
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
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WRFS", ex);
            }
            finally
            {
                Array.Clear(requestObj.Seed, 0, requestObj.Seed.Length);
                if (requestObj.Password.Length > 0)
                {
                    Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
                }
            }

            return responseObj;
        }

        private List<(string rawDescriptor, bool isInternal)> BuildSeedDescriptors(ExtKey masterKey, string fingerprintHex)
        {
            var descriptors = new List<(string, bool)>();

            // BIP44 — P2PKH (legacy addresses, coin type varies per chain)
            var bip44Key = masterKey.Derive(KeyPath.Parse($"m/44'/{CoinType}'/0'"));
            string bip44Xprv = bip44Key.ToString(Network.Main);
            descriptors.Add(($"pkh([{fingerprintHex}/44'/{CoinType}'/0']{bip44Xprv}/0/*)", false));
            descriptors.Add(($"pkh([{fingerprintHex}/44'/{CoinType}'/0']{bip44Xprv}/1/*)", true));

            if (SupportMultipleScriptTypes)
            {
                // BIP49 — P2SH-P2WPKH (wrapped segwit)
                string bip49Xprv = masterKey.Derive(KeyPath.Parse("m/49'/0'/0'")).ToString(Network.Main);
                descriptors.Add(($"sh(wpkh([{fingerprintHex}/49'/0'/0']{bip49Xprv}/0/*))", false));
                descriptors.Add(($"sh(wpkh([{fingerprintHex}/49'/0'/0']{bip49Xprv}/1/*))", true));

                // BIP84 — P2WPKH (native segwit / bech32)
                string bip84Xprv = masterKey.Derive(KeyPath.Parse("m/84'/0'/0'")).ToString(Network.Main);
                descriptors.Add(($"wpkh([{fingerprintHex}/84'/0'/0']{bip84Xprv}/0/*)", false));
                descriptors.Add(($"wpkh([{fingerprintHex}/84'/0'/0']{bip84Xprv}/1/*)", true));

                // BIP86 — P2TR (taproot)
                string bip86Xprv = masterKey.Derive(KeyPath.Parse("m/86'/0'/0'")).ToString(Network.Main);
                descriptors.Add(($"tr([{fingerprintHex}/86'/0'/0']{bip86Xprv}/0/*)", false));
                descriptors.Add(($"tr([{fingerprintHex}/86'/0'/0']{bip86Xprv}/1/*)", true));
            }

            return descriptors;
        }
        #endregion // Restore From Seed

        #region Transfer       
        public async Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj)
        {
            if (!string.IsNullOrEmpty(requestObj.TxData))
            {
                return await SendFundedPsbt(rpc, requestObj.TxData);
            }

            // Fallback: sendtoaddress when no pre-built tx is available
            TransferResponse responseObj = new();

            try
            {
                var (confTarget, estimateMode) = GetConfTarget(requestObj.Priority);

                var requestParams = new JObject
                {
                    ["address"] = requestObj.Destinations.FirstOrDefault()!.Address,
                    ["amount"] = requestObj.Destinations.FirstOrDefault()!.Amount,
                    ["comment"] = requestObj.Comment,
                    ["comment_to"] = requestObj.CommentTo,
                    ["subtractfeefromamount"] = requestObj.SubtractFeeFromAmount,
                    ["conf_target"] = confTarget,
                    ["estimate_mode"] = estimateMode
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "sendtoaddress",
                    ["params"] = requestParams
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
                Logger.LogException(CoinPrefix + ".WTRA", ex);
            }

            return responseObj;
        }

        private static (int confTarget, string estimateMode) GetConfTarget(string priority) => priority switch
        {
            SendPriority.Low => (36, "economical"),
            SendPriority.Normal => (6, "conservative"),
            SendPriority.Fast => (3, "conservative"),
            SendPriority.Urgent => (1, "conservative"),
            _ => (144, "economical") // Economy / default
        };

        public async Task<EstimateFeeResponse> EstimateFee(RpcBase rpc, TransferRequest requestObj)
        {
            EstimateFeeResponse responseObj = new();

            try
            {
                var destination = requestObj.Destinations.FirstOrDefault()!;
                var (confTarget, estimateMode) = GetConfTarget(requestObj.Priority);

                var outputsJson = new JArray { new JObject { [destination.Address] = destination.Amount } };
                var optionsJson = new JObject
                {
                    ["conf_target"] = confTarget,
                    ["estimate_mode"] = estimateMode
                };

                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "walletcreatefundedpsbt",
                    ["params"] = new JArray(new JArray(), outputsJson, 0, optionsJson)
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
                            responseObj.Fee = resultToken["fee"]?.Value<decimal>() ?? 0;
                            responseObj.TxData = resultToken["psbt"]?.ToString() ?? string.Empty;
                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".ESFE", ex);
            }

            return responseObj;
        }

        private async Task<TransferResponse> SendFundedPsbt(RpcBase rpc, string fundedPsbt)
        {
            TransferResponse responseObj = new();

            try
            {
                // Step 1: Sign
                var processJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "walletprocesspsbt",
                    ["params"] = new JArray(fundedPsbt)
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), processJson.ToString(), rpc.UserName, rpc.Password);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    return responseObj;
                }

                JObject jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
                var error = jsonObject["error"];
                if (error != null && error.Type != JTokenType.Null)
                {
                    responseObj.Error = GetServiceError(GetCallerName(), error);
                    return responseObj;
                }

                string signedPsbt = jsonObject.SelectToken("result.psbt")?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(signedPsbt))
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Failed to sign transaction";
                    return responseObj;
                }

                // Step 2: Finalize
                var finalizeJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "finalizepsbt",
                    ["params"] = new JArray(signedPsbt)
                };

                httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), finalizeJson.ToString(), rpc.UserName, rpc.Password);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    return responseObj;
                }

                jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
                error = jsonObject["error"];
                if (error != null && error.Type != JTokenType.Null)
                {
                    responseObj.Error = GetServiceError(GetCallerName(), error);
                    return responseObj;
                }

                string rawHex = jsonObject.SelectToken("result.hex")?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(rawHex))
                {
                    responseObj.Error.IsError = true;
                    responseObj.Error.Message = "Failed to finalize transaction";
                    return responseObj;
                }

                // Step 3: Broadcast
                var broadcastJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "sendrawtransaction",
                    ["params"] = new JArray(rawHex)
                };

                httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), broadcastJson.ToString(), rpc.UserName, rpc.Password);
                if (!httpResponse.IsSuccessStatusCode)
                {
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    return responseObj;
                }

                jsonObject = JObject.Parse(await httpResponse.Content.ReadAsStringAsync());
                error = jsonObject["error"];
                if (error != null && error.Type != JTokenType.Null)
                {
                    responseObj.Error = GetServiceError(GetCallerName(), error);
                }
                else
                {
                    responseObj.Error.IsError = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".SFPS", ex);
            }

            return responseObj;
        }
        #endregion // Transfer

        #region Rescan Blockchain
        public async Task<RescanBlockchainResponse> RescanBlockchain(RpcBase rpc, RescanBlockchainRequest requestObj)
        {
            RescanBlockchainResponse responseObj = new();

            try
            {
                bool proceedWithRescan = true;

                if (requestObj.Password.Length > 0)
                {
                    var unlockJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "walletpassphrase",
                        ["params"] = new JObject
                        {
                            ["passphrase"] = new string(requestObj.Password),
                            ["timeout"] = _blockchainScanTimeoutSeconds
                        }
                    };

                    HttpResponseMessage unlockResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), unlockJson.ToString(), rpc.UserName, rpc.Password);
                    if (unlockResponse.IsSuccessStatusCode)
                    {
                        JObject unlockResult = JObject.Parse(await unlockResponse.Content.ReadAsStringAsync());
                        var unlockError = unlockResult["error"];
                        if (unlockError != null && unlockError.Type != JTokenType.Null)
                        {
                            string errorCode = (unlockError as JObject)?["code"]?.ToString() ?? string.Empty;
                            if (errorCode != "-15")
                            {
                                // -15 means wallet is not encrypted — not an error, proceed without unlock
                                responseObj.Error = GetServiceError(GetCallerName(), unlockError);
                                proceedWithRescan = false;
                            }
                        }
                    }
                    else
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), unlockResponse);
                        proceedWithRescan = false;
                    }
                }

                if (proceedWithRescan)
                {
                    var requestJson = new JObject
                    {
                        ["jsonrpc"] = "2.0",
                        ["id"] = _id++,
                        ["method"] = "rescanblockchain",
                        ["params"] = new JArray()
                    };

                    // rescanblockchain is synchronous and can take a long time on large chains
                    HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password, _blockchainScanTimeout);
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
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".WRSB", ex);
            }
            finally
            {
                Array.Clear(requestObj.Password, 0, requestObj.Password.Length);
            }

            return responseObj;
        }
        #endregion // Rescan Blockchain

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

                                // Get pending (unconfirmed) balance: listunspent minconf=0,maxconf=0 returns only mempool UTXOs.
                                // getwalletinfo.unconfirmed_balance excludes trusted change from own transactions, so it is not useful here.
                                var pendingJson = new JObject
                                {
                                    ["jsonrpc"] = "2.0",
                                    ["id"] = _id++,
                                    ["method"] = "listunspent",
                                    ["params"] = new JObject { ["minconf"] = 0, ["maxconf"] = 0 }
                                };

                                HttpResponseMessage pendingResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), pendingJson.ToString(), rpc.UserName, rpc.Password);
                                if (pendingResponse.IsSuccessStatusCode)
                                {
                                    JObject pendingObject = JObject.Parse(await pendingResponse.Content.ReadAsStringAsync());
                                    var pendingError = pendingObject["error"];
                                    if (pendingError == null || pendingError.Type == JTokenType.Null)
                                    {
                                        var pendingResult = pendingObject.SelectToken("result") as JArray;
                                        if (pendingResult != null)
                                        {
                                            foreach (var utxo in pendingResult)
                                            {
                                                responseObj.BalancePending += utxo["amount"]?.Value<decimal>() ?? 0;
                                            }
                                        }
                                    }
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
                                    Amount = string.IsNullOrEmpty(entry.amount) ? 0 : Math.Abs(decimal.Parse(entry.amount, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign)),
                                    Type = GetTransactionType(entry.category),
                                    Confirmations = entry.confirmations
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

                            foreach (TransferDetails details in getTransfByTxIdResponse.Details)
                            {
                                decimal parsedAmount = string.IsNullOrEmpty(details.amount) ? 0 : decimal.Parse(details.amount, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
                                if (Math.Abs(parsedAmount) == requestObj.Amount)
                                {
                                    responseObj.Address = details.address;
                                    responseObj.Type = details.category;
                                    responseObj.Amount = Math.Abs(parsedAmount);
                                    responseObj.Fee = string.IsNullOrEmpty(details.fee) ? 0 : Math.Abs(decimal.Parse(details.fee, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign));
                                    break;
                                }
                            }

                            // Populate destinations from send-category detail entries (recipient addresses)
                            foreach (TransferDetails details in getTransfByTxIdResponse.Details)
                            {
                                if (details.category == "send" && !string.IsNullOrEmpty(details.address))
                                {
                                    decimal detailAmount = Math.Abs(string.IsNullOrEmpty(details.amount) ? 0 : decimal.Parse(details.amount, NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign));
                                    responseObj.Destinations.Add(details.address + " | " + detailAmount);
                                }
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
                                "# " + CoinName + " Wallet Descriptor Export\r\n" +
                                "# Wallet: " + GlobalData.OpenedWalletName + "\r\n" +
                                "# Exported: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" +
                                "# WARNING: Keep this file secure. Anyone with access can steal your funds.\r\n" +
                                "# To restore: use " + CoinName + " Core importdescriptors with the descriptors below.\r\n\r\n" +
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

        public async Task<LabelAccountResponse> LabelAccount(RpcBase rpc, LabelAccountRequest requestObj)
        {
            LabelAccountResponse responseObj = new();

            try
            {
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = _id++,
                    ["method"] = "setlabel",
                    ["params"] = new JObject
                    {
                        ["address"] = requestObj.Address,
                        ["label"] = requestObj.Label
                    }
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
                Logger.LogException(CoinPrefix + ".WLBA", ex);
            }

            return responseObj;
        }

        public Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
        {
            // TODO: importelectrumwallet
            // importwallet
            throw new NotImplementedException();
        }

        public Task<RescanSpentResponse> RescanSpent(RpcBase rpc, RescanSpentRequest requestObj)
        {
            // TODO: scantxoutset
            throw new NotImplementedException();
        }        
        #endregion // Unsupported Methods
    }
}
