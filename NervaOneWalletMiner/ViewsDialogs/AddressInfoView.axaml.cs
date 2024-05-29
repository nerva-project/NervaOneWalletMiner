using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Objects;
using System;
using NervaOneWalletMiner.Helpers;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class AddressInfoView : Window
    {
        // <Label + AddressShort, AddressFull>
        Dictionary<string, string> _accounts = [];

        public AddressInfoView()
        {
            InitializeComponent();
        }

        public AddressInfoView(int accountIndex)
        {
            InitializeComponent();

            foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
            {
                string accountValue = string.IsNullOrEmpty(account.Label) ? "No label" : account.Label + " (" + account.AddressShort + ")";
                if (!_accounts.ContainsKey(accountValue))
                {
                    _accounts.Add(accountValue, account.AddressFull);
                }
            }

            var cbxAccount = this.Get<ComboBox>("cbxAccount");
            cbxAccount.ItemsSource = _accounts.Keys;
            cbxAccount.SelectedIndex = accountIndex;
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
                Logger.LogException("AIWal.ASC", ex);
            }                       
        }

        public void MakeIntegratedAddressClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                // TODO: Implement
            }
            catch (Exception ex)
            {
                Logger.LogException("AIWal.MIGC", ex);
            }
        }

        public void CloseButtonClicked(object sender, RoutedEventArgs args)
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
                Logger.LogException("AIWal.CBC", ex);
            }
        }
    }
}