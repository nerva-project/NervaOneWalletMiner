namespace NervaOneWalletMiner.Rpc.Daemon.Requests
{
    public class StartMiningRequest
    {
        public string MiningAddress { get; set; } = string.Empty;
        public int ThreadCount { get; set; } = 1;       
    }
}