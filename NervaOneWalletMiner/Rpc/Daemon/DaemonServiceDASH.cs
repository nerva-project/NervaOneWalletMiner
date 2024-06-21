using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Dash implementation as of 6/18/24: https://github.com/dashpay/dash

    internal class DaemonServiceDASH : IDaemonService
    {
        private const double _blockSeconds = 150.0;

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
                    if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                    {
                        Logger.LogInfo("DAS.DSPD", "Response Content is empty");
                    }
                    else
                    {
                        dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                        var error = JObject.Parse(jsonObject.ToString())["error"];
                        if (error != null)
                        {
                            // Set Service error
                            responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
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
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.DSPD", ex);
            }

            return responseObj;
        }
        #endregion // Stop Daemon        

        #region Get Info
        public async Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj)
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
                    if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                    {
                        Logger.LogInfo("DAS.DGTI", "Response Content is empty");
                    }
                    else
                    {
                        dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                        var error = JObject.Parse(jsonObject.ToString())["error"];
                        if (error != null)
                        {
                            // Set Service error
                            responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                            responseObj.Status = "ERROR";
                        }
                        else
                        {
                            // Set successful response
                            isNetInfoSuccess = true;

                            ResGetNetInfo getInfoResponse = JsonConvert.DeserializeObject<ResGetNetInfo>(jsonObject.SelectToken("result").ToString());
                            responseObj.ConnectionCountOut = getInfoResponse.outboundconnections;
                            responseObj.ConnectionCountIn = getInfoResponse.inboundmnconnections;
                            responseObj.Version = getInfoResponse.buildversion;
                            responseObj.Status = "OK";

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                    responseObj.Status = "ERROR";
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
                        if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                        {
                            Logger.LogInfo("DAS.DGTI", "Response Content is empty");
                        }
                        else
                        {
                            dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                            var error = JObject.Parse(jsonObject.ToString())["error"];
                            if (error != null)
                            {
                                // Set Service error
                                responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                            }
                            else
                            {
                                // Set successful response
                                ResGetMiningInfo getInfoResponse = JsonConvert.DeserializeObject<ResGetMiningInfo>(jsonObject.SelectToken("result").ToString());

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
                    else
                    {
                        // Set HTTP error
                        responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
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
                        if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                        {
                            Logger.LogInfo("DAS.DGTI", "Response Content is empty");
                        }
                        else
                        {
                            dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                            var error = JObject.Parse(jsonObject.ToString())["error"];
                            if (error != null)
                            {
                                // Set Service error
                                responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                            }
                            else
                            {
                                long uptimeSec = jsonObject.SelectToken("result");
                                responseObj.StartTime = DateTime.Now.ToUniversalTime().AddSeconds(-uptimeSec);
                            }
                        }
                    }
                    else
                    {
                        // Set HTTP error
                        responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
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
            public ulong outboundconnections { get; set; }
            public ulong inboundmnconnections { get; set; }
        }

        private class ResGetMiningInfo
        {
            public ulong headers { get; set; }
            public ulong blocks { get; set; }
            public ulong difficulty { get; set; }
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
                    if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                    {
                        Logger.LogInfo("DAS.DGTC", "Response Content is empty");
                    }
                    else
                    {
                        dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                        var error = JObject.Parse(jsonObject.ToString())["error"];
                        if (error != null)
                        {
                            // Set Service error
                            responseObj.Error = CommonDASH.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                        }
                        else
                        {
                            if (jsonObject.SelectToken("result") != null)
                            {
                                // Set successful response
                                List<ResGetConnections> getConnectionsResponse = JsonConvert.DeserializeObject<List<ResGetConnections>>(jsonObject.SelectToken("result").ToString());

                                foreach (ResGetConnections connection in getConnectionsResponse)
                                {
                                    responseObj.Connections.Add(new Connection
                                    {
                                        Address = connection.addr,
                                        Height = connection.startingheight,
                                        LiveTime = (DateTime.Now - DateTime.UnixEpoch.AddSeconds(connection.conntime).ToLocalTime()).ToString(@"hh\:mm\:ss"),
                                        State = connection.connection_type,
                                        IsIncoming = connection.inbound
                                    });
                                }
                            }
                            else
                            {
                                Logger.LogInfo("DAS.DGTC", "Result missing: " + GlobalMethods.RemoveLineBreaksAndSpaces(httpResponse.Content.ReadAsStringAsync().Result));
                            }

                            responseObj.Error.IsError = false;
                        }
                    }
                }
                else
                {
                    // Set HTTP error
                    responseObj.Error = HttpHelper.GetHttpError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, httpResponse);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.DGTC", ex);
            }

            return responseObj;
        }

        private class ResGetConnections
        {
            public string addr { get; set; } = string.Empty;
            public long startingheight { get; set; }
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