namespace NervaOneWalletMiner.ViewModels
{
    internal class TransactionDetailsViewModel : ViewModelBase
    {
        public string TransactionId { get; }
        public int AccountIndex { get; }
        public decimal Amount { get; }

        public TransactionDetailsViewModel()
        {
            TransactionId = string.Empty;
        }

        public TransactionDetailsViewModel(string transactionId, int accountIndex, decimal amount)
        {
            TransactionId = transactionId;
            AccountIndex = accountIndex;
            Amount = amount;
        }
    }
}
