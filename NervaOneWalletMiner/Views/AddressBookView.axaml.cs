using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NervaOneWalletMiner.Views
{
    public partial class AddressBookView : UserControl
    {
        private DataGridTextColumn? _colDescription;
        private DataGridTextColumn? _colPaymentId;

        public AddressBookView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                // Index 1=Description, 3=PaymentId (Name=0, Description=1, Address=2, PaymentId=3)
                _colDescription = (DataGridTextColumn)dtgAddressBook.Columns[1];
                _colPaymentId = (DataGridTextColumn)dtgAddressBook.Columns[3];

                // Prevent row selection from triggering RequestBringIntoView
                dtgAddressBook.AddHandler(
                    RequestBringIntoViewEvent,
                    (object? sender, RequestBringIntoViewEventArgs e) => { e.Handled = true; },
                    RoutingStrategies.Bubble);

            }
            catch (Exception ex)
            {
                Logger.LogException("ADB.CONS", ex);
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            PopulateAddressBookGrid();
        }

        private void AddressBookView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            try
            {
                if (e.NewSize.Width < 450)
                {
                    // Narrow: buttons below icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*");
                    Grid.SetRow(spHeaderButtons, 1);
                    Grid.SetColumn(spHeaderButtons, 0);
                    spHeaderButtons.Margin = new Thickness(0, 10, 5, 0);

                    // Narrow: Name + Address
                    if (_colDescription != null) { _colDescription.IsVisible = false; }
                    if (_colPaymentId != null) { _colPaymentId.IsVisible = false; }
                }
                else if (e.NewSize.Width < 700)
                {
                    // Medium: buttons inline
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(spHeaderButtons, 0);
                    Grid.SetColumn(spHeaderButtons, 2);
                    spHeaderButtons.Margin = new Thickness(0, 0, 5, 0);

                    // Medium: Name + Description + Address
                    if (_colDescription != null) { _colDescription.IsVisible = true; }
                    if (_colPaymentId != null) { _colPaymentId.IsVisible = false; }
                }
                else
                {
                    // Wide: buttons inline
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(spHeaderButtons, 0);
                    Grid.SetColumn(spHeaderButtons, 2);
                    spHeaderButtons.Margin = new Thickness(0, 0, 5, 0);

                    // Wide: all columns
                    if (_colDescription != null) { _colDescription.IsVisible = true; }
                    if (_colPaymentId != null) { _colPaymentId.IsVisible = true; }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("ADB.AVSC", ex);
            }
        }

        private void PopulateAddressBookGrid()
        {
            GlobalMethods.LoadAddressBook();
            if (DataContext is AddressBookViewModel vm)
            {
                vm.Addresses = new ObservableCollection<AddressInfo>(
                    GlobalData.AddressBook.List.Where(a => !string.IsNullOrEmpty(a.Address)));
            }
        }

        private void AddressBook_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            OpenEditView();
        }

        private void Add_Clicked(object sender, RoutedEventArgs args)
        {
            UIManager.NavigateToAddressBookEntry(true);
        }

        private void Edit_Clicked(object sender, RoutedEventArgs args)
        {
            OpenEditView();
        }

        private void OpenEditView()
        {
            try
            {
                if (dtgAddressBook.SelectedItem is AddressInfo selected)
                {
                    UIManager.NavigateToAddressBookEntry(false, selected.Id, selected.Name, selected.Description, selected.Address, selected.PaymentId);
                }
                else
                {
                    _ = DialogService.ShowAsync(new MessageBoxView("Edit Address", "Please select an address first.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("ADB.ODEV", ex);
            }
        }

        private async void Transfer_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (dtgAddressBook.SelectedItem is not AddressInfo selectedAddress)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer", "Please select an address first.", true));
                }
                else if (!GlobalData.IsWalletOpen)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer", "Please open wallet first.", true));
                }
                else
                {
                    Logger.LogDebug("ADB.TRCL", "Navigating to Transfer Funds for address: " + GlobalMethods.GetShorterString(selectedAddress.Address, 12));
                    UIManager.NavigateToTransferFunds(0, selectedAddress.Address, selectedAddress.PaymentId, SplitViewPages.AddressBook);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("ADB.TRCL", ex);
            }
        }

        private async void Delete_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (dtgAddressBook.SelectedItem is not AddressInfo selectedAddress)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Delete", "Please select an address first.", true));
                }
                else
                {
                    Logger.LogDebug("ADB.DELC", "Deleting address with name: " + selectedAddress.Name);
                    GlobalData.AddressBook.List.Remove(selectedAddress);
                    GlobalMethods.SaveAddressBook();
                    PopulateAddressBookGrid();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("ADB.DELC", ex);
            }
        }
    }
}
