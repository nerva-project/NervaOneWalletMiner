using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings
{
    public class SettingsWallet
    {
        public RpcBase Rpc { get; set; } = new RpcBase();

        public uint LogLevel { get; set; } = 0;

        public string DisplayUnits { get; set; } = string.Empty;
    }
}