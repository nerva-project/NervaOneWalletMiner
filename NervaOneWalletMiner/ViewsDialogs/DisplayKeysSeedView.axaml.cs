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
        public DisplayKeysSeedView()
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();               

                GetAndShowKeys();
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CONS", ex);
            }
        }

        private async void GetAndShowKeys()
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

        public void CancelButtonClicked(object sender, RoutedEventArgs args)
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
    }
}