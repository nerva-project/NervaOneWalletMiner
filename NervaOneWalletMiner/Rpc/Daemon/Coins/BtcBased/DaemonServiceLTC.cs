namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Litecoin implementation as of 5/29/26: https://github.com/litecoin-project/litecoin

    internal class DaemonServiceLTC : DaemonServiceBaseBTC
    {
        protected override string CoinPrefix => "LTC";
    }
}
