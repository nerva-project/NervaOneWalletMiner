namespace NervaOneWalletMiner.Rpc.Daemon.Requests
{
    public class StartMiningRequest
    {
        public string MiningAddress { get; set; } = string.Empty;
        public int ThreadCount { get; set; } = 1;
        
        public bool EnableMiningThreshold { get; set;} = false;
        public int StopMiningThreshold { get; set; } = 0;
    }
}