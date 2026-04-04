namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Monero implementation as of 5/10/24: https://github.com/monero-project/monero
    public class DaemonServiceXMR : DaemonServiceBaseXMR
    {
        protected override string CoinPrefix => "XMR";
        protected override double BlockSeconds => 120.0;
    }
}
