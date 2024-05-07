namespace NervaWalletMiner.Objects.Settings
{
    public class AppSettings
    {
        public SettingsDaemon Daemon = new(false);
        public SettingsWallet Wallet = new();

        public SettingsMisc Misc = new();
    }
}