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
                bool isChanged = false;
                         
                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != tbxMiningAddress.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = tbxMiningAddress.Text!;
                    isChanged = true;
                }

                if (!string.IsNullOrEmpty(tbxDaemonDataDir.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir != tbxDaemonDataDir.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir = tbxDaemonDataDir.Text;
                    isChanged = true;
                }

                if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments != tbxAdditionalArguments.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments = tbxAdditionalArguments.Text!;
                    isChanged = true;
                }

                if (!string.IsNullOrEmpty(tbxPortNumber.Text))
                {
                    int portNumber = Convert.ToInt32(tbxPortNumber.Text);
                    if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port != portNumber)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port = portNumber;
                        isChanged = true;
                    }
                }

                if (!string.IsNullOrEmpty(tbxLogLevel.Text))
                {
                    int logLevel = Convert.ToInt32(tbxLogLevel.Text);
                    if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel != logLevel)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel = logLevel;
                        isChanged = true;
                    }
                }
                
                if (cbxAutoStartMining.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining = ((bool)(cbxAutoStartMining.IsChecked == null ? false : cbxAutoStartMining.IsChecked));
                    isChanged = true;
                }

                if (cbxStopOnExit.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit = ((bool)(cbxStopOnExit.IsChecked == null ? false : cbxStopOnExit.IsChecked));
                    isChanged = true;
                }

                if (cbxEnableConnectionsGuard.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].EnableConnectionsGuard = ((bool)(cbxEnableConnectionsGuard.IsChecked == null ? false : cbxEnableConnectionsGuard.IsChecked));
                    isChanged = true;
                }

                if (cbxUseNoAnalyticsFlag.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoAnalyticsFlag)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoAnalyticsFlag = ((bool)(cbxUseNoAnalyticsFlag.IsChecked == null ? false : cbxUseNoAnalyticsFlag.IsChecked));
                    isChanged = true;
                }

                if (cbxUseNoDnsFlag.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoDnsFlag)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].UseNoDnsFlag = ((bool)(cbxUseNoDnsFlag.IsChecked == null ? false : cbxUseNoDnsFlag.IsChecked));
                    isChanged = true;
                }


                // Save setting only if something changed
                if (isChanged)
                {                    
                    GlobalMethods.SaveConfig();

                    // Ask user if they want to restart daemon
                    MessageBoxView confirmDaemonRestart = new MessageBoxView("Restart Daemon?", "You've made changes to daemon setup. For those changes to take effect, restart is required.\r\n\r\nWould you like to restart daemon now?", false);
                    DialogResult confirmRestart = await confirmDaemonRestart.ShowDialog<DialogResult>(GetWindow());

                    if (confirmRestart != null && confirmRestart.IsOk)
                    {
                        RestartWithCommand(string.Empty);
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

                GlobalData.IsDaemonRestarting = true;
                ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].GenerateDaemonOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " " + restartOptions);
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

        public IStorageProvider GetStorageProvider()
        {
            return GetTopLevel().StorageProvider;
        }
        #endregion // Other Event Methods
    }
}