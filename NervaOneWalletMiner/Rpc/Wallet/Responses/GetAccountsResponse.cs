using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetAccountsResponse
    {
        public ServiceError Error { get; set; } = new();

        public decimal BalanceUnlocked { get; set; }
        public decimal BalanceTotal { get; set; }
        public List<Account> SubAccounts { get; set; } = [];
    }
}