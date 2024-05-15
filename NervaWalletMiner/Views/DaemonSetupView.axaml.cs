using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaWalletMiner.Helpers;
using System;

namespace NervaWalletMiner.Views
{
    public partial class DaemonSetupView : UserControl
    {
        public DaemonSetupView()
        {
            InitializeComponent();
        }

        public void CheckForUpdatesClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                // TODO: Implement in the future

            }
            catch (Exception ex)
            {
                Logger.LogException("WalS.CWC", ex);
            }
        }
    }
}