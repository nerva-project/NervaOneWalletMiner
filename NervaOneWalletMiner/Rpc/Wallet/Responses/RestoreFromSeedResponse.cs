using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class RestoreFromSeedResponse
    {
        public ServiceError Error { get; set; } = new();

        public string Address { get; set; } = string.Empty;
        public string Seed { get; set; } = string.Empty;
        public string Info { get; set;} = string.Empty;
        public bool WasDeprecated { get; set;} = false;
    }
}