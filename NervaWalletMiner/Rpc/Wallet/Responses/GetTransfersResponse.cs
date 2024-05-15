using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetTransfersResponse
    {
        public ServiceError Error { get; set; } = new();

        public List<Transfer> Transfers { get; set; } = [];
    }
}