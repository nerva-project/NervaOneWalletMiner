using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class CoinSetupView : UserControl
    {
        public CoinSetupView()
        {
            try
            {
                InitializeComponent();

                imgCoinIcon.Source = GlobalMethods.GetLogo();
                tbxCliDownloadUrl.Text = GlobalMethods.GetCliToolsDownloadLink(GlobalData.AppSettings.ActiveCoin);
                tbxRemoteNode.Text = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].RemotePublicNodeUrlDefault;
                PrunedNode.IsVisible = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPruningSupported;

                Loaded += (_, _) => tbxCliDownloadUrl.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("CSV.CONS", ex);
            }
        }

        private void NodeType_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is RadioButton { IsChecked: true } radioButton)
                {
                    spRemoteNode.IsVisible = radioButton.Name == NodeType.WalletOnly;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("CSV.NTCC", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(tbxCliDownloadUrl.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Coin Setup", "Client Tools Download URL is required.", true));
                    return;
                }

                if (WalletOnly.IsChecked == true && string.IsNullOrEmpty(tbxRemoteNode.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Coin Setup", "Remote Node is required.", true));
                    return;
                }

                string nodeType = PrunedNode.IsChecked == true ? NodeType.PrunedNode
                    : WalletOnly.IsChecked == true ? NodeType.WalletOnly
                    : NodeType.FullNode;

                GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress = tbxRemoteNode.Text!.Trim();
                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].NodeType = nodeType;
                GlobalMethods.SaveConfig();
                GlobalMethods.ShowHideDaemonTab();

                Logger.LogDebug("CSV.OKBC", "Attempting to download CLI tools from: " + tbxCliDownloadUrl.Text);
                GlobalData.DaemonState = DaemonState.Downloading;
                GlobalMethods.SetUpCliTools(tbxCliDownloadUrl.Text!, GlobalData.CliToolsDir);

                UIManager.NavigateToDefaultPage();
            }
            catch (Exception ex)
            {
                Logger.LogException("CSV.OKBC", ex);
            }
        }

        public async void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("CSV.CNCL", "Coin Setup cancelled.");
                GlobalData.DaemonState = DaemonState.CliToolsMissing;

                await DialogService.ShowAsync(new MessageBoxView("Client Tools Missing",
                    "NervaOne cannot run without client tools. Switch coin or restart to download client tools. "
                    + "Alternatively you can put your own client tools in Daemon Setup > Open Client Tools Folder", true));

                UIManager.NavigateToDefaultPage();
            }
            catch (Exception ex)
            {
                Logger.LogException("CSV.CNCL", ex);
            }
        }
    }
}
