using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class GetAccounts
    {
        public const string MethodName = "get_accounts";

        // TODO: Pass optional tag
        public static async Task<GetAccountsResponse> CallAsync(RpcSettings rpc)
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
                            resp = JsonConvert.DeserializeObject<GetAccountsResponse>(jsonObject.SelectToken("result").ToString());
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

        public ulong total_balance { get; set; }
        public ulong total_unlocked_balance { get; set; }
        public List<Account> subaddress_accounts { get; set; } = [];
    }

    public class Account
    {
        public int account_index { get; set; }
        public string base_address { get; set; } = string.Empty;
        public ulong balance { get; set; }
        public ulong unlocked_balance { get; set; }        
        public string label { get; set; } = string.Empty;
        public string tag { get; set; } = string.Empty;
    }
}
