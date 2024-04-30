using NervaWalletMiner.Helpers;
using System.IO;

namespace NervaWalletMiner.Objects
{
    public static class FileNames
    {
        public static string NERVA_DAEMON = GlobalMethods.IsWindows() ?  "nervad.exe" : "nervad";
        public static string NERVA_WALLET_RPC = GlobalMethods.IsWindows() ? "nerva-wallet-rpc.exe" : "nerva-wallet-rpc";

        public static bool DirectoryContainsCliTools(string path)
        {
            if (!Directory.Exists(path))
            {
                return false;
            }

            bool hasDaemon = File.Exists(Path.Combine(path, NERVA_DAEMON));
            bool hasRpcWallet = File.Exists(Path.Combine(path, NERVA_WALLET_RPC));

            return (hasRpcWallet && hasDaemon);
        }
    }
}