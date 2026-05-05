using Avalonia.Media.Imaging;

namespace NervaOneWalletMiner.Objects
{
    public class CoinListItem
    {
        public string Key { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Bitmap? Logo { get; set; }
    }
}
