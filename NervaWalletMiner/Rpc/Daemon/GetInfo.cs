using NervaWalletMiner.Helpers;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NervaWalletMiner.Rpc.Daemon
{
    public static class GetInfo
    {
        public const string MethodName = "get_info";
        public const string DaemonUrl = "http://127.0.0.1:17566/json_rpc";

        // TODO: Make something reusable out of this
        public static async Task<GetInfoResponse> CallServiceAsync()
        {
            GetInfoResponse? resp = new();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DaemonUrl);

                    // TODO: Change this
                    request.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"" + MethodName + "\"}");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    
                    //Logger.LogDebug("RDGI.CS", "Calling POST: " + DaemonUrl + " | " + MethodName);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("RDGI.CS", "Call returned: " + DaemonUrl + " | " + MethodName);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // TODO: Change the way this is done. Do not want to depend ton "result"
                        resp = JsonConvert.DeserializeObject<GetInfoResponse>(jsonObject.SelectToken("result").ToString());
                        //resp = JsonConvert.DeserializeObject<GetInfoResponse>(response.Content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        Logger.LogError("RDGI.CS", "Response failed. Code: " + response.StatusCode + ", Phrase: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RDGI.CS", ex);
            }

            return resp;
        }
    }

    public class GetInfoResponse
    {
        public ulong height { get; set; }
        public ulong target_height { get; set; }
        public ulong difficulty { get; set; }
        public ulong target { get; set; }
        public ulong tx_count { get; set; }
        public ulong tx_pool_size { get; set; }
        public ulong alt_blocks_count { get; set; }
        public ulong outgoing_connections_count { get; set; }
        public ulong incoming_connections_count { get; set; }
        public ulong rpc_connections_count { get; set; }
        public ulong white_peerlist_size { get; set; }
        public ulong grey_peerlist_size { get; set; }
        public bool mainnet { get; set; }
        public bool testnet { get; set; }
        public bool stagenet { get; set; }
        public string? nettype { get; set; }
        public string? top_block_hash { get; set; }
        public ulong cumulative_difficulty { get; set; }
        public ulong cumulative_difficulty_top64 { get; set; }
        public ulong block_size_limit { get; set; }
        public ulong block_weight_limit { get; set; }
        public ulong block_size_median { get; set; }
        public ulong block_weight_median { get; set; }
        public ulong start_time { get; set; }
        public ulong free_space { get; set; }
        public bool offline { get; set; }
        public string? bootstrap_daemon_address { get; set; }
        public ulong height_without_bootstrap { get; set; }
        public bool was_bootstrap_ever_used { get; set; }
        public ulong database_size { get; set; }
        public bool update_available { get; set; }
        public string? version { get; set; }
        public string? status { get; set; }
    }
}