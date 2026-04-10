using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Wallet.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class DisplayKeysSeedView : UserControl
    {
        private string _returnPage = string.Empty;

        public DisplayKeysSeedView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                Initialized += DisplayKeysSeedView_Initialized;
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CONS", ex);
            }
        }

        private void DisplayKeysSeedView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                string message = DataContext is DisplayKeysSeedViewModel vm ? vm.Message : string.Empty;
                _returnPage = DataContext is DisplayKeysSeedViewModel vm2 ? vm2.ReturnPage : string.Empty;

                tbkMessage.Text = message;
                GetAndShowKeys();
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.INIT", ex);
            }
        }

        private async void GetAndShowKeys()
        {
            try
            {
                Logger.LogDebug("DKV.GASK", "Querying keys for: " + GlobalData.OpenedWalletName);
                GetPrivateKeysResponse response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetPrivateKeysRequest() { KeyType = KeyType.AllViewSpend });

                if (response.Error.IsError)
                {
                    Logger.LogError("DKV.GASK", "Failed to query keys for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    tbxPublicViewKey.Text = response.PublicViewKey;
                    tbxPrivateViewKey.Text = new string(response.PrivateViewKey);
                    tbxPublicSpendKey.Text = response.PublicSpendKey;
                    tbxPrivateSpendKey.Text = new string(response.PrivateSpendKey);
                    Array.Clear(response.PrivateViewKey, 0, response.PrivateViewKey.Length);
                    Array.Clear(response.PrivateSpendKey, 0, response.PrivateSpendKey.Length);
                    response = new GetPrivateKeysResponse();

                    // Once you got keys, query mnemonic seed
                    response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetPrivateKeysRequest() { KeyType = KeyType.Mnemonic });
                    if (response.Error.IsError)
                    {
                        Logger.LogError("DKV.GASK", "Failed to query mnemonic seed for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    }
                    else
                    {
                        tbxMnemonicSeed.Text = new string(response.Mnemonic);
                        Array.Clear(response.Mnemonic, 0, response.Mnemonic.Length);
                        response = new GetPrivateKeysResponse();
                        Logger.LogDebug("DKV.GASK", "Keys and seed loaded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.GASK", ex);
            }
        }

        public void CloseButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                tbxPublicViewKey.Text = string.Empty;
                tbxPrivateViewKey.Text = string.Empty;
                tbxPublicSpendKey.Text = string.Empty;
                tbxPrivateSpendKey.Text = string.Empty;
                tbxMnemonicSeed.Text = string.Empty;
                TopLevel.GetTopLevel(this)?.Clipboard?.ClearAsync();

                UIManager.NavigateToPage(_returnPage);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CLBC", ex);
            }
        }

        public void CopyPublicViewKeyToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPublicViewKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CPUV", ex);
            }
        }

        public void CopyPrivateViewKeyToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPrivateViewKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CPRV", ex);
            }
        }

        public void ShowHidePrivateViewKey_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (tbxPrivateViewKey.RevealPassword)
                {
                    tbxPrivateViewKey.RevealPassword = false;
                    btnShowHidePrivateViewKey.Content = "Show";
                }
                else
                {
                    tbxPrivateViewKey.RevealPassword = true;
                    btnShowHidePrivateViewKey.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.SHPV", ex);
            }
        }

        public void CopyPublicSpendKeyToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPublicSpendKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CPUS", ex);
            }
        }

        public void CopyPrivateSpendKeyToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPrivateSpendKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CPRS", ex);
            }
        }

        public void ShowHidePrivateSpendKey_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (tbxPrivateSpendKey.RevealPassword)
                {
                    tbxPrivateSpendKey.RevealPassword = false;
                    btnShowHidePrivateSpendKey.Content = "Show";
                }
                else
                {
                    tbxPrivateSpendKey.RevealPassword = true;
                    btnShowHidePrivateSpendKey.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.SHPS", ex);
            }
        }

        public void CopySeedToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxMnemonicSeed.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.CPSD", ex);
            }
        }

        public void ShowHideSeed_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (tbxMnemonicSeed.RevealPassword)
                {
                    tbxMnemonicSeed.RevealPassword = false;
                    btnShowHideSeed.Content = "Show";
                }
                else
                {
                    tbxMnemonicSeed.RevealPassword = true;
                    btnShowHideSeed.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DKV.SHSD", ex);
            }
        }
    }
}
