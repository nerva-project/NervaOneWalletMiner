namespace NervaOneWalletMiner.Rpc.Daemon.Requests
{
    public class StopMiningRequest
    {
        public bool EnableMiningThreshold { get; set; } = false;
        public int StopMiningThreshold { get; set; } = 0;
    }
}