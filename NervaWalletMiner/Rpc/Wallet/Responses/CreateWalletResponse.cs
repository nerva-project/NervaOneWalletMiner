using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Rpc.Wallet.Responses
{
    public class CreateWalletResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}