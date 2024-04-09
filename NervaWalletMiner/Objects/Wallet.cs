using Avalonia.Media.Imaging;

namespace NervaWalletMiner.Objects
{
    public class Wallet
    {
        public int Id { get; set; }
        public string? Label { get; set; }
        public string? Address { get; set; }
        public double LockedBalance { get; set; }
        public double UnlockedBalance { get; set; }
        public Bitmap? WalletIcon { get; set; }
    }
}