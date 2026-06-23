using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using System;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    internal class CoinSettingsDASH : ICoinSettings
    {
        public double BlockSeconds { get; set; } = 150.0;
        public int DaemonPort { get; set; } = 9998;
        public string DisplayName { get; set; } = "Dash (DASH)";
        public string DisplayUnits { get; set; } = "DASH";
        public string WalletExtension { get; set; } = "directory";        

        // URLs and paths
        public string QuickSyncUrl { get; set; } = string.Empty;
        public string BlockchainDbUrl { get; set; } = string.Empty;
        public string BlockchainDbSubfolder { get; set; } = string.Empty;

        // https://github.com/dashpay/dash/releases
        public string CliUrlWindows64 { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-win64.zip";
        public string CliUrlWindows32 { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-win64.zip";
        public string CliUrlLinux64 { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-x86_64-linux-gnu.tar.gz";
        public string CliUrlLinux32 { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-x86_64-linux-gnu.tar.gz";
        public string CliUrlLinuxArm { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-aarch64-linux-gnu.tar.gz";
        public string CliUrlMacIntel { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-x86_64-apple-darwin.tar.gz";
        public string CliUrlMacArm { get; set; } = "https://github.com/dashpay/dash/releases/download/v23.1.4/dashcore-23.1.4-arm64-apple-darwin.tar.gz";
        public string CliUrlAndroid { get; set; } = string.Empty;

        public string RemotePublicNodeUrlDefault { get; set; } = string.Empty;
        public string LocalPublicNodeArgumentsDefault { get; set; } = string.Empty;

        public string DataDirWindows { get; set; } = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");
        public string DataDirLinux { get; set; } = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");
        public string DataDirMac { get; set; } = Path.Combine(GlobalMethods.GetDataDir(), "DashCore");

        // Daemon specific settings
        public int LogLevelDaemon { get; set; } = 0;
        public bool IsCpuMiningSupported { get; set; } = false;
        public bool IsPruningSupported { get; set; } = true;
        public int DefaultPruneSizeMB { get; set; } = 20480;
        public bool IsDbDownloadSupported { get; set; } = false;
        public bool IsWalletOnlySupported { get; set; } = false;
        public bool IsQuickSyncSupported { get; set; } = false;
        public bool IsPublicNodeSupported { get; set; } = false;
        public bool IsAnalyticsFlagSupported { get; set; } = false;
        public bool IsDaemonWalletSeparateApp { get; set; } = false;

        // Wallet specific settings
        public int LogLevelWallet { get; set; } = 0;
        public int CoinDecimalPlaces { get; set; } = 8;
        public int ConfirmationThreshold { get; set; } = 10;
        public bool IsSavingWalletSupported { get; set; } = false;
        public bool IsWalletHeightSupported { get; set; } = false;
        public bool IsPassRequiredToOpenWallet { get; set; } = false;
        public bool AreIntegratedAddressesSupported { get; set; } = false;
        public bool AreKeysDumpedToFile { get; set; } = true;
        public bool IsDefaultAddressAutoCreated { get; set; } = true;
        public bool IsPaymentIdSupported { get; set; } = false;
        public bool IsSplitTransferSupported { get; set; } = false;
        public bool IsSendFromSupported { get; set; } = false;
        public bool IsPoppingBlocksSupported { get; set; } = false;
        public bool IsRestoreFromSeedSupported { get; set; } = true;
        public bool IsRestoreFromKeysSupported { get; set; } = false;
        public bool IsRestoreFromDumpFileSupported { get; set; } = true;
        public bool IsRescanSpentSupported { get; set; } = false;
        public bool IsSweepBelowSupported { get; set; } = false;
        public bool IsWalletBtcStyle { get; set; } = true;


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

            if (daemonSettings.NodeType == NodeType.PrunedNode)
            {
                daemonCommand += " -prune=" + daemonSettings.PruneSizeMB;
            }

            if (!string.IsNullOrEmpty(daemonSettings.AdditionalArguments))
            {
                daemonCommand += " " + daemonSettings.AdditionalArguments;
            }

            return daemonCommand;
        }

        public string GenerateWalletOptions(SettingsWallet walletSettings, SettingsDaemon daemonSettings)
        {
            // Should not call this because daemon and wallet are the same process
            throw new NotImplementedException();
        }

        public string GeneratePopBlocksOption(int numberOfBlocks)
        {
            // Popping blocks is not supported
            throw new NotImplementedException();
        }
    }
}
