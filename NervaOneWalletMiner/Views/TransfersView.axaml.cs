using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.ViewsDialogs;
using System;

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
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.CONS", ex);
            }            
        }

        public void TransactionDetailsClicked(object sender, RoutedEventArgs args)
        {
            OpenTransactionDetailsView();
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
                    var window = new TransactionDetailsView(selectedItem.TransactionId, selectedItem.AccountIndex);
                    window.ShowDialog(GetWindow());
                }
                else
                {
                    Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Transaction Details", "Please select transaction", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }

            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.TDC", ex);
            }
        }
    }
}