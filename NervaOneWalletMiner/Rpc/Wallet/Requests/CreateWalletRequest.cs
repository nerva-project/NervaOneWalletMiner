namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class CreateWalletRequest
    {
        public string WalletName { get; set; } = string.Empty;
        public char[] Password { get; set; } = [];
        public string Language { get; set; } = string.Empty;
    }
}