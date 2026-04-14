using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class RestoreFromKeysView : UserControl
    {
        public RestoreFromKeysView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
                cbxLanguage.SelectedIndex = 0;

                Loaded += (_, _) => tbxWalletAddress.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("RFK.CONS", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(tbxWalletAddress.Text)
                    || string.IsNullOrEmpty(tbxViewKey.Text)
                    || string.IsNullOrEmpty(tbxSpendKey.Text)
                    || string.IsNullOrEmpty(tbxWalletName.Text)
                    || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Keys", "Wallet Address, View Key, Spend Key, Wallet Name and Password are all required.", true));
                    return;
                }

                string walletAddress = tbxWalletAddress.Text;
                string walletName = tbxWalletName.Text;
                char[] viewKey = tbxViewKey.Text.ToCharArray();
                char[] spendKey = tbxSpendKey.Text.ToCharArray();
                char[] walletPassword = tbxPassword.Text.ToCharArray();
                string walletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!;

                tbxViewKey.Text = string.Empty;
                tbxSpendKey.Text = string.Empty;
                tbxPassword.Text = string.Empty;

                btnOk.Content = "Restoring...";
                btnOk.IsEnabled = false;
                btnCancel.IsEnabled = false;
                tbxWalletAddress.IsEnabled = false;
                tbxWalletName.IsEnabled = false;
                cbxLanguage.IsEnabled = false;

                Logger.LogDebug("RFK.OKBC", "Restoring wallet from keys: " + walletName);

                if (GlobalData.IsWalletOpen)
                {
                    GlobalMethods.ForceWalletClose();
                    GlobalMethods.WalletClosedOrErrored();
                }

                RestoreFromKeysRequest request = new()
                {
                    WalletAddress = walletAddress,
                    ViewKey = viewKey,
                    SpendKey = spendKey,
                    WalletName = walletName,
                    Password = walletPassword,
                    Language = walletLanguage
                };

                RestoreFromKeysResponse response = await GlobalData.WalletService.RestoreFromKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                Array.Clear(viewKey, 0, viewKey.Length);
                Array.Clear(spendKey, 0, spendKey.Length);
                Array.Clear(walletPassword, 0, walletPassword.Length);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();
                    Logger.LogError("RFK.OKBC", "Failed to restore wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Keys", "Error restoring " + walletName + " wallet\r\n" + response.Error.Message, true));
                    btnOk.Content = "Restore";
                    btnOk.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    tbxWalletAddress.IsEnabled = true;
                    tbxWalletName.IsEnabled = true;
                    cbxLanguage.IsEnabled = true;
                    return;
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("RFK.OKBC", "Wallet " + walletName + " restored successfully");
                await DialogService.ShowAsync(new MessageBoxView("Restore From Keys", walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.", true));

                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("RFK.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("RFK.CNCL", "Restore from keys cancelled");
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("RFK.CNCL", ex);
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
                Logger.LogException("RFK.SHPC", ex);
            }
        }
    }
}
