using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.ObjectModel;

namespace NervaOneWalletMiner.ViewModels
{
    internal class AddressBookViewModel : ViewModelBase
    {
        private ObservableCollection<AddressInfo> _addresses = new();
        public ObservableCollection<AddressInfo> Addresses
        {
            get => _addresses;
            set => this.RaiseAndSetIfChanged(ref _addresses, value);
        }
    }
}
