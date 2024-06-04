using System;
using NervaOneWalletMiner.Helpers;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class CommonXNV
    {
        public static ServiceError GetServiceError(string source, dynamic error)
        {
            ServiceError serviceError = new();

            try
            {
                serviceError.IsError = true;
                serviceError.Code = error["code"].ToString();
                serviceError.Message = error["message"].ToString();

                Logger.LogError("CXNV.GSE", source + " - error from service. Code: " + serviceError.Code + ", Message: " + serviceError.Message);
            }
            catch (Exception ex)
            {
                Logger.LogException("HTTP.GHE", ex);
            }

            return serviceError;
        }

        public static uint GetPriority(string stringPriority)
        {
            uint priority = 0;

            switch(stringPriority)
            {
                case SendPriority.Low:
                    priority = 1;
                    break;
                case SendPriority.Medium:
                    priority = 2;
                    break;
                case SendPriority.High:
                    priority = 3;
                    break;
                default:
                    priority = 0;
                    break;

            }
            return priority;
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