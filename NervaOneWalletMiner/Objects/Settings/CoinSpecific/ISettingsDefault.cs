namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public interface ISettingsDefault
    {
        uint DaemonPort { get; set; }
        double BlockSeconds { get; set; }
        string DisplayUnits { get; set; }
        uint LogLevelDaemon { get; set; }
        uint LogLevelWallet { get; set; }
    }
}