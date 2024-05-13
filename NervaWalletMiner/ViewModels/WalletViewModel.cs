using Avalonia.Media.Imaging;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.Generic;

namespace NervaWalletMiner.ViewModels
{
    internal class WalletViewModel : ViewModelBase
    {
        // TODO: Figure out how to do this in one place instead of on each view
        private Bitmap _CoinIcon = GlobalData.Logo;
        public Bitmap CoinIcon
        {
            get => _CoinIcon;
            set => this.RaiseAndSetIfChanged(ref _CoinIcon, value);
        }

        private string _OpenCloseWallet = StatusWallet.OpenWallet;
        public string OpenCloseWallet
        {
            get => _OpenCloseWallet;
            set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
        }

        private string _TotalCoins = "";
        public string TotalCoins
        {
            get => _TotalCoins;
            set => this.RaiseAndSetIfChanged(ref _TotalCoins, value);
        }

        private string _UnlockedCoins = "";
        public string UnlockedCoins
        {
            get => _UnlockedCoins;
            set => this.RaiseAndSetIfChanged(ref _UnlockedCoins, value);
        }

        private string _TotalLockedLabel = "Total: ";
        public string TotalLockedLabel
        {
            get => _TotalLockedLabel;
            set => this.RaiseAndSetIfChanged(ref _TotalLockedLabel, value);
        }

        private string _TotalUnlockedLabel = "Unlocked: ";
        public string TotalUnlockedLabel
        {
            get => _TotalUnlockedLabel;
            set => this.RaiseAndSetIfChanged(ref _TotalUnlockedLabel, value);
        }

        private List<Account> _WalletAddresses = new();
        public List<Account> WalletAddresses
        {
            get => _WalletAddresses;
            set => this.RaiseAndSetIfChanged(ref _WalletAddresses, value);
        }       
    }
}