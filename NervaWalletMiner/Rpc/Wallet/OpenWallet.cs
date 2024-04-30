using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class OpenWallet
    {
        public const string MethodName = "open_wallet";
        public const string DaemonUrl = "http://127.0.0.1:17566/json_rpc";

        public static async Task<string> CallAsync(string walletName, string walletPassword)
        {
            string resp = string.Empty;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DaemonUrl);

                    // TODO: Change this
                    request.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"" + MethodName + "\", \"params\":{\"filename\":\"" + walletName + "\", \"password\":\"" + walletPassword + "\"}}");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("RWGA.CSA", "Calling POST: " + DaemonUrl + " | " + MethodName);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("RWGA.CSA", "Call returned: " + DaemonUrl + " | " + MethodName);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // TODO: Change the way this is done. Do not want to depend on "result"
                        resp = jsonObject.ToString();
                    }
                    else
                    {
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
}
