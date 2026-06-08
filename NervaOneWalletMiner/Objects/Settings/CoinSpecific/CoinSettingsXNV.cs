using NervaOneWalletMiner.Helpers;
using System;
using System.IO;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class CoinSettingsXNV : ICoinSettings
    {
        public double BlockSeconds { get; set; } = 60.0;
        public int DaemonPort { get; set; } = 17566;
        public string DisplayName { get; set; } = "Nerva (XNV)";
        public string DisplayUnits { get; set; } = "XNV";
        public string WalletExtension { get; set; } = ".cache";

        // URLs and paths
        public string QuickSyncUrl { get; set; } = "https://nerva.one/quicksync/quicksync.raw";
        public string BlockchainDbUrl { get; set; } = "https://nerva.one/database/nerva_blockchain_db.zip";
        public string BlockchainDbSubfolder { get; set; } = "lmdb";

        // https://github.com/nerva-project/nerva/releases
        public string CliUrlWindows64 { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-windows-x64-v0.2.2.0.zip";
        public string CliUrlWindows32 { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-windows-x32-v0.2.2.0.zip";
        public string CliUrlLinux64 { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-linux-x86_64-v0.2.2.0.tar.bz2";
        public string CliUrlLinux32 { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-linux-i686-v0.2.2.0.tar.bz2";
        public string CliUrlLinuxArm { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-linux-armv8-v0.2.2.0.tar.bz2";
        public string CliUrlMacIntel { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-macos-x64-v0.2.2.0.tar.bz2";
        public string CliUrlMacArm { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-macos-armv8-v0.2.2.0.tar.bz2";
        public string CliUrlAndroid { get; set; } = "https://github.com/nerva-project/nerva/releases/download/v0.2.2.0/nerva-android-armv8-v0.2.2.0.tar.bz2";

        public string RemotePublicNodeUrlDefault { get; set; } = "node.nerva.one:17566";
        public string LocalPublicNodeArgumentsDefault { get; set; } = "--rpc-bind-ip 0.0.0.0 --confirm-external-bind";

        public string DataDirWindows { get; set; } = "C:/ProgramData/nerva";
        public string DataDirLinux { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nerva");
        public string DataDirMac { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nerva");

        // Daemon specific settings
        public int LogLevelDaemon { get; set; } = 1;
        public bool IsCpuMiningSupported { get; set; } = true;
        public bool IsPruningSupported { get; set; } = false;
        public int DefaultPruneSizeMB { get; set; } = 0;
        public bool IsDbDownloadSupported { get; set; } = true;
        public bool IsWalletOnlySupported { get; set; } = true;
        public bool IsQuickSyncSupported { get; set; } = true;
        public bool IsPublicNodeSupported { get; set; } = true;
        public bool IsAnalyticsFlagSupported { get; set; } = true;
        public bool IsDaemonWalletSeparateApp { get; set; } = true;

        // Wallet specific settings
        public int LogLevelWallet { get; set; } = 1;
        public int CoinDecimalPlaces { get; set; } = 4;
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
                Logger.LogDebug("XNV.CGDO", "Connecting to testnet...");
                daemonCommand += " --testnet";
            }

            if (daemonSettings.AutoStartMining && !string.IsNullOrEmpty(daemonSettings.MiningAddress))
            {
                Logger.LogDebug("XNV.CGDO", "Enabling startup mining @ " + daemonSettings.MiningAddress);
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

            if (daemonSettings.UseNoAnalyticsFlag)
            {
                daemonCommand += " --no-analytics";
            }

            if (daemonSettings.UseNoDnsFlag)
            {
                daemonCommand += " --no-dns";
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
                Logger.LogDebug("XNV.CGWO", "Connecting to testnet...");
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
