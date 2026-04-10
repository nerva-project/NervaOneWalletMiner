using NervaOneWalletMiner.Objects.Constants;

namespace NervaOneWalletMiner.ViewModels
{
    internal class TransferFundsViewModel : ViewModelBase
    {
        public uint SelectedAccountIndex { get; }
        public string ToAddress { get; }
        public string PaymentId { get; }
        public string ReturnPage { get; }

        public TransferFundsViewModel()
        {
            ToAddress = string.Empty;
            PaymentId = string.Empty;
            ReturnPage = SplitViewPages.Wallet;
        }

        public TransferFundsViewModel(uint selectedAccountIndex, string toAddress, string paymentId, string returnPage = SplitViewPages.Wallet)
        {
            SelectedAccountIndex = selectedAccountIndex;
            ToAddress = toAddress;
            PaymentId = paymentId;
            ReturnPage = returnPage;
        }
    }
}
