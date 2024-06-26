using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Objects.Stats
{
    public class StatsDaemon
    {
        public Dictionary<string, Connection> Connections = [];

        public ulong NetHeight { get; set; } = 0;
        public ulong YourHeight { get; set; } = 0;
        public string NetHash { get; set; } = "0 KH/s";
        public string RunTime { get; set; } = "00:00:00";
        public ulong NetHashRate { get; set; } = 0;

        public string MinerStatus { get; set; } = StatusMiner.Inactive;
        public string YourHash { get; set; } = "0 h/s";
        public int YourBlockMinutes { get; set; } = 0;
        public string BlockTime { get; set; } = "∞";
        public string MiningAddress { get; set; } = "None";

        public string Version { get; set; } = "0";
        public ulong ConnectionsIn { get; set; } = 0;
        public ulong ConnectionsOut { get; set; } = 0;
        public string StatusSync { get; set; } = "None";
    }
}