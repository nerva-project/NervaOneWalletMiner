using System.IO;

namespace NervaWalletMiner.Objects
{
    public static class FileNames
    {

#if WINDOWS
        public const string NERVAD = "nervad.exe";
        public const string RPC_WALLET = "nerva-wallet-rpc.exe";
#else
        public const string NERVAD = "nervad";
        public const string RPC_WALLET = "nerva-wallet-rpc";
#endif

        public static bool DirectoryContainsCliTools(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            bool hasDaemon = File.Exists(Path.Combine(path, NERVAD));
            bool hasRpcWallet = File.Exists(Path.Combine(path, RPC_WALLET));

            return (hasRpcWallet && hasDaemon);
        }
    }
}