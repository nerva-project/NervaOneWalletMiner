using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Objects.Settings
{
    public class SettingsWallet
    {
        public RpcBase Rpc { get; set; } = new RpcBase((uint)GlobalData.RandomGenerator.Next(10000, 50000));

        public uint LogLevel { get; set; } = 1;

        public string DisplayUnits { get; set; } = "XNV";

        public int NumTransfersToDisplay { get; set; } = 50;
    }
}