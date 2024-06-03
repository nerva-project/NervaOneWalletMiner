namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class MakeIntegratedAddressRequest
    {
        public string StandardAddress { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
    }
}