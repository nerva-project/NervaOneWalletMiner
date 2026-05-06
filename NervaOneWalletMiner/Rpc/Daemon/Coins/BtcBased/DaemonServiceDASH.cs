namespace NervaOneWalletMiner.Rpc.Daemon
{
    // Dash implementation as of 6/18/24: https://github.com/dashpay/dash
    internal class DaemonServiceDASH : DaemonServiceBaseBTC
    {
        protected override string CoinPrefix => "DAS";
    }
}
