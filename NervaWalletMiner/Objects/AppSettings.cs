namespace NervaWalletMiner.Objects
{
    public class AppSettings
    {
        public SettingsDaemon Daemon = new(false);
        public SettingsWallet Wallet = new();
    }
}