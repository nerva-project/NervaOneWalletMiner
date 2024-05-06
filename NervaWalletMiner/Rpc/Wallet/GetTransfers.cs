using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class GetTransfers
    {
        // TODO: Make Wallet methods interfaces. Implement here for given coin.
        // Change Request and Response objects to generic, UI specific objects and build them here
        public static async Task<GetTransfersResponse> CallAsync(SettingsRpc rpc, GetTransfersRequest transfersRequest)
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

                    var requestParams = new JObject();
                    requestParams["in"] = transfersRequest.In;
                    requestParams["out"] = transfersRequest.Out;
                    requestParams["pending"] = transfersRequest.pending;
                    requestParams["failed"] = transfersRequest.failed;
                    requestParams["pool"] = transfersRequest.pool;
                    requestParams["filter_by_height"] = transfersRequest.filter_by_height;
                    requestParams["min_height"] = transfersRequest.min_height;
                    requestParams["account_index"] = transfersRequest.account_index;
                    requestParams["subaddr_indices"] = new JArray(transfersRequest.subaddr_indices);
                    requestParams["all_accounts"] = transfersRequest.all_accounts;

                    var requestJson = new JObject();
                    requestJson["jsonrpc"] = "2.0";
                    requestJson["id"] = "0";
                    requestJson["method"] = "get_transfers";
                    requestJson["params"] = requestParams;                   


                    request.Content = new StringContent(requestJson.ToString());
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

    public class GetTransfersRequest
    {
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
        public List<uint> subaddr_indices { get; set; } = [];
        public bool all_accounts { get; set; }
    }

    public class GetTransfersResponse
    {
        public RpcError Error { get; set; } = new();


        public List<TransferEntry> In { get; set; } = [];
        public List<TransferEntry> Out { get; set; } = [];
        public List<TransferEntry> pending { get; set; } = [];
        public List<TransferEntry> failed { get; set; } = [];
        public List<TransferEntry> pool { get; set; } = [];
    }

    public class TransferEntry
    {
        public string txid { get; set; }
        public string payment_id { get; set; }
        public ulong height { get; set; }
        public ulong timestamp { get; set; }
        public ulong amount { get; set; }
        public ulong fee { get; set; }
        public string note { get; set; }
        public List<TransferDestination> destinations { get; set; }
        public string type { get; set; }
        public ulong unlock_time { get; set; }
        public bool locked { get; set; }
        public SubaddressIndex subaddr_index { get; set; }
        public List<SubaddressIndex> subaddr_indices { get; set; }
        public string address { get; set; }
        public bool double_spend_seen { get; set; }
        public ulong confirmations { get; set; }
        public ulong suggested_confirmations_threshold { get; set; }
    }

    public class TransferDestination
    {
        public ulong amount { get; set; }

        public string address { get; set; }
    }

    public class SubaddressIndex
    {
        public uint major { get; set; }

        public uint minor { get; set; }
    }
}