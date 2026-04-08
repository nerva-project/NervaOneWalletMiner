using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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
        public WalletView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

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
                        // Open wallet dialog
                        Logger.LogDebug("WAL.OCWC", "Opening wallet dialog");
                        var window = new OpenWalletView();
                        DialogResult result = await window.ShowDialog<DialogResult>(TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner"));
                        if (result != null && result.IsOk)
                        {
                            OpenUserWallet(result.WalletName, result.WalletPassword);

                            // WalletPassword is zeroed inside OpenUserWallet's finally block but do it here again anyways
                            Array.Clear(result.WalletPassword, 0, result.WalletPassword.Length);
                        }
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

        private async void OpenUserWallet(string walletName, char[] walletPassword)
        {
            OpenWalletRequest request;

            try
            {
                request = new()
                {
                    WalletName = walletName,
                    Password = walletPassword
                };

                // Need this before OpenWallet as password will be cleared there
                string walletPasswordHash = Hashing.Hash(walletPassword);

                OpenWalletResponse response = await GlobalData.WalletService.OpenWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();

                    Logger.LogDebug("WAL.OUWT", "Error opening " + walletName + " wallet | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Open Wallet", "Error opening " + walletName + " wallet\r\n" + response.Error.Message, true));
                    });
                }
                else
                {
                    GlobalMethods.WalletJustOpened(walletName);

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                    {
                        GlobalData.WalletPassProvidedTime = DateTime.Now;
                        GlobalData.WalletPasswordHash = walletPasswordHash;
                    }

                    Logger.LogDebug("WAL.OUWT", "Wallet " + walletName + " opened successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.OUWT", ex);
            }
            finally
            {
                Array.Clear(walletPassword, 0, walletPassword.Length);
                request = new();
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
                var window = new TextBoxView("Create Account", "Account Label", string.Empty, "Enter new account label", false);
                DialogResult result = await DialogService.ShowAsync<DialogResult>(window);
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
                    var window = new TextBoxView("Change Account Label", "Account Label", selectedItem.Label, string.Empty, false);
                    DialogResult result = await DialogService.ShowAsync<DialogResult>(window);
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

                    var window = new TransferFundsView(selectedIndex, toAddress, paymentId);
                    DialogResult result = await window.ShowDialog<DialogResult>(TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner"));
                    if (result != null && result.IsOk)
                    {
                        if (!string.IsNullOrEmpty(result.SendToAddress) && result.SendAmount > 0)
                        {
                            if (result.IsSplitTranfer)
                            {
                                MakeTransferSplit(result.SendFromAddressIndex, result.SendToAddress, result.SendAmount, result.SendPaymentId, result.Priority);
                            }
                            else
                            {
                                MakeTransfer(result.SendFromAddressIndex, result.SendToAddress, result.SendAmount, result.SendPaymentId, result.Priority);
                            }
                        }
                    }
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

        private async void MakeTransfer(uint sendFromAccountIndex, string sendToAddress, decimal amount, string paymentId, string priority)
        {
            try
            {
                TransferRequest request = new()
                {
                    Destinations = [new() { Amount = amount, Address = sendToAddress }],
                    AccountIndex = sendFromAccountIndex,
                    Priority = priority,
                    PaymentId = paymentId
                };

                TransferResponse response = await GlobalData.WalletService.Transfer(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WAL.MKTR", "Transfer error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Transfer error\r\n\r\n" + response.Error.Message, true));
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.MKTR", "Transfer successful");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Transfer successful!", true));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.MKTR", ex);
            }
        }

        private async void MakeTransferSplit(uint sendFromAccountIndex, string sendToAddress, decimal amount, string paymentId, string priority)
        {
            try
            {
                TransferRequest request = new()
                {
                    Destinations = [new() { Amount = amount, Address = sendToAddress }],
                    AccountIndex = sendFromAccountIndex,
                    Priority = priority,
                    PaymentId = paymentId
                };

                TransferResponse response = await GlobalData.WalletService.TransferSplit(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WAL.MTSP", "Split transfer error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Split", "Transfer error\r\n\r\n" + response.Error.Message, true));
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.MTSP", "Transfer Split successful");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Split", "Transfer successful!", true));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.MTSP", ex);
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
                    var dtgAccounts = this.Get<DataGrid>("dtgAccounts");
                    AddressInfoView window;

                    if (dtgAccounts.SelectedItem != null)
                    {
                        Account selectedItem = (Account)dtgAccounts.SelectedItem;
                        window = new AddressInfoView((int)selectedItem.Index);
                    }
                    else
                    {
                        window = new AddressInfoView(0);
                    }

                    await DialogService.ShowAsync(window);
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
                    Logger.LogDebug("WAL.CLUW", "Wallet " + GlobalData.OpenedWalletName + " closed successfully");

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
                        await DialogService.ShowAsync(new TextBoxView("Export Selected", "Transactions have been exported to below file", exportFile, string.Empty));
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
                Logger.LogException("WAL.EXAC", ex);
            }
        }
        #endregion // Export Transactions
    }
}
