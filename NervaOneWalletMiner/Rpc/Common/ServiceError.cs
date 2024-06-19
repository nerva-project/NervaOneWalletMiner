namespace NervaOneWalletMiner.Rpc.Common
{
    public class ServiceError
    {
        public bool IsError { get; set; } = true;
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? Content { get; set; }
    }
}