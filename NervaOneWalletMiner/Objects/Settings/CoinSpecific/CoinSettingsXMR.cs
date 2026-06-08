using NervaOneWalletMiner.Helpers;
using System;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class CoinSettingsXMR : ICoinSettings
    {
        public double BlockSeconds { get; set; } = 120.0;
        public int DaemonPort { get; set; } = 18081;
        public string DisplayName { get; set; } = "Monero (XMR)";
        public string DisplayUnits { get; set; } = "XMR";
        public string WalletExtension { get; set; } = ".";
       
        // URLs and paths
        public string QuickSyncUrl { get; set; } = string.Empty;
        public string BlockchainDbUrl { get; set; } = string.Empty;
        public string BlockchainDbSubfolder { get; set; } = "lmdb";

        // https://github.com/monero-project/monero/releases
        public string CliUrlWindows64 { get; set; } = "https://downloads.getmonero.org/cli/monero-win-x64-v0.18.5.0.zip";
        public string CliUrlWindows32 { get; set; } = "https://downloads.getmonero.org/cli/monero-win-x86-v0.18.5.0.zip";
        public string CliUrlLinux64 { get; set; } = "https://downloads.getmonero.org/cli/monero-linux-x64-v0.18.5.0.tar.bz2";
        public string CliUrlLinux32 { get; set; } = "https://downloads.getmonero.org/cli/monero-linux-x86-v0.18.5.0.tar.bz2";
        public string CliUrlLinuxArm { get; set; } = "https://downloads.getmonero.org/cli/monero-linux-armv8-v0.18.5.0.tar.bz2";
        public string CliUrlMacIntel { get; set; } = "https://downloads.getmonero.org/cli/monero-mac-x64-v0.18.5.0.tar.bz2";
        public string CliUrlMacArm { get; set; } = "https://downloads.getmonero.org/cli/monero-mac-armv8-v0.18.5.0.tar.bz2";
        public string CliUrlAndroid { get; set; } = "https://downloads.getmonero.org/cli/monero-android-armv8-v0.18.5.0.tar.bz2";

        public string RemotePublicNodeUrlDefault { get; set; } = "xmr-node.cakewallet.com:18081";
        public string LocalPublicNodeArgumentsDefault { get; set; } = "--rpc-bind-ip 0.0.0.0 --confirm-external-bind --restricted-rpc";

        public string DataDirWindows { get; set; } = "C:/ProgramData/monero";
        public string DataDirLinux { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".monero");
        public string DataDirMac { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".monero");

        // Daemon specific settings
        public int LogLevelDaemon { get; set; } = 0;
        public bool IsCpuMiningSupported { get; set; } = true;
        public bool IsPruningSupported { get; set; } = true;
        public int DefaultPruneSizeMB { get; set; } = 0;
        public bool IsDbDownloadSupported { get; set; } = false;
        public bool IsWalletOnlySupported { get; set; } = true;
        public bool IsQuickSyncSupported { get; set; } = false;
        public bool IsPublicNodeSupported { get; set; } = true;
        public bool IsAnalyticsFlagSupported { get; set; } = false;
        public bool IsDaemonWalletSeparateApp { get; set; } = true;

        // Wallet specific settings
        public int LogLevelWallet { get; set; } = 0;
        public int CoinDecimalPlaces { get; set; } = 6;
        public int ConfirmationThreshold { get; set; } = 20;
        public bool IsSavingWalletSupported { get; set; } = true;
        public bool IsWalletHeightSupported { get; set; } = true;
        public bool IsPassRequiredToOpenWallet { get; set; } = true;
        public bool AreIntegratedAddressesSupported { get; set; } = true;
        public bool AreKeysDumpedToFile { get; set; } = false;
        public bool IsDefaultAddressAutoCreated { get; set; } = false;
        public bool IsPaymentIdSupported { get; set; } = true;
        public bool IsSplitTransferSupported { get; set; } = true;
        public bool IsSendFromSupported { get; set; } = true;
        public bool IsPoppingBlocksSupported { get; set; } = true;
        public bool IsRestoreFromSeedSupported { get; set; } = true;
        public bool IsRestoreFromKeysSupported { get; set; } = true;
        public bool IsRestoreFromDumpFileSupported { get; set; } = false;
        public bool IsRescanSpentSupported { get; set; } = true;
        public bool IsSweepBelowSupported { get; set; } = true;
        public bool IsWalletBtcStyle { get; set; } = false;


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

            if (daemonSettings.AutoStartMining && !string.IsNullOrEmpty(daemonSettings.MiningAddress))
            {
                Logger.LogDebug("XMR.CGDO", "Enabling startup mining @ " + daemonSettings.MiningAddress);
                daemonCommand += " --start-mining " + daemonSettings.MiningAddress + " --mining-threads " + daemonSettings.MiningThreads;
            }

            if (GlobalMethods.IsAndroid() || GlobalMethods.IsLinux() || GlobalMethods.IsOsx())
            {
                daemonCommand += " --detach";
            }

            if (GlobalMethods.IsAndroid())
            {
                daemonCommand += " --rpc-ssl disabled";
            }

            if (daemonSettings.IsPublicNode && !string.IsNullOrEmpty(daemonSettings.PublicNodeArguments))
            {
                daemonCommand += " " + daemonSettings.PublicNodeArguments;
            }

            if (daemonSettings.NodeType == Objects.Constants.NodeType.PrunedNode)
            {
                daemonCommand += " --prune-blockchain";
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

            appCommand += " --rpc-bind-ip 127.0.0.1";
            appCommand += " --rpc-bind-port " + walletSettings.Rpc.Port;
            appCommand += " --rpc-login " + walletSettings.Rpc.UserName + ":" + walletSettings.Rpc.Password;
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

        public string GeneratePopBlocksOption(int numberOfBlocks)
        {
            return "--pop-blocks " + numberOfBlocks;
        }
    }
}
