namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class LabelAccountRequest
    {
        public uint AccountIndex { get; set; } = 0;
        public string Label { get; set; } = string.Empty;
    }
}