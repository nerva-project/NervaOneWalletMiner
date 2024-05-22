﻿using Avalonia.Media.Imaging;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using ReactiveUI;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewModels
{
    internal class TransfersViewModel : ViewModelBase
    {
        // TODO: Figure out how to do this in one place instead of on each view
        private Bitmap _CoinIcon = GlobalData.Logo;
        public Bitmap CoinIcon
        {
            get => _CoinIcon;
            set => this.RaiseAndSetIfChanged(ref _CoinIcon, value);
        }

        private string _OpenCloseWallet = StatusWallet.OpenWallet;
        public string OpenCloseWallet
        {
            get => _OpenCloseWallet;
            set => this.RaiseAndSetIfChanged(ref _OpenCloseWallet, value);
        }

        private List<Transfer> _Transactions = new();
        public List<Transfer> Transactions
        {
            get => _Transactions;
            set => this.RaiseAndSetIfChanged(ref _Transactions, value);
        }
    }
}