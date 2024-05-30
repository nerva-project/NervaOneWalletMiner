using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class CreateAccountResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}