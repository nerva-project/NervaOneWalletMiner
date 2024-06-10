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

                // TODO: Check if wallet open and show error if not

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
                // TODO: For multi-coin support, need to make generic KeyType values and handle them in interface implementation

                QueryKeyResponse response = await GlobalData.WalletService.QueryKey(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new QueryKeyRequest() { KeyType = KeyType.AllViewSpend });

                if (response.Error.IsError)
                {
                    Logger.LogError("DKD.GASK", "Failed to query keys for " + GlobalData.OpenedWalletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                }
                else
                {
                    this.Get<TextBox>("tbxPublicViewKey").Text = response.PublicViewKey;
                    this.Get<TextBox>("tbxPrivateViewKey").Text = response.PrivateViewKey;
                    this.Get<TextBox>("tbxPublicSpendKey").Text = response.PublicSpendKey;
                    this.Get<TextBox>("tbxPrivateSpendKey").Text = response.PrivateSpendKey;
                    response = new QueryKeyResponse();


                    // Once you got keys, query mnemonic seed
                    response = await GlobalData.WalletService.QueryKey(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new QueryKeyRequest() { KeyType = KeyType.Mnemonic });
                    if (response.Error.IsError)
                    {
                        Logger.LogError("DKD.GASK", "Failed to query mnemonic seed for " + GlobalData.OpenedWalletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    }
                    else
                    {
                        this.Get<TextBox>("tbxMnemonicSeed").Text = response.Mnemonic;
                        response = new QueryKeyResponse();
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

                // TODO: Clear those even if user closes window without clicking close
                this.Get<TextBox>("tbxPublicViewKey").Text = "";
                this.Get<TextBox>("tbxPrivateViewKey").Text = "";
                this.Get<TextBox>("tbxPublicSpendKey").Text = "";
                this.Get<TextBox>("tbxPrivateSpendKey").Text = "";
                this.Get<TextBox>("tbxMnemonicSeed").Text = "";

                Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("DKD.CLBC", ex);
            }
        }
    }
}