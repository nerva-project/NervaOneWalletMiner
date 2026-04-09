namespace NervaOneWalletMiner.ViewModels
{
    internal class TransferFundsViewModel : ViewModelBase
    {
        public uint SelectedAccountIndex { get; }
        public string ToAddress { get; }
        public string PaymentId { get; }

        public TransferFundsViewModel()
        {
            ToAddress = string.Empty;
            PaymentId = string.Empty;
        }

        public TransferFundsViewModel(uint selectedAccountIndex, string toAddress, string paymentId)
        {
            SelectedAccountIndex = selectedAccountIndex;
            ToAddress = toAddress;
            PaymentId = paymentId;
        }
    }
}
