using Avalonia.Controls;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewModels
{
    internal class WalletViewModel : ViewModelBase
    {
        public delegate void TransferAction(Window owner, string toAddress, string paymentId);
        public event TransferAction? TransferEvent = null;
        public void Transfer(Window owner, string toAddress, string paymentId)
        {
            TransferEvent!.Invoke(owner, toAddress, paymentId);
        }


        private string _OpenCloseWallet = StatusWallet.OpenWallet;
        public string OpenCloseWallet
        {
            get => _OpenCloseWallet;
            set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
        }

        private string _TotalCoins = string.Empty;
        public string TotalCoins
        {
            get => _TotalCoins;
            set => this.RaiseAndSetIfChanged(ref _TotalCoins, value);
        }

        private string _UnlockedCoins = string.Empty;
        public string UnlockedCoins
        {
            get => _UnlockedCoins;
            set => this.RaiseAndSetIfChanged(ref _UnlockedCoins, value);
        }

        private string _TotalLockedLabel = "Total";
        public string TotalLockedLabel
        {
            get => _TotalLockedLabel;
            set => this.RaiseAndSetIfChanged(ref _TotalLockedLabel, value);
        }

        private string _TotalUnlockedLabel = "Unlocked";
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