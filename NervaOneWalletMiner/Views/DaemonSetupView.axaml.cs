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
            InitializeComponent();

            var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
            tbxMiningAddress.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;

            var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");
            tbxDaemonDataDir.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir;

            var cbxAutoStartMining = this.Get<CheckBox>("cbxAutoStartMining");
            cbxAutoStartMining.IsChecked = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AutoStartMining;

            var tbxAdditionalArguments = this.Get<TextBox>("tbxAdditionalArguments");
            tbxAdditionalArguments.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments;
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
                Logger.LogException("DaeSV.OCTFC", ex);
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

                var tbxAdditionalArguments = this.Get<TextBox>("tbxAdditionalArguments");
                if (!string.IsNullOrEmpty(tbxAdditionalArguments.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments != tbxAdditionalArguments.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].AdditionalArguments = tbxAdditionalArguments.Text;
                    isChanged = true;
                }

                if (isChanged)
                {
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DaeSV.SSC", ex);
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
                Logger.LogException("DaeSV.RWQSC", ex);
            }
        }

        public static async void RestartWithQuickSync()
        {
            try
            {
                bool isSuccess = await GlobalMethods.DownloadFileToFolder(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl, GlobalData.CliToolsDir);
                if (isSuccess)
                {
                    Logger.LogDebug("DaeSV.RSQS", "Restarting CLI");
                    WalletProcess.ForceClose();
                    DaemonProcess.ForceClose();

                    GlobalData.IsDaemonRestarting = true;
                    string quickSyncFile = Path.Combine(GlobalData.CliToolsDir, Path.GetFileName(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl));
                    ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), DaemonProcess.GenerateOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " --quicksync \"" + quickSyncFile + "\"");
                }
                else
                {
                    Logger.LogError("DaeSV.RSQS", "Failed to download file: " + GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].QuickSyncUrl + " to " + GlobalData.CliToolsDir);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DaeSV.RSQS", ex);
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
                Logger.LogException("DaeSV.RWCC", ex);
            }
        }

        private void RestartWithCommandDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                RestartWithCommand(result.RestartOptions);
            }
        }

        public static void RestartWithCommand(string restartOptions)
        {
            try
            {
                Logger.LogDebug("DaeSV.RWC", "Restarting CLI");
                WalletProcess.ForceClose();
                DaemonProcess.ForceClose();

                GlobalData.IsDaemonRestarting = true;
                ProcessManager.StartExternalProcess(GlobalMethods.GetDaemonProcess(), DaemonProcess.GenerateOptions(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin]) + " " + restartOptions);
            }
            catch (Exception ex)
            {
                Logger.LogException("DaeSV.RWC", ex);
            }
        }
    }
}