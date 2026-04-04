namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class UnlockWithPassRequest
    {
        public char[] Password { get; set; } = [];
        public int TimeoutInSeconds { get; set; } = 60;
    }
}