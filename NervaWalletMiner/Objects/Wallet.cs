using Avalonia.Media.Imaging;

namespace NervaWalletMiner.Objects
{
    public class Wallet
    {
        public int Index { get; set; }
        public string? Label { get; set; }
        public string? Address { get; set; }
        public double BalanceLocked { get; set; }
        public double BalanceUnlocked { get; set; }
        public Bitmap? WalletIcon { get; set; }
    }
}