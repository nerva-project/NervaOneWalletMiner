using Avalonia.Media.Imaging;
using System.ComponentModel;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Connection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Properties that need to be refreshed
        private string _liveTime = string.Empty;
        public string LiveTime
        {
            get { return _liveTime; }
            set
            {
                if (_liveTime != value)
                {
                    _liveTime = value;
                    OnPropertyChanged(nameof(LiveTime));
                }
            }
        }

        private long _height;
        public long Height
        {
            get { return _height; }
            set
            {
                if(_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private string _state = string.Empty;
        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }


        // Properties that do not need to be refreshed
        public string Address { get; set; } = string.Empty;        
        public bool IsIncoming { get; set; }
        public Bitmap? InOutIcon { get; set; }
    }
}