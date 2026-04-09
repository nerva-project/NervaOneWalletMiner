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
    public partial class CreateWalletView : UserControl
    {
        public CreateWalletView()
        {
            try
            {
                InitializeComponent();

                cbxLanguage.ItemsSource = GlobalMethods.GetSupportedLanguages();
                cbxLanguage.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.CONS", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(tbxWalletName.Text) || string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Wallet Name and Password are required.", true));
                    return;
                }

                string walletName = tbxWalletName.Text;
                char[] walletPassword = tbxPassword.Text.ToCharArray();
                string walletLanguage = cbxLanguage.SelectedValue == null ? Language.English : cbxLanguage.SelectedValue.ToString()!;

                tbxPassword.Text = string.Empty;

                Logger.LogDebug("CWV.OKBC", "Creating wallet: " + walletName);

                // Hash before request as password array will be cleared
                string walletPasswordHash = Hashing.Hash(walletPassword);

                CreateWalletRequest request = new()
                {
                    WalletName = walletName,
                    Password = walletPassword,
                    Language = walletLanguage
                };

                CreateWalletResponse response = await GlobalData.WalletService.CreateWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                Array.Clear(walletPassword, 0, walletPassword.Length);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();
                    Logger.LogError("CWV.OKBC", "Failed to create wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await DialogService.ShowAsync(new MessageBoxView("Create Wallet", "Error creating " + walletName + " wallet\r\n" + response.Error.Message, true));
                    return;
                }

                GlobalMethods.WalletJustOpened(walletName);

                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                {
                    GlobalData.WalletPassProvidedTime = DateTime.Now;
                    GlobalData.WalletPasswordHash = walletPasswordHash;
                }
                Logger.LogDebug("CWV.OKBC", "Wallet " + walletName + " created successfully");

                await DialogService.ShowAsync(new DisplayKeysSeedView("Wallet created and opened successfully! Save your seed phrase and keys to a safe place. You'll need them to restore your wallet. Keep them private - anyone with access can steal your funds!"));

                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("CWV.CNCL", "Create wallet cancelled");
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("CWV.CNCL", ex);
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
                Logger.LogException("CWV.SHPC", ex);
            }
        }
    }
}
