using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Helpers;
using System.ComponentModel;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Account : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Properties that need to be refreshed
        private string _label = string.Empty;
        public string Label
        {
            get { return _label; }
            set
            {
                if (_label != value)
                {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                    OnPropertyChanged(nameof(DisplayLabel));
                }
            }
        }

        public string DisplayLabel => string.IsNullOrWhiteSpace(Label) || Label == "Untitled account"
            ? AddressShort
            : Label;

        private decimal _balanceTotal;
        public decimal BalanceTotal
        {
            get { return _balanceTotal; }
            set
            {
                if(_balanceTotal != value)
                {
                    _balanceTotal = value;
                    OnPropertyChanged(nameof(BalanceTotal));
                    OnPropertyChanged(nameof(BalanceTotalDisplay));
                }
            }
        }

        public string BalanceTotalDisplay => GlobalMethods.FormatAmount(BalanceTotal);

        private decimal _balanceUnlocked;
        public decimal BalanceUnlocked
        {
            get { return _balanceUnlocked; }
            set
            {
                if(_balanceUnlocked != value)
                {
                    _balanceUnlocked = value;
                    OnPropertyChanged(nameof(BalanceUnlocked));
                    OnPropertyChanged(nameof(BalanceUnlockedDisplay));
                }
            }
        }

        public string BalanceUnlockedDisplay => GlobalMethods.FormatAmount(BalanceUnlocked);


        // Properties that do not need to be refreshed
        public uint Index { get; set; }
        public string AddressFull { get; set; } = string.Empty;
        public string AddressShort { get; set; } = string.Empty;                       
        public Bitmap? WalletIcon { get; set; }
    }
}