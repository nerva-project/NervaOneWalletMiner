using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using System.Collections.Generic;

namespace NervaWalletMiner.Objects.Settings
{
    public class AppSettings
    {
        public Dictionary<string, SettingsDaemon> Daemon = GlobalMethods.GetDaemonSettings();
        public Dictionary<string, SettingsWallet> Wallet = GlobalMethods.GetWalletSettings();

        public SettingsMisc Misc = new();

        public string ActiveCoin = Coin.XNV;
    }
}