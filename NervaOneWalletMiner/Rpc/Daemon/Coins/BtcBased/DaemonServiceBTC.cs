namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Bitcoin implementation as of 4/19/26: https://github.com/bitcoin/bitcoin
    internal class DaemonServiceBTC : DaemonServiceBaseBTC
    {
        protected override string CoinPrefix => "BTC";
    }
}
