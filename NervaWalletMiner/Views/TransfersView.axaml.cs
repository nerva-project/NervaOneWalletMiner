using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using System;

namespace NervaWalletMiner.Views
{
    public partial class TransfersView : UserControl
    {
        public TransfersView()
        {
            InitializeComponent();
        }

        public void OpenCloseWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                // TODO: Implement this in a different way so it works for both Wallet and Transfers screen
            }
            catch (Exception ex)
            {
                Logger.LogException("Trans.SSMC", ex);
            }
        }
    }
}