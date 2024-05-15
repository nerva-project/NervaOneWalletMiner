using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class OpenWalletResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}