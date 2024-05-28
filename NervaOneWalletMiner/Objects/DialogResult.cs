using NervaOneWalletMiner.Objects.Constants;

namespace NervaOneWalletMiner.Objects
{
    public class DialogResult
    {
        // All screens
        public bool IsOk { get; set; } = false;
        public bool IsCancel { get; set; } = false;

        // Open, create wallet, restore from seed
        public string WalletName { get; set; } = string.Empty;
        public string WalletPassword { get; set; } = string.Empty;

        // Create wallet, restore from seed
        public string WalletLanguage { get; set; } = Language.English;

        // Transfer funds
        public string SendFromAddress { get; set; } = string.Empty;
        public uint SendFromAddressIndex { get; set; } = 0;
        public string SendToAddress { get; set; } = string.Empty;
        public decimal SendAmount { get; set; } = 0;
        public string SendPaymentId { get; set; } = string.Empty;

        // Restore from Seed
        public string SeedPhrase { get; set; } = string.Empty;
        public string SeedOffset { get; set; } = string.Empty;

        // Restore from Keys
        public string WalletAddress { get; set;} = string.Empty;
        public string ViewKey { get; set; } = string.Empty;
        public string SpendKey {  get; set; } = string.Empty;

        // Restart with commands
        public string RestartOptions { get; set; } = string.Empty;
    }
}