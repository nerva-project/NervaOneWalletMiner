namespace NervaOneWalletMiner.Objects.Settings.CoinSpecific
{
    public class CoinSettingsXMR : ICoinSettings
    {
        #region Private Default Variables
        private int _DaemonPort = 18081;
        private double _BlockSeconds = 120.0;
        private string _DisplayUnits = "XMR";
        private int _LogLevelDaemon = 0;
        private int _LogLevelWallet = 0;

        private string _CliWin64Url = "https://downloads.getmonero.org/cli/monero-win-x64-v0.18.3.3.zip";
        private string _CliWin32Url = "https://downloads.getmonero.org/cli/monero-win-x86-v0.18.3.3.zip";
        private string _CliLin64Url = "https://downloads.getmonero.org/cli/monero-linux-x64-v0.18.3.3.tar.bz2";
        private string _CliLin32Url = "https://downloads.getmonero.org/cli/monero-linux-x86-v0.18.3.3.tar.bz2";
        private string _CliLinArmUrl = "https://downloads.getmonero.org/cli/monero-linux-armv7-v0.18.3.3.tar.bz2";
        private string _CliMacIntelUrl = "https://downloads.getmonero.org/cli/monero-mac-x64-v0.18.3.3.tar.bz2";
        private string _CliMacArmUrl = "https://downloads.getmonero.org/cli/monero-mac-armv8-v0.18.3.3.tar.bz2";

        private string _DataDirWin = "C:/ProgramData/monero";
        private string _DataDirLin = "~/.monero";
        private string _DataDirMac = "~/.monero";

        private string _QuickSyncUrl = string.Empty;
        #endregion // Private Default Variables


        #region Interface Implementation
        public int DaemonPort { get => _DaemonPort; set => _DaemonPort = value; }
        public double BlockSeconds { get => _BlockSeconds; set => _BlockSeconds = value; }
        public string DisplayUnits { get => _DisplayUnits; set => _DisplayUnits = value; }
        public int LogLevelDaemon { get => _LogLevelDaemon; set => _LogLevelDaemon = value; }
        public int LogLevelWallet { get => _LogLevelWallet; set => _LogLevelWallet = value; }

        public string CliWin64Url { get => _CliWin64Url; set => _CliWin64Url = value; }
        public string CliWin32Url { get => _CliWin32Url; set => _CliWin32Url = value; }
        public string CliLin64Url { get => _CliLin64Url; set => _CliLin64Url = value; }
        public string CliLin32Url { get => _CliLin32Url; set => _CliLin32Url = value; }
        public string CliLinArmUrl { get => _CliLinArmUrl; set => _CliLinArmUrl = value; }
        public string CliMacIntelUrl { get => _CliMacIntelUrl; set => _CliMacIntelUrl = value; }
        public string CliMacArmUrl { get => _CliMacArmUrl; set => _CliMacArmUrl = value; }

        public string DataDirWin { get => _DataDirWin; set => _DataDirWin = value; }
        public string DataDirLin { get => _DataDirLin; set => _DataDirLin = value; }
        public string DataDirMac { get => _DataDirMac; set => _DataDirMac = value; }

        public string QuickSyncUrl { get => _QuickSyncUrl; set => _QuickSyncUrl = value; }
        #endregion // Interface Implementation
    }
}