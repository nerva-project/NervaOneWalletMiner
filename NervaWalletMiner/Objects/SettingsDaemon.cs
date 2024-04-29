namespace NervaWalletMiner.Objects
{
    public class SettingsDaemon(bool isTestnet)
    {
        public SettingsRpc Rpc { get; set; } = new SettingsRpc(17566);

        public bool StopOnExit { get; set; } = false;

        public bool AutoStartMining { get; set; } = false;

        public string MiningAddress { get; set; } = string.Empty;

        public int MiningThreads { get; set; } = 0;

        public string AdditionalArguments { get; set; } = string.Empty;
        
        public bool IsTestnet {  get; set; } = isTestnet;
    }
}