using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using NervaWalletMiner.Helpers;
using System.Collections.Generic;

namespace NervaWalletMiner.Rpc.Daemon
{
    public static class GetConnections
    {
        public const string MethodName = "get_connections";
        public const string DaemonUrl = "http://localhost:17566/json_rpc";

        // TODO: Make something reusable out of this
        public static async Task<List<GetConnectionsResponse>> CallServiceAsync()
        {
            List<GetConnectionsResponse>? resp = new();

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

                    Logger.LogDebug("RDGC.CSA", "Calling POST: " + DaemonUrl + " | " + MethodName);

                    response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // TODO: Change the way this is done. Do not want to depend ton "result"
                        resp = JsonConvert.DeserializeObject<List<GetConnectionsResponse>>(jsonObject.SelectToken("result.connections").ToString());
                    }
                    else
                    {
                        Logger.LogError("RDGC.CSA", "Response failed. Code: " + response.StatusCode + ", Phrase: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RDGC.CSA", ex);
            }

            return resp;
        }
    }

    public class GetConnectionsResponse
    {
        public bool incoming { get; set; }
        public bool localhost { get; set; }
        public bool local_ip { get; set; }
        public bool ssl { get; set; }
        public string? address { get; set; }
        public string? host { get; set; }
        public string? ip { get; set; }
        public string? port { get; set; }
        public uint rpc_port { get; set; }
        public string? peer_id { get; set; }
        public ulong recv_count { get; set; }
        public ulong recv_idle_time { get; set; }
        public ulong send_count { get; set; }
        public ulong send_idle_time { get; set; }
        public string? state { get; set; }
        public ulong live_time { get; set; }
        public ulong avg_download { get; set; }
        public ulong current_download { get; set; }
        public ulong avg_upload { get; set; }
        public ulong current_upload { get; set; }
        public uint support_flags { get; set; }
        public string? connection_id { get; set; }
        public ulong height { get; set; }
        public uint pruning_seed { get; set; }
        public uint address_type { get; set; }
    }
}