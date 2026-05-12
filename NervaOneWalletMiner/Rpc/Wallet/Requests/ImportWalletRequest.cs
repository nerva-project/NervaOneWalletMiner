namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class ImportWalletRequest
    {
        public string WalletName { get; set; } = string.Empty;
        public string DumpFileWithPath { get; set; } = string.Empty;
        public char[] Password { get; set; } = [];
    }
}
