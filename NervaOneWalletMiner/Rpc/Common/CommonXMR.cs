using NervaOneWalletMiner.Helpers;
using System;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class CommonXMR
    {
        public static ServiceError GetServiceError(string source, dynamic error)
        {
            ServiceError serviceError = new();

            try
            {
                serviceError.IsError = true;
                serviceError.Code = error["code"].ToString();
                serviceError.Message = error["message"].ToString();

                Logger.LogError("CXMR.GSE", source + " - error from service. Code: " + serviceError.Code + ", Message: " + serviceError.Message);
            }
            catch (Exception ex)
            {
                Logger.LogException("CXMR.GSE", ex);
            }

            return serviceError;
        }

        public static decimal DoubleAmountFromAtomicUnits(ulong balanceAtomic, int decimalPlaces)
        {
            return Math.Round(Convert.ToDecimal(balanceAtomic / 1000000000000.0), decimalPlaces);
        }

        public static ulong AtomicUnitsFromDoubleAmount(decimal amount)
        {
            return (ulong)(amount * Convert.ToDecimal(1000000000000.0));
        }
    }
}