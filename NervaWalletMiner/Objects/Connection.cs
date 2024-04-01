namespace NervaWalletMiner.Objects
{
    public class Connection
    {
        public string? Address { get; set; }
        public ulong Height { get; set; }
        public string? LiveTime { get; set; }
        public string? State { get; set; }
        public bool IsIncoming { get; set; }
    }
}