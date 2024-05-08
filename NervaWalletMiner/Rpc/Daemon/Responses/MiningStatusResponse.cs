using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Rpc.Daemon.Responses
{
    public class MiningStatusResponse
    {
        public ServiceError Error { get; set; } = new();

        public bool IsActive { get; set; }
        public long Speed { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}