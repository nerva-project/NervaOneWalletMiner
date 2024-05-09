using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System.Diagnostics;

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

        public static string GenerateCommandLine()
        {
            string appCommand = ProcessManager.GenerateCommandLine(GlobalMethods.GetRpcWalletPath(), GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc);
            appCommand += " --disable-rpc-login";
            appCommand += $" --wallet-dir \"{GlobalData.WalletDir}\"";
            appCommand += $" --daemon-address 127.0.0.1:{GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port}";

            // TODO: Uncomment to enable rpc user:pass.
            // string ip = d.IsPublic ? $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind" : $" --rpc-bind-ip 127.0.0.1";
            // appCommand += $"{ip} --rpc-login {d.Login}:{d.Pass}";

            return appCommand;
        }
    }
}