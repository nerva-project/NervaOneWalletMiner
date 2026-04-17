using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.DataGrid;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class AddressPickerView : UserControl
    {
        private List<AddressInfo> _addresses = [];

        // Not used but designer will complain without it
        public AddressPickerView()
        {
            InitializeComponent();
        }

        public AddressPickerView(string title)
        {
            try
            {
                InitializeComponent();

                tbkTitle.Text = title;

                GlobalMethods.LoadAddressBook();
                _addresses = GlobalData.AddressBook.List.Where(a => !string.IsNullOrEmpty(a.Address)).ToList();

                lbxAddresses.ItemsSource = _addresses.Select(a =>
                    string.IsNullOrEmpty(a.Name)
                        ? GlobalMethods.GetShorterString(a.Address, 12)
                        : a.Name + " (" + GlobalMethods.GetShorterString(a.Address, 12) + ")").ToList();
            }
            catch (Exception ex)
            {
                Logger.LogException("APD.CONS", ex);
            }
        }

        private void SelectCurrent()
        {
            try
            {
                if (lbxAddresses.SelectedIndex < 0 || lbxAddresses.SelectedIndex >= _addresses.Count)
                {
                    return;
                }

                AddressInfo selected = _addresses[lbxAddresses.SelectedIndex];

                DialogResult result = new()
                {
                    IsOk = true,
                    SendToAddress = selected.Address,
                    SendPaymentId = selected.PaymentId
                };

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("APD.SELC", ex);
            }
        }

        public void AddressList_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs args)
        {
            SelectCurrent();
        }

        public void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (lbxAddresses.SelectedIndex < 0)
                {
                    _ = DialogService.ShowAsync(new MessageBoxView("Pick Address", "Please select an address.", true));
                    return;
                }

                SelectCurrent();
            }
            catch (Exception ex)
            {
                Logger.LogException("APD.OKBC", ex);
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

                DialogService.Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("APD.CLBC", ex);
            }
        }
    }
}
