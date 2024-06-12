using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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