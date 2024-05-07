using System.Collections.Generic;
using NervaWalletMiner.Objects.DataGrid;

namespace NervaWalletMiner.Objects.Stats
{
    public class StatsTransfers
    {
        public Dictionary<string, Transfer> Transactions { get; set; } = [];
    }
}