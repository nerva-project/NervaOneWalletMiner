using System.Collections.Generic;
using NervaOneWalletMiner.Objects.DataGrid;

namespace NervaOneWalletMiner.Objects.Stats
{
    public class StatsWallet
    {
        // This is cleared every time UI updates so cannot rely on this for status bar info such as number of accounts
        public Dictionary<uint, Account> Subaddresses { get; set; } = [];

        // UI still relies on those fields for updates
        public decimal BalanceTotal { get; set; } = 0;
        public decimal BalanceUnlocked { get; set; } = 0;

    }
}