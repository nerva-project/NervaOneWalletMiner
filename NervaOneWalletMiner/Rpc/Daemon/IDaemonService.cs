using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.Rpc.Daemon.Responses;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Daemon
{
    public interface IDaemonService
    {
        Task<StartMiningResponse> StartMining(RpcBase rpc, StartMiningRequest requestObj);

        Task<StopMiningResponse> StopMining(RpcBase rpc, StopMiningRequest requestObj);

        Task<StopDaemonResponse> StopDaemon(RpcBase rpc, StopDaemonRequest requestObj);

        Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj);

        Task<GetConnectionsResponse> GetConnections(RpcBase rpc, GetConnectionsRequest requestObj);

        Task<MiningStatusResponse> MiningStatus(RpcBase rpc, MiningStatusRequest requestObj);
    }
}