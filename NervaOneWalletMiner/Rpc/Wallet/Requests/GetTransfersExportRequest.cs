using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class GetTransfersExportRequest
    {
        public uint AccountIndex { get; set; }
        public List<uint> SubaddressIndices { get; set; } = [];
        public bool IsAllAccounts { get; set; }
    }
}