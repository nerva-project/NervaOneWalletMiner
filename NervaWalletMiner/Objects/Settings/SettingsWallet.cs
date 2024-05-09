using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Objects.Settings
{
    public class SettingsWallet
    {
        public RpcSettings Rpc { get; set; } = new RpcSettings((uint)GlobalData.RandomGenerator.Next(10000, 50000));

        public string WalletProcessName { get; set; } = GlobalMethods.IsWindows() ? "nerva-wallet-rpc.exe" : "nerva-wallet-rpc";        

        public int NumTransfersToDisplay { get; set; } = 50;
    }
}