using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;
using Newtonsoft.Json;

namespace NervaWalletMiner.Rpc.Daemon
{
    public static class MiningStatus
    {
        public const string DaemonUrl = "http://127.0.0.1:17566/mining_status";

        // TODO: Make something reusable out of this
        public static async Task<MiningStatusResponse> CallServiceAsync()
        {
            MiningStatusResponse resp = new();
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

                    Logger.LogDebug("MS.CS", "Calling POST: " + DaemonUrl);

                    response = await client.SendAsync(request);

                    Logger.LogDebug("MS.CS", "Call returned: " + DaemonUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // TODO: Change the way this is done. Do not want to depend ton "result"
                        resp = JsonConvert.DeserializeObject<MiningStatusResponse>(jsonObject.ToString());
                    }
                    else
                    {
                        Logger.LogError("MS.CS", "Response failed. Code: " + response.StatusCode + ", Phrase: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("MS.CS", ex);
            }

            return resp;
        }

        public class MiningStatusResponse
        {
            public bool active { get; set; }
            public long speed { get; set; }
            public int threads_count { get; set; }
            public string? address { get; set; }
            public bool is_background_mining_enabled { get; set; }
            public int bg_idle_threshold { get; set; }
            public int bg_min_idle_seconds { get; set; }
            public bool bg_ignore_battery { get; set; }
            public int bg_target { get; set; }
            public int block_target { get; set; }
            public long block_reward { get; set; }
            public long difficulty { get; set; }
        }
    }
}