using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    public interface IWalletService
    {
        Task<OpenWalletResponse> OpenWallet(RpcBase rpc, OpenWalletRequest requestObj);

        Task<UnlockWithPassResponse> UnlockWithPass(RpcBase rpc, UnlockWithPassRequest requestObj);

        Task<CloseWalletResponse> CloseWallet(RpcBase rpc, CloseWalletRequest requestObj);

        Task<CreateWalletResponse> CreateWallet(RpcBase rpc, CreateWalletRequest requestObj);

        Task<CreateAccountResponse> CreateAccount(RpcBase rpc, CreateAccountRequest requestObj);

        Task<LabelAccountResponse> LabelAccount(RpcBase rpc, LabelAccountRequest requestObj);

        Task<SaveWalletResponse> SaveWallet(RpcBase rpc, SaveWalletRequest requestObj);

        Task<RestoreFromSeedResponse> RestoreFromSeed(RpcBase rpc, RestoreFromSeedRequest requestObj);

        Task<RestoreFromKeysResponse> RestoreFromKeys(RpcBase rpc, RestoreFromKeysRequest requestObj);

        Task<TransferResponse> Transfer(RpcBase rpc, TransferRequest requestObj);

        Task<TransferResponse> TransferSplit(RpcBase rpc, TransferRequest requestObj);

        Task<RescanSpentResponse> RescanSpent(RpcBase rpc, RescanSpentRequest requestObj);

        Task<RescanBlockchainResponse> RescanBlockchain(RpcBase rpc, RescanBlockchainRequest requestObj);

        Task<MakeIntegratedAddressResponse> MakeIntegratedAddress(RpcBase rpc, MakeIntegratedAddressRequest requestObj);


        // TODO: Pass optional tag
        Task<GetAccountsResponse> GetAccounts(RpcBase rpc, GetAccountsRequest requestObj);

        Task<GetTransfersResponse> GetTransfers(RpcBase rpc, GetTransfersRequest requestObj);

        Task<GetTransferByTxIdResponse> GetTransferByTxId(RpcBase rpc, GetTranserByTxIdRequest requestObj);

        Task<GetHeightResponse> GetHeight(RpcBase rpc, GetHeightRequest requestObj);

        Task<GetPrivateKeysResponse> GetPrivateKeys(RpcBase rpc, GetPrivateKeysRequest requestObj);
    }
}