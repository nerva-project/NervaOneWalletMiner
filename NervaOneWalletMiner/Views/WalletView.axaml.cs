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
using System.IO;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class WalletView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

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
                    vm.TransferUiEvent += (owner, toAddress, paymentId) => ShowTransferDialog(owner, toAddress, paymentId);
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
                    // Open wallet dialog
                    Logger.LogDebug("WAL.OCWC", "Opening wallet dialog");
                    var window = new OpenWalletView();
                    await window.ShowDialog(GetWindow()).ContinueWith(OpenWalletDialogClosed);
                }
                else
                {
                    // Close wallet
                    await CloseUserWallet(GetWindow());
                    btnOpenCloseWallet.Content = StatusWallet.OpenWallet;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.OCWC", ex);
            }
        }

        private void OpenWalletDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // Open wallet
                OpenUserWallet(result.WalletName, result.WalletPassword);
            }
        }

        private async void OpenUserWallet(string walletName, string walletPassword)
        {
            try
            {
                OpenWalletRequest request = new()
                {
                    WalletName = walletName,
                    Password = walletPassword
                };

                OpenWalletResponse response = await GlobalData.WalletService.OpenWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();

                    Logger.LogDebug("WAL.OUWT", "Error opening " + walletName + " wallet | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Open Wallet", "Error opening " + walletName + " wallet\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    GlobalMethods.WalletJustOpened(walletName);

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                    {
                        GlobalData.WalletPassProvidedTime = DateTime.Now;                        
                        GlobalData.WalletPassword = request.Password;
                    }

                    Logger.LogDebug("WAL.OUWT", "Wallet " + walletName + " opened successfully");
                }

                GlobalData.WalletHeight = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.OUWT", ex);
            }            
        }
        #endregion // Open Wallet

        #region Create Account
        private void CreateAccount_Clicked(object sender, RoutedEventArgs args)
        {
            ShowCreateAccount();
        }

        private void ShowCreateAccount()
        {
            try
            {
                var window = new TextBoxView("Create Account", "Account Label", string.Empty, "Enter new account label", false);
                window.ShowDialog(GetWindow()).ContinueWith(CreateAccounDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.SRCA", ex);
            }
        }

        private async void CreateAccounDialogClosed(Task task)
        {
            try
            {
                DialogResult result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk)
                {
                    CreateAccountRequest request = new()
                    {
                        Label = result.TextBoxValue
                    };

                    CreateAccountResponse response = await GlobalData.WalletService.CreateAccount(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                    if (response.Error.IsError)
                    {
                        Logger.LogError("WAL.CADC", "Failed to create account " + request.Label + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            MessageBoxView window = new("Create Account", "Error creating account\r\n" + response.Error.Message, true);
                            await window.ShowDialog(GetWindow());
                        });
                    }
                    else
                    {
                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                        {
                            Logger.LogDebug("WAL.CADC", "New account created successfully");
                            GlobalMethods.SaveWallet();
                        }

                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            MessageBoxView window = new("Create Account", "Account created successfully!", true);
                            await window.ShowDialog(GetWindow());
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.CADC", ex);
            }            
        }
        #endregion Create Account

        #region Rename Label
        private void RenameLabel_Clicked(object sender, RoutedEventArgs args)
        {
            ShowRenameLabel();
        }

        private void ShowRenameLabel()
        {
            try
            {
                if (dtgAccounts.SelectedItem != null)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;
                    var window = new TextBoxView("Change Account Label", "Account Label", selectedItem.Label, string.Empty, false);
                    window.ShowDialog(GetWindow()).ContinueWith(RenameLabelDialogClosed);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.SHRL", ex);
            }
        }

        private async void RenameLabelDialogClosed(Task task)
        {
            try
            {
                DialogResult result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;

                    LabelAccountRequest request = new()
                    {
                        AccountIndex = selectedItem.Index,
                        Label = result.TextBoxValue
                    };

                    LabelAccountResponse response = await GlobalData.WalletService.LabelAccount(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                    if (response.Error.IsError)
                    {
                        Logger.LogError("WAL.RLDC", "Failed to rename account " + request.Label + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);

                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            MessageBoxView window = new("Rename Account", "Error renaming account\\r\\n\" + response.Error.Message", true);
                            await window.ShowDialog(GetWindow());
                        });
                    }
                    else
                    {
                        Logger.LogDebug("WAL.RLDC", "Account label changed successfully to " + request.Label);
                        GlobalMethods.WalletUiUpdate();

                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSavingWalletSupported)
                        {
                            GlobalMethods.SaveWallet();
                        }

                        await Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            MessageBoxView window = new("Rename Account", "Account label changed successfully!", true);
                            await window.ShowDialog(GetWindow());
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.RLDC", ex);
            }            
        }
        #endregion Rename Label

        #region Transfer Funds
        private void TransferFunds_Clicked(object sender, RoutedEventArgs args)
        {
            ShowTransferDialog(GetWindow(), string.Empty, string.Empty);
        }

        public void ShowTransferDialog(Window owner, string toAddress, string paymentId)
        {
            try
            {
                if(GlobalData.IsWalletOpen)
                {
                    var dtgAccounts = this.Get<DataGrid>("dtgAccounts");
                    TransferFundsView window;

                    uint selectedIndex = 0;

                    if (dtgAccounts.SelectedItem != null)
                    {
                        Account selectedItem = (Account)dtgAccounts.SelectedItem;
                        selectedIndex = selectedItem.Index;
                    }

                    window = new TransferFundsView(selectedIndex, toAddress, paymentId);
                    window.ShowDialog(owner).ContinueWith(TransferDialogClosed);
                }
                else
                {
                    MessageBoxView window = new("Transfer Funds", "Please open wallet first.", true);
                    window.ShowDialog(owner);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.STRD", ex);
            }
        }

        private void TransferDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // Submit transfer
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
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Transfer Funds", "Transfer error\r\n\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.MKTR", "Transfer successful");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Transfer Funds", "Transfer successful!", true);
                        await window.ShowDialog(GetWindow());
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
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Transfer Split", "Transfer error\r\n\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.MTSP", "Transfer Split successful");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Transfer Split", "Transfer successful!", true);
                        await window.ShowDialog(GetWindow());
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

        private void OpenAddressInfoView()
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

                    window.ShowDialog(GetWindow());
                }
                else
                {
                    MessageBoxView window = new("Address Info", "Please open wallet first.", true);
                    window.ShowDialog(GetWindow());
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
            // Need to wait until wallet is closed or might as well not even atempt this
            await CloseUserWallet(null, false);
            return true;
        }
        private async Task<bool> CloseUserWallet(Window? owner, bool isUiThread = true)
        {
            bool isSuccess = false;

            try
            {
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
                            MessageBoxView window = new("Close Wallet", "Error closing wallet\r\n" + response.Error.Message, true);
                            await window.ShowDialog(owner!);
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
                            MessageBoxView window = new("Close Wallet", "Wallet closed successfully!", true);
                            await window.ShowDialog(owner!);
                        });
                    }
                }

                GlobalMethods.WalletClosedOrErrored();
                GlobalData.WalletHeight = 0;
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

                    if(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress != selectedItem.AddressFull)
                    {
                        GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress = selectedItem.AddressFull;
                        Logger.LogDebug("WAL.STMC", "Setting and saving new mining address: " + GlobalMethods.GetShorterString(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningAddress, 12));
                        GlobalMethods.SaveConfig();
                    }

                    Logger.LogDebug("WAL.STMC", "Calling Start Mining Daemon method");
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMiningUi(GetWindow(), GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
                }
                else
                {
                    Logger.LogDebug("WAL.STMC", "No rows selected");
                    MessageBoxView window = new("Start Mining", "Please select address to start mining to it", true);
                    window.ShowDialog(GetWindow());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.STMC", ex);
            }
        }
        #endregion // Start Mining

        public async void ExportSelected_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (dtgAccounts.SelectedItem != null)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;
                    string exportFile = GlobalMethods.GetExportFileNameWithPath();

                    GetTransfersExportRequest request = new()
                    {
                        AccountIndex = selectedItem.Index,
                        IsAllAccounts = false
                    };

                    GetTransfersExportResponse response = await GlobalMethods.ExportTranfers(request, exportFile);
                    if (response.Error.IsError)
                    {
                        Logger.LogError("WAL.EXSC", "GetExport Error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                        MessageBoxView window = new("Export Selected", "Error exporting:\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    }
                    else
                    {
                        var window = new TextBoxView("Export Selected", "Transactions have been exported to below file", exportFile, string.Empty);
                        await window.ShowDialog(GetWindow());
                    }
                }
                else
                {
                    Logger.LogDebug("WAL.EXSC", "No rows selected");
                    MessageBoxView window = new("Export Selected", "Please select addres to export", true);
                    await window.ShowDialog(GetWindow());
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
                        MessageBoxView window = new("Export All", "Error exporting:\r\n" + response.Error.Message, true);
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
                Logger.LogException("WAL.EXAC", ex);
            }
        }
    }
}