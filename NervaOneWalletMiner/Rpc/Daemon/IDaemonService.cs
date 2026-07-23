using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Daemon
{
    public interface IDaemonService
    {
        Task<StartMiningResponse> StartMining(RpcBase rpc, StartMiningRequest requestObj);

        Task<StopMiningResponse> StopMining(RpcBase rpc, StopMiningRequest requestObj);
        
        Task<StopDaemonResponse> StopDaemon(RpcBase rpc, StopDaemonRequest requestObj);

        // timeout overrides the default so a connection test can fail fast instead of hanging
        Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj, TimeSpan? timeout = null);

        Task<GetConnectionsResponse> GetConnections(RpcBase rpc, GetConnectionsRequest requestObj);

        Task<MiningStatusResponse> GetMiningStatus(RpcBase rpc, MiningStatusRequest requestObj);
    }
}