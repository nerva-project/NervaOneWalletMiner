using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.IO;

namespace NervaOneWalletMiner.Views
{
    public partial class TransfersView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

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

                    string fileName = GlobalData.WalletExportFileName + "_" + DateTime.Now.ToString("yyyMMdd_hhmmss") + ".csv";
                    string exportFile = Path.Combine(GlobalData.ExportsDir, fileName);

                    GetExportRequest request = new()
                    {
                        IsAllAccounts = true
                    };

                    GetExportResponse response = await GlobalMethods.ExportTranfers(request, exportFile);
                    if (response.Error.IsError)
                    {
                        Logger.LogError("TRA.EXAC", "ExportTranfers Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        MessageBoxView window = new("Export Selected", "Please select addres to export", true);
                        await window.ShowDialog(GetWindow());
                    }
                    else
                    {
                        var window = new TextBoxView("Export All", "Transactions have been exported to below file", exportFile, string.Empty);
                        await window.ShowDialog(GetWindow());
                    }
                }
                else
                {
                    MessageBoxView window = new("Export All", "Please open wallet first.", true);
                    await window.ShowDialog(GetWindow());
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

        private void OpenTransactionDetailsView()
        {
            try
            {
                var dtgTransactions = this.Get<DataGrid>("dtgTransactions");

                if (dtgTransactions.SelectedItem != null)
                {
                    Transfer selectedItem = (Transfer)dtgTransactions.SelectedItem;
                    var window = new TransactionDetailsView(selectedItem.TransactionId, selectedItem.AccountIndex, selectedItem.Amount);
                    window.ShowDialog(GetWindow());
                }
                else
                {
                    Logger.LogDebug("TRA.OTDV", "Opening Transfer transaction details view");
                    Dispatcher.UIThread.Invoke(async () =>
                    {                        
                        MessageBoxView window = new("Transaction Details", "Please select transaction", true);
                        await window.ShowDialog(GetWindow());
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