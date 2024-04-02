using NervaWalletMiner.Objects;
using System;
using System.Collections.Generic;

namespace NervaWalletMiner.Helpers
{
    public static class GlobalData
    {
        public const string AppName = "NervaWalletMiner";
        public const string LogsDirName = "logs";
        public const string WalletDirName = "wallets";
        public const string Version = "0.5.0";

        public static readonly string DataDir = GlobalMethods.GetDataDir();
        public static readonly string LogDir = GlobalMethods.GetLogDir();
        public static readonly string WalletDir = GlobalMethods.GetWalletDir();

        public static DaemonStats NetworkStats = new();        
        public static List<Connection> Connections = new List<Connection>();

        public static int CpuThreadCount = Environment.ProcessorCount;
        public static int DefaultMiningThreads = CpuThreadCount > 1 ? Convert.ToInt32(Math.Floor(CpuThreadCount / 2.00)) : 1;
    }
}