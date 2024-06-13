using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Objects.Settings
{
    public class ApplicationSettings
    {
        // Coin specific settings
        public Dictionary<string, SettingsDaemon> Daemon = [];
        public Dictionary<string, SettingsWallet> Wallet = [];

        // Common settings
        public string ActiveCoin { get; set; } = Coin.XNV;
        public string Theme { get; set; } = "Dark";
        public int TimerIntervalMultiplier { get; set; } = 5;
    }
}