using System.Collections.Generic;
using NervaOneWalletMiner.Objects.DataGrid;

namespace NervaOneWalletMiner.Objects.Stats
{
    public class StatsWallet
    {
        public Dictionary<uint, Account> Subaddresses { get; set; } = [];
        public double TotalBalanceLocked { get; set; } = 0.00;
        public double TotalBalanceUnlocked { get; set; } = 0.00;

    }
}