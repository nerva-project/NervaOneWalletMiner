using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Objects.Settings;
using NervaWalletMiner.Rpc.Common;
using System.Diagnostics;
using System.Runtime;

namespace NervaWalletMiner.Rpc
{
    public static class WalletProcess
    {
        public static void ForceClose()
        {
            ProcessManager.Kill(GlobalData.WalletProcessName);
        }

        public static bool IsRunning()
        {
            ProcessManager.IsRunning(GlobalData.WalletProcessName, out Process? process);

            if (process != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GenerateOptions(SettingsWallet walletSettings, RpcBase daemonRpc)
        {
            // TODO: Need to make this coin specific when adding coins that use different startup options

            string appCommand = "--daemon-address " + daemonRpc.Host + ":" + daemonRpc.Port;
            appCommand += " --rpc-bind-port " + walletSettings.Rpc.Port;
            appCommand += " --disable-rpc-login";
            appCommand += " --wallet-dir \"" + GlobalData.WalletDir + "\"";
            appCommand += " --log-level " + walletSettings.LogLevel;
            appCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetRpcWalletProcess()) + "\"";

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsTestnet)
            {
                Logger.LogDebug("PM.GCL", "Connecting to testnet...");
                appCommand += " --testnet";
            }
                       
            // TODO: Uncomment to enable rpc user:pass.
            // string ip = d.IsPublic ? $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind" : $" --rpc-bind-ip 127.0.0.1";
            // appCommand += $"{ip} --rpc-login {d.Login}:{d.Pass}";

            return appCommand;
        }
    }
}