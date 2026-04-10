using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class TransactionDetailsView : UserControl
    {
        public TransactionDetailsView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                Initialized += TransactionDetailsView_Initialized;
            }
            catch (Exception ex)
            {
                Logger.LogException("TDV.CONS", ex);
            }
        }

        private void TransactionDetailsView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                if (DataContext is TransactionDetailsViewModel vm && !string.IsNullOrEmpty(vm.TransactionId))
                {
                    GetAndShowTransactionDetails(vm.TransactionId, vm.AccountIndex, vm.Amount);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TDV.INIT", ex);
            }
        }

        private async void GetAndShowTransactionDetails(string transactionId, int accountIndex, decimal amount)
        {
            try
            {
                GetTranserByTxIdRequest request = new()
                {
                    TransactionId = transactionId,
                    AccountIndex = accountIndex,
                    Amount = amount
                };

                GetTransferByTxIdResponse response = await GlobalData.WalletService.GetTransferByTxId(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("TDV.GSTD", "Failed to get transaction by ID | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    tbxYourAddress.Text = response.Address;
                    tbxTransactionId.Text = response.TransactionId;

                    lblType.Content = response.Type;
                    lblHeight.Content = response.Height;
                    lblAmount.Content = response.Amount;
                    lblFee.Content = response.Fee;
                    lblTime.Content = response.Timestamp;
                    lblConfirmations.Content = response.Confirmations;
                    lblPaymentId.Content = response.PaymentId;
                    lblNote.Content = response.Note;

                    if (response.Destinations.Count > 0)
                    {
                        stbkDestinationInfo.Text = string.Empty;
                        foreach (string destination in response.Destinations)
                        {
                            stbkDestinationInfo.Text += destination + "\r\n";
                        }
                    }
                    else
                    {
                        stbkDestinationInfo.Text = "Not available\r\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TDV.GSTD", ex);
            }
        }

        public void CloseButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                UIManager.NavigateToPage(SplitViewPages.Transfers);
            }
            catch (Exception ex)
            {
                Logger.LogException("TDV.CLBC", ex);
            }
        }

        public void CopyYourAddressToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxYourAddress.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("TDV.CYAC", ex);
            }
        }

        public void CopyTransactionIdToClipboard_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                GlobalMethods.CopyToClipboard(this, tbxTransactionId.Text!);
            }
            catch (Exception ex)
            {
                Logger.LogException("TDV.CTIC", ex);
            }
        }
    }
}
