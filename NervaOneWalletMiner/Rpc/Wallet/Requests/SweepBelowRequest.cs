namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class SweepBelowRequest
    {
        public double Amount { get; set; } = 0.5;
        public string WalletAddress { get; set; } = string.Empty;
    }
}