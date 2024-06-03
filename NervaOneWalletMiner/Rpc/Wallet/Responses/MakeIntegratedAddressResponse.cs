using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class MakeIntegratedAddressResponse
    {
        public ServiceError Error { get; set; } = new();

        public string IntegratedAddress { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
    }
}