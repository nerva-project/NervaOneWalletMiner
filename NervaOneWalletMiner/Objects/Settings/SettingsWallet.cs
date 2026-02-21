using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings
{
    public class SettingsWallet
    {
        public RpcBase Rpc { get; set; } = new RpcBase();

        public string PublicNodeAddress { get; set; }  = string.Empty;

        public int LogLevel { get; set; } = -1;

        public string DisplayUnits { get; set; } = string.Empty;

        public int UnlockMinutes { get; set; } = 10;
    }
}