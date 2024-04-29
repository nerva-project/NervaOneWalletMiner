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
            string parameters = ProcessManager.GenerateCommandLine(GlobalMethods.GetDaemonPath(), GlobalData.DaemonSettings.Rpc);

            if (GlobalData.ApplicationSettings.AutoStartMining)
            {
                string ma = GlobalData.ApplicationSettings.MiningAddress;

                Logger.LogDebug("DP.GCL", $"Enabling startup mining @ {ma}");
                parameters += $" --start-mining {ma} --mining-threads {GlobalData.ApplicationSettings.MiningThreads}";
            }

#if UNIX
            parameters += " --detach";
#endif

            parameters += $" {GlobalData.ApplicationSettings.AdditionalDaemonArguments}";

            if (!string.IsNullOrEmpty(extraParams))
            {
                parameters += " " + extraParams;
            }

            return parameters;
        }
    }
}