using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
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
        private DataGridTextColumn? _colConf;
        private DataGridTextColumn? _colAddress;

        public TransfersView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                // Index 1=Height, 2=Conf., 5=Address (icon=0, Time=3, Amount=4)
                _colHeight = (DataGridTextColumn)dtgTransactions.Columns[1];
                _colConf = (DataGridTextColumn)dtgTransactions.Columns[2];
                _colAddress = (DataGridTextColumn)dtgTransactions.Columns[5];

                // Prevent row selection from triggering RequestBringIntoView
                dtgTransactions.AddHandler(
                    RequestBringIntoViewEvent,
                    (object? sender, RequestBringIntoViewEventArgs e) => { e.Handled = true; },
                    RoutingStrategies.Bubble);

                Initialized += TransfersView_Initialized;
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.CONS", ex);
            }
        }

        private void TransfersView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                // Master timer refreshes this too but that can take a few seconds
                UIManager.RefreshTransfersEmptyState();
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.TRVI", ex);
            }
        }

        #region Empty State
        // Wallets are created and restored on Wallet Setup, and opened on Wallet screen. Transfers has
        // nothing of its own to offer here, so it just points at whichever one user needs
        public void EmptySetUpWallet_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("TRA.ESWC", "Navigating to Wallet Setup page");
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.ESWC", ex);
            }
        }

        public void EmptyGoToWallet_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("TRA.EGWC", "Navigating to Wallet page");
                UIManager.NavigateToPage(SplitViewPages.Wallet);
            }
            catch (Exception ex)
            {
                Logger.LogException("TRA.EGWC", ex);
            }
        }

        // Alignment has to be set on buttons themselves. Setting it on their panel is not enough as
        // buttons keep their own width and end up sitting off to the side of centered message
        private void SetEmptyStateButtonAlignment(HorizontalAlignment alignment)
        {
            btnEmptySetUpWallet.HorizontalAlignment = alignment;
            btnEmptyGoToWallet.HorizontalAlignment = alignment;
        }
        #endregion // Empty State

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
                    btnTransactionDetails.Margin = new Thickness(0, 10, 5, 0);

                    // Narrow: icon + Time + Amount
                    if (_colHeight != null) { _colHeight.IsVisible = false; }
                    if (_colConf != null) { _colConf.IsVisible = false; }
                    if (_colAddress != null) { _colAddress.IsVisible = false; }

                    // Narrow: full width empty state button, anchored to top so it stays put instead of
                    // floating in middle of a tall phone screen
                    spEmptyState.VerticalAlignment = VerticalAlignment.Top;
                    spEmptyStateButtons.HorizontalAlignment = HorizontalAlignment.Stretch;
                    SetEmptyStateButtonAlignment(HorizontalAlignment.Stretch);
                }
                else if (e.NewSize.Width < 700)
                {
                    // Medium: button inline
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(btnTransactionDetails, 0);
                    Grid.SetColumn(btnTransactionDetails, 2);
                    btnTransactionDetails.Margin = new Thickness(0, 0, 5, 0);

                    // Medium: icon + Height + Conf. + Time + Amount
                    if (_colHeight != null) { _colHeight.IsVisible = true; }
                    if (_colConf != null) { _colConf.IsVisible = true; }
                    if (_colAddress != null) { _colAddress.IsVisible = false; }

                    // Medium: empty state button centered
                    spEmptyState.VerticalAlignment = VerticalAlignment.Center;
                    spEmptyStateButtons.HorizontalAlignment = HorizontalAlignment.Center;
                    SetEmptyStateButtonAlignment(HorizontalAlignment.Center);
                }
                else
                {
                    // Wide: button inline
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(btnTransactionDetails, 0);
                    Grid.SetColumn(btnTransactionDetails, 2);
                    btnTransactionDetails.Margin = new Thickness(0, 0, 5, 0);

                    // Wide: all columns
                    if (_colHeight != null) { _colHeight.IsVisible = true; }
                    if (_colConf != null) { _colConf.IsVisible = true; }
                    if (_colAddress != null) { _colAddress.IsVisible = true; }

                    // Wide: empty state button centered
                    spEmptyState.VerticalAlignment = VerticalAlignment.Center;
                    spEmptyStateButtons.HorizontalAlignment = HorizontalAlignment.Center;
                    SetEmptyStateButtonAlignment(HorizontalAlignment.Center);
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
                        await DialogService.ShowAsync(new TextBoxView(title: "Export All", labelValue: "Transactions have been exported to below file", textValue: exportFile, textWatermark: string.Empty));
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
