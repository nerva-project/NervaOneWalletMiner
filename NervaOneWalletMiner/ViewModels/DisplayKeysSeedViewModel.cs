using NervaOneWalletMiner.Objects.Constants;

namespace NervaOneWalletMiner.ViewModels
{
    internal class DisplayKeysSeedViewModel : ViewModelBase
    {
        public string Message { get; }
        public string ReturnPage { get; }

        public DisplayKeysSeedViewModel()
        {
            Message = string.Empty;
            ReturnPage = SplitViewPages.WalletSetup;
        }

        public DisplayKeysSeedViewModel(string message, string returnPage = SplitViewPages.WalletSetup)
        {
            Message = message;
            ReturnPage = returnPage;
        }
    }
}
