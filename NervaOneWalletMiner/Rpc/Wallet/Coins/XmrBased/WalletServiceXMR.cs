namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Monero implementation as of 5/13/24: https://github.com/monero-project/monero
    public class WalletServiceXMR : WalletServiceBaseXMR
    {
        protected override string CoinPrefix => "XMR";
    }
}
