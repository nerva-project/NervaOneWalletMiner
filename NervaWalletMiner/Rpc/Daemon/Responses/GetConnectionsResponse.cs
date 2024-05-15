using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Rpc.Daemon.Responses
{
    public class GetConnectionsResponse
    {
        public ServiceError Error { get; set; } = new();

        public List<Connection> Connections { get; set; } = [];
    }
}