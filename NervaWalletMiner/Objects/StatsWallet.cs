using System.Collections.Generic;

namespace NervaWalletMiner.Objects
{
    public class StatsWallet
    {
        public List<Wallet> Subaddresses { get; set; } = [];
        public double TotalBalanceLocked { get; set; } = 0.00;
        public double TotalBalanceUnlocked { get; set;} = 0.00;

    }
}