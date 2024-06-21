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

        int LogLevelDaemon { get; set; }
        int LogLevelWallet { get; set; }

        string CliWin64Url { get; set; }
        string CliWin32Url { get; set; }
        string CliLin64Url{ get; set; }
        string CliLin32Url { get; set; }
        string CliLinArmUrl { get; set; }
        string CliMacIntelUrl { get; set; }
        string CliMacArmUrl { get; set; }

        string DataDirWin { get; set; }
        string DataDirLin { get; set; }
        string DataDirMac { get; set; }

        string QuickSyncUrl { get; set; }        


        string GenerateWalletOptions(SettingsWallet walletSettings, RpcBase daemonRpc);
        string GenerateDaemonOptions(SettingsDaemon daemonSettings);
    }
}