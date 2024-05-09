using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace NervaWalletMiner.Objects.Settings
{
    public class SettingsMisc
    {
        public Bitmap Logo { get; set; } = new Bitmap(AssetLoader.Open(new Uri("avares://NervaWalletMiner/Assets/xnv/logo.png")));        
    }
}