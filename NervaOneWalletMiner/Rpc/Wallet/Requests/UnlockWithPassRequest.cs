namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class UnlockWithPassRequest
    {
        public string Password { get; set; } = string.Empty;
        public int TimeoutInSeconds { get; set; } = 60;
    }
}