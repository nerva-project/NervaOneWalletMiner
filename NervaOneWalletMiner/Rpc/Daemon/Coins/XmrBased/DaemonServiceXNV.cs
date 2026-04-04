namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Nerva implementation as of 5/10/24: https://github.com/nerva-project/nerva
    public class DaemonServiceXNV : DaemonServiceBaseXMR
    {
        protected override string CoinPrefix => "XNV";
        protected override double BlockSeconds => 60.0;
    }
}
