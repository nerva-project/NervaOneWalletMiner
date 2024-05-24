﻿using Avalonia.Media.Imaging;

namespace NervaOneWalletMiner.Objects.DataGrid
{
    public class Account
    {
        public uint Index { get; set; }
        public string Label { get; set; } = string.Empty;
        public string AddressFull { get; set; } = string.Empty;
        public string AddressShort { get; set; } = string.Empty;        
        public double BalanceLocked { get; set; }
        public double BalanceUnlocked { get; set; }
        public Bitmap? WalletIcon { get; set; }
    }
}