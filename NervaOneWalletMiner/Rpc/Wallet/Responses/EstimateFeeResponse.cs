using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class EstimateFeeResponse
    {
        public ServiceError Error { get; set; } = new();
        public decimal Fee { get; set; } = 0;
        public string TxData { get; set; } = string.Empty;
    }
}
