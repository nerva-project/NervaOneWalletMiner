using NervaWalletMiner.Objects;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;

namespace NervaWalletMiner.ViewModels
{
    public class WalletViewModel : ViewModelBase
    {
        private string _OpenCloseWallet = "Open Wallet";
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

        private List<Wallet> _WalletAddresses = new();
        public List<Wallet> WalletAddresses
        {
            get => _WalletAddresses;
            set => this.RaiseAndSetIfChanged(ref _WalletAddresses, value);
        }


        public ICommand OpenWalletCommand { get; }

        public Interaction<OpenWalletViewModel, SelectedWalletViewModel?> ShowDialog { get; }

        public WalletViewModel()
        {
            ShowDialog = new Interaction<OpenWalletViewModel, SelectedWalletViewModel?>();

            OpenWalletCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new OpenWalletViewModel();

                var result = await ShowDialog.Handle(store);
            });
        }        
    }
}