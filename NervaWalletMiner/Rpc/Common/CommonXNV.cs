using System;
using NervaWalletMiner.Helpers;

namespace NervaWalletMiner.Rpc.Common
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
    }
}