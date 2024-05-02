namespace NervaWalletMiner.Objects
{
    public class RpcError
    {
        public bool IsError { get; set; } = true;
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}