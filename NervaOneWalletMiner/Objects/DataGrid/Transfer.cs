using Avalonia.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Transfer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void Notify([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // -1 means that given coin does not use Account Indexes
        public int AccountIndex { get; set; } = -1;
        public string TransactionId { get; set; } = string.Empty;
        public string AddressShort { get; set; } = string.Empty;
        public string AddressLabel { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.MinValue;
        public decimal Amount { get; set; } = 0;
        public string Type { get; set; } = string.Empty;
        public string BlockHash { get; set; } = string.Empty;
        public Bitmap? Icon { get; set; }


        private ulong _height = 0;
        public ulong Height
        {
            get => _height;
            set { if (_height != value) { _height = value; Notify(); } }
        }

        private string _heightDisplay = string.Empty;
        public string HeightDisplay
        {
            get => _heightDisplay;
            set { if (_heightDisplay != value) { _heightDisplay = value; Notify(); } }
        }

        private long _confirmations = 0;
        public long Confirmations
        {
            get => _confirmations;
            set { if (_confirmations != value) { _confirmations = value; Notify(); } }
        }
    }
}
