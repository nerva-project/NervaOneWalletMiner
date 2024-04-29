namespace NervaWalletMiner.Objects
{
    public class AppSettings
    {
        public string MiningAddress { get; set; } = "";
        public int MiningThreads { get; set; } = 0;
        public string DaemonAddress { get; set; } = "127.0.0.1";
        public string DaemonPort { get; set; } = "17566";
        public bool AutoStartMining { get; set; } = false;
        public string AdditionalDaemonArguments { get; set; } = string.Empty;
        public bool IsTestnet { get; set; } = false;
    }
}