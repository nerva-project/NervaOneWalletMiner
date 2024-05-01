using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class OpenWallet
    {
        public const string MethodName = "open_wallet";

        public static async Task<OpenWalletResponse> CallAsync(SettingsRpc rpc, string walletName, string walletPassword)
        {
            OpenWalletResponse resp = new();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string serviceUrl = rpc.HTProtocol + "://" + rpc.Host + ":" + rpc.Port + "/json_rpc";

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
                            resp.IsError = true;
                            resp.ErrorCode = error["code"].ToString();
                            resp.ErrorMessage = error["message"].ToString();
                            Logger.LogError("RWGA.CSA", "Error from service. Code: " + resp.ErrorCode + ", Message: " + resp.ErrorMessage);
                        }
                        else
                        {
                            resp.IsError = false;

                        }
                    }
                    else
                    {
                        resp.IsError = true;
                        resp.ErrorCode = response.StatusCode.ToString();
                        resp.ErrorMessage = response.ReasonPhrase;

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
        public bool IsError { get; set; } = true;
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
