using NervaOneWalletMiner.Rpc.Common;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetExportResponse
    {
        public ServiceError Error { get; set; } = new();

        public string ExportString { get; set; } = string.Empty;
    }
}