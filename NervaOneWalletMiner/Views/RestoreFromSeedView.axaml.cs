using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class RestoreFromSeedView : UserControl
    {
        public RestoreFromSeedView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
                cbxLanguage.SelectedIndex = 0;

                Loaded += (_, _) => tbxSeedPhrase.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("RFS.CONS", ex);
            }
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
                string walletName = tbxWalletName.Text == null ? string.Empty : tbxWalletName.Text;

                if (string.IsNullOrEmpty(tbxSeedPhrase.Text)
                    || string.IsNullOrEmpty(walletName)
                    || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", "Seed Phrase, Wallet Name and Password are all required.", true));
                    return;
                }
                else if (walletName.Contains('/') || walletName.Contains('\\') || walletName.Contains(".."))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", "Wallet Name cannot contain path characters.", true));
                    return;
                }

                char[] seed = tbxSeedPhrase.Text.ToCharArray();
                string seedOffset = tbxSeedOffset.Text ?? string.Empty;
                char[] walletPassword = tbxPassword.Text.ToCharArray();
                string walletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!;

                tbxSeedPhrase.Text = string.Empty;
                tbxPassword.Text = string.Empty;

                btnOk.Content = "Restoring...";
                btnOk.IsEnabled = false;
                btnCancel.IsEnabled = false;
                tbxWalletName.IsEnabled = false;
                tbxSeedOffset.IsEnabled = false;
                cbxLanguage.IsEnabled = false;

                Logger.LogDebug("RFS.OKBC", "Restoring wallet from seed: " + walletName);

                if (GlobalData.IsWalletOpen)
                {
                    GlobalMethods.ForceWalletClose();
                    GlobalMethods.WalletClosedOrErrored();
                }

                RestoreFromSeedRequest request = new()
                {
                    Seed = seed,
                    SeedOffset = seedOffset,
                    WalletName = walletName,
                    Password = walletPassword,
                    Language = walletLanguage
                };

                RestoreFromSeedResponse response = await GlobalData.WalletService.RestoreFromSeed(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                Array.Clear(seed, 0, seed.Length);
                Array.Clear(walletPassword, 0, walletPassword.Length);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();
                    Logger.LogError("RFS.OKBC", "Failed to restore wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", "Error restoring " + walletName + " wallet\r\n" + response.Error.Message, true));
                    btnOk.Content = "Restore";
                    btnOk.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    tbxWalletName.IsEnabled = true;
                    tbxSeedOffset.IsEnabled = true;
                    cbxLanguage.IsEnabled = true;
                    return;
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("RFS.OKBC", "Wallet " + walletName + " restored successfully");
                await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.", true));

                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("RFS.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("RFS.CNCL", "Restore from seed cancelled");
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("RFS.CNCL", ex);
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
                Logger.LogException("RFS.SHPC", ex);
            }
        }
    }
}
