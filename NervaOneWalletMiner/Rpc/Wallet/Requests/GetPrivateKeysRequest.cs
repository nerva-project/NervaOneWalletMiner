namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class GetPrivateKeysRequest
    {
        public string KeyType { get; set; } = string.Empty;
        public string DumpFileWithPath { get; set; } = string.Empty;
    }
}