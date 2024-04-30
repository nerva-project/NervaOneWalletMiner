using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System.Diagnostics;

namespace NervaWalletMiner.Rpc
{
    public static class DaemonProcess
    {
        public static void ForceClose()
        {
            ProcessManager.Kill(FileNames.NERVA_DAEMON);
        }

        public static bool IsRunning()
        {
            ProcessManager.IsRunning(FileNames.NERVA_DAEMON, out Process? process);

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
            string appCommand = ProcessManager.GenerateCommandLine(GlobalMethods.GetDaemonPath(), GlobalData.ApplicationSettings.Daemon.Rpc);

            if (GlobalData.ApplicationSettings.Daemon.AutoStartMining)
            {
                string ma = GlobalData.ApplicationSettings.Daemon.MiningAddress;

                Logger.LogDebug("DP.GCL", $"Enabling startup mining @ {ma}");
                appCommand += $" --start-mining {ma} --mining-threads {GlobalData.ApplicationSettings.Daemon.MiningThreads}";
            }

            if(GlobalMethods.IsLinux())
            {
                appCommand += " --detach";
            }

            appCommand += $" {GlobalData.ApplicationSettings.Daemon.AdditionalArguments}";

            if (!string.IsNullOrEmpty(extraParams))
            {
                appCommand += " " + extraParams;
            }

            return appCommand;
        }
    }
}