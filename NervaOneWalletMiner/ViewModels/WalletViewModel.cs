using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.ViewModels
{
    internal class WalletViewModel : ViewModelBase
    {
        public delegate void TransferUiAction(string toAddress, string paymentId);
        public event TransferUiAction? TransferUiEvent = null;
        public void TransferUi(string toAddress, string paymentId)
        {
            TransferUiEvent!.Invoke(toAddress, paymentId);
        }

        public delegate Task<bool> CloseWalletNonUiAction();
        public event CloseWalletNonUiAction? CloseWalletNonUiEvent = null;
        public Task<bool> CloseWalletNonUi()
        {
            return CloseWalletNonUiEvent!.Invoke();
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

        // Empty state replaces accounts grid when no wallet is open. It tells new users what to do next
        // instead of leaving them looking at empty grid. UIManager.UpdateWalletView keeps these in sync
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

        // True when user has no wallets at all, which means they need Create/Restore instead of Open
        private bool _IsNoWalletsState = false;
        public bool IsNoWalletsState
        {
            get => _IsNoWalletsState;
            set => this.RaiseAndSetIfChanged(ref _IsNoWalletsState, value);
        }

        private ObservableCollection<Account> _WalletAddresses = new();
        public ObservableCollection<Account> WalletAddresses
        {
            get => _WalletAddresses;
            set => this.RaiseAndSetIfChanged(ref _WalletAddresses, value);
        }
    }
}