using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class RescanBlockchainResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}