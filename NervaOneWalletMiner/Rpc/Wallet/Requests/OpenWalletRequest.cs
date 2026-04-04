namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class OpenWalletRequest
    {
        public string WalletName { get; set; } = string.Empty;
        public char[] Password { get; set; } = [];
    }
}