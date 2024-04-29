using NervaWalletMiner.Helpers;

namespace NervaWalletMiner.Objects
{
    public class SettingsWallet
    {
        public SettingsRpc Rpc { get; set; } = new SettingsRpc((uint)GlobalData.RandomGenerator.Next(10000, 50000));

        public int NumTransfersToDisplay { get; set; } = 50;
    }
}