using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetHeightResponse
    {
        public ServiceError Error { get; set; } = new();

        public ulong Height { get; set; } = 0;
    }
}