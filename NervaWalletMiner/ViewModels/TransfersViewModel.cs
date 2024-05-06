using NervaWalletMiner.Objects;
using ReactiveUI;
using System.Collections.Generic;

namespace NervaWalletMiner.ViewModels
{
    internal class TransfersViewModel : ViewModelBase
    {
        private string _OpenCloseWallet = StatusWallet.OpenWallet;
        public string OpenCloseWallet
        {
            get => _OpenCloseWallet;
            set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
        }

        private List<Transfer> _Transactions = new();
        public List<Transfer> Transactions
        {
            get => _Transactions;
            set => this.RaiseAndSetIfChanged(ref _Transactions, value);
        }
    }
}