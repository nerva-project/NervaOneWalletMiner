using NervaWalletMiner.Objects.DataGrid;
using NervaWalletMiner.Rpc.Common;
using System.Collections.Generic;

namespace NervaWalletMiner.Rpc.Wallet.Responses
{
    public class GetTransfersResponse
    {
        public RpcError Error { get; set; } = new();
        public List<Transfer> Transfers { get; set; } = [];
    }
}