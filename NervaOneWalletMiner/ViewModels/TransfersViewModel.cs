using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NervaOneWalletMiner.ViewModels
{
    internal class TransfersViewModel : ViewModelBase
    {
        // Empty state replaces transactions grid when no wallet is open, same as on Wallet screen.
        // UIManager.UpdateTransfersView keeps these in sync
        private bool _IsEmptyStateVisible = true;
        public bool IsEmptyStateVisible
        {
            get => _IsEmptyStateVisible;
            set => this.RaiseAndSetIfChanged(ref _IsEmptyStateVisible, value);
        }

        private string _EmptyStateMessage = string.Empty;
        public string EmptyStateMessage
        {
            get => _EmptyStateMessage;
            set => this.RaiseAndSetIfChanged(ref _EmptyStateMessage, value);
        }

        // True when user has no wallets at all, which means Wallet Setup instead of Wallet screen
        private bool _IsNoWalletsState = false;
        public bool IsNoWalletsState
        {
            get => _IsNoWalletsState;
            set => this.RaiseAndSetIfChanged(ref _IsNoWalletsState, value);
        }

        private ObservableCollection<Transfer> _Transactions = [];
        public ObservableCollection<Transfer> Transactions
        {
            get => _Transactions;
            set => this.RaiseAndSetIfChanged(ref _Transactions, value);
        }
    }
}