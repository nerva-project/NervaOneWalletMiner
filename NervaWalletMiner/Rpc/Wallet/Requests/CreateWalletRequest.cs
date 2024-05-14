namespace NervaWalletMiner.Rpc.Wallet.Requests
{
    public class CreateWalletRequest
    {
        public string WalletName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
    }
}