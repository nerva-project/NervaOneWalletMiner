using NervaWalletMiner.Objects;
using ReactiveUI;

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
    }
}