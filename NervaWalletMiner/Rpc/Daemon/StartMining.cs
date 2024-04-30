using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;

namespace NervaWalletMiner.Rpc.Daemon
{
    public static class StartMining
    {
        public const string DaemonUrl = "http://127.0.0.1:17566/start_mining";

        // TODO: Make something reusable out of this
        public static async Task<MiningResponse> CallServiceAsync(string address, int threads)
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
                    request.Content = new StringContent("{\"miner_address\":\"" + address + "\",\"threads_count\":\"" + threads + "\"}");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("StM.CS", "Calling POST: " + DaemonUrl + " | " + threads + " | " + address);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("StM.CS", "Call returned: " + DaemonUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        resp = JsonConvert.DeserializeObject<MiningResponse>(jsonObject.ToString());
                    }
                    else
                    {
                        Logger.LogError("StM.CS", "Response failed. Code: " + response.StatusCode + ", Phrase: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("StM.CS", ex);
            }

            return resp;
        }

        public class MiningResponse
        {
            public string? status { get; set; }
            public string? untrusted { get; set; }
        }
    }
}
