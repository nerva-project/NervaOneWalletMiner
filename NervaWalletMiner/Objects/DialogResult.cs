namespace NervaWalletMiner.Objects
{
    public class DialogResult
    {
        public bool IsOk { get; set; } = false;
        public bool IsCancel { get; set; } = false;
        public string? WalletName { get; set; } = string.Empty;
        public string? WalletPassword { get; set; } = string.Empty;
    }
}