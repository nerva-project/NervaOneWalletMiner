using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetPrivateKeysResponse
    {
        public ServiceError Error { get; set; } = new();

        public string PublicViewKey { get; set; } = string.Empty;
        public char[] PrivateViewKey { get; set; } = [];
        public string PublicSpendKey { get; set; } = string.Empty;
        public char[] PrivateSpendKey { get; set; } = [];
        public char[] Mnemonic { get; set; } = [];
    }
}