using NervaWalletMiner.Objects.Constants;

namespace NervaWalletMiner.Objects
{
    public class DialogResult
    {
        // All screens
        public bool IsOk { get; set; } = false;
        public bool IsCancel { get; set; } = false;

        // Open, create wallet
        public string WalletName { get; set; } = string.Empty;
        public string WalletPassword { get; set; } = string.Empty;

        // Create wallet
        public string WalletLanguage { get; set; } = Language.English;

        // Transfer funds
        public string SendFromAddress { get; set; } = string.Empty;
        public uint SendFromAddressIndex { get; set; } = 0;
        public string SendToAddress { get; set; } = string.Empty;
        public double SendAmount { get; set; } = 0.0;
        public string SendPaymentId { get; set; } = string.Empty;
    }
}