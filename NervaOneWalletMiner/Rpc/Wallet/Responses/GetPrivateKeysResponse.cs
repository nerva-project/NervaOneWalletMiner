using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetPrivateKeysResponse
    {
        public ServiceError Error { get; set; } = new();

        public string PublicViewKey { get; set; } = string.Empty;
        public string PrivateViewKey { get; set; } = string.Empty;
        public string PublicSpendKey { get; set; } = string.Empty;
        public string PrivateSpendKey { get; set; } = string.Empty;
        public string Mnemonic { get; set; } = string.Empty;
    }
}