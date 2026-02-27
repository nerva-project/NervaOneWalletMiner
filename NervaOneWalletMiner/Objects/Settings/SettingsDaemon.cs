using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings
{
    public class SettingsDaemon
    {
        public bool IsWalletOnly { get; set; } = false;

        public RpcBase Rpc { get; set; } = new RpcBase();

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = false;

        public bool EnableConnectionsGuard { get; set; } = true;

        public string MiningAddress { get; set; } = string.Empty;

        public int MiningThreads { get; set; } = 0;
        
        public bool EnableMiningThreshold { get; set; } = false;

        public int StopMiningThreshold { get; set; } = 500;

        public string AdditionalArguments { get; set; } = string.Empty;

        public string DataDir { get; set; } = string.Empty;

        public int LogLevel { get; set; } = -1;

        public bool IsTestnet { get; set; } = false;

        public bool UseNoAnalyticsFlag { get; set; } = false;
        
        public bool UseNoDnsFlag { get; set; } = false;
    }
}