namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Wownero implementation as of 5/10/24: https://git.wownero.com/wownero/wownero
    public class WalletServiceWOW : WalletServiceBaseXMR
    {
        protected override string CoinPrefix => "WOW";
        protected override decimal CoinAtomicUnits => 100_000_000_000m;
    }
}
