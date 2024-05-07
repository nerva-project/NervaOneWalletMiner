using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.DataGrid;
using NervaWalletMiner.Rpc.Common;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public static class GetAccounts
    {
        // TODO: Pass optional tag
        public static async Task<GetAccountsResponse> CallAsync(RpcSettings rpc, GetAccountsRequest requestObj)
        {
            GetAccountsResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_accounts"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error.IsError = true;
                        responseObj.Error.Code = error["code"].ToString();
                        responseObj.Error.Message = error["message"].ToString();
                        Logger.LogError("RWGA.CA", "Error from service. Code: " + responseObj.Error.Code + ", Message: " + responseObj.Error.Message);
                    }
                    else
                    {
                        RpcResponse getAccountsResponse = JsonConvert.DeserializeObject<RpcResponse>(jsonObject.SelectToken("result").ToString());
                        responseObj.BalanceUnlocked = GlobalMethods.XnvFromAtomicUnits(getAccountsResponse.total_unlocked_balance, 4);
                        responseObj.BalanceLocked = GlobalMethods.XnvFromAtomicUnits(getAccountsResponse.total_balance, 4);

                        foreach (WalletAccount account in getAccountsResponse.subaddress_accounts)
                        {
                            Account newAccount = new()
                            {
                                Index = account.account_index,
                                Label = account.label,
                                Address = GlobalMethods.GetShorterString(account.base_address, 12),
                                BalanceLocked = GlobalMethods.XnvFromAtomicUnits(account.balance, 1),
                                BalanceUnlocked = GlobalMethods.XnvFromAtomicUnits(account.unlocked_balance, 1)
                            };

                            responseObj.SubAccounts.Add(newAccount);
                        }

                        responseObj.Error.IsError = false;
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error.IsError = true;
                    responseObj.Error.Code = httpResponse.StatusCode.ToString();
                    responseObj.Error.Message = httpResponse.ReasonPhrase;

                    Logger.LogError("RWGA.CA", "Response failed. Code: " + httpResponse.StatusCode + ", Phrase: " + httpResponse.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RWGA.CA", ex);
            }

            return responseObj;
        }

        // Internal helper obejcts used to interact with service
        private class RpcResponse
        {
            public ulong total_balance { get; set; }
            public ulong total_unlocked_balance { get; set; }
            public List<WalletAccount> subaddress_accounts { get; set; } = [];
        }

        private class WalletAccount
        {
            public int account_index { get; set; }
            public string base_address { get; set; } = string.Empty;
            public ulong balance { get; set; }
            public ulong unlocked_balance { get; set; }
            public string label { get; set; } = string.Empty;
            public string tag { get; set; } = string.Empty;
        }
    }
}