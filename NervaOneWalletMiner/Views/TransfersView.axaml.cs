using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views
{
    public partial class TransfersView : UserControl
    {
        public TransfersView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.CONS", ex);
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
                    await DialogService.ShowAsync(new TransactionDetailsView(selectedItem.TransactionId, selectedItem.AccountIndex, selectedItem.Amount));
                }
                else
                {
                    Logger.LogDebug("TRA.OTDV", "Opening Transfer transaction details view");
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
