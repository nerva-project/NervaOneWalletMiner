using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
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
            InitializeComponent();
        }

        #region Open Wallet        
        public void OpenCloseWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

                if (btnOpenCloseWallet.Content!.ToString()!.Equals(StatusWallet.OpenWallet))
                {
                    // Open wallet dialog
                    var window = new OpenWalletView();
                    window.ShowDialog(GetWindow()).ContinueWith(OpenWalletDialogClosed);
                }
                else
                {
                    // TODO: Close wallet
                    CloseUserWallet();
                    btnOpenCloseWallet.Content = StatusWallet.OpenWallet;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("Wal.OCWC", ex);
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
            else
            {
                // Cancelled or closed. Don't need to do anything

            }
        }

        private static async void OpenUserWallet(string walletName, string walletPassword)
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

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Open Wallet", "Error opening " + walletName + " wallet\r\n" + response.Error.Message, ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
            else
            {
                GlobalData.IsWalletOpen = true;
                GlobalData.IsWalletJustOpened = true;
                GlobalData.OpenedWalletName = walletName;
                GlobalData.NewestTransactionHeight = 0;
            }

            GlobalData.WalletHeight = 0;
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
                Logger.LogException("Wal.SCA", ex);
            }
        }

        private async void CreateAccounDialogClosed(Task task)
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
                    Logger.LogError("Wal.CADC", "Failed to create account | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Create Account", "Error creating account\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
                else
                {
                    Logger.LogDebug("Wal.CADC", "New account created successfully.");
                    GlobalMethods.SaveWallet();

                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Create Account", "Account created successfully!", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });                    
                }
            }
            else
            {
                // Cancelled or closed. Don't need to do anything

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
                Logger.LogException("Wal.SCA", ex);
            }
        }

        private async void RenameLabelDialogClosed(Task task)
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
                    Logger.LogError("Wal.RLDC", "Failed to rename account | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rename Account", "Error renaming account\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
                else
                {
                    Logger.LogDebug("Wal.RLDC", "Account label changed successfully.");                    
                    GlobalMethods.WalletUiUpdate();
                    GlobalMethods.SaveWallet();

                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rename Account", "Account label changed successfully!", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            else
            {
                // Cancelled or closed. Don't need to do anything

            }
        }
        #endregion Rename Label

        #region Transfer Funds
        private void TransferFunds_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var dtgAccounts = this.Get<DataGrid>("dtgAccounts");
                TransferFundsView window;

                if (dtgAccounts.SelectedItem != null)
                {
                    Account selectedItem = (Account)dtgAccounts.SelectedItem;
                    window = new TransferFundsView((int)selectedItem.Index);
                }
                else
                {
                    window = new TransferFundsView(0);
                }
                
                window.ShowDialog(GetWindow()).ContinueWith(TransferDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("Wal.TFC", ex);
            }
        }

        private void TransferDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // Submit trannsfer
                if (!string.IsNullOrEmpty(result.SendToAddress) && result.SendAmount > 0)
                {
                    MakeTransfer(result.SendFromAddressIndex, result.SendToAddress, result.SendAmount, result.SendPaymentId, result.Priority);
                }
            }
            else
            {
                // Cancelled or closed. Don't need to do anything

            }
        }

        private static async void MakeTransfer(uint sendFromAccountIndex, string sendToAddress, decimal amount, string paymentId, string priority)
        {
            // TODO: Add other options
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
                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Transfer", "Transfer error\r\n\r\n" + response.Error.Message, ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Transfer", "Transfer successful!", ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
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
            catch (Exception ex)
            {
                Logger.LogException("Wal.OAIV", ex);
            }
        }
        #endregion // Address Info

        #region Close Wallet
        private static async void CloseUserWallet()
        {
            CloseWalletRequest request = new();

            CloseWalletResponse response = await GlobalData.WalletService.CloseWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

            if (response.Error.IsError)
            {
                GlobalData.IsWalletOpen = false;
                GlobalData.IsWalletJustOpened = false;
                GlobalData.OpenedWalletName = string.Empty;                

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Close Wallet", "Error closing wallet\r\n" + response.Error.Message, ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
            else
            {
                GlobalData.IsWalletOpen = false;
                GlobalData.IsWalletJustOpened = false;
                GlobalData.OpenedWalletName = string.Empty;

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Close Wallet", "Wallet closed successfully!", ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }

            GlobalData.WalletHeight = 0;
        }
        #endregion //Close Wallet
    }
}