namespace NervaOneWalletMiner.ViewModels
{
    internal class AddressInfoViewModel : ViewModelBase
    {
        public int AccountIndex { get; }

        public AddressInfoViewModel() { }

        public AddressInfoViewModel(int accountIndex)
        {
            AccountIndex = accountIndex;
        }
    }
}
