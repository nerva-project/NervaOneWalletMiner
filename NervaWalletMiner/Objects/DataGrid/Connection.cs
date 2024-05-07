using Avalonia.Media.Imaging;

namespace NervaWalletMiner.Objects.DataGrid
{
    public class Connection
    {
        public string? Address { get; set; }
        public ulong Height { get; set; }
        public string? LiveTime { get; set; }
        public string? State { get; set; }
        public bool IsIncoming { get; set; }
        public Bitmap? InOutIcon { get; set; }
    }
}