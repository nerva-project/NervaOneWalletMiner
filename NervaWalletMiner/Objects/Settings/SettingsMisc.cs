using NervaWalletMiner.Objects.Constants;

namespace NervaWalletMiner.Objects.Settings
{
    public class SettingsMisc
    {
        public string ActiveCoin { get; set; } = Coin.XNV;
        public int TimerIntervalMultiplier { get; set; } = 5;
    }
}