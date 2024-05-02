using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class GetTransfers
    {
        public const string MethodName = "get_transfers";

        // TODO: This is just copied from another class
        public static async Task<GetTransfersResponse> CallAsync(SettingsRpc rpc)
        {
            GetTransfersResponse resp = new();

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
                            resp = JsonConvert.DeserializeObject<GetTransfersResponse>(jsonObject.SelectToken("result").ToString());
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

    public class GetTransfersResponse
    {
        public RpcError Error { get; set; } = new();

        // TODO: Not sure if this will work as those are reserved keywords. Might need to use: [JsonProperty("in")]
        public bool In { get; set; }
        public bool Out { get; set; }

        public bool pending { get; set; }
        public bool failed { get; set; }
        public bool pool { get; set; }
        public bool filter_by_height { get; set; }
        public ulong min_height { get; set; }
        public ulong max_height { get; set; }
        public uint account_index { get; set; }
        public List<uint>? subaddr_indices { get; set; }
        public bool all_accounts { get; set; }
    }
}
