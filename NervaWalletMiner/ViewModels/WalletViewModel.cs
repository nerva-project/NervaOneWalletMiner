using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;

namespace NervaWalletMiner.ViewModels
{
    internal class WalletViewModel : ViewModelBase
    {
        private string _OpenCloseWallet = StatusWallet.OpenWallet;
        public string OpenCloseWallet
        {
            get => _OpenCloseWallet;
            set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
        }

        private string _TotalXnv = "";
        public string TotalXnv
        {
            get => _TotalXnv;
            set => this.RaiseAndSetIfChanged(ref _TotalXnv, value);
        }

        private string _UnlockedXnv = "";
        public string UnlockedXnv
        {
            get => _UnlockedXnv;
            set => this.RaiseAndSetIfChanged(ref _UnlockedXnv, value);
        }

        private List<Account> _WalletAddresses = new();
        public List<Account> WalletAddresses
        {
            get => _WalletAddresses;
            set => this.RaiseAndSetIfChanged(ref _WalletAddresses, value);
        }       
    }
}