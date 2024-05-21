using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    public interface IWalletService
    {
        Task<OpenWalletResponse> OpenWallet(RpcBase rpc, OpenWalletRequest requestObj);

        Task<CloseWalletResponse> CloseWallet(RpcBase rpc, CloseWalletRequest requestObj);

        Task<CreateWalletResponse> CreateWallet(RpcBase rpc, CreateWalletRequest requestObj);

        Task<SaveWalletResponse> SaveWallet(RpcBase rpc, SaveWalletRequest requestObj);

        Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj);

        Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj);

        Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj);

        
        // TODO: Pass optional tag
        Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj);

        Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj);

        Task<GetHeightResponse> GetHeight(RpcBase rpc, GetHeightRequest requestObj);

        Task<QueryKeyResponse> QueryKey(RpcBase rpc, QueryKeyRequest requestObj);
    }
}