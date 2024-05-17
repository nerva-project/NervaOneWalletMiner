namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class RestoreFromKeysRequest
    {
        public ulong RestoreHeight { get; set; } = 0;
        public string WalletAddress { get; set; } = string.Empty;
        public string ViewKey { get; set; } = string.Empty;
        public string SpendKey { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool AutoSave { get; set; } = true;
    }
}