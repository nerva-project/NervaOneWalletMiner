using Avalonia.Media.Imaging;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Connection
    {
        public string? Address { get; set; }
        public long Height { get; set; }
        public string? LiveTime { get; set; }
        public string? State { get; set; }
        public bool IsIncoming { get; set; }
        public Bitmap? InOutIcon { get; set; }
    }
}