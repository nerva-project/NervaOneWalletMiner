using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class RescanSpentResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}