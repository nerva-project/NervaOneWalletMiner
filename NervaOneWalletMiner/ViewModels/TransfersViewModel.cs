using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NervaOneWalletMiner.ViewModels
{
    internal class TransfersViewModel : ViewModelBase
    {
        private string _OpenCloseWallet = StatusWallet.OpenWallet;
        public string OpenCloseWallet
        {
            get => _OpenCloseWallet;
            set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
        }

        private ObservableCollection<Transfer> _Transactions = [];
        public ObservableCollection<Transfer> Transactions
        {
            get => _Transactions;
            set => this.RaiseAndSetIfChanged(ref _Transactions, value);
        }
    }
}