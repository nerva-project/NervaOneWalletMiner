using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class GetTransfersRequest
    {
        public bool IncludeIn { get; set; }
        public bool IncludeOut { get; set; }
        public bool IncludePending { get; set; }
        public bool IncludeFailed { get; set; }
        public bool IncludePool { get; set; }
        public bool IsFilterByHeight { get; set; }
        public ulong MinHeight { get; set; }
        public uint AccountIndex { get; set; }
        public List<uint> SubaddressIndices { get; set; } = [];
        public bool IsAllAccounts { get; set; }

        public string SinceBlockHash { get; set; } = string.Empty;
    }
}