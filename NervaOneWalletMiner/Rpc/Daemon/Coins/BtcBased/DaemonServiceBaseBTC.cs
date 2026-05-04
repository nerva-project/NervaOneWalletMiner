using NervaOneWalletMiner.Helpers;
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
        protected abstract double BlockSeconds { get; }

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

        public abstract Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj);

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
                                        Height = connection.startingheight,
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
