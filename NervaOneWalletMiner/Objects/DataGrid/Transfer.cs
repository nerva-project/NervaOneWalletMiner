using Avalonia.Media.Imaging;
using System;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Transfer
    {
        public uint AccountIndex { get; set; } = 0;
        public string TransactionId { get; set; } = string.Empty;
        public string TransactionIdShort { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public ulong Height { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public decimal Amount { get; set; } = 0;
        public string Type { get; set; } = string.Empty;
        public Bitmap? Icon { get; set; }
    }
}