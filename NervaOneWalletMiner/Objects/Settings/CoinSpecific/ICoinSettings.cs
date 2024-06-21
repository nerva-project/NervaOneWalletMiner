using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public interface ICoinSettings
    {
        int DaemonPort { get; set; }
        string DisplayUnits { get; set; }
        string WalletExtension { get; set; }

        bool IsCpuMiningSupported { get; set; }
        bool IsDaemonWalletSeparateApp { get; set; }
        bool IsSavingWalletSupported { get; set; }
        bool IsWalletHeightSupported { get; set; }
        bool IsPassRequiredToOpenWallet { get; set; }
        bool AreIntegratedAddressesSupported { get; set; }
        bool AreKeysDumpedToFile {  get; set; }

        int LogLevelDaemon { get; set; }
        int LogLevelWallet { get; set; }

        string CliUrlWindows64 { get; set; }
        string CliUrlWindows32 { get; set; }
        string CliUrlLinux64 { get; set; }
        string CliUrlLinux32 { get; set; }
        string CliUrlLinuxArm { get; set; }
        string CliUrlMacIntel { get; set; }
        string CliUrlMacArm { get; set; }

        string DataDirWindows { get; set; }
        string DataDirLinux { get; set; }
        string DataDirMac { get; set; }

        string QuickSyncUrl { get; set; }


        string GenerateWalletOptions(SettingsWallet walletSettings, RpcBase daemonRpc);
        string GenerateDaemonOptions(SettingsDaemon daemonSettings);
    }
}