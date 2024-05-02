namespace NervaWalletMiner.Objects
{
    public class StatsDaemon
    {
        public ulong NetHeight { get; set; } = 0;
        public ulong YourHeight { get; set; } = 0;
        public string NetHash { get; set; } = "0 kH/s";
        public string RunTime { get; set; } = "00:00:00";
        public ulong Difficulty { get; set; } = 0;

        public string MinerStatus { get; set; } = NervaWalletMiner.Objects.StatusMiner.Inactive;
        public string YourHash { get; set; } = "0 h/s";
        public string BlockTime { get; set; } = "∞";
        public string MiningAddress { get; set; } = "None";

        public string Version { get; set; } = "0";
        public ulong ConnectionsIn { get; set; } = 0;
        public ulong ConnectionsOut { get; set; } = 0;
        public string StatusSync { get; set; } = "None";
    }
}