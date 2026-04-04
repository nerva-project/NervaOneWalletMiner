namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class RestoreFromKeysRequest
    {
        public ulong RestoreHeight { get; set; } = 0;
        public string WalletAddress { get; set; } = string.Empty;
        public char[] ViewKey { get; set; } = [];
        public char[] SpendKey { get; set; } = [];
        public string WalletName { get; set; } = string.Empty;
        public char[] Password { get; set; } = [];
        public string Language { get; set; } = string.Empty;
        public bool AutoSave { get; set; } = true;
    }
}