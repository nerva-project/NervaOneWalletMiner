using NervaWalletMiner.Objects.DataGrid;
using NervaWalletMiner.Rpc.Common;
using System.Collections.Generic;

namespace NervaWalletMiner.Rpc.Wallet.Responses
{
    public class GetAccountsResponse
    {
        public ServiceError Error { get; set; } = new();

        public double BalanceUnlocked { get; set; }
        public double BalanceLocked { get; set; }
        public List<Account> SubAccounts { get; set; } = [];
    }
}