using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;
using Newtonsoft.Json;
using static NervaWalletMiner.Rpc.Daemon.StartMining;

namespace NervaWalletMiner.Rpc.Daemon
{
    public static class StopMining
    {
        public const string DaemonUrl = "http://127.0.0.1:17566/stop_mining";

        // TODO: Make something reusable out of this
        public static async Task<MiningResponse> CallServiceAsync()
        {
            MiningResponse resp = new();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DaemonUrl);

                    // TODO: Change this
                    request.Content = new StringContent("");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    Logger.LogDebug("SpM.CS", "Calling POST: " + DaemonUrl);

                    response = await client.SendAsync(request);

                    Logger.LogDebug("SpM.CS", "Call returned: " + DaemonUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        resp = JsonConvert.DeserializeObject<MiningResponse>(jsonObject.ToString());
                    }
                    else
                    {
                        Logger.LogError("SpM.CS", "Response failed. Code: " + response.StatusCode + ", Phrase: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SpM.CS", ex);
            }

            return resp;
        }
    }
}
