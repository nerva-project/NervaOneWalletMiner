namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public interface ICoinSettings
    {
        double BlockSeconds { get; set; }
        int DaemonPort { get; set; }
        string DisplayName { get; set; }
        string DisplayUnits { get; set; }
        string WalletExtension { get; set; }
        
        // URLs and paths
        string QuickSyncUrl { get; set; }
        string BlockchainDbUrl { get; set; }
        string BlockchainDbSubfolder { get; set; }
        string CliUrlWindows64 { get; set; }
        string CliUrlWindows32 { get; set; }
        string CliUrlLinux64 { get; set; }
        string CliUrlLinux32 { get; set; }
        string CliUrlLinuxArm { get; set; }
        string CliUrlMacIntel { get; set; }
        string CliUrlMacArm { get; set; }
        string CliUrlAndroid { get; set; }

        string RemotePublicNodeUrlDefault { get; set; }
        string LocalPublicNodeArgumentsDefault { get; set; }

        string DataDirWindows { get; set; }
        string DataDirLinux { get; set; }
        string DataDirMac { get; set; }

        // Daemon specific settings
        int LogLevelDaemon { get; set; }
        bool IsCpuMiningSupported { get; set; }
        bool IsPruningSupported { get; set; }
        int DefaultPruneSizeMB { get; set; }
        bool IsDbDownloadSupported { get; set; }
        bool IsWalletOnlySupported { get; set; }
        bool IsQuickSyncSupported { get; set; }
        bool IsPublicNodeSupported { get; set; }
        bool IsAnalyticsFlagSupported { get; set; }
        bool IsDaemonWalletSeparateApp { get; set; }

        // Wallet specific settings
        int LogLevelWallet { get; set; }
        int CoinDecimalPlaces { get; set; }
        int ConfirmationThreshold { get; set; }
        bool IsSavingWalletSupported { get; set; }
        bool IsWalletHeightSupported { get; set; }
        bool IsPassRequiredToOpenWallet { get; set; }
        bool AreIntegratedAddressesSupported { get; set; }
        bool AreKeysDumpedToFile { get; set; }
        bool IsDefaultAddressAutoCreated { get; set; }
        bool IsPaymentIdSupported { get; set; }
        bool IsSplitTransferSupported { get; set; }
        bool IsSendFromSupported { get; set; }
        bool IsPoppingBlocksSupported { get; set; }
        bool IsRestoreFromSeedSupported { get; set; }
        bool IsRestoreFromKeysSupported { get; set; }
        bool IsRestoreFromDumpFileSupported { get; set; }
        bool IsRescanSpentSupported { get; set; }
        bool IsSweepBelowSupported { get; set; }
        bool IsWalletBtcStyle { get; set; }


        string GenerateWalletOptions(SettingsWallet walletSettings, SettingsDaemon daemonSettings);
        string GenerateDaemonOptions(SettingsDaemon daemonSettings);
        string GeneratePopBlocksOption(int numberOfBlocks);
    }
}