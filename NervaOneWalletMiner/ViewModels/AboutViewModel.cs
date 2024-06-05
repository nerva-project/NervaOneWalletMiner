using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Helpers;
using ReactiveUI;

namespace NervaOneWalletMiner.ViewModels
{
    internal class AboutViewModel : ViewModelBase
    {
        // TODO: Figure out how to do this in one place instead of on each view
        private Bitmap _CoinIcon = GlobalData.Logo;
        public Bitmap CoinIcon
        {
            get => _CoinIcon;
            set => this.RaiseAndSetIfChanged(ref _CoinIcon, value);
        }
    }
}