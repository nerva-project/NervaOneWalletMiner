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
    public partial class DisplayKeysSeedView : Window
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
                Icon = GlobalMethods.GetWindowIcon();               

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
                GetPrivateKeysResponse response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new GetPrivateKeysRequest() { KeyType = KeyType.AllViewSpend });

                if (response.Error.IsError)
                {
                    Logger.LogError("DKD.GASK", "Failed to query keys for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    tbxPublicViewKey.Text = response.PublicViewKey;
                    tbxPrivateViewKey.Text = response.PrivateViewKey;
                    tbxPublicSpendKey.Text = response.PublicSpendKey;
                    tbxPrivateSpendKey.Text = response.PrivateSpendKey;
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
                        tbxMnemonicSeed.Text = response.Mnemonic;
                        response = new GetPrivateKeysResponse();
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
                    // Reveal was true, so hide
                    tbxPrivateViewKey.RevealPassword = false;
                    btnShowHidePrivateViewKey.Content = "Show";
                }
                else
                {
                    // Reveal was false, so show
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
                    // Reveal was true, so hide
                    tbxPrivateSpendKey.RevealPassword = false;
                    btnShowHidePrivateSpendKey.Content = "Show";
                }
                else
                {
                    // Reveal was false, so show
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
                    // Reveal was true, so hide
                    tbxMnemonicSeed.RevealPassword = false;
                    btnShowHideSeed.Content = "Show";
                }
                else
                {
                    // Reveal was false, so show
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
                DialogResult result = new()
                {
                    IsCancel = true
                };

                Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CLBC", ex);
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            tbxPublicViewKey.Text = "";
            tbxPrivateViewKey.Text = "";
            tbxPublicSpendKey.Text = "";
            tbxPrivateSpendKey.Text = "";
            tbxMnemonicSeed.Text = "";

            base.OnClosing(e);
        }
        #endregion // Events
    }
}