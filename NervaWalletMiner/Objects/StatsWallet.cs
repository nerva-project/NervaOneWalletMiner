using System.Collections.Generic;

namespace NervaWalletMiner.Objects
{
    public class StatsWallet
    {
        public List<SubaddressAccount> Subaddresses { get; set; } = [];
        public ulong TotalBalanceLocked { get; set; } = 0;
        public ulong TotalBalanceUnlocked { get; set;} = 0;

    }
}