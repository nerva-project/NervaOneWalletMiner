using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    internal class CoinSettingsDASH : ICoinSettings
    {
        #region Private Default Variables
        private int _DaemonPort = 9998;
        private double _BlockSeconds = 150.0;
        private string _DisplayUnits = "DASH";
        private int _LogLevelDaemon = 0;
        private int _LogLevelWallet = 0;
        private bool _IsCpuMiningPossible = false;

        private string _CliWin64Url = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-win64.zip";
        private string _CliWin32Url = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-win64.zip";
        private string _CliLin64Url = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-x86_64-linux-gnu.tar.gz";
        private string _CliLin32Url = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-x86_64-linux-gnu.tar.gz";
        private string _CliLinArmUrl = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-arm-linux-gnueabihf.tar.gz";
        private string _CliMacIntelUrl = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-x86_64-apple-darwin.tar.gz";
        private string _CliMacArmUrl = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-arm64-apple-darwin.tar.gz";

        private string _DataDirWin = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");
        private string _DataDirLin = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");
        private string _DataDirMac = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");

        private string _QuickSyncUrl = string.Empty;
        #endregion // Private Default Variables


        #region Interface Variables
        public int DaemonPort { get => _DaemonPort; set => _DaemonPort = value; }
        public double BlockSeconds { get => _BlockSeconds; set => _BlockSeconds = value; }
        public string DisplayUnits { get => _DisplayUnits; set => _DisplayUnits = value; }
        public int LogLevelDaemon { get => _LogLevelDaemon; set => _LogLevelDaemon = value; }
        public int LogLevelWallet { get => _LogLevelWallet; set => _LogLevelWallet = value; }
        public bool IsCpuMiningPossible { get => _IsCpuMiningPossible; set => _IsCpuMiningPossible = value; }

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
            string daemonCommand = "-rpcport=" + daemonSettings.Rpc.Port;
            //daemonCommand += " --log-level " + daemonSettings.LogLevel;
            daemonCommand += " -debuglogfile=\"" + GlobalMethods.CycleLogFile(GlobalMethods.GetDaemonProcess()) + "\"";

            if (!string.IsNullOrEmpty(daemonSettings.DataDir))
            {
                daemonCommand += " -datadir=\"" + daemonSettings.DataDir + "\"";
            }

            if (daemonSettings.IsTestnet)
            {
                Logger.LogDebug("DAS.CGDO", "Connecting to testnet...");
                daemonCommand += " -testnet";
            }

            daemonCommand += " -rpcuser=" + daemonSettings.Rpc.UserName + " -rpcpassword=" + daemonSettings.Rpc.Password;

            if (!string.IsNullOrEmpty(daemonSettings.AdditionalArguments))
            {
                daemonCommand += " " + daemonSettings.AdditionalArguments;
            }

            return daemonCommand;
        }

        public string GenerateWalletOptions(SettingsWallet walletSettings, RpcBase daemonRpc)
        {
            // TODO: This is just copied from XNV. Need to change
            string appCommand = "--daemon-address " + daemonRpc.Host + ":" + daemonRpc.Port;
            appCommand += " --rpc-bind-port " + walletSettings.Rpc.Port;
            appCommand += " --disable-rpc-login";
            appCommand += " --wallet-dir \"" + GlobalData.WalletDir + "\"";
            appCommand += " --log-level " + walletSettings.LogLevel;
            appCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetRpcWalletProcess()) + "\"";

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsTestnet)
            {
                Logger.LogDebug("DAS.CGDO", "Connecting to testnet...");
                appCommand += " --testnet";
            }

            return appCommand;
        }
        #endregion // Interface Methods
    }
}