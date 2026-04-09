using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects.DataGrid;
using System;
using NervaOneWalletMiner.Helpers;
using System.Collections.Generic;
using Avalonia.Threading;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;

namespace NervaOneWalletMiner.Views
{
    public partial class AddressInfoView : UserControl
    {
        // <display key, Account>
        Dictionary<string, Account> _accounts = [];

        public AddressInfoView()
        {
            InitializeComponent();
            Initialized += AddressInfoView_Initialized;
        }

        private void AddressInfoView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                int accountIndex = DataContext is AddressInfoViewModel vm ? vm.AccountIndex : 0;

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].AreIntegratedAddressesSupported)
                {
                    btnMakeIntegratedAddress.IsEnabled = false;
                    tbxIntegratedAddress.IsEnabled = false;
                    btnCopyIntegratedAddressToClipboard.IsEnabled = false;
                    tbxPaymentId.IsEnabled = false;
                    btnCopyPaymentIdToClipboard.IsEnabled = false;
                }

                foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
                {
                    string accountKey = string.IsNullOrEmpty(account.Label) ? "No label" + " (" + account.AddressShort + ")" : account.Label + " (" + account.AddressShort + ")";
                    if (!_accounts.ContainsKey(accountKey))
                    {
                        _accounts.Add(accountKey, account);
                    }
                }

                cbxAccount.ItemsSource = _accounts.Keys;
                cbxAccount.SelectedIndex = accountIndex;
            }
            catch (Exception ex)
            {
                Logger.LogException("AIV.INIT", ex);
            }
        }

        private void AccountSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string key = cbxAccount.SelectedValue?.ToString()!;
                if (_accounts.TryGetValue(key, out Account? account))
                {
                    tbxWalletAddress.Text = account.AddressFull;
                    tbxWalletLabel.Text = account.Label;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("AIV.ASC1", ex);
            }
        }

        public async void SaveLabel_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string key = cbxAccount.SelectedValue?.ToString()!;
                if (_accounts.TryGetValue(key, out Account? account))
                {
                    LabelAccountRequest request = new()
                    {
                        AccountIndex = account.Index,
                        Label = tbxWalletLabel.Text ?? string.Empty
                    };

                    Logger.LogDebug("AIV.SVLC", "Saving label for account " + account.Index + ": " + request.Label);
                    LabelAccountResponse response = await GlobalData.WalletService.LabelAccount(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                    if (response.Error.IsError)
                    {
                        Logger.LogError("AIV.SVLC", "Failed to save label | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await DialogService.ShowAsync(new MessageBoxView("Save Label", "Error saving label\r\n" + response.Error.Message, true));
                        });
                    }
                    else
                    {
                        Logger.LogDebug("AIV.SVLC", "Label saved successfully");
                        account.Label = tbxWalletLabel.Text ?? string.Empty;
                        UIManager.CallWalletDataMethodsInSync();

                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                        {
                            GlobalMethods.SaveWallet();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("AIV.SVLC", ex);
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
                Logger.LogException("AIV.MIAC", ex);
            }
        }

        private async void MakeIntegratedAddress(string walletAddress)
        {
            try
            {
                MakeIntegratedAddressRequest request = new()
                {
                    StandardAddress = walletAddress,
                };

                Logger.LogDebug("AIV.MIA1", "Making integrated address for: " + GlobalMethods.GetShorterString(walletAddress, 12));
                MakeIntegratedAddressResponse response = await GlobalData.WalletService.MakeIntegratedAddress(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("AIV.MIA1", "Failed to make integrated address | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Make Integrated Address", "Error making integrated address\r\n" + response.Error.Message, true));
                    });
                }
                else
                {
                    Logger.LogDebug("AIV.MIA1", "Integrated address created successfully");
                    tbxIntegratedAddress.Text = response.IntegratedAddress;
                    tbxPaymentId.Text = response.PaymentId;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("AIV.MIA1", ex);
            }
        }

        public void BackButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("AIV.BACK", "Navigating back to Wallet");
                UIManager.NavigateToPage(Objects.Constants.SplitViewPages.Wallet);
            }
            catch (Exception ex)
            {
                Logger.LogException("AIV.BACK", ex);
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
                Logger.LogException("AIV.CWCC", ex);
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
                Logger.LogException("AIV.CIAC", ex);
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
                Logger.LogException("AIV.CPIC", ex);
            }
        }
    }
}
