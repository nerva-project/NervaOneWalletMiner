using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class GetAccounts
    {
        public const string MethodName = "get_accounts";

        // TODO: This is just copied from another class
        public static async Task<GetAccountsResponse> CallAsync(SettingsRpc rpc)
        {
            GetAccountsResponse resp = new();

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string serviceUrl = GlobalMethods.GetServiceUrl(rpc);

                    client.Timeout = TimeSpan.FromSeconds(30);
                    HttpResponseMessage response;

                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);

                    // TODO: Change this
                    request.Content = new StringContent("{\"jsonrpc\":\"2.0\",\"id\":\"0\",\"method\":\"" + MethodName + "\"}");
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //Logger.LogDebug("RWGA.CSA", "Calling POST: " + DaemonUrl + " | " + MethodName);

                    response = await client.SendAsync(request);

                    //Logger.LogDebug("RWGA.CSA", "Call returned: " + DaemonUrl + " | " + MethodName);

                    if (response.IsSuccessStatusCode)
                    {
                        var dataObjects = response.Content.ReadAsStringAsync();
                        dynamic jsonObject = JObject.Parse(dataObjects.Result);

                        // Check for errors
                        var error = JObject.Parse(jsonObject.ToString())["error"];
                        if (error != null)
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


    public class GetAccountsResponse
    {
        public RpcError Error { get; set; } = new();

        // TODO: Replace this with actual fields
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
