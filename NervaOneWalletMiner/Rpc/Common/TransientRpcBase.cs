using Newtonsoft.Json;

namespace NervaOneWalletMiner.Rpc.Common
{
    // Credentials are excluded from serialization - regenerated fresh each session
    public class TransientRpcBase : RpcBase
    {
        [JsonIgnore]
        public override string UserName { get; set; } = string.Empty;

        [JsonIgnore]
        public override string Password { get; set; } = string.Empty;
    }
}
