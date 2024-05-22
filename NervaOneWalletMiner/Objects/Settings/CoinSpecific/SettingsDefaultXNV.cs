namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class SettingsDefaultXNV : ISettingsDefault
    {
        protected uint _DaemonPort = 17566;
        protected double _BlockSeconds = 60.0;
        protected string _DisplayUnits = "XNV";
        protected uint _LogLevelDaemon = 1;
        protected uint _LogLevelWallet = 1;

        public uint DaemonPort { get => _DaemonPort; set => _DaemonPort = value; }
        public double BlockSeconds { get => _BlockSeconds; set => _BlockSeconds = value; }
        public string DisplayUnits { get => _DisplayUnits; set => _DisplayUnits = value; }
        public uint LogLevelDaemon { get => _LogLevelDaemon; set => _LogLevelDaemon = value; }
        public uint LogLevelWallet { get => _LogLevelWallet; set => _LogLevelWallet = value; }
    }
}