using NervaWalletMiner.Objects.DataGrid;
using NervaWalletMiner.Rpc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Daemon.Responses
{
    public class GetConnectionsResponse
    {
        public ServiceError Error { get; set; } = new();

        public List<Connection> Connections { get; set; } = [];
    }
}