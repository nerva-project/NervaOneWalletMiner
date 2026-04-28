using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Diagnostics;
using System.Linq;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonSetupView : UserControl
    {
        TopLevel GetTopLevel() => TopLevel.GetTopLevel(this) ?? throw new NullReferenceException("Invalid Owner");
        DaemonSetupViewModel GetVm() => (DaemonSetupViewModel)DataContext!;

        public DaemonSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                btnOpenCliToolsFolder.IsVisible = !GlobalMethods.IsAndroid();
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.CONS", ex);
            }
        }

        #region Save Settings
        public async void SaveSettings_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var vm = GetVm();

                if (vm.IsWalletOnly && string.IsNullOrEmpty(vm.RemoteNodeAddress))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Remote Node Required", "Remote Node Address or IP is required.", true));
                    return;
                }

                var result = vm.ApplySettings();

                if (!result.IsSaved)
                {
                    return;
                }

                if (result.LocalRemoteChanged)
                {
                    Logger.LogDebug("DMS.SSC1", "Local/Remote settings changed. Restarting");
                    GlobalMethods.ShowHideDaemonTab();
                    GetVm().RestartWithCommand(string.Empty);
                }
                else if (result.RestartRequired)
                {
                    Logger.LogDebug("DMS.SSC1", "Restart required. Asking user");
                    MessageBoxView confirmDaemonRestart = new("Restart Daemon?", "You've made changes to daemon setup. For those changes to take effect, restart is required.\r\n\r\nWould you like to restart daemon now?", false, true);
                    DialogResult? confirmRestart = await DialogService.ShowAsync<DialogResult>(confirmDaemonRestart);

                    if (confirmRestart != null && confirmRestart.IsOk)
                    {
                        Logger.LogDebug("DMS.SSC1", "User confirmed restart. Restarting");
                        GetVm().RestartWithCommand(string.Empty);
                    }
                }
                else
                {
                    await DialogService.ShowAsync(new MessageBoxView("Daemon Setting", "Changes saved successfully.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.SSC1", ex);
            }
        }
        #endregion // Save Settings

        #region Restart With QuickSync
        public async void RestartWithQuickSync_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (!GetVm().IsQuickSyncSupported())
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restart with QuickSync", "Error, " + GlobalData.AppSettings.ActiveCoin.ToUpper() + " does not support QuickSync.", true));
                }
                else
                {
                    bool isSuccess = await GlobalMethods.RestartWithQuickSync();
                    if (isSuccess)
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Restart with QuickSync", "Daemon restarted with QuickSync.", true));
                    }
                    else
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Restart with QuickSync", "Failed to restart daemon with QuickSync. Please check logs for details.", true));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RQSC", ex);
            }
        }
        #endregion // Restart With QuickSync

        #region Public Node Setup
        public void PublicNodeSetup_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                UIManager.NavigateToPublicNodeSetup();
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.PNSC", ex);
            }
        }
        #endregion // Public Node Setup

        #region Update Client Tools
        public async void UpdateClientTools_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string cliToolsLink = GetVm().GetCliToolsDownloadLink();
                Logger.LogDebug("DMS.UCTC", "Updating client tools");

                var window = new TextBoxView(title: "Update Client Tools", labelValue: "Client Tools Download Link", textValue: cliToolsLink, textWatermark: string.Empty, okButtonText: "Update");
                DialogResult? result = await DialogService.ShowAsync<DialogResult>(window);
                if (result != null && result.IsOk && !string.IsNullOrEmpty(result.TextBoxValue))
                {
                    await GetVm().PerformCliToolsUpdate(result.TextBoxValue);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.UCTC", ex);
            }
        }
        #endregion // Update Client Tools

        #region Download Blockchain Db
        public async void DownloadBlockchainDb_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string blockchainDbLink = GetVm().GetBlockchainDbDownloadLink();
                Logger.LogDebug("DMS.DBDC", "Downloading blockchain database");

                var window = new TextBoxView(title: "Download Blockchain Db", labelValue: "Blockchain Db Download Link", textValue: blockchainDbLink, textWatermark: string.Empty, okButtonText: "Download");
                DialogResult? result = await DialogService.ShowAsync<DialogResult>(window);
                if (result != null && result.IsOk && !string.IsNullOrEmpty(result.TextBoxValue))
                {
                    await GetVm().PerformBlockchainDbDownload(result.TextBoxValue);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.DBDC", ex);
            }
        }
        #endregion // Download Blockchain Db

        #region Restart with Command
        public async void RestartWithCommand_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new TextBoxView(title: "Restart With Command", labelValue: "Additional Restart Options", textValue: string.Empty, textWatermark: "Ex: --pop-blocks 1000", isTextRequired: false, okButtonText: "Restart");
                DialogResult? result = await DialogService.ShowAsync<DialogResult>(window);
                if (result != null && result.IsOk)
                {
                    GetVm().RestartWithCommand(result.TextBoxValue);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RWCC", ex);
            }
        }
        #endregion // Restart with Command

        #region Other Event Methods
        public void OpenCliToolsFolder_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = GlobalData.CliToolsDir, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.OCFC", ex);
            }
        }

        public async void ChangeDataDir_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var result = await GetTopLevel().StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select Data Directory Folder",
                    SuggestedStartLocation = await GetTopLevel().StorageProvider.TryGetFolderFromPathAsync(GetVm().DataDir),
                    AllowMultiple = false
                });

                if (result.FirstOrDefault() is IStorageFolder item)
                {
                    GetVm().DataDir = item.Path.AbsolutePath;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.CDDC", ex);
            }
        }

        private void NoDns_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox)
                {
                    if (checkBox.IsChecked!.Value)
                    {
                        cbxUseNoAnalyticsFlag.IsChecked = true;
                        cbxUseNoAnalyticsFlag.IsEnabled = false;
                    }
                    else
                    {
                        cbxUseNoAnalyticsFlag.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.NDCC", ex);
            }
        }

        private void ThresholdEnabled_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is CheckBox checkBox)
                {
                    hashThreshold.IsEnabled = checkBox.IsChecked!.Value;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.NHMT", ex);
            }
        }

        private void NodeType_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is RadioButton radioButton)
                {
                    switch (radioButton.Name)
                    {
                        case "FullNode":
                            if (radioButton.IsChecked!.Value)
                            {
                                spFullNode.IsVisible = true;
                                spWalletOnly.IsVisible = false;
                            }
                            break;
                        case "RemoteNode":
                            if (radioButton.IsChecked!.Value)
                            {
                                spWalletOnly.IsVisible = true;
                                spFullNode.IsVisible = false;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.NTCC", ex);
            }
        }
        #endregion // Other Event Methods
    }
}
