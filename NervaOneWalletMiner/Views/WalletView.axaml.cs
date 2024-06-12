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
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public WalletView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

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
                    vm.TransferEvent += (owner, toAddress, paymentId) => ShowTransferDialog(owner, toAddress, paymentId);
                    GlobalData.AreWalletEventsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.WAVI", ex);
            }
        }

        #region Open Wallet        
        public void OpenCloseWallet_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

                if (btnOpenCloseWallet.Content!.ToString()!.Equals(StatusWallet.OpenWallet))
                {
                    // Open wallet dialog
                    Logger.LogDebug("WAL.OCWC", "Opening wallet dialog.");
                    var window = new OpenWalletView();
                    window.ShowDialog(GetWindow()).ContinueWith(OpenWalletDialogClosed);
                }
                else
                {
                    // Close wallet
                    CloseUserWallet();
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
                if (!string.IsNullOrEmpty(result.WalletName) && !string.IsNullOrEmpty(result.WalletPassword))
                {
                    OpenUserWallet(result.WalletName, result.WalletPassword);
                }
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
                    GlobalData.IsWalletOpen = false;
                    GlobalData.IsWalletJustOpened = false;
                    GlobalData.OpenedWalletName = string.Empty;

                    Logger.LogDebug("WAL.OUWT", "Error opening " + walletName + " wallet | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Open Wallet", "Error opening " + walletName + " wallet\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    GlobalData.IsWalletOpen = true;
                    GlobalData.IsWalletJustOpened = true;
                    GlobalData.OpenedWalletName = walletName;
                    GlobalData.NewestTransactionHeight = 0;

                    Logger.LogDebug("WAL.OUWT", "Wallet " + walletName + " opened successfully.");
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
                var window = new TextBoxView("Create Account", string.Empty, "Enter new account label", "Account Label", false);
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
                        Logger.LogError("WAL.CADC", "Failed to create account " + request.Label + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                        await Dispatcher.UIThread.Invoke(async () =>
                        {
                            MessageBoxView window = new("Create Account", "Error creating account\r\n" + response.Error.Message, true);
                            await window.ShowDialog(GetWindow());
                        });
                    }
                    else
                    {
                        Logger.LogDebug("WAL.CADC", "New account created successfully.");
                        GlobalMethods.SaveWallet();

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
                    var window = new TextBoxView("Change Account Label", selectedItem.Label, string.Empty, "Account Label", false);
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
                        Logger.LogError("WAL.RLDC", "Failed to rename account " + request.Label + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);

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
                        GlobalMethods.SaveWallet();

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
                    Logger.LogError("WAL.MKTR", "Transfer error | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Transfer Funds", "Transfer error\r\n\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.MKTR", "Transfer successful.");
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
                    Logger.LogError("WAL.MTSP", "Split transfer error | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Transfer Split", "Transfer error\r\n\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.MTSP", "Transfer Split successful.");
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
        private async void CloseUserWallet()
        {
            try
            {
                CloseWalletRequest request = new();

                CloseWalletResponse response = await GlobalData.WalletService.CloseWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WAL.CLUW", "Error closing wallet " + GlobalData.OpenedWalletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);

                    GlobalData.IsWalletOpen = false;
                    GlobalData.IsWalletJustOpened = false;
                    GlobalData.OpenedWalletName = string.Empty;
                   
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Close Wallet", "Error closing wallet\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAL.CLUW", "Wallet " + GlobalData.OpenedWalletName + " closed successfully.");

                    GlobalData.IsWalletOpen = false;
                    GlobalData.IsWalletJustOpened = false;
                    GlobalData.OpenedWalletName = string.Empty;
                    GlobalData.WalletStats = new();
                    
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Close Wallet", "Wallet closed successfully!", true);
                        await window.ShowDialog(GetWindow());
                    });
                }

                GlobalData.WalletHeight = 0;
            }
            catch (Exception ex)
            {
                Logger.LogException("WAL.CLUW", ex);
            }            
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
                    ((DaemonViewModel)GlobalData.ViewModelPages[SplitViewPages.Daemon]).StartMining(GetWindow(), GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads);
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
    }
}