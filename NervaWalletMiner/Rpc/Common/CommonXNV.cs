using NervaWalletMiner.Helpers;

namespace NervaWalletMiner.Rpc.Common
{
    public static class CommonXNV
    {
        public static ServiceError GetServiceError(string source, dynamic error)
        {
            ServiceError serviceError = new ServiceError
            {
                IsError = true,
                Code = error["code"].ToString(),
                Message = error["message"].ToString()
            };

            Logger.LogError("CXNV.GSE", source + " - error from service. Code: " + serviceError.Code + ", Message: " + serviceError.Message);

            return serviceError;
        }
    }
}