using NervaWalletMiner.Rpc.Common;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using System.Threading.Tasks;

namespace NervaWalletMiner.Rpc.Wallet
{
    public interface IWalletService
    {
        Task<OpenWalletResponse> OpenWallet(RpcSettings rpc, OpenWalletRequest requestObj);
    }
}