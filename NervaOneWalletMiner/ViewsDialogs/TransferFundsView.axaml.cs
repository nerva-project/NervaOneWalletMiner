using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.DataGrid;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class TransferFundsView : Window
    {
        Dictionary<uint, string> _accounts = [];
        public TransferFundsView()
        {
            InitializeComponent();

            foreach(Account account in GlobalData.WalletStats.Subaddresses.Values)
            {
                if(!_accounts.ContainsKey(account.Index))
                {
                    _accounts.Add(account.Index, string.IsNullOrEmpty(account.Label) ? "No label" : account.Label + " (" + account.AddressShort + ")");
                }
            }

            var cbxSendFrom = this.Get<ComboBox>("cbxSendFrom");

            cbxSendFrom.ItemsSource = _accounts.Values;
            cbxSendFrom.SelectedIndex = 0;            
        }

        public void OkButtonClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var cbxSendFrom = this.Get<ComboBox>("cbxSendFrom");
                var tbxSendTo = this.Get<TextBox>("tbxSendTo");
                var tbxAmount = this.Get<TextBox>("tbxAmount");
                var tbxPaymentId = this.Get<TextBox>("tbxPaymentId");

                if (string.IsNullOrEmpty(tbxSendTo.Text) || string.IsNullOrEmpty(tbxAmount.Text))
                {
                    // TODO:  Let user know that Send To and Amount are required

                }
                else
                {
                    uint fromAccountIndex = 0;
                    string? fromAddress = cbxSendFrom.SelectedValue?.ToString();

                    // TODO: Since addresses are shortened, you could potentally have 2 of the same ones
                    foreach (uint index in _accounts.Keys)
                    {
                        if(_accounts[index].Equals(fromAddress))
                        {
                            fromAccountIndex = index;
                        }
                    }

                    DialogResult result = new()
                    {
                        IsOk = true,
                        SendFromAddress = fromAddress,
                        SendFromAddressIndex = fromAccountIndex,
                        SendToAddress = tbxSendTo.Text,
                        SendAmount = Convert.ToDouble(tbxAmount.Text),
                        SendPaymentId = tbxPaymentId.Text
                    };

                    Close(result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TFWal.OBC", ex);
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
                Logger.LogException("TFWal.CBC", ex);
            }
        }
    }
}
