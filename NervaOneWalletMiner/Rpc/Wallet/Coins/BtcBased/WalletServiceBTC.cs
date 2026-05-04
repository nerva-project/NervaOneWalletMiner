namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Bitcoin implementation as of 4/19/26: https://github.com/bitcoin/bitcoin

    internal class WalletServiceBTC : WalletServiceBaseBTC
    {
        protected override string CoinPrefix => "BTC";
    }
}
