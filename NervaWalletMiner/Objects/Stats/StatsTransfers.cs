using System.Collections.Generic;
using NervaOneWalletMiner.Objects.DataGrid;

namespace NervaOneWalletMiner.Objects.Stats
{
    public class StatsTransfers
    {
        public Dictionary<string, Transfer> Transactions { get; set; } = [];
    }
}