namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class GetTranserByTxIdRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public uint AccountIndex { get; set; } = 0;
    }
}