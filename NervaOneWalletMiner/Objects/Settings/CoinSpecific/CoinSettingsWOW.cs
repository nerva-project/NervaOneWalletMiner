using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using System;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class CoinSettingsWOW : ICoinSettings
    {
        #region Private Default Variables
        private int _DaemonPort = 34568;
        private string _DisplayUnits = "WOW";
        private string _WalletExtension = ".";

        private bool _IsCpuMiningSupported = true;
        private bool _IsDaemonWalletSeparateApp = true;
        private bool _IsSavingWalletSupported = true;
        private bool _IsWalletHeightSupported = true;
        private bool _IsPassRequiredToOpenWallet = true;
        private bool _AreIntegratedAddressesSupported = true;
        private bool _AreKeysDumpedToFile = false;
        private bool _IsPoppingBlocksSupported = true;

        private int _LogLevelDaemon = 0;
        private int _LogLevelWallet = 0;

        private string _CliUrlWindows64 = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-x86_64-w64-mingw32-v0.11.3.0.zip";
        private string _CliUrlWindows32 = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-i686-w64-mingw32-v0.11.3.0.zip";
        private string _CliUrlLinux64 = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-x86_64-linux-gnu-v0.11.3.0.tar.bz2";
        private string _CliUrlLinux32 = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-i686-linux-gnu-v0.11.3.0.tar.bz2";
        private string _CliUrlLinuxArm = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-aarch64-linux-gnu-v0.11.3.0.tar.bz2";
        private string _CliUrlMacIntel = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-x86_64-apple-darwin11-v0.11.3.0.tar.bz2";
        private string _CliUrlMacArm = "https://codeberg.org/wownero/wownero/releases/download/v0.11.3.0/wownero-aarch64-apple-darwin11-v0.11.3.0.tar.bz2";

        private string _PublicNodeUrlDefault = "N/A";

        private string _DataDirWindows = "C:/ProgramData/wownero";
        private string _DataDirLinux = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wownero");
        private string _DataDirMac = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".wownero");

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

        public string PublicNodeUrlDefault { get => _PublicNodeUrlDefault; set => _PublicNodeUrlDefault = value; }

        public string DataDirWindows { get => _DataDirWindows; set => _DataDirWindows = value; }
        public string DataDirLinux { get => _DataDirLinux; set => _DataDirLinux = value; }
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
                Logger.LogDebug("WOW.CGDO", "Connecting to testnet...");
                daemonCommand += " --testnet";
            }

            if (daemonSettings.AutoStartMining)
            {
                string miningAddress = daemonSettings.MiningAddress;
                Logger.LogDebug("WOW.CGDO", "Enabling startup mining @ " + miningAddress);
                daemonCommand += " --start-mining " + miningAddress + " --mining-threads " + daemonSettings.MiningThreads;
            }

            if (GlobalMethods.IsLinux() || GlobalMethods.IsOsx())
            {
                daemonCommand += " --detach";
            }

            if (!string.IsNullOrEmpty(daemonSettings.AdditionalArguments))
            {
                daemonCommand += " " + daemonSettings.AdditionalArguments;
            }

            return daemonCommand;
        }

        public string GenerateWalletOptions(SettingsWallet walletSettings, SettingsDaemon daemonSettings)
        {
            string appCommand = string.Empty;

            if (daemonSettings.IsWalletOnly)
            {
                appCommand = "--daemon-address " + walletSettings.PublicNodeAddress;
            }
            else
            {
                appCommand = "--daemon-address " + daemonSettings.Rpc.Host + ":" + daemonSettings.Rpc.Port;
            }
                
            appCommand += " --rpc-bind-port " + walletSettings.Rpc.Port;
            appCommand += " --disable-rpc-login";
            appCommand += " --wallet-dir \"" + GlobalData.WalletDir + "\"";
            appCommand += " --log-level " + walletSettings.LogLevel;
            appCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetRpcWalletProcess()) + "\"";

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsTestnet)
            {
                Logger.LogDebug("WOW.CGWO", "Connecting to testnet...");
                appCommand += " --testnet";
            }

            return appCommand;
        }

        public string GeneratePopBlocksOption(int numberOfBlocks)
        {
            return "--pop-blocks " + numberOfBlocks;
        }
        #endregion // Interface Methods
    }
}
