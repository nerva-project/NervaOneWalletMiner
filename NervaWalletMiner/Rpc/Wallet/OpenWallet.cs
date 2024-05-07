using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class OpenWallet
    {
        public const string MethodName = "open_wallet";

        public static async Task<OpenWalletResponse> CallAsync(RpcSettings rpc, string walletName, string walletPassword)
        {
            OpenWalletResponse resp = new();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string serviceUrl = GlobalMethods.GetServiceUrl(rpc);

                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);

                    // TODO: Change this
                    request.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"" + MethodName + "\",\"params\":{\"filename\":\"" + walletName + "\",\"password\":\"" + walletPassword + "\"}}");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("RWGA.CSA", "Calling POST: " + serviceUrl + " | " + MethodName);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("RWGA.CSA", "Call returned: " + serviceUrl + " | " + MethodName);

                    if (response.IsSuccessStatusCode)
                    {                       
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // Check for errors
                        var error = JObject.Parse(jsonObject.ToString())["error"];
                        if(error != null)
                        {
                            resp.Error.IsError = true;
                            resp.Error.Code = error["code"].ToString();
                            resp.Error.Message = error["message"].ToString();
                            Logger.LogError("RWGA.CSA", "Error from service. Code: " + resp.Error.Code + ", Message: " + resp.Error.Message);
                        }
                        else
                        {
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
    }

    public class OpenWalletResponse
    {
        public RpcError Error { get; set; } = new();
    }
}
