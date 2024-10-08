using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using System;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class TransactionDetailsView : Window
    {
        private string _transactionId = string.Empty;
        private int _accountIndex = 0;
        private decimal _amount = 0;

        // Not used but designer will complain without it
        public TransactionDetailsView()
        {
            InitializeComponent();
        }
        
        public TransactionDetailsView(string transactionId, int accountIndex, decimal amount)
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();

                _transactionId = transactionId;
                _accountIndex = accountIndex;
                _amount = amount;

                GetAndShowTransactionDetails();
            }
            catch (Exception ex)
            {
                Logger.LogException("TDD.CONS", ex);
            }
        }

        private async void GetAndShowTransactionDetails()
        {
            try
            {
                GetTranserByTxIdRequest request = new GetTranserByTxIdRequest();
                request.TransactionId = _transactionId;
                request.AccountIndex = _accountIndex;
                request.Amount = _amount;

                GetTransferByTxIdResponse response = await GlobalData.WalletService.GetTransferByTxId(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("TDD.GSTD", "Failed to get transaction by ID | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    this.Get<TextBox>("tbxYourAddress").Text = response.Address;                    
                    this.Get<TextBox>("tbxTransactionId").Text = response.TransactionId;

                    this.Get<Label>("lblPaymentId").Content = response.PaymentId;
                    this.Get<Label>("lblNote").Content = response.Note;
                    this.Get<Label>("lblType").Content = response.Type;
                    this.Get<Label>("lblHeight").Content = response.Height;
                    this.Get<Label>("lblAmount").Content = response.Amount;
                    this.Get<Label>("lblFee").Content = response.Fee;                    
                    this.Get<Label>("lblTime").Content = response.Timestamp;
                    this.Get<Label>("lblConfirmations").Content = response.Confirmations;

                    if (response.Destinations.Count > 0)
                    {
                        this.Get<SelectableTextBlock>("stbkDestinationInfo").Text = string.Empty;

                        foreach (string destination in response.Destinations)
                        {
                            this.Get<SelectableTextBlock>("stbkDestinationInfo").Text += destination + "\r\n";
                        }
                    }
                    else
                    {
                        this.Get<SelectableTextBlock>("stbkDestinationInfo").Text = "Not available\r\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TDD.GSTD", ex);
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
                Logger.LogException("TDD.CLBC", ex);
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
                Logger.LogException("TDD.CYAC", ex);
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
                Logger.LogException("TDD.CTIC", ex);
            }
        }
    }
}
