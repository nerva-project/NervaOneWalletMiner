using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Common;
using NervaWalletMiner.Rpc.Daemon.Requests;
using NervaWalletMiner.Rpc.Daemon.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Daemon
{
    public class DaemonServiceXNV : IDaemonService
    {
        #region StartMining        
        public async Task<StartMiningResponse> StartMining(RpcSettings rpc, StartMiningRequest requestObj)
        {
            StartMiningResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject
                {
                    ["miner_address"] = requestObj.MiningAddress,
                    ["threads_count"] = requestObj.ThreadCount
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "start_mining"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonXNV.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResStartMining startMiningResponse = JsonConvert.DeserializeObject<ResStartMining>(jsonObject.ToString());

                        responseObj.Status = startMiningResponse.status;
                        responseObj.Untrusted = startMiningResponse.untrusted;
                        responseObj.Error.IsError = false;
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
                Logger.LogException("RpcD.StM", ex);
            }

            return responseObj;
        }

        // Internal helper obejcts used to interact with service
        private class ResStartMining
        {
            public string status { get; set; } = string.Empty;
            public string untrusted { get; set; } = string.Empty;
        }
        #endregion // StartMining

        #region StopMining
        public async Task<StopMiningResponse> StopMining(RpcSettings rpc, StopMiningRequest requestObj)
        {
            StopMiningResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject();

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "stop_mining"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    dynamic jsonObject = JObject.Parse(httpResponse.Content.ReadAsStringAsync().Result);

                    var error = JObject.Parse(jsonObject.ToString())["error"];
                    if (error != null)
                    {
                        // Set Service error
                        responseObj.Error = CommonXNV.GetServiceError(System.Reflection.MethodBase.GetCurrentMethod()!.Name, error);
                    }
                    else
                    {
                        ResStartMining startMiningResponse = JsonConvert.DeserializeObject<ResStartMining>(jsonObject.ToString());

                        responseObj.Status = startMiningResponse.status;
                        responseObj.Untrusted = startMiningResponse.untrusted;
                        responseObj.Error.IsError = false;
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
                Logger.LogException("RpcD.SpM", ex);
            }

            return responseObj;
        }
        #endregion // StopMining
    }
}