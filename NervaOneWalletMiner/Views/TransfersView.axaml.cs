using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class TransfersView : UserControl
    {
        private DataGridTextColumn? _colHeight;
        private DataGridTextColumn? _colAddress;

        public TransfersView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                // Index 1=Height, 4=Address (icon=0, Time=2, Amount=3)
                _colHeight = (DataGridTextColumn)dtgTransactions.Columns[1];
                _colAddress = (DataGridTextColumn)dtgTransactions.Columns[4];
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.CONS", ex);
            }
        }

        private void TransfersView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            try
            {
                if (e.NewSize.Width < 450)
                {
                    // Narrow: button below icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*");
                    Grid.SetRow(btnTransactionDetails, 1);
                    Grid.SetColumn(btnTransactionDetails, 0);

                    if (_colHeight != null) { _colHeight.IsVisible = false; }
                    if (_colAddress != null) { _colAddress.IsVisible = false; }
                }
                else
                {
                    // Wide: button on the right of icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(btnTransactionDetails, 0);
                    Grid.SetColumn(btnTransactionDetails, 2);

                    if (_colHeight != null) { _colHeight.IsVisible = true; }
                    if (_colAddress != null) { _colAddress.IsVisible = true; }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.TVSC", ex);
            }
        }

        public void TransactionDetails_Clicked(object sender, RoutedEventArgs args)
        {
            OpenTransactionDetailsView();
        }

        public async void ExportAll_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (GlobalData.IsWalletOpen)
                {
                    string exportFile = GlobalMethods.GetExportFileNameWithPath();

                    GetTransfersExportRequest request = new()
                    {
                        IsAllAccounts = true
                    };

                    GetTransfersExportResponse response = await GlobalMethods.ExportTranfers(request, exportFile);
                    if (response.Error.IsError)
                    {
                        Logger.LogError("TRA.EXAC", "ExportTranfers Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        await DialogService.ShowAsync(new MessageBoxView("Export All", "Error exporting:\r\n" + response.Error.Message, true));
                    }
                    else
                    {
                        await DialogService.ShowAsync(new TextBoxView("Export All", "Transactions have been exported to below file", exportFile, string.Empty));
                    }
                }
                else
                {
                    await DialogService.ShowAsync(new MessageBoxView("Export All", "Please open wallet first.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.EXAC", ex);
            }
        }

        private void TransactionDetails_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            OpenTransactionDetailsView();
        }

        private async void OpenTransactionDetailsView()
        {
            try
            {
                var dtgTransactions = this.Get<DataGrid>("dtgTransactions");

                if (dtgTransactions.SelectedItem != null)
                {
                    Transfer selectedItem = (Transfer)dtgTransactions.SelectedItem;
                    UIManager.NavigateToTransactionDetails(selectedItem.TransactionId, selectedItem.AccountIndex, selectedItem.Amount);
                }
                else
                {
                    Logger.LogDebug("TRA.OTDV", "No transaction selected");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transaction Details", "Please select transaction", true));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.OTDV", ex);
            }
        }
    }
}
