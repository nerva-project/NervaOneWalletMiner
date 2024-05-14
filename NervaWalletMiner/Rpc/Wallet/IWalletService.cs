using NervaWalletMiner.Rpc.Common;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public interface IWalletService
    {
        Task<OpenWalletResponse> OpenWallet(RpcBase rpc, OpenWalletRequest requestObj);

        Task<CloseWalletResponse> CloseWallet(RpcBase rpc, CloseWalletRequest requestObj);

        Task<CreateWalletResponse> CreateWallet(RpcBase rpc, CreateWalletRequest requestObj);


        // TODO: Pass optional tag
        Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj);

        Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj);        
    }
}