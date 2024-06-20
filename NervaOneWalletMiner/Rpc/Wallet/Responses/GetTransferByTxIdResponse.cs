using NervaOneWalletMiner.Rpc.Common;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Wallet.Responses
{
    public class GetTransferByTxIdResponse
    {
        public ServiceError Error { get; set; } = new();

        public string Address { get; set; } = string.Empty;
        public List<string> Destinations { get; set; } = [];
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public ulong Height { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public decimal Amount { get; set; } = 0;
        public decimal Fee { get; set; } = 0;
        public string Note { get; set; } = string.Empty;           
        public long Confirmations { get; set; } = 0;
    }
}