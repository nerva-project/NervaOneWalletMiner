using NervaWalletMiner.Rpc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet.Responses
{
    public class TransferResponse
    {
        public ServiceError Error { get; set; } = new();

    }
}
