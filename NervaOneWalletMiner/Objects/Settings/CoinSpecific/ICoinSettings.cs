namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public interface ICoinSettings
    {
        uint DaemonPort { get; set; }
        double BlockSeconds { get; set; }
        string DisplayUnits { get; set; }
        uint LogLevelDaemon { get; set; }
        uint LogLevelWallet { get; set; }

        string CliWin64Url { get; set; }
        string CliWin32Url { get; set; }
        string CliLin64Url{ get; set; }
        string CliLin32Url { get; set; }
        string CliLinArmUrl { get; set; }
        string CliMacIntelUrl { get; set; }
        string CliMacArmUrl { get; set; }

        string QuickSyncUrl { get; set; }
    }
}