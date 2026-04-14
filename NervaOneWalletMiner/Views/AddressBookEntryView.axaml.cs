using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Linq;

namespace NervaOneWalletMiner.Views
{
    public partial class AddressBookEntryView : UserControl
    {
        private bool _isNew = true;
        private int _id = 0;

        public AddressBookEntryView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                Initialized += AddressBookEntryView_Initialized;
                Loaded += (_, _) => tbxName.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("ABE.CONS", ex);
            }
        }

        private void AddressBookEntryView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                if (DataContext is AddressBookEntryViewModel vm)
                {
                    _isNew = vm.IsNew;
                    _id = vm.Id;

                    tbxName.Text = vm.Name;
                    tbxDescription.Text = vm.Description;
                    tbxAddress.Text = vm.Address;
                    tbxPaymentId.Text = vm.PaymentId;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("ABE.INIT", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(tbxAddress.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Address Book", "Address is required", true));
                    return;
                }

                if (_isNew)
                {
                    int newId = GlobalData.AddressBook.List.Count > 0
                        ? GlobalData.AddressBook.List.Max(a => a.Id) + 1
                        : 0;

                    GlobalData.AddressBook.List.Add(new AddressInfo
                    {
                        Id = newId,
                        Name = tbxName.Text ?? string.Empty,
                        Description = tbxDescription.Text ?? string.Empty,
                        Address = tbxAddress.Text,
                        PaymentId = tbxPaymentId.Text ?? string.Empty
                    });

                    Logger.LogDebug("ABE.OKBC", "Added new address book entry: " + GlobalMethods.GetShorterString(tbxAddress.Text, 12));
                }
                else
                {
                    AddressInfo? existing = GlobalData.AddressBook.List.FirstOrDefault(a => a.Id == _id);
                    if (existing != null)
                    {
                        existing.Name = tbxName.Text ?? string.Empty;
                        existing.Description = tbxDescription.Text ?? string.Empty;
                        existing.Address = tbxAddress.Text;
                        existing.PaymentId = tbxPaymentId.Text ?? string.Empty;

                        Logger.LogDebug("ABE.OKBC", "Updated address book entry: " + GlobalMethods.GetShorterString(tbxAddress.Text, 12));
                    }
                }

                GlobalMethods.SaveAddressBook();
                UIManager.NavigateToAddressBook();
            }
            catch (Exception ex)
            {
                Logger.LogException("ABE.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("ABE.CNCL", "Address book entry cancelled");
                UIManager.NavigateToAddressBook();
            }
            catch (Exception ex)
            {
                Logger.LogException("ABE.CNCL", ex);
            }
        }
    }
}
