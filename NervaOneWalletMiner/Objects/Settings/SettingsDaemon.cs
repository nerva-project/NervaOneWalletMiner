using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings
{
    public class SettingsDaemon(uint rcpPort)
    {
        public RpcBase Rpc { get; set; } = new RpcBase(rcpPort);

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = false;

        public string MiningAddress { get; set; } = string.Empty;

        public int MiningThreads { get; set; } = 0;

        public string AdditionalArguments { get; set; } = string.Empty;

        public string DataDir { get; set; } = string.Empty;

        public uint LogLevel { get; set; } = 1;

        public bool IsTestnet { get; set; } = false;

        public double BlockSeconds { get; set; } = 0.0;
    }
}