using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class DisplayKeysSeedView : Window
    {
        public DisplayKeysSeedView()
        {
            InitializeComponent();

            // TODO: Check if wallet open and show error if not

            GetAndShowKeys();
        }

        private async void GetAndShowKeys()
        {
            try
            {
                // TODO: For multi-coin support, need to make generic KeyType values and handle them in interface implementation

                QueryKeyResponse response = await GlobalData.WalletService.QueryKey(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new QueryKeyRequest() { KeyType = "all_keys" });

                if (response.Error.IsError)
                {
                    Logger.LogError("WalSV.RFK", "Failed to query keys for " + GlobalData.OpenedWalletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                }
                else
                {
                    this.Get<TextBox>("tbxPublicViewKey").Text = response.PublicViewKey;
                    this.Get<TextBox>("tbxPrivateViewKey").Text = response.PrivateViewKey;
                    this.Get<TextBox>("tbxPublicSpendKey").Text = response.PublicSpendKey;
                    this.Get<TextBox>("tbxPrivateSpendKey").Text = response.PrivateSpendKey;
                    
                    // Once you got keys, query mnemonic seed
                    response = await GlobalData.WalletService.QueryKey(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new QueryKeyRequest() { KeyType = "mnemonic" });
                    if (response.Error.IsError)
                    {
                        Logger.LogError("WalSV.RFK", "Failed to query mnemonic seed for " + GlobalData.OpenedWalletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    }
                    else
                    {
                        this.Get<TextBox>("tbxMnemonicSeed").Text = response.Mnemonic;
                    }                        
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("DKSWal.GASK", ex);
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
                Logger.LogException("DKSWal.CBC", ex);
            }
        }
    }
}