using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class SaveWalletResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}