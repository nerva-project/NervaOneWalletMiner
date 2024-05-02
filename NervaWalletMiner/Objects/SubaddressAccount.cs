namespace NervaWalletMiner.Objects
{
    public class SubaddressAccount
    {
        public int Index { get; set; }
        public ulong BalanceLocked { get; set; }
        public ulong BalanceUnlocked { get; set; }
        public string? Address { get; set; }
        public string? Label { get; set; }
        public string? Tag { get; set; }        
    }
}