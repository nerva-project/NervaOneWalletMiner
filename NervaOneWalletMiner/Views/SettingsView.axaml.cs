using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class SettingsView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public SettingsView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                var cbxThemeVariants = this.Get<ComboBox>("cbxThemeVariants");
                cbxThemeVariants.SelectedItem = Application.Current!.RequestedThemeVariant;
                cbxThemeVariants.SelectionChanged += (sender, e) =>
                {
                    if (cbxThemeVariants.SelectedItem is ThemeVariant themeVariant)
                    {
                        Application.Current!.RequestedThemeVariant = themeVariant;
                    }
                };

                // TODO: Should probably come up with a simpler way to do this
                ComboBoxItem selectedCoin = (ComboBoxItem)cbxCoin.Items[0]!;
                foreach (ComboBoxItem? coin in cbxCoin.Items)
                {
                    if (coin!.Name!.Equals(GlobalData.AppSettings.ActiveCoin))
                    {
                        selectedCoin = coin;
                    }
                }
                cbxCoin.SelectedValue = selectedCoin;
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.CONS", ex);
            }            
        }

        public async void SaveSettings_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;

                if (cbxThemeVariants.SelectedItem is ThemeVariant themeVariant)
                {
                    if(GlobalData.AppSettings.Theme != themeVariant.Key.ToString())
                    {
                        GlobalData.AppSettings.Theme = themeVariant.Key.ToString()!;
                        isChanged = true;
                    }
                }

                string selectedCoin = ((ComboBoxItem)cbxCoin.SelectedItem!).Name!;
                if (!selectedCoin.Equals(GlobalData.AppSettings.ActiveCoin))
                {
                    // We're switching to a different coin so need to clean up
                    if(GlobalData.IsWalletOpen)
                    {
                        Logger.LogDebug("SET.SSCL", "Closing wallet: " + GlobalData.OpenedWalletName);
                        await ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).CloseWalletNonUi();
                    }
                   
                    Logger.LogDebug("SET.SSCL", "Calling wallet ForceClose");
                    WalletProcess.ForceClose();                   

                    isChanged = true;
                    GlobalMethods.SetCoin(selectedCoin);

                    if (!GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                    {
                        // CLI tools missing. Need to download
                        string cliToolsLink = GlobalMethods.GetCliToolsDownloadLink();
                        Logger.LogDebug("SET.SSCL", "CLI tools not found. Asking user to confirm download link: " + cliToolsLink);
                        
                        var window = new TextBoxView("Get Client Tools", cliToolsLink, string.Empty, "Client Tools Download Link", true);
                        await window.ShowDialog(GetWindow()).ContinueWith(CliToolsLinkDialogClosed);
                    }
                }

                if(isChanged)
                {
                    imgCoinIcon.Source = GlobalMethods.GetLogo();
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.SSCL", ex);
            }
        }

        private async void CliToolsLinkDialogClosed(Task task)
        {
            try
            {
                DialogResult result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk)
                {
                    Logger.LogDebug("SET.CTLC", "Attempting to download CLI tools from: " + result.TextBoxValue);
                    if (!string.IsNullOrEmpty(result.TextBoxValue))
                    {
                        // Download and extract CLI tools
                        GlobalMethods.SetUpCliTools(result.TextBoxValue, GlobalData.CliToolsDir);
                    }
                }
                else
                {
                    Logger.LogDebug("SET.CTLC", "CLI tools download cancelled");
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Client Tools Missing", "NervaOne cannot run without client tools. Switch coin or restart to download client tools. "
                            + "Alternatively you can put your own client tools in Daemon Setup > Open Client Tools Folder", true);
                        await window.ShowDialog(GetWindow());
                    });

                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.CTLC", ex);
            }
        }
    }
}