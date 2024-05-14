using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects.Constants;
using System;

namespace NervaWalletMiner.Views
{
    public partial class WalletSetupView : UserControl
    {
        public WalletSetupView()
        {
            InitializeComponent();
        }

        public void CreateWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

            }
            catch (Exception ex)
            {
                Logger.LogException("Hom.SSMC", ex);
            }
        }
    }
}