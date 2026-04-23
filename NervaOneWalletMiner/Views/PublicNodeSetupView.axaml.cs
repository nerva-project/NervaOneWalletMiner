using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Net.Http;

namespace NervaOneWalletMiner.Views
{
    public partial class PublicNodeSetupView : UserControl
    {
        PublicNodeSetupViewModel GetVm() => (PublicNodeSetupViewModel)DataContext!;

        public PublicNodeSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                int port = GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc.Port;
                runPort.Text = port.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException("PNV.CONS", ex);
            }
        }

        private async void DetectIp_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                btnDetectIp.IsEnabled = false;
                btnDetectIp.Content = "Detecting...";

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(10);
                string ip = await client.GetStringAsync("https://api.ipify.org");
                ip = ip.Trim();

                if (!string.IsNullOrEmpty(ip))
                {
                    GetVm().PublicIp = ip;
                    Logger.LogDebug("PNV.DIPC", "Detected public IP: " + ip);
                }
                else
                {
                    await DialogService.ShowAsync(new MessageBoxView("Detect IP", "Could not detect external IP. Please enter it manually.\r\n\r\nVisit https://api.ipify.org to find your IP.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PNV.DIPC", ex);
                await DialogService.ShowAsync(new MessageBoxView("Detect IP", "Could not detect external IP. Please enter it manually.\r\n\r\nVisit https://api.ipify.org to find your IP.", true));
            }
            finally
            {
                btnDetectIp.IsEnabled = true;
                btnDetectIp.Content = "Detect";
            }
        }

        private async void Save_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool saved = GetVm().ApplySettings();

                if (saved)
                {
                    Logger.LogDebug("PNV.SAVC", "Settings saved. Asking user about restart");
                    MessageBoxView confirmRestart = new("Restart Daemon?", "You've made changes to public node setup. For those changes to take effect, restart is required.\r\n\r\nWould you like to restart daemon now?", false, true);
                    DialogResult? result = await DialogService.ShowAsync<DialogResult>(confirmRestart);

                    if (result != null && result.IsOk)
                    {
                        Logger.LogDebug("PNV.SAVC", "User confirmed restart. Restarting");
                        ((DaemonSetupViewModel)GlobalData.ViewModelPages[SplitViewPages.DaemonSetup]).RestartWithCommand(string.Empty);
                    }
                }
                else
                {
                    await DialogService.ShowAsync(new MessageBoxView("Public Node Setup", "No changes to save.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PNV.SAVC", ex);
            }
        }

        private void CopyAddress_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string address = GetVm().NodeAddress;
                if (!string.IsNullOrEmpty(address))
                {
                    GlobalMethods.CopyToClipboard(this, address);
                    Logger.LogDebug("PNV.CPYC", "Copied node address: " + address);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("PNV.CPYC", ex);
            }
        }

        private void Back_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                UIManager.NavigateToPage(SplitViewPages.DaemonSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("PNV.BAKC", ex);
            }
        }
    }
}
