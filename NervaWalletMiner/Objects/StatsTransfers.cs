using System.Collections.Generic;

namespace NervaWalletMiner.Objects
{
    public class StatsTransfers
    {
        public Dictionary<string, Transfer> Transactions { get; set; } = [];
    }
}