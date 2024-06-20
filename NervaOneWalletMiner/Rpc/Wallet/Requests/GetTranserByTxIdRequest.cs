namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class GetTranserByTxIdRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public int AccountIndex { get; set; } = 0;
    }
}