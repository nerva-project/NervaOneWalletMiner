using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Rpc.Wallet.Responses
{
    public class OpenWalletResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}