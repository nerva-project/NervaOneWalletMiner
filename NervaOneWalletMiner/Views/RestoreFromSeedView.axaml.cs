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
        private bool _isBtcStyle;

        public RestoreFromSeedView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                _isBtcStyle = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle;

                if (_isBtcStyle)
                {
                    pnlLanguage.IsVisible = false;
                    lbSeedOffset.Content = "BIP39 Passphrase (optional 25th word)";
                    tbxSeedOffset.Watermark = "Optional - extra passphrase added to your seed phrase";
                    lbPassword.Content = "Wallet Password (Optional)";
                    tbxPassword.Watermark = "Optional - encrypt the wallet with a password";
                }
                else
                {
                    cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
                    cbxLanguage.SelectedIndex = 0;
                }

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
                string walletName = tbxWalletName.Text ?? string.Empty;

                if (string.IsNullOrEmpty(tbxSeedPhrase.Text) || string.IsNullOrEmpty(walletName))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", "Seed Phrase and Wallet Name are required.", true));
                    return;
                }

                if (!_isBtcStyle && string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", "Seed Phrase, Wallet Name and Password are all required.", true));
                    return;
                }

                if (walletName.Contains('/') || walletName.Contains('\\') || walletName.Contains(".."))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", "Wallet Name cannot contain path characters.", true));
                    return;
                }

                char[] seed = tbxSeedPhrase.Text.ToCharArray();
                string seedOffset = tbxSeedOffset.Text ?? string.Empty;
                char[] walletPassword = string.IsNullOrEmpty(tbxPassword.Text) ? [] : tbxPassword.Text.ToCharArray();
                string walletLanguage = _isBtcStyle
                    ? Language.English
                    : (cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!);

                tbxSeedPhrase.Text = string.Empty;
                tbxPassword.Text = string.Empty;

                // Disable controls
                btnOk.Content = "Restoring...";
                btnOk.IsEnabled = false;
                btnCancel.IsEnabled = false;
                tbxSeedPhrase.IsEnabled = false;
                tbxSeedOffset.IsEnabled = false;
                tbxWalletName.IsEnabled = false;                
                tbxPassword.IsEnabled = false;
                btnShowHidePassword.IsEnabled = false;

                if (!_isBtcStyle)
                {
                    cbxLanguage.IsEnabled = false;
                }

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
                    
                    // Enable controls
                    btnOk.Content = "Restore";
                    btnOk.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    tbxSeedPhrase.IsEnabled = true;
                    tbxSeedOffset.IsEnabled = true;
                    tbxWalletName.IsEnabled = true;                    
                    tbxPassword.IsEnabled = true;
                    btnShowHidePassword.IsEnabled = true;
                    
                    if (!_isBtcStyle)
                    {
                        cbxLanguage.IsEnabled = true;
                    }
                    
                    return;
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("RFS.OKBC", "Wallet " + walletName + " restored successfully");

                string successMessage = _isBtcStyle
                    ? walletName + " wallet restored\r\n\r\nA full blockchain rescan has been started. This can take several hours on a mainnet node. Your transactions will appear once the rescan is complete."
                    : walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.";

                await DialogService.ShowAsync(new MessageBoxView("Restore From Seed", successMessage, true));

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
