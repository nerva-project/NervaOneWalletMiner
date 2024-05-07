using NervaWalletMiner.Rpc.Common;

namespace NervaWalletMiner.Rpc.Wallet.Responses
{
    public class OpenWalletResponse
    {
        public RpcError Error { get; set; } = new();
    }
}