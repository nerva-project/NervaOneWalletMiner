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
        public char[] WalletPassword { get; set; } = [];

        // Create wallet, restore from seed
        public string WalletLanguage { get; set; } = Language.English;

        // Transfer funds
        public string SendFromAddress { get; set; } = string.Empty;
        public uint SendFromAddressIndex { get; set; } = 0;
        public string SendToAddress { get; set; } = string.Empty;
        public decimal SendAmount { get; set; } = 0;
        public string SendPaymentId { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public bool IsSplitTranfer { get;set; } = false;

        // Restore from Seed
        public char[] SeedPhrase { get; set; } = [];
        public string SeedOffset { get; set; } = string.Empty;

        // Restore from Keys
        public string WalletAddress { get; set;} = string.Empty;
        public char[] ViewKey { get; set; } = [];
        public char[] SpendKey {  get; set; } = [];

        // Restart with commands
        public string RestartOptions { get; set; } = string.Empty;

        // TextBox
        public string TextBoxValue { get; set; } = string.Empty;
    }
}