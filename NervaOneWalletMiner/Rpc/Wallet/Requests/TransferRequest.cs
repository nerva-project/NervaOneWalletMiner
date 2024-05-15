using NervaOneWalletMiner.Rpc.Common;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Rpc.Wallet.Requests
{
    public class TransferRequest
    {
        public List<TransferDestination> Destinations { get; set; } = [];
        public uint AccountIndex { get; set; } = 0;
        public List<uint> SubAddressIndices { get; set; } = [];
        public uint Priority { get; set; } = 0;
        public ulong UnlockTime { get; set; } = 0;
        public string PaymentId { get; set; } = "";
        public bool GetTxKey { get; set; } = true;
        public bool DoNotRelay { get; set; } = false;
        public bool GetTxHex { get; set; } = false;
        public bool GetTxMetadata { get; set;} = false;

        // XMR specific
        public List<uint> SubtractFeeFromOutputs { get; set; } = [];
        public ulong ring_size { get; set; } = 0;
    }
}