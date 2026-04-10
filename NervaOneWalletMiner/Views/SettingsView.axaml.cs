using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewModels;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                btnOpenLogsFolder.IsVisible = !GlobalMethods.IsAndroid();

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

                cbxCoin.SelectedValue = selectedCoin;
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.CONS", ex);
            }            
        }

        public void ViewAppLogs_Clicked(object sender, RoutedEventArgs args)
        {
            Logger.LogDebug("SET.VALC", "Navigating to View Logs - App Logs tab");
            UIManager.NavigateToViewLogs("app");
        }

        public void ViewCliLogs_Clicked(object sender, RoutedEventArgs args)
        {
            Logger.LogDebug("SET.VCLC", "Navigating to View Logs - CLI Tool Logs tab");
            UIManager.NavigateToViewLogs("cli");
        }

        public void ViewWalletExports_Clicked(object sender, RoutedEventArgs args)
        {
            Logger.LogDebug("SET.VWEC", "Navigating to View Logs - Wallet Exports tab");
            UIManager.NavigateToViewLogs("exports");
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
                        // CLI tools missing. Navigate to Coin Setup page
                        GlobalData.IsCliToolsFound = false;
                        Logger.LogDebug("SET.SSCL", "CLI tools not found. Navigating to Coin Setup View.");
                        UIManager.NavigateToCoinSetup();
                    }
                }

                if(isChanged)
                {
                    imgCoinIcon.Source = GlobalMethods.GetLogo();
                    GlobalData.NetworkStats = new();
                    GlobalMethods.SaveConfig();

                    GlobalMethods.ShowHideDaemonTab();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("SET.SSCL", ex);
            }
        }
    }
}