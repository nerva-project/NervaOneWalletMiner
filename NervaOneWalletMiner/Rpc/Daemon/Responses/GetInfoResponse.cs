using NervaOneWalletMiner.Rpc.Common;
using System;

namespace NervaOneWalletMiner.Rpc.Daemon.Responses
{
    public class GetInfoResponse
    {
        public ServiceError Error { get; set; } = new();

        public ulong Height { get; set; }
        public ulong TargetHeight { get; set; }
        public ulong NetworkHashRate { get; set; }
        public ulong ConnectionCountOut { get; set; }
        public ulong ConnectionCountIn { get; set; }
        public DateTime StartTime { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}