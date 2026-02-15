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

                // Name of coin as define in Settings axaml needs to equal to ActiveCoin
                ComboBoxItem selectedCoin = (ComboBoxItem)cbxCoin.Items[0]!;
                foreach (ComboBoxItem? coin in cbxCoin.Items)
                {
                    if (coin!.Name!.Equals(GlobalData.AppSettings.ActiveCoin))
                    {
                        selectedCoin = coin;
                    }
                }
                
                var savedThreshold = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold;
                if (savedThreshold > 0)
                {
                    cbxThresholdEnabled.IsChecked = true;
                    hashThreshold.IsEnabled = true;
                    hashThreshold.Value = savedThreshold;
                }
                else
                {
                    cbxThresholdEnabled.IsChecked = false;
                    hashThreshold.IsEnabled = false;
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

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsDaemonWalletSeparateApp)
                    {
                        Logger.LogDebug("SET.SSCL", "Calling wallet ForceClose");
                        ProcessManager.Kill(GlobalData.WalletProcessName);
                        
                        if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
                        {
                            Logger.LogDebug("SET.SSCL", "Stopping daemon because switching coin and StopOnExit is set");
                            GlobalMethods.StopAndCloseDaemon();
                        }
                    }

                    isChanged = true;
                    GlobalMethods.SetCoin(selectedCoin);

                    if (!GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
                    {
                        // CLI tools missing. Need to download                        
                        GlobalData.IsCliToolsFound = false;
                        string cliToolsLink = GlobalMethods.GetCliToolsDownloadLink(GlobalData.AppSettings.ActiveCoin);
                        Logger.LogDebug("SET.SSCL", "CLI tools not found. Asking user to confirm download link: " + cliToolsLink);
                        
                        var window = new TextBoxView("Get Client Tools", "Client Tools Download Link", cliToolsLink, string.Empty);
                        await window.ShowDialog(GetWindow()).ContinueWith(CliToolsLinkDialogClosed);
                    }
                }

                if(isChanged)
                {
                    imgCoinIcon.Source = GlobalMethods.GetLogo();
                    GlobalData.NetworkStats = new();
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.SSCL", ex);
            }
        }
        
        
        private void ThresholdEnabled_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox)
                {
                    if (checkBox.IsChecked!.Value)
                    {
                        hashThreshold.IsEnabled = true;
                    }
                    else
                    {
                        hashThreshold.IsEnabled = false;
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.NHMT", ex); // Net Hash Mining Threshold
            }
        }
        
        private void hashThreshold_ValueChanged(object sender, NumericUpDownValueChangedEventArgs args)
        {
            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold != hashThreshold.Value)
            {
                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold = Convert.ToInt32(hashThreshold.Value);
                Logger.LogDebug("DMN.NHMT", "Setting net hash mining threshold: " + GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold);
                GlobalMethods.SaveConfig();
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
                    GlobalData.IsCliToolsDownloading = true;

                    if (!string.IsNullOrEmpty(result.TextBoxValue))
                    {
                        // Download and extract CLI tools
                        GlobalMethods.SetUpCliTools(result.TextBoxValue, GlobalData.CliToolsDir);
                    }
                }
                else
                {
                    Logger.LogDebug("SET.CTLC", "CLI tools download cancelled");
                    GlobalData.IsCliToolsDownloading = false;

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