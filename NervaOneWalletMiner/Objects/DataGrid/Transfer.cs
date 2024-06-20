using Avalonia.Media.Imaging;
using System;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Transfer
    {
        // -1 means that given coin does not use Account Indexes
        public int AccountIndex { get; set; } = -1;
        public string TransactionId { get; set; } = string.Empty;
        public string AddressShort { get; set; } = string.Empty;
        public string AddressLabel { get; set; } = string.Empty;
        public ulong Height { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public decimal Amount { get; set; } = 0;
        public string Type { get; set; } = string.Empty;
        public string BlockHash { get; set; } = string.Empty;
        public Bitmap? Icon { get; set; }
    }
}