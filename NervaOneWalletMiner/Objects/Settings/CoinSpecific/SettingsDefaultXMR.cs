namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class SettingsDefaultXMR : ISettingsDefault
    {
        protected uint _DaemonPort = 18081;
        protected double _BlockSeconds = 120.0;
        protected string _DisplayUnits = "XMR";
        protected uint _LogLevelDaemon = 0;
        protected uint _LogLevelWallet = 0;

        public uint DaemonPort { get => _DaemonPort; set => _DaemonPort = value; }
        public double BlockSeconds { get => _BlockSeconds; set => _BlockSeconds = value; }
        public string DisplayUnits { get => _DisplayUnits; set => _DisplayUnits = value; }
        public uint LogLevelDaemon { get => _LogLevelDaemon; set => _LogLevelDaemon = value; }
        public uint LogLevelWallet { get => _LogLevelWallet; set => _LogLevelWallet = value; }
    }
}