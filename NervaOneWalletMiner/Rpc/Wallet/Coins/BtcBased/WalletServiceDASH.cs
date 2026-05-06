namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Dash implementation as of 6/18/24: https://github.com/dashpay/dash

    internal class WalletServiceDASH : WalletServiceBaseBTC
    {
        protected override string CoinPrefix => "DAS";
        protected override string CoinName => "Dash";
        protected override uint CoinType => 5;
        protected override bool SupportMultipleScriptTypes => false;
    }
}
