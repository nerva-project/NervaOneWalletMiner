using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class CoinSettingsXMR : ICoinSettings
    {
        #region Private Default Variables
        private int _DaemonPort = 18081;
        private string _DisplayUnits = "XMR";
        private string _WalletExtension = ".";

        private bool _IsCpuMiningPossible = true;
        private bool _IsDaemonWalletSeparateApp = true;
        private bool _IsSavingWalletSupported = true;
        private bool _IsWalletHeightSupported = true;

        private int _LogLevelDaemon = 0;
        private int _LogLevelWallet = 0;

        private string _CliWin64Url = "https://downloads.getmonero.org/cli/monero-win-x64-v0.18.3.3.zip";
        private string _CliWin32Url = "https://downloads.getmonero.org/cli/monero-win-x86-v0.18.3.3.zip";
        private string _CliLin64Url = "https://downloads.getmonero.org/cli/monero-linux-x64-v0.18.3.3.tar.bz2";
        private string _CliLin32Url = "https://downloads.getmonero.org/cli/monero-linux-x86-v0.18.3.3.tar.bz2";
        private string _CliLinArmUrl = "https://downloads.getmonero.org/cli/monero-linux-armv7-v0.18.3.3.tar.bz2";
        private string _CliMacIntelUrl = "https://downloads.getmonero.org/cli/monero-mac-x64-v0.18.3.3.tar.bz2";
        private string _CliMacArmUrl = "https://downloads.getmonero.org/cli/monero-mac-armv8-v0.18.3.3.tar.bz2";

        private string _DataDirWin = "C:/ProgramData/monero";
        private string _DataDirLin = "~/.monero";
        private string _DataDirMac = "~/.monero";

        private string _QuickSyncUrl = string.Empty;
        #endregion // Private Default Variables


        #region Interface Variables
        public int DaemonPort { get => _DaemonPort; set => _DaemonPort = value; }
        public string DisplayUnits { get => _DisplayUnits; set => _DisplayUnits = value; }
        public string WalletExtension { get => _WalletExtension; set => _WalletExtension = value; }

        public bool IsCpuMiningPossible { get => _IsCpuMiningPossible; set => _IsCpuMiningPossible = value; }
        public bool IsDaemonWalletSeparateApp { get => _IsDaemonWalletSeparateApp; set => _IsDaemonWalletSeparateApp = value; }
        public bool IsSavingWalletSupported { get => _IsSavingWalletSupported; set => _IsSavingWalletSupported = value; }
        public bool IsWalletHeightSupported { get => _IsWalletHeightSupported; set => _IsWalletHeightSupported = value; }

        public int LogLevelDaemon { get => _LogLevelDaemon; set => _LogLevelDaemon = value; }
        public int LogLevelWallet { get => _LogLevelWallet; set => _LogLevelWallet = value; }

        public string CliWin64Url { get => _CliWin64Url; set => _CliWin64Url = value; }
        public string CliWin32Url { get => _CliWin32Url; set => _CliWin32Url = value; }
        public string CliLin64Url { get => _CliLin64Url; set => _CliLin64Url = value; }
        public string CliLin32Url { get => _CliLin32Url; set => _CliLin32Url = value; }
        public string CliLinArmUrl { get => _CliLinArmUrl; set => _CliLinArmUrl = value; }
        public string CliMacIntelUrl { get => _CliMacIntelUrl; set => _CliMacIntelUrl = value; }
        public string CliMacArmUrl { get => _CliMacArmUrl; set => _CliMacArmUrl = value; }

        public string DataDirWin { get => _DataDirWin; set => _DataDirWin = value; }
        public string DataDirLin { get => _DataDirLin; set => _DataDirLin = value; }
        public string DataDirMac { get => _DataDirMac; set => _DataDirMac = value; }

        public string QuickSyncUrl { get => _QuickSyncUrl; set => _QuickSyncUrl = value; }
        #endregion // Interface Variables

        #region Interface Methods
        public string GenerateDaemonOptions(SettingsDaemon daemonSettings)
        {
            string daemonCommand = "--rpc-bind-port " + daemonSettings.Rpc.Port;
            daemonCommand += " --log-level " + daemonSettings.LogLevel;
            daemonCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetDaemonProcess()) + "\"";

            if (!string.IsNullOrEmpty(daemonSettings.DataDir))
            {
                daemonCommand += " --data-dir \"" + daemonSettings.DataDir + "\"";
            }

            if (daemonSettings.IsTestnet)
            {
                Logger.LogDebug("XMR.CGDO", "Connecting to testnet...");
                daemonCommand += " --testnet";
            }

            if (daemonSettings.AutoStartMining)
            {
                string miningAddress = daemonSettings.MiningAddress;
                Logger.LogDebug("XMR.CGDO", "Enabling startup mining @ " + miningAddress);
                daemonCommand += " --start-mining " + miningAddress + " --mining-threads " + daemonSettings.MiningThreads;
            }

            if (GlobalMethods.IsLinux())
            {
                daemonCommand += " --detach";
            }

            if (!string.IsNullOrEmpty(daemonSettings.AdditionalArguments))
            {
                daemonCommand += " " + daemonSettings.AdditionalArguments;
            }

            return daemonCommand;
        }

        public string GenerateWalletOptions(SettingsWallet walletSettings, RpcBase daemonRpc)
        {
            string appCommand = "--daemon-address " + daemonRpc.Host + ":" + daemonRpc.Port;
            appCommand += " --rpc-bind-port " + walletSettings.Rpc.Port;
            appCommand += " --disable-rpc-login";
            appCommand += " --wallet-dir \"" + GlobalData.WalletDir + "\"";
            appCommand += " --log-level " + walletSettings.LogLevel;
            appCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetRpcWalletProcess()) + "\"";

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsTestnet)
            {
                Logger.LogDebug("XMR.CGWO", "Connecting to testnet...");
                appCommand += " --testnet";
            }

            return appCommand;
        }
        #endregion // Interface Methods
    }
}