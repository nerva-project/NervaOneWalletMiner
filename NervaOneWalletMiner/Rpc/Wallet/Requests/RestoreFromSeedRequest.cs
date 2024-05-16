namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class RestoreFromSeedRequest
    {
        public ulong RestoreHeight { get; set; } = 0;
        public string Seed { get; set; } = string.Empty;
        public string SeedOffset { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool AutoSave { get; set; } = true;
    }
}