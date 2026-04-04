using System;

namespace NervaOneWalletMiner.Rpc.Wallet
{
    // Wownero implementation as of 5/10/24: https://git.wownero.com/wownero/wownero
    public class WalletServiceWOW : WalletServiceBaseXMR
    {
        protected override string CoinPrefix => "WOW";

        protected override decimal AmountFromAtomicUnits(ulong value, int decimalPlaces) =>
            Math.Round(Convert.ToDecimal(value / 100000000000.0), decimalPlaces);

        protected override ulong AtomicUnitsFromAmount(decimal amount) =>
            (ulong)(amount * Convert.ToDecimal(100000000000.0));
    }
}
