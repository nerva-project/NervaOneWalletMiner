namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Wownero implementation as of 5/10/24: https://git.wownero.com/wownero/wownero
    public class DaemonServiceWOW : DaemonServiceBaseXMR
    {
        protected override string CoinPrefix => "WOW";
        protected override double BlockSeconds => 300.0;
    }
}
