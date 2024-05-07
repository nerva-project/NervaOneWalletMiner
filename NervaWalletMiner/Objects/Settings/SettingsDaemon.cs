using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Objects.Settings
{
    public class SettingsDaemon(bool isTestnet)
    {
        public RpcSettings Rpc { get; set; } = new RpcSettings(17566);

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = false;

        public string MiningAddress { get; set; } = string.Empty;

        public int MiningThreads { get; set; } = 0;

        public string AdditionalArguments { get; set; } = string.Empty;

        public bool IsTestnet { get; set; } = isTestnet;
    }
}