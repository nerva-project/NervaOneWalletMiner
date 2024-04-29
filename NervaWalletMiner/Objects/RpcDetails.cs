using NervaWalletMiner.Helpers;

namespace NervaWalletMiner.Objects
{
    public class RpcDetails
    {
        public bool IsPublic { get; set; } = false;

        public string Host { get; set; } = "127.0.0.1";

        public uint Port { get; set; } = 0;

        public string Login { get; set; } = string.Empty;

        public string Pass { get; set; } = string.Empty;

        public uint LogLevel { get; set; } = 1;


        public RpcDetails(uint port)
        {
            Port = port;
            Login = GlobalMethods.GenerateRandomString(24);
            Pass = GlobalMethods.GenerateRandomString(24);
        }
    }
}