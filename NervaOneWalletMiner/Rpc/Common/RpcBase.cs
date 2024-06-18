using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.Rpc.Common
{
    public class RpcBase
    {
        public bool IsPublic { get; set; } = false;
        public string HTProtocol { get; set; } = "http";
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = -1;

        // Required for some coins only
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}