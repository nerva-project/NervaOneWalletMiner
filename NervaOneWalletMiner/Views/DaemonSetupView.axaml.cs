using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
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

        public DaemonSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                tbxMiningAddress.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;
                tbxDaemonDataDir.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir;
                tbxAdditionalArguments.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments;
                tbxPortNumber.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port.ToString();
                tbxLogLevel.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel.ToString();

                cbxAutoStartMining.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining;
                cbxStopOnExit.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit;
                cbxEnableConnectionsGuard.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard;
                cbxUseNoAnalyticsFlag.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoAnalyticsFlag;
                cbxUseNoDnsFlag.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoDnsFlag;
                cbxThresholdEnabled.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold;
                hashThreshold.Value = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold;

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold)
                {
                    hashThreshold.IsEnabled = true;
                }
                else
                {
                    hashThreshold.IsEnabled = false;
                }

                // Set saved address if there, or default if not
                tbxRemoteNode.Text = string.IsNullOrEmpty(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress)
                    ? GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].PublicNodeUrlDefault
                    : GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress;

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    RemoteNode.IsChecked = true;
                    spFullNode.IsVisible = false;
                    spWalletOnly.IsVisible = true;                    
                }
                else
                {
                    FullNode.IsChecked = true;
                    spFullNode.IsVisible = true;
                    spWalletOnly.IsVisible = false;
                }
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
                bool isSaveSettingsNeeded = false;
                bool isRestartRequired = false;
                bool isLocalRemoteNodeChange = false;

                // Settings that apply to running Full Node or Remote Node
                if(RemoteNode.IsChecked!.Value)
                {
                    if(string.IsNullOrEmpty(tbxRemoteNode.Text))
                    {
                        MessageBoxView errorMessage = new MessageBoxView("Remote Node Required", "Remote Node Address or IP is required.", true);
                        await errorMessage.ShowDialog<DialogResult>(GetWindow());
                        return;
                    }
                    
                    if (!GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly = true;
                        isSaveSettingsNeeded = isLocalRemoteNodeChange = true;
                    }

                    if (GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress != tbxRemoteNode.Text.Trim())
                    {
                        GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].PublicNodeAddress = tbxRemoteNode.Text.Trim();
                        isSaveSettingsNeeded = isLocalRemoteNodeChange = true;
                    }
                }
                else
                {
                    if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly = false;
                        isSaveSettingsNeeded = isLocalRemoteNodeChange = true;
                    }
                }

                // Settings that apply to Daemon
                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != tbxMiningAddress.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = tbxMiningAddress.Text!;
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (!string.IsNullOrEmpty(tbxDaemonDataDir.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir != tbxDaemonDataDir.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir = tbxDaemonDataDir.Text;
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments != tbxAdditionalArguments.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments = tbxAdditionalArguments.Text!;
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (!string.IsNullOrEmpty(tbxPortNumber.Text))
                {
                    int portNumber = Convert.ToInt32(tbxPortNumber.Text);
                    if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port != portNumber)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port = portNumber;
                        isSaveSettingsNeeded = isRestartRequired = true;
                    }
                }

                if (!string.IsNullOrEmpty(tbxLogLevel.Text))
                {
                    int logLevel = Convert.ToInt32(tbxLogLevel.Text);
                    if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel != logLevel)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel = logLevel;
                        isSaveSettingsNeeded = isRestartRequired = true;
                    }
                }
                
                if (cbxAutoStartMining.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining = ((bool)(cbxAutoStartMining.IsChecked == null ? false : cbxAutoStartMining.IsChecked));
                    isSaveSettingsNeeded = true;
                }

                if (cbxStopOnExit.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit = ((bool)(cbxStopOnExit.IsChecked == null ? false : cbxStopOnExit.IsChecked));
                    isSaveSettingsNeeded = true;
                }

                if (cbxEnableConnectionsGuard.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard = ((bool)(cbxEnableConnectionsGuard.IsChecked == null ? false : cbxEnableConnectionsGuard.IsChecked));
                    isSaveSettingsNeeded = true;
                }

                if (cbxUseNoAnalyticsFlag.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoAnalyticsFlag)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoAnalyticsFlag = ((bool)(cbxUseNoAnalyticsFlag.IsChecked == null ? false : cbxUseNoAnalyticsFlag.IsChecked));
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (cbxUseNoDnsFlag.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoDnsFlag)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoDnsFlag = ((bool)(cbxUseNoDnsFlag.IsChecked == null ? false : cbxUseNoDnsFlag.IsChecked));
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (cbxThresholdEnabled.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableMiningThreshold = ((bool)(cbxThresholdEnabled.IsChecked == null ? false : cbxThresholdEnabled.IsChecked));                    
                    isSaveSettingsNeeded = isRestartRequired = true;
                }

                if (hashThreshold.Value != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold = Convert.ToInt32(hashThreshold.Value);
                    isSaveSettingsNeeded = isRestartRequired = true;
                    Logger.LogDebug("DMS.SSC1", "Setting net hash mining threshold: " + GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopMiningThreshold);                    
                }

                // Save setting only if something changed
                if (isSaveSettingsNeeded)
                {                    
                    GlobalMethods.SaveConfig();

                    if(isLocalRemoteNodeChange)
                    {
                        Logger.LogDebug("DMS.SSC1", "Local/Remote settings changed. Restarting.");
                        GlobalMethods.ShowHideDaemonTab();
                        RestartWithCommand(string.Empty);
                    }
                    else if (isRestartRequired)
                    {
                        // Ask user if they want to restart daemon
                        Logger.LogDebug("DMS.SSC1", "Restart required. Asking user.");
                        MessageBoxView confirmDaemonRestart = new MessageBoxView("Restart Daemon?", "You've made changes to daemon setup. For those changes to take effect, restart is required.\r\n\r\nWould you like to restart daemon now?", false, true);
                        DialogResult confirmRestart = await confirmDaemonRestart.ShowDialog<DialogResult>(GetWindow());

                        if (confirmRestart != null && confirmRestart.IsOk)
                        {
                            Logger.LogDebug("DMS.SSC1", "User confirmed restart. Restarting.");
                            RestartWithCommand(string.Empty);
                        }
                    }
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
                if (string.IsNullOrEmpty(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl))
                {
                    MessageBoxView window = new("Restart with QuickSync", "Error, " + GlobalData.AppSettings.ActiveCoin.ToUpper() + " does not support QuickSync.", true);
                    window.ShowDialog(GetWindow());
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
                string cliToolsLink = GlobalMethods.GetCliToolsDownloadLink(GlobalData.AppSettings.ActiveCoin);
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
                if (result != null && result.IsOk)
                {                                      
                    if (!string.IsNullOrEmpty(result.TextBoxValue))
                    {
                        // We'll be downloading new client tools so clean up
                        GlobalData.IsCliToolsDownloading = true;

                        if (GlobalData.IsWalletOpen)
                        {
                            Logger.LogDebug("DMS.CTLC", "Closing wallet: " + GlobalData.OpenedWalletName);
                            await((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).CloseWalletNonUi();
                        }

                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsDaemonWalletSeparateApp)
                        {
                            Logger.LogDebug("DMS.CTLC", "Calling wallet ForceClose...");
                            ProcessManager.Kill(GlobalData.WalletProcessName);
                        }

                        Logger.LogDebug("DMS.CTLC", "Stopping daemon...");
                        GlobalMethods.StopAndCloseDaemon();

                        // Download and extract CLI tools
                        GlobalMethods.SetUpCliTools(result.TextBoxValue, GlobalData.CliToolsDir);
                    }
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
                    RestartWithCommand(result.RestartOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RCDC", ex);
            }
        }

        public static void RestartWithCommand(string restartOptions)
        {
            try
            {
                Logger.LogDebug("DMS.RWC1", "Restarting with command");
                ProcessManager.Kill(GlobalData.WalletProcessName);

                GlobalMethods.StopAndCloseDaemon();
               
                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
                {
                    Logger.LogDebug("DMS.RWC1", "Running as Wallet Only");
                }
                else
                {
                    Logger.LogDebug("DMS.RWC1", "Running as Full Node");
                    GlobalData.IsDaemonRestarting = true;
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " " + restartOptions);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RWC1", ex);
            }
        }
        #endregion // Restart with Command

        #region Other Event Methods
        public void OpenCliToolsFolder_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.CliToolsDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
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
                var result = await GetStorageProvider().OpenFolderPickerAsync(new FolderPickerOpenOptions()
                {
                    Title = "Select Data Directory Folder",
                    SuggestedStartLocation = await GetStorageProvider().TryGetFolderFromPathAsync(tbxDaemonDataDir.Text!),
                    AllowMultiple = false
                });

                if(result.FirstOrDefault() is IStorageFolder item)
                {
                    tbxDaemonDataDir.Text = item.Path.AbsolutePath;
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
                    if (checkBox.IsChecked!.Value)
                    {
                        hashThreshold.IsEnabled = true;
                    }
                    else
                    {
                        hashThreshold.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.NHMT", ex); // Net Hash Mining Threshold
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

        public IStorageProvider GetStorageProvider()
        {
            return GetTopLevel().StorageProvider;
        }
        #endregion // Other Event Methods
    }
}