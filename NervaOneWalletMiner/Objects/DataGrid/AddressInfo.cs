namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class AddressInfo
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
    }
}