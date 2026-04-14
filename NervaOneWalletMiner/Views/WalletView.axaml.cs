using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.Layout;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class WalletView : UserControl
    {
        private DataGridTextColumn? _colId;
        private DataGridTextColumn? _colAddress;
        private DataGridTextColumn? _colUnlocked;

        public WalletView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                // Index 1=Id, 3=Address, 5=Unlocked (icon=0, Label=2, Balance=4)
                _colId = (DataGridTextColumn)dtgAccounts.Columns[1];
                _colAddress = (DataGridTextColumn)dtgAccounts.Columns[3];
                _colUnlocked = (DataGridTextColumn)dtgAccounts.Columns[5];

                // Prevent row selection from scrolling the outer ScrollViewer
                dtgAccounts.AddHandler(
                    RequestBringIntoViewEvent,
                    (object? sender, RequestBringIntoViewEventArgs e) => { e.Handled = true; },
                    RoutingStrategies.Bubble);

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsCpuMiningSupported)
                {
                    miStartMining.IsEnabled = false;
                }

                Initialized += WalletView_Initialized;
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.CONS", ex);
            }
        }

        private void WalletView_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            try
            {
                if (e.NewSize.Width < 450)
                {
                    // Narrow: Open Wallet button below icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*");
                    Grid.SetRow(btnOpenCloseWallet, 1);
                    Grid.SetColumn(btnOpenCloseWallet, 0);

                    // Narrow: Transfer/Address buttons below balances
                    grdStats.ColumnDefinitions = ColumnDefinitions.Parse("200,Auto");
                    grdBalances.RowDefinitions = RowDefinitions.Parse("26,26");
                    Grid.SetColumn(grdWalletButtons, 0);
                    Grid.SetRow(grdWalletButtons, 1);
                    grdWalletButtons.HorizontalAlignment = HorizontalAlignment.Left;

                    // Narrow: icon + Label + Balance
                    if (_colId != null) { _colId.IsVisible = false; }
                    if (_colAddress != null) { _colAddress.IsVisible = false; }
                    if (_colUnlocked != null) { _colUnlocked.IsVisible = false; }
                }
                else if (e.NewSize.Width < 700)
                {
                    // Medium: Open Wallet button on the right of icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(btnOpenCloseWallet, 0);
                    Grid.SetColumn(btnOpenCloseWallet, 2);

                    // Medium: Transfer/Address buttons on the right
                    grdStats.ColumnDefinitions = ColumnDefinitions.Parse("200,*,200");
                    grdBalances.RowDefinitions = RowDefinitions.Parse("35,35");
                    Grid.SetColumn(grdWalletButtons, 2);
                    Grid.SetRow(grdWalletButtons, 0);
                    grdWalletButtons.HorizontalAlignment = HorizontalAlignment.Right;

                    // Medium: icon + Label + Address + Balance
                    if (_colId != null) { _colId.IsVisible = false; }
                    if (_colAddress != null) { _colAddress.IsVisible = true; }
                    if (_colUnlocked != null) { _colUnlocked.IsVisible = false; }
                }
                else
                {
                    // Wide: Open Wallet button on the right of icon/label
                    grdHeader.ColumnDefinitions = ColumnDefinitions.Parse("Auto,*,Auto");
                    Grid.SetRow(btnOpenCloseWallet, 0);
                    Grid.SetColumn(btnOpenCloseWallet, 2);

                    // Wide: Transfer/Address buttons on the right
                    grdStats.ColumnDefinitions = ColumnDefinitions.Parse("200,*,200");
                    grdBalances.RowDefinitions = RowDefinitions.Parse("35,35");
                    Grid.SetColumn(grdWalletButtons, 2);
                    Grid.SetRow(grdWalletButtons, 0);
                    grdWalletButtons.HorizontalAlignment = HorizontalAlignment.Right;

                    // Wide: all columns
                    if (_colId != null) { _colId.IsVisible = true; }
                    if (_colAddress != null) { _colAddress.IsVisible = true; }
                    if (_colUnlocked != null) { _colUnlocked.IsVisible = true; }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.WVSC", ex);
            }
        }

        private void WalletView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                if (!GlobalData.AreWalletEventsRegistered)
                {
                    WalletViewModel vm = (WalletViewModel)DataContext!;
                    vm.TransferUiEvent += (toAddress, paymentId) => ShowTransferDialog(toAddress, paymentId);
                    vm.CloseWalletNonUiEvent += CloseUserWalletNonUi;
                    GlobalData.AreWalletEventsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.WAVI", ex);
            }
        }

        #region Open Wallet
        public async void OpenCloseWallet_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

                if (btnOpenCloseWallet.Content!.ToString()!.Equals(StatusWallet.OpenWallet))
                {
                    if (!GlobalData.IsCliToolsFound)
                    {
                        Logger.LogDebug("WAL.OCWC", "Trying to open wallet but CLI tools not found");
                        await DialogService.ShowAsync(new MessageBoxView("Open Wallet", "Client tools missing. Cannot open wallet until client tools are downloaded and running", true));
                    }
                    else if (!GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly && !GlobalData.IsInitialDaemonConnectionSuccess)
                    {
                        Logger.LogDebug("WAL.OCWC", "Trying to open wallet but daemon not running");
                        await DialogService.ShowAsync(new MessageBoxView("Open Wallet", "Daemon not running. Cannot open wallet until connection is established", true));
                    }
                    else
                    {
                        Logger.LogDebug("WAL.OCWC", "Navigating to Open Wallet page");
                        UIManager.NavigateToOpenWallet();
                    }
                }
                else
                {
                    // Close wallet
                    await CloseUserWallet();
                    btnOpenCloseWallet.Content = StatusWallet.OpenWallet;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.OCWC", ex);
            }
        }
        #endregion // Open Wallet

        #region Create Account
        private void CreateAccount_Clicked(object sender, RoutedEventArgs args)
        {
            ShowCreateAccount();
        }

        private async void ShowCreateAccount()
        {
            try
            {
                var window = new TextBoxView(title: "Create Account", labelValue: "Account Label", textValue: string.Empty, textWatermark: "Enter new account label", isTextRequired: false, okButtonText: "Create");
                DialogResult? result = await DialogService.ShowAsync<DialogResult>(window);
                if (result != null && result.IsOk)
                {
                    CreateAccountRequest request = new()
                    {
                        Label = result.TextBoxValue
                    };

                    CreateAccountResponse response = await GlobalData.WalletService.CreateAccount(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                    if (response.Error.IsError)
                    {
                        Logger.LogError("WAL.SRCA", "Failed to create account " + request.Label + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        await DialogService.ShowAsync(new MessageBoxView("Create Account", "Error creating account\r\n" + response.Error.Message, true));
                    }
                    else
                    {
                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                        {
                            Logger.LogDebug("WAL.SRCA", "New account created successfully");
                            GlobalMethods.SaveWallet();
                        }

                        await DialogService.ShowAsync(new MessageBoxView("Create Account", "Account created successfully!", true));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.SRCA", ex);
            }
        }
        #endregion Create Account

        #region Rename Label
        private void RenameLabel_Clicked(object sender, RoutedEventArgs args)
        {
            ShowRenameLabel();
        }

        private async void ShowRenameLabel()
        {
            try
            {
                if (dtgAccounts.SelectedItem != null)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;
                    var window = new TextBoxView(title: "Change Account Label", labelValue: "Account Label", textValue: selectedItem.Label, textWatermark: string.Empty, isTextRequired: false, okButtonText: "Save");
                    DialogResult? result = await DialogService.ShowAsync<DialogResult>(window);
                    if (result != null && result.IsOk)
                    {
                        LabelAccountRequest request = new()
                        {
                            AccountIndex = selectedItem.Index,
                            Label = result.TextBoxValue
                        };

                        LabelAccountResponse response = await GlobalData.WalletService.LabelAccount(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                        if (response.Error.IsError)
                        {
                            Logger.LogError("WAL.SHRL", "Failed to rename account " + request.Label + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                            await DialogService.ShowAsync(new MessageBoxView("Rename Account", "Error renaming account\r\n" + response.Error.Message, true));
                        }
                        else
                        {
                            Logger.LogDebug("WAL.SHRL", "Account label changed successfully to " + request.Label);
                            UIManager.CallWalletDataMethodsInSync();

                            if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                            {
                                GlobalMethods.SaveWallet();
                            }

                            await DialogService.ShowAsync(new MessageBoxView("Rename Account", "Account label changed successfully!", true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.SHRL", ex);
            }
        }
        #endregion Rename Label

        #region Transfer Funds
        private void TransferFunds_Clicked(object sender, RoutedEventArgs args)
        {
            ShowTransferDialog(string.Empty, string.Empty);
        }

        public async void ShowTransferDialog(string toAddress, string paymentId)
        {
            try
            {
                if (GlobalData.IsWalletOpen)
                {
                    var dtgAccounts = this.Get<DataGrid>("dtgAccounts");

                    uint selectedIndex = 0;

                    if (dtgAccounts.SelectedItem != null)
                    {
                        Account selectedItem = (Account)dtgAccounts.SelectedItem;
                        selectedIndex = selectedItem.Index;
                    }

                    UIManager.NavigateToTransferFunds(selectedIndex, toAddress, paymentId);
                }
                else
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Please open wallet first.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.STRD", ex);
            }
        }
        #endregion // Transfer Funds

        #region Address Info
        private void AddressInfo_Clicked(object sender, RoutedEventArgs args)
        {
            OpenAddressInfoView();
        }

        private void DtgAccounts_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            OpenAddressInfoView();
        }

        private async void OpenAddressInfoView()
        {
            try
            {
                if (GlobalData.IsWalletOpen)
                {
                    int accountIndex = 0;
                    if (dtgAccounts.SelectedItem != null)
                    {
                        accountIndex = (int)((Account)dtgAccounts.SelectedItem).Index;
                    }

                    UIManager.NavigateToAddressInfo(accountIndex);
                }
                else
                {
                    await DialogService.ShowAsync(new MessageBoxView("Address Info", "Please open wallet first.", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.OAIV", ex);
            }
        }
        #endregion // Address Info

        #region Close Wallet
        private async Task<bool> CloseUserWalletNonUi()
        {
            // Need to wait until wallet is closed or might as well not even attempt this
            await CloseUserWallet(false);
            return true;
        }

        private async Task<bool> CloseUserWallet(bool isUiThread = true)
        {
            bool isSuccess = false;

            try
            {
                GlobalMethods.WalletClosedOrErrored();

                CloseWalletRequest request = new CloseWalletRequest
                {
                    WalletName = GlobalData.OpenedWalletName
                };

                CloseWalletResponse response = await GlobalData.WalletService.CloseWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WAL.CLUW", "Error closing wallet " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);

                    if (isUiThread)
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await DialogService.ShowAsync(new MessageBoxView("Close Wallet", "Error closing wallet\r\n" + response.Error.Message, true));
                        });
                    }
                }
                else
                {
                    isSuccess = true;
                    Logger.LogDebug("WAL.CLUW", "Wallet closed successfully");

                    if (isUiThread)
                    {
                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await DialogService.ShowAsync(new MessageBoxView("Close Wallet", "Wallet closed successfully!", true));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.CLUW", ex);
            }

            return isSuccess;
        }
        #endregion //Close Wallet

        #region Start Mining
        private void StartMining_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (dtgAccounts.SelectedItem != null)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;

                    if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != selectedItem.AddressFull)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = selectedItem.AddressFull;
                        Logger.LogDebug("WAL.STMC", "Setting and saving new mining address: " + GlobalMethods.GetShorterString(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress, 12));
                        GlobalMethods.SaveConfig();
                    }

                    Logger.LogDebug("WAL.STMC", "Calling Start Mining Daemon method");
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningUi(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                }
                else
                {
                    Logger.LogDebug("WAL.STMC", "No rows selected");
                    _ = DialogService.ShowAsync(new MessageBoxView("Start Mining", "Please select address to start mining to it", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.STMC", ex);
            }
        }
        #endregion // Start Mining

        #region Export Transactions
        public async void ExportSelected_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (dtgAccounts.SelectedItem != null)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;
                    string exportFile = GlobalMethods.GetExportFileNameWithPath(selectedItem.Label);

                    GetTransfersExportRequest request = new()
                    {
                        AccountIndex = selectedItem.Index,
                        IsAllAccounts = false
                    };

                    GetTransfersExportResponse response = await GlobalMethods.ExportTranfers(request, exportFile);
                    if (response.Error.IsError)
                    {
                        Logger.LogError("WAL.EXSC", "GetExport Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        await DialogService.ShowAsync(new MessageBoxView("Export Selected", "Error exporting:\r\n" + response.Error.Message, true));
                    }
                    else
                    {
                        await DialogService.ShowAsync(new TextBoxView(title: "Export Selected", labelValue: "Transactions have been exported to below file", textValue: exportFile, textWatermark: string.Empty));
                    }
                }
                else
                {
                    Logger.LogDebug("WAL.EXSC", "No rows selected");
                    await DialogService.ShowAsync(new MessageBoxView("Export Selected", "Please select addres to export", true));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.EXSC", ex);
            }
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
                        Logger.LogError("WAL.EXAC", "GetExport Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
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
                Logger.LogException("WAL.EXAC", ex);
            }
        }
        #endregion // Export Transactions
    }
}
