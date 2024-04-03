namespace NervaWalletMiner.Objects
{
    public class AppSettings
    {
        public string MiningAddress { get; set; } = "";
        public string DaemonAddress { get; set; } = "127.0.0.1";
        public string DaemonPort { get; set; } = "17566";
    }
}