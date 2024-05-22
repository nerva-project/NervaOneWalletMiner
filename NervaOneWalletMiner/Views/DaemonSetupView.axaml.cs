using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc;
using System;
using System.Diagnostics;
using System.IO;

namespace NervaOneWalletMiner.Views
{
    public partial class DaemonSetupView : UserControl
    {
        public DaemonSetupView()
        {
            InitializeComponent();

            var tbxMiningAddress = this.Get<TextBox>("tbxMiningAddress");
            tbxMiningAddress.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress;

            var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");
            tbxDaemonDataDir.Text = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir;
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
                var tbxDaemonDataDir = this.Get<TextBox>("tbxDaemonDataDir");

                if (!string.IsNullOrEmpty(tbxMiningAddress.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != tbxMiningAddress.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = tbxMiningAddress.Text;
                    isChanged = true;
                }

                if (!string.IsNullOrEmpty(tbxDaemonDataDir.Text) && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir != tbxDaemonDataDir.Text)
                {
                    GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].DataDir = tbxDaemonDataDir.Text;
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
                if(isSuccess)
                {
                    Logger.LogDebug("DaeSV.RSQS", "Restarting CLI");
                    WalletProcess.ForceClose();
                    DaemonProcess.ForceClose();

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
    }
}