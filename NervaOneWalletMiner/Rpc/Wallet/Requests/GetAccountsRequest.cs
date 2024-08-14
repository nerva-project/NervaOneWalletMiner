namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class GetAccountsRequest
    {
        // Do not need those so currently not set or used
        public string Tag { get; set; } = string.Empty;
        public bool StrictBalances { get; set; } = false;
        public string RegEx { get; set; } = string.Empty;
    }
}