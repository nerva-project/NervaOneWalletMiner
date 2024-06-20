using NervaOneWalletMiner.Objects.Constants;

namespace NervaOneWalletMiner.Rpc.Common
{
    public static class CommonDASH
    {
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
