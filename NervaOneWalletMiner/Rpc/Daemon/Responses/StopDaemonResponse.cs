using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Daemon.Responses
{
    public class StopDaemonResponse
    {
        public ServiceError Error { get; set; } = new();
    }
}