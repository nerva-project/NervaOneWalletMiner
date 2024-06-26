using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;
using System.Collections.Generic;
using Avalonia.Threading;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.ViewModels;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class AddressInfoView : Window
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        // <Label + AddressShort, AddressFull>
        Dictionary<string, string> _accounts = [];

        // Not used but designer will complain without it
        public AddressInfoView()
        {            
            InitializeComponent();
        }

        public AddressInfoView(int accountIndex)
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].AreIntegratedAddressesSupported)
                {
                    btnMakeIntegratedAddress.IsEnabled = false;
                    tbxIntegratedAddress.IsEnabled = false;
                    btnCopyIntegratedAddressToClipboard.IsEnabled = false;
                    tbxPaymentId.IsEnabled = false;
                    btnCopyPaymentIdToClipboard.IsEnabled = false;
                }

                foreach (Account account in ((WalletViewModel)GlobalData.ViewModelPages[SplitViewPages.Wallet]).WalletAddresses)
                {
                    string accountValue = string.IsNullOrEmpty(account.Label) ? "No label" + " (" + account.AddressShort + ")" : account.Label + " (" + account.AddressShort + ")";
                    if (!_accounts.ContainsKey(accountValue))
                    {
                        _accounts.Add(accountValue, account.AddressFull);
                    }
                }

                var cbxAccount = this.Get<ComboBox>("cbxAccount");
                cbxAccount.ItemsSource = _accounts.Keys;
                cbxAccount.SelectedIndex = accountIndex;
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.CONS", ex);
            }
        }

        private void AccountSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var cbxAccount = this.Get<ComboBox>("cbxAccount");
                var tbxWalletAddress = this.Get<TextBox>("tbxWalletAddress");
                if(_accounts.ContainsKey(cbxAccount.SelectedValue?.ToString()!))
                {
                    tbxWalletAddress.Text = _accounts[cbxAccount.SelectedValue?.ToString()!];
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.ASC1", ex);
            }                       
        }

        public void MakeIntegratedAddress_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                MakeIntegratedAddress(tbxWalletAddress.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.MIAC", ex);
            }
        }

        private async void MakeIntegratedAddress(string walletAddress)
        {
            try
            {
                // TODO: Current GUI does not pass payment_id. Do we need it?
                MakeIntegratedAddressRequest request = new()
                {
                    StandardAddress = walletAddress,
                };

                Logger.LogError("AID.MIA1", "Making integrated address for: " + GlobalMethods.GetShorterString(walletAddress, 12));
                MakeIntegratedAddressResponse response = await GlobalData.WalletService.MakeIntegratedAddress(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("AID.MIA1", "Failed to make integrated address | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Make Integrated Address", "Error making integrated address\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogError("AID.MIA1", "Integrated address created successfully");
                    tbxIntegratedAddress.Text = response.IntegratedAddress;
                    tbxPaymentId.Text = response.PaymentId;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.MIA1", ex);
            }
        }

        public void CloseButton_Clicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("AID.CBC1", ex);
            }
        }

        public void CopyWalletToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxWalletAddress.Text!);            
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.CWCC", ex);
            }
        }

        public void CopyIntegratedAddressToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxIntegratedAddress.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.CIAC", ex);
            }
        }

        public void CopyPaymentIdToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxPaymentId.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("AID.CPIC", ex);
            }
        }
    }
}