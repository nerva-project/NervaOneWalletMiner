using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Rpc;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public DaemonSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
                tbxMiningAddress.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;

                var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");
                tbxDaemonDataDir.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir;

                var cbxAutoStartMining = this.Get<CheckBox>("cbxAutoStartMining");
                cbxAutoStartMining.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining;

                var cbxStopOnExit = this.Get<CheckBox>("cbxStopOnExit");
                cbxStopOnExit.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit;

                var tbxAdditionalArguments = this.Get<TextBox>("tbxAdditionalArguments");
                tbxAdditionalArguments.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments;

                var tbxLogLevel = this.Get<TextBox>("tbxLogLevel");
                tbxLogLevel.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.CONS", ex);
            }
        }

        public void OpenCliToolsFolderClicked(object sender, RoutedEventArgs args)
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

        public void SaveSettingsClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;

                var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");                             
                if (!string.IsNullOrEmpty(tbxMiningAddress.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != tbxMiningAddress.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = tbxMiningAddress.Text;
                    isChanged = true;
                }

                var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");
                if (!string.IsNullOrEmpty(tbxDaemonDataDir.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir != tbxDaemonDataDir.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir = tbxDaemonDataDir.Text;
                    isChanged = true;
                }

                var cbxAutoStartMining = this.Get<CheckBox>("cbxAutoStartMining");
                if (cbxAutoStartMining.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining = ((bool)(cbxAutoStartMining.IsChecked == null ? false : cbxAutoStartMining.IsChecked));
                    isChanged = true;
                }

                var cbxStopOnExit = this.Get<CheckBox>("cbxStopOnExit");
                if (cbxStopOnExit.IsChecked != GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit = ((bool)(cbxStopOnExit.IsChecked == null ? false : cbxStopOnExit.IsChecked));
                    isChanged = true;
                }

                var tbxAdditionalArguments = this.Get<TextBox>("tbxAdditionalArguments");
                if (!string.IsNullOrEmpty(tbxAdditionalArguments.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments != tbxAdditionalArguments.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments = tbxAdditionalArguments.Text;
                    isChanged = true;
                }

                uint logLevel = Convert.ToUInt32(tbxLogLevel.Text);
                if (!string.IsNullOrEmpty(tbxLogLevel.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel != logLevel)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].LogLevel = logLevel;
                    isChanged = true;
                }

                if (isChanged)
                {
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.SSC1", ex);
            }
        }

        public void RestartWithQuickSyncClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                RestartWithQuickSync();
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RQSC", ex);
            }
        }

        public static async void RestartWithQuickSync()
        {
            try
            {
                bool isSuccess = await GlobalMethods.DownloadFileToFolder(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl, GlobalData.CliToolsDir);
                if (isSuccess)
                {
                    Logger.LogDebug("DMS.RSQS", "Restarting CLI");
                    WalletProcess.ForceClose();
                    DaemonProcess.ForceClose();

                    GlobalData.IsDaemonRestarting = true;
                    string quickSyncFile = Path.Combine(GlobalData.CliToolsDir, Path.GetFileName(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl));
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), DaemonProcess.GenerateOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " --quicksync \"" + quickSyncFile + "\"");
                }
                else
                {
                    Logger.LogError("DMS.RSQS", "Failed to download file: " + GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl + " to " + GlobalData.CliToolsDir);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RSQS", ex);
            }
        }

        public void RestartWithCommandClicked(object sender, RoutedEventArgs args)
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
                Logger.LogDebug("DMS.RWC1", "Restarting CLI");
                WalletProcess.ForceClose();
                DaemonProcess.ForceClose();

                GlobalData.IsDaemonRestarting = true;
                ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), DaemonProcess.GenerateOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " " + restartOptions);
            }
            catch (Exception ex)
            {
                Logger.LogException("DMS.RWC1", ex);
            }
        }
    }
}