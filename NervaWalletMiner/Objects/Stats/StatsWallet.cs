using System.Collections.Generic;
using NervaWalletMiner.Objects.DataGrid;

namespace NervaWalletMiner.Objects.Stats
{
    public class StatsWallet
    {
        public Dictionary<uint, Account> Subaddresses { get; set; } = [];
        public double TotalBalanceLocked { get; set; } = 0.00;
        public double TotalBalanceUnlocked { get; set; } = 0.00;

    }
}