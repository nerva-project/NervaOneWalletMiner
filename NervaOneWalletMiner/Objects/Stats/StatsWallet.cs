using System.Collections.Generic;
using NervaOneWalletMiner.Objects.DataGrid;

namespace NervaOneWalletMiner.Objects.Stats
{
    public class StatsWallet
    {
        public Dictionary<uint, Account> Subaddresses { get; set; } = [];

        public decimal BalanceTotal { get; set; } = 0;
        public decimal BalanceUnlocked { get; set; } = 0;

        // Used for BTC based coins
        public decimal BalancePending { get; set; } = 0;

    }
}