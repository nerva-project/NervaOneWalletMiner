using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Objects.Settings;
using System.Diagnostics;
using System.Runtime;

namespace NervaWalletMiner.Rpc
{
    public static class DaemonProcess
    {
        public static void ForceClose()
        {
            ProcessManager.Kill(GlobalData.DaemonProcessName);
        }

        public static bool IsRunning()
        {
            ProcessManager.IsRunning(GlobalData.DaemonProcessName, out Process? process);

            if (process != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GenerateOptions(SettingsDaemon daemonSettings)
        {
            // TODO: Need to make this coin specific when adding coins that use different startup options

            string daemonCommand = "--rpc-bind-port " + daemonSettings.Rpc.Port;
            daemonCommand += " --log-level " + daemonSettings.LogLevel;
            daemonCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetDaemonProcess()) + "\"";
            
            if (!string.IsNullOrEmpty(daemonSettings.DataDir))
            {
                daemonCommand += " --data-dir \"" + daemonSettings.DataDir + "\"";
            }

            if (daemonSettings.IsTestnet)
            {
                Logger.LogDebug("PM.GCL", "Connecting to testnet...");
                daemonCommand += " --testnet";
            }

            if (daemonSettings.AutoStartMining)
            {
                string miningAddress = daemonSettings.MiningAddress;
                Logger.LogDebug("DP.GCL", "Enabling startup mining @ " + miningAddress);
                daemonCommand += " --start-mining " + miningAddress + " --mining-threads " + daemonSettings.MiningThreads;
            }

            if(GlobalMethods.IsLinux())
            {
                daemonCommand += " --detach";
            }

            if(!string.IsNullOrEmpty(daemonSettings.AdditionalArguments))
            {
                daemonCommand += " " + daemonSettings.AdditionalArguments;
            }          

            return daemonCommand;
        }
    }
}