using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using System.Configuration;
using System.Diagnostics;

namespace NervaWalletMiner.Rpc
{
    public static class DaemonProcess
    {
        public static void ForceClose()
        {
            ProcessManager.Kill(FileNames.NERVAD);
        }

        public static bool IsRunning()
        {
            Process process = null;
            ProcessManager.IsRunning(FileNames.NERVAD, out process);

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
            string parameters = ProcessManager.GenerateCommandLine(GlobalMethods.GetDaemonPath(), GlobalData.ApplicationSettings.Daemon.Rpc);

            if (GlobalData.ApplicationSettings.Daemon.AutoStartMining)
            {
                string ma = GlobalData.ApplicationSettings.Daemon.MiningAddress;

                Logger.LogDebug("DP.GCL", $"Enabling startup mining @ {ma}");
                parameters += $" --start-mining {ma} --mining-threads {GlobalData.ApplicationSettings.Daemon.MiningThreads}";
            }

#if UNIX
            parameters += " --detach";
#endif

            parameters += $" {GlobalData.ApplicationSettings.Daemon.AdditionalArguments}";

            if (!string.IsNullOrEmpty(extraParams))
            {
                parameters += " " + extraParams;
            }

            return parameters;
        }
    }
}