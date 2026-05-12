using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Daemon
{
    public abstract class DaemonServiceBaseBTC : IDaemonService
    {
        protected abstract string CoinPrefix { get; }

        /*
         * difficulty = hashrate / (2^256 / max_target / block_time)
         *            = hashrate / (2^256 / (0xFFFF * 2^208) / block_time)
         *            = hashrate / (2^48 / 65535 / block_time)
         * MaxTargetConstant = 2^48 / 65535 ≈ 4295032832.7
         * Applies to all Bitcoin-derived chains sharing the same difficulty_1 target (nBits = 0x1d00ffff).
         */
        private const double MaxTargetConstant = 4295032832.7;

        protected static string GetCallerName([CallerMemberName] string name = "") => name;

        protected ServiceError GetServiceError(string source, JToken error)
        {
            ServiceError serviceError = new();

            try
            {
                serviceError.IsError = true;
                if (error is JObject errorObj)
                {
                    serviceError.Code = errorObj["code"]?.ToString() ?? string.Empty;
                    serviceError.Message = errorObj["message"]?.ToString() ?? string.Empty;
                }
                else
                {
                    serviceError.Message = error.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".CGSE", ex);
            }

            return serviceError;
        }

        #region Stop Daemon
        public async Task<StopDaemonResponse> StopDaemon(RpcBase rpc, StopDaemonRequest requestObj)
        {
            StopDaemonResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["method"] = "stop",
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
                        Logger.LogInfo(CoinPrefix + ".DSPD", "Response Content is empty");
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
                            // Set successful response
                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".DSPD", ex);
            }

            return responseObj;
        }
        #endregion // Stop Daemon

        #region Get Info
        public virtual async Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj)
        {
            GetInfoResponse responseObj = new();

            try
            {
                bool isNetInfoSuccess = false;

                var requestJson = new JObject
                {
                    ["method"] = "getnetworkinfo",
                    ["id"] = "0",
                    ["params"] = new JObject()
                };

                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                if (httpResponse.IsSuccessStatusCode)
                {
                    string responseContent = await httpResponse.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(responseContent))
                    {
                        Logger.LogInfo(CoinPrefix + ".DGTI", "Response Content is empty");
                    }
                    else
                    {
                        JObject jsonObject = JObject.Parse(responseContent);

                        var error = jsonObject["error"];
                        if (error != null && error.Type != JTokenType.Null)
                        {
                            responseObj.Error = GetServiceError(GetCallerName(), error);
                            responseObj.Status = StatusDaemon.Error;
                        }
                        else
                        {
                            isNetInfoSuccess = true;

                            var resultToken = jsonObject.SelectToken("result");
                            if (resultToken == null)
                            {
                                responseObj.Error.IsError = true;
                                responseObj.Error.Message = "Response missing 'result' field";
                            }
                            else
                            {
                                ResGetNetInfo netInfo = JsonConvert.DeserializeObject<ResGetNetInfo>(resultToken.ToString())!;
                                responseObj.ConnectionCountOut = netInfo.connections_out;
                                responseObj.ConnectionCountIn = netInfo.connections_in;
                                responseObj.Version = netInfo.subversion.Trim('/');
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
                        Logger.LogDebug(CoinPrefix + ".DGTI", "Daemon warming up: " + errorJson?.SelectToken("error.message")?.ToString());
                        responseObj.Status = StatusDaemon.WarmingUp;
                    }
                    else
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                        responseObj.Status = StatusDaemon.Error;
                    }
                }

                if (isNetInfoSuccess)
                {
                    requestJson = new JObject
                    {
                        ["method"] = "getblockchaininfo",
                        ["id"] = "0",
                        ["params"] = new JObject()
                    };

                    httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(responseContent))
                        {
                            Logger.LogInfo(CoinPrefix + ".DGTI", "Response Content is empty");
                        }
                        else
                        {
                            JObject jsonObject = JObject.Parse(responseContent);

                            var error = jsonObject["error"];
                            if (error != null && error.Type != JTokenType.Null)
                            {
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
                                    ResGetBlockchainInfo chainInfo = JsonConvert.DeserializeObject<ResGetBlockchainInfo>(resultToken.ToString())!;
                                    responseObj.Height = chainInfo.blocks;
                                    responseObj.TargetHeight = chainInfo.headers;
                                    responseObj.NetworkHashRate = chainInfo.difficulty * (MaxTargetConstant / GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].BlockSeconds);
                                }
                            }
                        }
                    }
                    else
                    {
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    }
                }

                if (isNetInfoSuccess)
                {
                    requestJson = new JObject
                    {
                        ["method"] = "uptime",
                        ["id"] = "0",
                        ["params"] = new JObject()
                    };

                    httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, string.Empty), requestJson.ToString(), rpc.UserName, rpc.Password);
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();
                        if (string.IsNullOrEmpty(responseContent))
                        {
                            Logger.LogInfo(CoinPrefix + ".DGTI", "Response Content is empty");
                        }
                        else
                        {
                            JObject jsonObject = JObject.Parse(responseContent);

                            var error = jsonObject["error"];
                            if (error != null && error.Type != JTokenType.Null)
                            {
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
                        responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".DGTI", ex);
            }

            return responseObj;
        }

        private class ResGetNetInfo
        {
            public string subversion { get; set; } = string.Empty;
            public ulong connections_out { get; set; }
            public ulong connections_in { get; set; }
        }

        private class ResGetBlockchainInfo
        {
            public ulong headers { get; set; }
            public ulong blocks { get; set; }
            public double difficulty { get; set; }
        }
        #endregion // Get Info

        #region Get Connections
        public async Task<GetConnectionsResponse> GetConnections(RpcBase rpc, GetConnectionsRequest requestObj)
        {
            GetConnectionsResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["method"] = "getpeerinfo",
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
                        Logger.LogInfo(CoinPrefix + ".DGTC", "Response Content is empty");
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
                            if (resultToken != null)
                            {
                                // Set successful response
                                List<ResGetConnections> getConnectionsResponse = JsonConvert.DeserializeObject<List<ResGetConnections>>(resultToken.ToString())!;

                                foreach (ResGetConnections connection in getConnectionsResponse)
                                {
                                    responseObj.Connections.Add(new Connection
                                    {
                                        Address = connection.addr,
                                        Height = connection.synced_headers,
                                        LiveTime = (DateTime.Now - DateTime.UnixEpoch.AddSeconds(connection.conntime).ToLocalTime()).ToString(@"%d\.hh\:mm"),
                                        State = connection.connection_type,
                                        IsIncoming = connection.inbound
                                    });
                                }
                            }
                            else
                            {
                                Logger.LogInfo(CoinPrefix + ".DGTC", "Result missing: " + GlobalMethods.RemoveLineBreaksAndSpaces(responseContent));
                            }

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = await HttpHelper.GetHttpError(GetCallerName(), httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(CoinPrefix + ".DGTC", ex);
            }

            return responseObj;
        }

        private class ResGetConnections
        {
            public string addr { get; set; } = string.Empty;
            public long synced_blocks { get; set; }
            public long synced_headers { get; set; }
            public ulong conntime { get; set; }
            public string connection_type { get; set; } = string.Empty;
            public bool inbound { get; set; }
        }
        #endregion // Get Connections

        #region Unsupported Methods
        public Task<MiningStatusResponse> GetMiningStatus(RpcBase rpc, MiningStatusRequest requestObj)
        {
            throw new NotImplementedException();
        }

        public Task<StartMiningResponse> StartMining(RpcBase rpc, StartMiningRequest requestObj)
        {
            throw new NotImplementedException();
        }

        public Task<StopMiningResponse> StopMining(RpcBase rpc, StopMiningRequest requestObj)
        {
            throw new NotImplementedException();
        }
        #endregion // Unsupported Methods
    }
}
