namespace NervaWalletMiner.Objects
{
    public class DaemonSettings
    {
        public RpcDetails Rpc { get; set; } = new RpcDetails(17566);

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = false;

        public string MiningAddress { get; set; } = string.Empty;

        public int MiningThreads { get; set; } = 0;

        public string AdditionalArguments { get; set; } = string.Empty;
    }
}