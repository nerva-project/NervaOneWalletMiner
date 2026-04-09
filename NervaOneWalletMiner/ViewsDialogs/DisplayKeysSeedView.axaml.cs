using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class DisplayKeysSeedView : UserControl
    {
        #region Constructors and Loading
        // Not used but designer will complain without it
        public DisplayKeysSeedView()
        {
            InitializeComponent();
        }

        public DisplayKeysSeedView(string message)
        {
            try
            {
                InitializeComponent();

                GetAndShowKeys(message);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CONS", ex);
            }
        }

        private async void GetAndShowKeys(string message)
        {
            try
            {
                Logger.LogDebug("DKD.GASK", "Querying keys for: " + GlobalData.OpenedWalletName);
                GetPrivateKeysResponse response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetPrivateKeysRequest() { KeyType = KeyType.AllViewSpend });

                if (response.Error.IsError)
                {
                    Logger.LogError("DKD.GASK", "Failed to query keys for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    tbxPublicViewKey.Text = response.PublicViewKey;
                    tbxPrivateViewKey.Text = new string(response.PrivateViewKey);
                    tbxPublicSpendKey.Text = response.PublicSpendKey;
                    tbxPrivateSpendKey.Text = new string(response.PrivateSpendKey);
                    Array.Clear(response.PrivateViewKey, 0, response.PrivateViewKey.Length);
                    Array.Clear(response.PrivateSpendKey, 0, response.PrivateSpendKey.Length);
                    tbkMessage.Text = message;
                    response = new GetPrivateKeysResponse();

                    // Once you got keys, query mnemonic seed
                    response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetPrivateKeysRequest() { KeyType = KeyType.Mnemonic });
                    if (response.Error.IsError)
                    {
                        Logger.LogError("DKD.GASK", "Failed to query mnemonic seed for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    }
                    else
                    {
                        tbxMnemonicSeed.Text = new string(response.Mnemonic);
                        Array.Clear(response.Mnemonic, 0, response.Mnemonic.Length);
                        response = new GetPrivateKeysResponse();
                        Logger.LogDebug("DKD.GASK", "Keys and seed loaded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.GASK", ex);
            }
        }
        #endregion // Constructors and Loading

        #region Events
        public void CopyPublicViewKeyToClipboardButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPublicViewKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CPUV", ex);
            }
        }

        public void CopyPrivateViewKeyToClipboardButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPrivateViewKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CPRV", ex);
            }
        }

        public void ShowHidePrivateViewKeyButton_Clicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("DKD.SHPV", ex);
            }
        }

        public void CopyPublicSpendKeyToClipboardButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPublicSpendKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CPUS", ex);
            }
        }

        public void CopyPrivateSpendKeyToClipboardButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPrivateSpendKey.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CPRS", ex);
            }
        }

        public void ShowHidePrivateSpendKeyButton_Clicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("DKD.SHPS", ex);
            }
        }

        public void CopySeedToClipboardButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxMnemonicSeed.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CPRS", ex);
            }
        }

        public void ShowHideSeedButton_Clicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("DKD.SHPS", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                // Clear sensitive data before closing
                tbxPublicViewKey.Text = "";
                tbxPrivateViewKey.Text = "";
                tbxPublicSpendKey.Text = "";
                tbxPrivateSpendKey.Text = "";
                tbxMnemonicSeed.Text = "";
                TopLevel.GetTopLevel(this)?.Clipboard?.ClearAsync();

                DialogResult result = new()
                {
                    IsCancel = true
                };

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CLBC", ex);
            }
        }
        #endregion // Events
    }
}
