using NervaWalletMiner.Rpc.Common;
using NervaWalletMiner.Rpc.Daemon.Requests;
using NervaWalletMiner.Rpc.Daemon.Responses;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Daemon
{
    public interface IDaemonService
    {
        Task<StartMiningResponse> StartMining(RpcBase rpc, StartMiningRequest requestObj);

        Task<StopMiningResponse> StopMining(RpcBase rpc, StopMiningRequest requestObj);

        Task<GetInfoResponse> GetInfo(RpcBase rpc, GetInfoRequest requestObj);

        Task<GetConnectionsResponse> GetConnections(RpcBase rpc, GetConnectionsRequest requestObj);

        Task<MiningStatusResponse> MiningStatus(RpcBase rpc, MiningStatusRequest requestObj);
    }
}