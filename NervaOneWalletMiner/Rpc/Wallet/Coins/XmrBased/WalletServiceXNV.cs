using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Nerva implementation as of 5/10/24: https://github.com/nerva-project/nerva
    public class WalletServiceXNV : WalletServiceBaseXMR
    {
        protected override string CoinPrefix => "XNV";

        #region Restore from Seed
        /* RPC request params:
         *  uint64_t restore_height;                OPT
         *  std::string filename;
         *  std::string seed;
         *  std::string seed_offset;
         *  std::string password;
         *  std::string language;
         *  bool autosave_current;                  OPT
         */
        public override async Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj)
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
                    ["method"] = "restore_wallet_from_seed",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(nameof(RestoreFromSeed), error);
                    }
                    else
                    {
                        ResRestoreFromSeed createWalletResponse = JsonConvert.DeserializeObject<ResRestoreFromSeed>(jsonObject.SelectToken("result")!.ToString())!;
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
                    responseObj.Error = HttpHelper.GetHttpError(nameof(RestoreFromSeed), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("XNV.WRFS", ex);
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
         *  std::string language;
         *  bool autosave_current;                  OPT
         */
        public override async Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj)
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
                    ["method"] = "restore_wallet_from_keys",
                    ["params"] = requestParams
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(nameof(RestoreFromKeys), error);
                    }
                    else
                    {
                        ResRestoreFromKeys createWalletResponse = JsonConvert.DeserializeObject<ResRestoreFromKeys>(jsonObject.SelectToken("result")!.ToString())!;
                        responseObj.Address = createWalletResponse.address;
                        responseObj.Info = createWalletResponse.info;

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(nameof(RestoreFromKeys), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("XNV.WRFK", ex);
            }

            return responseObj;
        }

        private class ResRestoreFromKeys
        {
            public string address { get; set; } = string.Empty;
            public string info { get; set; } = string.Empty;
        }
        #endregion // Restore from Keys

        #region Query Key
        /* RPC request params:
         *  std::string key_type;
         */
        public override async Task<GetPrivateKeysResponse> GetPrivateKeys(RpcBase rpc, GetPrivateKeysRequest requestObj)
        {
            // XNV uses a single all_keys call that returns mnemonic, view_key and spend_key in one response

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
                        ["params"] = new JObject() { ["key_type"] = "all_keys" }
                    };
                }

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    JObject jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = jsonObject["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = GetServiceError(nameof(GetPrivateKeys), error);
                    }
                    else
                    {
                        ResQueryKey queryKeyResponse = JsonConvert.DeserializeObject<ResQueryKey>(jsonObject.SelectToken("result")!.ToString())!;
                        responseObj.PublicViewKey = queryKeyResponse.public_view_key;
                        responseObj.PrivateViewKey = queryKeyResponse.private_view_key.ToCharArray();
                        responseObj.PublicSpendKey = queryKeyResponse.public_spend_key;
                        responseObj.PrivateSpendKey = queryKeyResponse.private_spend_key.ToCharArray();
                        responseObj.Mnemonic = queryKeyResponse.mnemonic.ToCharArray();

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(nameof(GetPrivateKeys), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("XNV.WGPK", ex);
            }

            return responseObj;
        }

        private class ResQueryKey
        {
            public string public_view_key { get; set; } = string.Empty;
            public string private_view_key { get; set; } = string.Empty;
            public string public_spend_key { get; set; } = string.Empty;
            public string private_spend_key { get; set; } = string.Empty;
            public string mnemonic { get; set; } = string.Empty;
        }
        #endregion // Query Key
    }
}
