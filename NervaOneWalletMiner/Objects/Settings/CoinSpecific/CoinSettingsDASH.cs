using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using System;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    internal class CoinSettingsDASH : ICoinSettings
    {
        #region Private Default Variables
        private int _DaemonPort = 9998;
        private string _DisplayUnits = "DASH";
        private string _WalletExtension = "directory";

        private bool _IsCpuMiningSupported = false;
        private bool _IsDaemonWalletSeparateApp = false;
        private bool _IsSavingWalletSupported = false;
        private bool _IsWalletHeightSupported = false;
        private bool _IsPassRequiredToOpenWallet = false;
        private bool _AreIntegratedAddressesSupported = false;
        private bool _AreKeysDumpedToFile = true;
        private bool _IsPoppingBlocksSupported = false;

        private int _LogLevelDaemon = 0;
        private int _LogLevelWallet = 0;

        private string _CliUrlWindows64 = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-win64.zip";
        private string _CliUrlWindows32 = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-win64.zip";
        private string _CliUrlLinux64 = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-x86_64-linux-gnu.tar.gz";
        private string _CliUrlLinux32 = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-x86_64-linux-gnu.tar.gz";
        private string _CliUrlLinuxArm = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-arm-linux-gnueabihf.tar.gz";
        private string _CliUrlMacIntel = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-x86_64-apple-darwin.tar.gz";
        private string _CliUrlMacArm = "https://github.com/dashpay/dash/releases/download/v20.1.1/dashcore-20.1.1-arm64-apple-darwin.tar.gz";

        private string _DataDirWindows = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");
        private string _DataDirLinux = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");
        private string _DataDirMac = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");

        private string _QuickSyncUrl = string.Empty;
        #endregion // Private Default Variables


        #region Interface Variables
        public int DaemonPort { get => _DaemonPort; set => _DaemonPort = value; }
        public string DisplayUnits { get => _DisplayUnits; set => _DisplayUnits = value; }
        public string WalletExtension { get => _WalletExtension; set => _WalletExtension = value; }

        public bool IsCpuMiningSupported { get => _IsCpuMiningSupported; set => _IsCpuMiningSupported = value; }
        public bool IsDaemonWalletSeparateApp { get => _IsDaemonWalletSeparateApp; set => _IsDaemonWalletSeparateApp = value; }
        public bool IsSavingWalletSupported { get => _IsSavingWalletSupported; set => _IsSavingWalletSupported = value; }
        public bool IsWalletHeightSupported { get => _IsWalletHeightSupported; set => _IsWalletHeightSupported = value; }
        public bool IsPassRequiredToOpenWallet { get => _IsPassRequiredToOpenWallet; set => _IsPassRequiredToOpenWallet = value; }
        public bool AreIntegratedAddressesSupported { get => _AreIntegratedAddressesSupported; set => _AreIntegratedAddressesSupported = value; }
        public bool AreKeysDumpedToFile { get => _AreKeysDumpedToFile; set => _AreKeysDumpedToFile = value; }
        public bool IsPoppingBlocksSupported { get => _IsPoppingBlocksSupported; set => _IsPoppingBlocksSupported = value; }

        public int LogLevelDaemon { get => _LogLevelDaemon; set => _LogLevelDaemon = value; }
        public int LogLevelWallet { get => _LogLevelWallet; set => _LogLevelWallet = value; }

        public string CliUrlWindows64 { get => _CliUrlWindows64; set => _CliUrlWindows64 = value; }
        public string CliUrlWindows32 { get => _CliUrlWindows32; set => _CliUrlWindows32 = value; }
        public string CliUrlLinux64 { get => _CliUrlLinux64; set => _CliUrlLinux64 = value; }
        public string CliUrlLinux32 { get => _CliUrlLinux32; set => _CliUrlLinux32 = value; }
        public string CliUrlLinuxArm { get => _CliUrlLinuxArm; set => _CliUrlLinuxArm = value; }
        public string CliUrlMacIntel { get => _CliUrlMacIntel; set => _CliUrlMacIntel = value; }
        public string CliUrlMacArm { get => _CliUrlMacArm; set => _CliUrlMacArm = value; }

        public string DataDirWindows { get => _DataDirWindows; set => _DataDirWindows = value; }
        public string DataDirLinux { get => _DataDirLinux; set => _DataDirLinux = value; }
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
            daemonCommand += " -walletdir=\"" + GlobalData.WalletDir + "\"";

            if (!string.IsNullOrEmpty(daemonSettings.AdditionalArguments))
            {
                daemonCommand += " " + daemonSettings.AdditionalArguments;
            }

            return daemonCommand;
        }

        public string GenerateWalletOptions(SettingsWallet walletSettings, RpcBase daemonRpc)
        {
            // Should not call this because daemon and wallet are the same process
            throw new NotImplementedException();
        }

        public string GeneratePopBlocksOption(int numberOfBlocks)
        {
            // Popping blocks is not supported
            throw new NotImplementedException();
        }
        #endregion // Interface Methods
    }
}