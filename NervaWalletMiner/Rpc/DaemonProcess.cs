using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System.Diagnostics;

namespace NervaWalletMiner.Rpc
{
    public static class DaemonProcess
    {
        public static void ForceClose()
        {
            ProcessManager.Kill(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DaemonProcessName);
        }

        public static bool IsRunning()
        {
            ProcessManager.IsRunning(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DaemonProcessName, out Process? process);

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
            return GenerateCommandLine(string.Empty);
        }

        public static string GenerateCommandLine(string extraParams)
        {
            string appCommand = ProcessManager.GenerateCommandLine(GlobalMethods.GetDaemonPath(), GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc);

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining)
            {
                string ma = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;

                Logger.LogDebug("DP.GCL", $"Enabling startup mining @ {ma}");
                appCommand += $" --start-mining {ma} --mining-threads {GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads}";
            }

            if(GlobalMethods.IsLinux())
            {
                appCommand += " --detach";
            }

            appCommand += $" {GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments}";

            if (!string.IsNullOrEmpty(extraParams))
            {
                appCommand += " " + extraParams;
            }

            return appCommand;
        }
    }
}