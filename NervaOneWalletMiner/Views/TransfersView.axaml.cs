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
            InitializeComponent();
        }

        public async void TransactionDetailsClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var dtgTransactions = this.Get<DataGrid>("dtgTransactions");

                if(dtgTransactions.SelectedItem != null)
                {
                    Transfer selectedItem = (Transfer)dtgTransactions.SelectedItem;
                    var window = new TransactionDetailsView(selectedItem.TransactionId, selectedItem.AccountIndex);
                    await window.ShowDialog(GetWindow());
                }
                else
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Transaction Details", "Please select transaction", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }

            }
            catch (Exception ex)
            {
                Logger.LogException("TranV.TDC", ex);
            }
        }
    }
}