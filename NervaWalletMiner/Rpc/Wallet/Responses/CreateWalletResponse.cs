using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class CreateWalletResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}