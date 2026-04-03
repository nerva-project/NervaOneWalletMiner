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
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");
        TopLevel GetTopLevel() => TopLevel.GetTopLevel(this) ?? throw new NullReferenceException("Invalid Owner");
        DaemonSetupViewModel GetVm() => (DaemonSetupViewModel)DataContext!;

        public DaemonSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
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
                    await new MessageBoxView("Remote Node Required", "Remote Node Address or IP is required.", true).ShowDialog<DialogResult>(GetWindow());
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
                    DaemonSetupViewModel.RestartWithCommand(string.Empty);
                }
                else if (result.RestartRequired)
                {
                    Logger.LogDebug("DMS.SSC1", "Restart required. Asking user");
                    MessageBoxView confirmDaemonRestart = new("Restart Daemon?", "You've made changes to daemon setup. For those changes to take effect, restart is required.\r\n\r\nWould you like to restart daemon now?", false, true);
                    DialogResult confirmRestart = await confirmDaemonRestart.ShowDialog<DialogResult>(GetWindow());

                    if (confirmRestart != null && confirmRestart.IsOk)
                    {
                        Logger.LogDebug("DMS.SSC1", "User confirmed restart. Restarting");
                        DaemonSetupViewModel.RestartWithCommand(string.Empty);
                    }
                }
                else
                {
                    await new MessageBoxView("Daemon Setting", "Changes saved successfully.", true).ShowDialog(GetWindow());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.SSC1", ex);
            }
        }
        #endregion // Save Settings

        #region Restart With QuickSync
        public void RestartWithQuickSync_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (!GetVm().IsQuickSyncSupported())
                {
                    new MessageBoxView("Restart with QuickSync", "Error, " + GlobalData.AppSettings.ActiveCoin.ToUpper() + " does not support QuickSync.", true).ShowDialog(GetWindow());
                }
                else
                {
                    GlobalMethods.RestartWithQuickSync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RQSC", ex);
            }
        }
        #endregion // Restart With QuickSync

        #region Update Client Tools
        public void UpdateClientTools_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string cliToolsLink = GetVm().GetCliToolsDownloadLink();
                Logger.LogDebug("DMS.UCTC", "Updating client tools");

                var window = new TextBoxView("Update Client Tools", "Client Tools Download Link", cliToolsLink, string.Empty);
                window.ShowDialog(GetWindow()).ContinueWith(CliToolsLinkDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.UCTC", ex);
            }
        }

        private async void CliToolsLinkDialogClosed(Task task)
        {
            try
            {
                DialogResult result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk && !string.IsNullOrEmpty(result.TextBoxValue))
                {
                    await GetVm().PerformCliToolsUpdate(result.TextBoxValue);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.CTLC", ex);
            }
        }
        #endregion // Update Client Tools

        #region Restart with Command
        public void RestartWithCommand_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new RestartWithCommandView();
                window.ShowDialog(GetWindow()).ContinueWith(RestartWithCommandDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RWCC", ex);
            }
        }

        private void RestartWithCommandDialogClosed(Task task)
        {
            try
            {
                DialogResult result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk)
                {
                    DaemonSetupViewModel.RestartWithCommand(result.RestartOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RCDC", ex);
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