using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.Rpc.Common
{
    public class RpcBase
    {
        public bool IsPublic { get; set; } = false;
        public string HTProtocol { get; set; } = "http";
        public string Host { get; set; } = "127.0.0.1";
        public uint Port { get; set; } = 0;
    }
}