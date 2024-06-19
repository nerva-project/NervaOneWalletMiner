namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class OpenWalletRequest
    {
        public string WalletName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool LoadOnStartup { get; set; } = false;
    }
}