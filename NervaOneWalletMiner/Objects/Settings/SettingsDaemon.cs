﻿using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Objects.Settings
{
    public class SettingsDaemon
    {
        public RpcBase Rpc { get; set; } = new RpcBase();

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = false;

        public string MiningAddress { get; set; } = string.Empty;

        public int MiningThreads { get; set; } = 0;

        public string AdditionalArguments { get; set; } = string.Empty;

        public string DataDir { get; set; } = string.Empty;

        public int LogLevel { get; set; } = -1;

        public bool IsTestnet { get; set; } = false;
    }
}