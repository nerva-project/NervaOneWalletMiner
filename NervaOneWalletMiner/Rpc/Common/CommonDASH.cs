using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using System;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class CommonDASH
    {
        public static ServiceError GetServiceError(string source, dynamic error)
        {
            ServiceError serviceError = new();

            try
            {
                serviceError.IsError = true;
                serviceError.Code = error["code"].ToString();
                serviceError.Message = error["message"].ToString();

                Logger.LogError("DAS.CGSE", source + " - error from service. Code: " + serviceError.Code + ", Message: " + serviceError.Message);
            }
            catch (Exception ex)
            {
                Logger.LogException("DAS.CGSE", ex);
            }

            return serviceError;
        }

        public static string GetTransactionType(string type)
        {
            string returnType = TransferType.Unknown;

            switch (type.ToLower())
            {
                case "receive":
                    returnType = TransferType.In;
                    break;
                case "send":
                    returnType = TransferType.Out;
                    break;
                case "coinjoin":
                    returnType = TransferType.Out;
                    break;
                case "generate":
                    returnType = TransferType.Block;
                    break;
                case "immature":
                    returnType = TransferType.Block;
                    break;
                default:
                    returnType = TransferType.Unknown;
                    break;
            }

            return returnType;
        }
    }
}
