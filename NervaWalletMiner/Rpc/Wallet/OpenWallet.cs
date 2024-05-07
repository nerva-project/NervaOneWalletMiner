using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Common;
using NervaWalletMiner.Rpc.Wallet.Responses;
using NervaWalletMiner.Rpc.Wallet.Requests;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class OpenWallet
    {
        public static async Task<OpenWalletResponse> CallAsync(RpcSettings rpc, OpenWalletRequest requestObj)
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
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error.IsError = true;
                        responseObj.Error.Code = error["code"].ToString();
                        responseObj.Error.Message = error["message"].ToString();
                        Logger.LogError("RWOW.CA", "Error from service. Code: " + responseObj.Error.Code + ", Message: " + responseObj.Error.Message);
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
                    responseObj.Error.IsError = true;
                    responseObj.Error.Code = httpResponse.StatusCode.ToString();
                    responseObj.Error.Message = httpResponse.ReasonPhrase;

                    Logger.LogError("RWOW.CA", "Response failed. Code: " + httpResponse.StatusCode + ", Phrase: " + httpResponse.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RWOW.CA", ex);
            }

            return responseObj;
        }
    }
}