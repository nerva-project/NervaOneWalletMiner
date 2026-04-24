using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Collections.Generic;
using System.IO;

namespace NervaOneWalletMiner.Views
{
    public partial class OpenWalletView : UserControl
    {
        public OpenWalletView()
        {
            try
            {
                InitializeComponent();

                imgCoinIcon.Source = GlobalMethods.GetLogo();

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                {
                    tbxPassword.IsEnabled = false;
                    tbxPassword.Watermark = "Password not required to open wallet";
                    btnShowHidePassword.IsEnabled = false;
                }

                cbxWalletName.ItemsSource = GetWalletFileNames();
                cbxWalletName.SelectedIndex = 0;

                Loaded += (_, _) => cbxWalletName.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("OWV.CONS", ex);
            }
        }

        private List<string> GetWalletFileNames()
        {
            List<string> wallets = [];

            try
            {
                DirectoryInfo walletsDir = new(GlobalMethods.GetWalletDir());
                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].WalletExtension.Equals("directory"))
                {
                    foreach (DirectoryInfo dir in walletsDir.GetDirectories())
                    {
                        wallets.Add(dir.Name);
                    }
                }
                else
                {
                    FileInfo[] files = walletsDir.GetFiles("*" + GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].WalletExtension, SearchOption.TopDirectoryOnly);
                    foreach (FileInfo file in files)
                    {
                        wallets.Add(Path.GetFileNameWithoutExtension(file.FullName));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("OWV.GWFN", ex);
            }

            return wallets;
        }

        public void Password_KeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                OkButton_Clicked(sender, args);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet && string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Open Wallet", "Password cannot be empty", true));
                    return;
                }

                if (string.IsNullOrEmpty(cbxWalletName.SelectedValue?.ToString()))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Open Wallet", "Wallet Name cannot be empty", true));
                    return;
                }

                string walletName = cbxWalletName.SelectedValue.ToString()!;
                char[] walletPassword = tbxPassword.Text?.ToCharArray() ?? [];

                tbxPassword.Text = string.Empty;
                btnOk.Content = "Opening...";
                btnOk.IsEnabled = false;
                btnCancel.IsEnabled = false;
                cbxWalletName.IsEnabled = false;
                tbxPassword.IsEnabled = false;

                Logger.LogDebug("OWV.OKBC", "Opening wallet: " + walletName);

                // Hash before request as password array will be cleared
                string walletPasswordHash = Hashing.Hash(walletPassword);

                OpenWalletRequest request = new()
                {
                    WalletName = walletName,
                    Password = walletPassword
                };

                OpenWalletResponse response = await GlobalData.WalletService.OpenWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                Array.Clear(walletPassword, 0, walletPassword.Length);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();
                    Logger.LogDebug("OWV.OKBC", "Error opening " + walletName + " wallet | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Open Wallet", "Error opening " + walletName + " wallet\r\n" + response.Error.Message, true));
                    });
                    btnOk.Content = "Open";
                    btnOk.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    cbxWalletName.IsEnabled = true;
                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                    {
                        tbxPassword.IsEnabled = true;
                    }
                    return;
                }

                GlobalMethods.WalletJustOpened(walletName);

                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                {
                    GlobalData.WalletPassProvidedTime = DateTime.Now;
                    GlobalData.WalletPasswordHash = walletPasswordHash;
                }

                Logger.LogDebug("OWV.OKBC", "Wallet " + walletName + " opened successfully");
                UIManager.NavigateToPage(SplitViewPages.Wallet);
            }
            catch (Exception ex)
            {
                Logger.LogException("OWV.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("OWV.CNCL", "Open wallet cancelled");
                UIManager.NavigateToPage(SplitViewPages.Wallet);
            }
            catch (Exception ex)
            {
                Logger.LogException("OWV.CNCL", ex);
            }
        }

        public void ShowHidePassword_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (tbxPassword.RevealPassword)
                {
                    tbxPassword.RevealPassword = false;
                    btnShowHidePassword.Content = "Show";
                }
                else
                {
                    tbxPassword.RevealPassword = true;
                    btnShowHidePassword.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("OWV.SHPC", ex);
            }
        }
    }
}
