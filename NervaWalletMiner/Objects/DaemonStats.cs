namespace NervaWalletMiner.Objects
{
    public class DaemonStats
    {
        public ulong NetHeight { get; set; }
        public ulong YourHeight { get; set; }
        public string? NetHash { get; set; }
        public string? RunTime { get; set; }
        public ulong ConnectionsIn { get; set; }
        public ulong ConnectionsOut { get; set; }
        public string? StatusSync { get; set; }
        public string? Version { get; set; }
    }
}