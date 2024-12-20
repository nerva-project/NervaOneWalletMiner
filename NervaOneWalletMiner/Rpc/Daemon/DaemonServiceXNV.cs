﻿using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Nerva implementation as of 5/10/24: https://github.com/nerva-project/nerva
    public class DaemonServiceXNV : IDaemonService
    {
        private const double _blockSeconds = 60.0;

        #region Start Mining
        /* RPC request params:
         *  std::string miner_address;
         *  uint64_t    threads_count;
         *  bool        do_background_mining;
         *  bool        ignore_battery;
         */
        public async Task<StartMiningResponse> StartMining(RpcBase rpc, StartMiningRequest requestObj)
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
                        // Set successful response
                        ResGeneric startMiningResponse = JsonConvert.DeserializeObject<ResGeneric>(jsonObject.ToString());

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
                Logger.LogException("XNV.DSTM", ex);
            }

            return responseObj;
        }
        #endregion // Start Mining

        #region Stop Mining
        public async Task<StopMiningResponse> StopMining(RpcBase rpc, StopMiningRequest requestObj)
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
                        // Set successful response
                        ResGeneric startMiningResponse = JsonConvert.DeserializeObject<ResGeneric>(jsonObject.ToString());

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
                Logger.LogException("XNV.DSPM", ex);
            }

            return responseObj;
        }
        #endregion // Stop Mining

        #region Stop Daemon
        public async Task<StopDaemonResponse> StopDaemon(RpcBase rpc, StopDaemonRequest requestObj)
        {
            StopDaemonResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject();

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "stop_daemon"), requestJson.ToString());
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
                        // Set successful response
                        ResGeneric startMiningResponse = JsonConvert.DeserializeObject<ResGeneric>(jsonObject.ToString());

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
                Logger.LogException("XNV.DSPD", ex);
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
                // Build request content json
                var requestJson = new JObject
                {
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_info"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    if(string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                    {
                        Logger.LogInfo("XNV.DGTI", "Response Content is empty");
                    }
                    else
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
                            // Set successful response
                            ResGetInfo getInfoResponse = JsonConvert.DeserializeObject<ResGetInfo>(jsonObject.SelectToken("result").ToString());

                            responseObj.Height = getInfoResponse.height;
                            responseObj.TargetHeight = getInfoResponse.target_height;
                            responseObj.NetworkHashRate = (ulong)(getInfoResponse.difficulty / _blockSeconds);
                            responseObj.ConnectionCountOut = getInfoResponse.outgoing_connections_count;
                            responseObj.ConnectionCountIn = getInfoResponse.incoming_connections_count;
                            responseObj.StartTime = GlobalMethods.UnixTimeStampToDateTime(getInfoResponse.start_time);
                            responseObj.Version = getInfoResponse.version;
                            responseObj.Status = getInfoResponse.status;

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
                Logger.LogException("XNV.DGTI", ex);
            }

            return responseObj;
        }

        // Internal helper obejcts used to interact with service
        private class ResGetInfo
        {
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
            public string nettype { get; set; } = string.Empty;
            public string top_block_hash { get; set; } = string.Empty;
            public ulong cumulative_difficulty { get; set; }
            public ulong cumulative_difficulty_top64 { get; set; }
            public ulong block_size_limit { get; set; }
            public ulong block_weight_limit { get; set; }
            public ulong block_size_median { get; set; }
            public ulong block_weight_median { get; set; }
            public ulong start_time { get; set; }
            public ulong free_space { get; set; }
            public bool offline { get; set; }
            public string bootstrap_daemon_address { get; set; } = string.Empty;
            public ulong height_without_bootstrap { get; set; }
            public bool was_bootstrap_ever_used { get; set; }
            public ulong database_size { get; set; }
            public bool update_available { get; set; }
            public string version { get; set; } = string.Empty;

            // From base
            public string status { get; set; } = string.Empty;
            public bool untrusted { get; set; }
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
                    ["jsonrpc"] = "2.0",
                    ["id"] = "0",
                    ["method"] = "get_connections"
                };

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "json_rpc"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                    {
                        Logger.LogInfo("XNV.DGTC", "Response Content is empty");
                    }
                    else
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
                            if (jsonObject.SelectToken("result.connections") != null)
                            {
                                // Set successful response
                                List<ResGetConnections> getConnectionsResponse = JsonConvert.DeserializeObject<List<ResGetConnections>>(jsonObject.SelectToken("result.connections").ToString());

                                foreach (ResGetConnections connection in getConnectionsResponse)
                                {
                                    responseObj.Connections.Add(new Connection
                                    {
                                        Address = connection.address,
                                        Height = Convert.ToInt64(connection.height),
                                        LiveTime = TimeSpan.FromSeconds(connection.live_time).ToString(@"%d\.hh\:mm"),
                                        State = connection.state,
                                        IsIncoming = connection.incoming
                                    });
                                }                                
                            }
                            else
                            {
                                Logger.LogInfo("XNV.DGTC", "Connections missing: " + GlobalMethods.RemoveLineBreaksAndSpaces(httpResponse.Content.ReadAsStringAsync().Result));
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
                Logger.LogException("XNV.DGTC", ex);
            }

            return responseObj;
        }

        private class ResGetConnections
        {
            public bool incoming { get; set; }
            public bool localhost { get; set; }
            public bool local_ip { get; set; }
            public bool ssl { get; set; }
            public string address { get; set; } = string.Empty;
            public string host { get; set; } = string.Empty;
            public string ip { get; set; } = string.Empty;
            public string port { get; set; } = string.Empty;
            public uint rpc_port { get; set; }
            public string peer_id { get; set; } = string.Empty;
            public ulong recv_count { get; set; }
            public ulong recv_idle_time { get; set; }
            public ulong send_count { get; set; }
            public ulong send_idle_time { get; set; }
            public string state { get; set; } = string.Empty;
            public ulong live_time { get; set; }
            public ulong avg_download { get; set; }
            public ulong current_download { get; set; }
            public ulong avg_upload { get; set; }
            public ulong current_upload { get; set; }
            public uint support_flags { get; set; }
            public string connection_id { get; set; } = string.Empty;
            public ulong height { get; set; }
            public uint pruning_seed { get; set; }
            public uint address_type { get; set; }
        }
        #endregion // Get Connections

        #region Mining Status
        public async Task<MiningStatusResponse> GetMiningStatus(RpcBase rpc, MiningStatusRequest requestObj)
        {
            MiningStatusResponse responseObj = new();

            try
            {
                // Build request content json
                var requestJson = new JObject();

                // Call service and process response
                HttpResponseMessage httpResponse = await HttpHelper.GetPostFromService(HttpHelper.GetServiceUrl(rpc, "mining_status"), requestJson.ToString());
                if (httpResponse.IsSuccessStatusCode)
                {
                    if (string.IsNullOrEmpty(httpResponse.Content.ReadAsStringAsync().Result))
                    {
                        Logger.LogInfo("XNV.DMSS", "Response Content is empty");
                    }
                    else
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
                            // Set successful response
                            ResMiningStatus getConnectionsResponse = JsonConvert.DeserializeObject<ResMiningStatus>(jsonObject.ToString());

                            responseObj.IsActive = getConnectionsResponse.active;
                            responseObj.Speed = getConnectionsResponse.speed;
                            responseObj.Address = getConnectionsResponse.address;

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
                Logger.LogException("XNV.DMSS", ex);
            }

            return responseObj;
        }

        private class ResMiningStatus
        {
            public bool active { get; set; }
            public long speed { get; set; }
            public int threads_count { get; set; }
            public string address { get; set; } = string.Empty;
            public bool is_background_mining_enabled { get; set; }
            public int bg_idle_threshold { get; set; }
            public int bg_min_idle_seconds { get; set; }
            public bool bg_ignore_battery { get; set; }
            public int bg_target { get; set; }
            public int block_target { get; set; }
            public long block_reward { get; set; }
            public long difficulty { get; set; }
        }
        #endregion //Mining Status


        // Internal helper obejcts used to interact with service
        private class ResGeneric
        {
            public string status { get; set; } = string.Empty;
            public string untrusted { get; set; } = string.Empty;
        }
    }
}