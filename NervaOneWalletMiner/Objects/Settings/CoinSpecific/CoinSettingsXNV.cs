using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Common;
using System;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class CoinSettingsXNV : ICoinSettings
    {
        #region Private Default Variables
        private int _DaemonPort = 17566;
        private string _DisplayUnits = "XNV";
        private string _WalletExtension = ".cache";

        private bool _IsCpuMiningSupported = true;
        private bool _IsDaemonWalletSeparateApp = true;
        private bool _IsSavingWalletSupported = true;
        private bool _IsWalletHeightSupported = true;
        private bool _IsPassRequiredToOpenWallet = true;
        private bool _AreIntegratedAddressesSupported = true;
        private bool _AreKeysDumpedToFile = false;
        private bool _IsPoppingBlocksSupported = true;

        private int _LogLevelDaemon = 1;
        private int _LogLevelWallet = 1;

        private string _CliUrlWindows64 = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-windows-x64-v0.2.0.0.zip";
        private string _CliUrlWindows32 = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-windows-x32-v0.2.0.0.zip";
        private string _CliUrlLinux64 = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-linux-x86_64-v0.2.0.0.tar.bz2";
        private string _CliUrlLinux32 = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-linux-i686-v0.2.0.0.tar.bz2";
        private string _CliUrlLinuxArm = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-linux-armv7-v0.2.0.0.tar.bz2";
        private string _CliUrlMacIntel = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-macos-x64-v0.2.0.0.tar.bz2";
        private string _CliUrlMacArm = "https://github.com/nerva-project/nerva/releases/download/v0.2.0.0/nerva-macos-armv8-v0.2.0.0.tar.bz2";

        private string _DataDirWindows = "C:/ProgramData/nerva";
        private string _DataDirLinux = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nerva");
        private string _DataDirMac = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nerva");

        private string _QuickSyncUrl = "https://nerva.one/quicksync/quicksync.raw";        
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
            string daemonCommand = "--rpc-bind-port " + daemonSettings.Rpc.Port;
            daemonCommand += " --log-level " + daemonSettings.LogLevel;
            daemonCommand += " --log-file \"" + GlobalMethods.CycleLogFile(GlobalMethods.GetDaemonProcess()) + "\"";

            if (!string.IsNullOrEmpty(daemonSettings.DataDir))
            {
                daemonCommand += " --data-dir \"" + daemonSettings.DataDir + "\"";
            }

            if (daemonSettings.IsTestnet)
            {
                Logger.LogDebug("XNV.CGDO", "Connecting to testnet...");
                daemonCommand += " --testnet";
            }

            if (daemonSettings.AutoStartMining)
            {
                string miningAddress = daemonSettings.MiningAddress;
                Logger.LogDebug("XNV.CGDO", "Enabling startup mining @ " + miningAddress);
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
                Logger.LogDebug("CGDO.CGWO", "Connecting to testnet...");
                appCommand += " --testnet";
            }

            // TODO: Uncomment to enable rpc user:pass.
            // string ip = d.IsPublic ? $" --rpc-bind-ip 0.0.0.0 --confirm-external-bind" : $" --rpc-bind-ip 127.0.0.1";
            // appCommand += $"{ip} --rpc-login {d.Login}:{d.Pass}";

            return appCommand;
        }

        public string GeneratePopBlocksOption(int numberOfBlocks)
        {
            return "--pop-blocks " + numberOfBlocks;
        }
        #endregion // Interface Methods
    }
}