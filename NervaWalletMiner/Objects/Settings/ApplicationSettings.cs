using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using System.Collections.Generic;

namespace NervaWalletMiner.Objects.Settings
{
    public class ApplicationSettings
    {
        // Coin specific settings
        public Dictionary<string, SettingsDaemon> Daemon = GlobalMethods.GetDaemonSettings();
        public Dictionary<string, SettingsWallet> Wallet = GlobalMethods.GetWalletSettings();
        public Dictionary<string, SettingsMisc> Misc = GlobalMethods.GetMiscSettings();

        // Common settings
        public string ActiveCoin = Coin.XNV;
        public int TimerIntervalMultiplier { get; set; } = 5;
    }
}