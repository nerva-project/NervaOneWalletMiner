using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System.Diagnostics;

namespace NervaWalletMiner.Rpc
{
    public static class DaemonProcess
    {
        public static void ForceClose()
        {
            ProcessManager.Kill(GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].DaemonProcessName);
        }

        public static bool IsRunning()
        {
            ProcessManager.IsRunning(GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].DaemonProcessName, out Process? process);

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
            string appCommand = ProcessManager.GenerateCommandLine(GlobalMethods.GetDaemonPath(), GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].Rpc);

            if (GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].AutoStartMining)
            {
                string ma = GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningAddress;

                Logger.LogDebug("DP.GCL", $"Enabling startup mining @ {ma}");
                appCommand += $" --start-mining {ma} --mining-threads {GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads}";
            }

            if(GlobalMethods.IsLinux())
            {
                appCommand += " --detach";
            }

            appCommand += $" {GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].AdditionalArguments}";

            if (!string.IsNullOrEmpty(extraParams))
            {
                appCommand += " " + extraParams;
            }

            return appCommand;
        }
    }
}