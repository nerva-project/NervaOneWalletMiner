namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Litecoin implementation as of 5/29/26: https://github.com/litecoin-project/litecoin

    internal class WalletServiceLTC : WalletServiceBaseBTC
    {
        protected override string CoinPrefix => "LTC";
        protected override string CoinName => "Litecoin";
        protected override uint CoinType => 2;
        protected override bool SupportMultipleScriptTypes => true;
        protected override bool SupportTaproot => false;
        protected override bool UseDescriptorWallet => false;
    }
}
