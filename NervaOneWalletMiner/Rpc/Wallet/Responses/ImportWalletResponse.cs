using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class ImportWalletResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}
