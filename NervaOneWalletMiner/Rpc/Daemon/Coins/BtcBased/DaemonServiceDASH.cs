using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Dash implementation as of 6/18/24: https://github.com/dashpay/dash

    internal class DaemonServiceDASH : DaemonServiceBaseBTC
    {
        protected override string CoinPrefix => "DAS";
        protected override double BlockSeconds => 150.0;

        #region Get Info
        public override async Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj)
        {
            GetInfoResponse responseObj = new();

            try
            {
                bool isNetInfoSuccess = false;

                // Build request content json
                var requestJson = new JObject
                {
                    ["method"] = "getnetworkinfo",
                    ["id"] = "0",
                    ["params"] = new JObject()
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(responseContent))
                    {
                        Logger.LogInfo("DAS.DGTI", "Response Content is empty");
                    }
                    else
                    {
                        JObject jsonObject = JObject.Parse(responseContent);

                        var error = jsonObject["error"];
                        if (error != null && error.Type != JTokenType.Null)
                        {
                            // Set Service error
                            responseObj.Error = GetServiceError(GetCallerName(), error);
                            responseObj.Status = StatusDaemon.Error;
                        }
                        else
                        {
                            // Set successful response
                            isNetInfoSuccess = true;

                            var resultToken = jsonObject.SelectToken("result");
                            if (resultToken == null)
                            {
                                responseObj.Error.IsError = true;
                                responseObj.Error.Message = "Response missing 'result' field";
                            }
                            else
                            {
                                ResGetNetInfo getInfoResponse = JsonConvert.DeserializeObject<ResGetNetInfo>(resultToken.ToString())!;
                                responseObj.ConnectionCountOut = getInfoResponse.connections_out;
                                responseObj.ConnectionCountIn = getInfoResponse.connections_in;
                                responseObj.Version = getInfoResponse.buildversion;
                                responseObj.Status = StatusDaemon.Ok;

                                responseObj.Error.IsError = false;
                            }
                        }
                    }
                }
                else
                {
                    string errorContent = await httpResponse.Content.ReadAsStringAsync();
                    JObject? errorJson = null;
                    try { errorJson = JObject.Parse(errorContent); } catch { }

                    string? errorCode = errorJson?.SelectToken("error.code")?.ToString();
                    if (errorCode == "-28")
                    {
                        Logger.LogDebug("DAS.DGTI", "Daemon warming up: " + errorJson?.SelectToken("error.message")?.ToString());
                        responseObj.Status = StatusDaemon.WarmingUp;
                    }
                    else
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                        responseObj.Status = StatusDaemon.Error;
                    }
                }

                if(isNetInfoSuccess)
                {
                    // Build request content json
                    requestJson = new JObject
                    {
                        ["method"] = "getblockchaininfo",
                        ["id"] = "0",
                        ["params"] = new JObject()
                    };

                    // Call service and process response
                    httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(responseContent))
                        {
                            Logger.LogInfo("DAS.DGTI", "Response Content is empty");
                        }
                        else
                        {
                            JObject jsonObject = JObject.Parse(responseContent);

                            var error = jsonObject["error"];
                            if (error != null && error.Type != JTokenType.Null)
                            {
                                // Set Service error
                                responseObj.Error = GetServiceError(GetCallerName(), error);
                            }
                            else
                            {
                                var resultToken = jsonObject.SelectToken("result");
                                if (resultToken == null)
                                {
                                    responseObj.Error.IsError = true;
                                    responseObj.Error.Message = "Response missing 'result' field";
                                }
                                else
                                {
                                    // Set successful response
                                    ResGetMiningInfo getInfoResponse = JsonConvert.DeserializeObject<ResGetMiningInfo>(resultToken.ToString())!;
                                    responseObj.Height = getInfoResponse.blocks;
                                    responseObj.TargetHeight = getInfoResponse.headers;
                                    responseObj.NetworkHashRate = getInfoResponse.difficulty * 28633552;

                                    /*
                                     * difficulty = hashrate / (2^256 / max_target / intended_time_per_block)
                                     * = hashrate / (2^256 / (2^208*65535) / 150)
                                     * = hashrate / (2^48 / 65535 / 150)
                                     * = hashrate / 28633552.22
                                     */
                                }
                            }
                        }
                    }
                    else
                    {
                        // Set HTTP error
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    }
                }

                if (isNetInfoSuccess)
                {
                    // Build request content json
                    requestJson = new JObject
                    {
                        ["method"] = "uptime",
                        ["id"] = "0",
                        ["params"] = new JObject()
                    };

                    // Call service and process response
                    httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(responseContent))
                        {
                            Logger.LogInfo("DAS.DGTI", "Response Content is empty");
                        }
                        else
                        {
                            JObject jsonObject = JObject.Parse(responseContent);

                            var error = jsonObject["error"];
                            if (error != null && error.Type != JTokenType.Null)
                            {
                                // Set Service error
                                responseObj.Error = GetServiceError(GetCallerName(), error);
                            }
                            else
                            {
                                long uptimeSec = jsonObject.SelectToken("result")?.Value<long>() ?? 0;
                                responseObj.StartTime = DateTime.Now.ToUniversalTime().AddSeconds(-uptimeSec);
                            }
                        }
                    }
                    else
                    {
                        // Set HTTP error
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.DGTI", ex);
            }

            return responseObj;
        }

        private class ResGetNetInfo
        {
            public string buildversion { get; set; } = string.Empty;
            public ulong connections_in { get; set; }
            public ulong connections_out { get; set; }
        }

        private class ResGetMiningInfo
        {
            public ulong headers { get; set; }
            public ulong blocks { get; set; }
            public ulong difficulty { get; set; }
        }
        #endregion // Get Info
    }
}
