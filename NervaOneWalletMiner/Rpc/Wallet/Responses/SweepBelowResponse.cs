using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class SweepBelowResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}