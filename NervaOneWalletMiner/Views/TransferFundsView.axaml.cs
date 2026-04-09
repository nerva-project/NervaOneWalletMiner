using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.Views
{
    public partial class TransferFundsView : UserControl
    {
        Dictionary<uint, string> _accounts = [];

        public TransferFundsView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                Initialized += TransferFundsView_Initialized;
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.CONS", ex);
            }
        }

        private void TransferFundsView_Initialized(object? sender, EventArgs e)
        {
            try
            {
                uint selectedAccountIndex = DataContext is TransferFundsViewModel vm ? vm.SelectedAccountIndex : 0;
                string toAddress = DataContext is TransferFundsViewModel vm2 ? vm2.ToAddress : string.Empty;
                string paymentId = DataContext is TransferFundsViewModel vm3 ? vm3.PaymentId : string.Empty;

                foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
                {
                    if (!_accounts.ContainsKey(account.Index))
                    {
                        _accounts.Add(account.Index, string.IsNullOrEmpty(account.Label) ? "No label" + " (" + account.AddressShort + ")" : account.Label + " (" + account.AddressShort + ")");
                    }
                }

                cbxSendFrom.ItemsSource = _accounts.Values;
                cbxSendFrom.SelectedIndex = (int)selectedAccountIndex;

                if (!string.IsNullOrEmpty(toAddress))
                {
                    tbxSendTo.Text = toAddress;
                }

                if (!string.IsNullOrEmpty(paymentId))
                {
                    tbxPaymentId.Text = paymentId;
                }

                List<string> priorityList =
                [
                    SendPriority.Default,
                    SendPriority.Low,
                    SendPriority.Medium,
                    SendPriority.High,
                ];

                cbxPriority.ItemsSource = priorityList;
                cbxPriority.SelectedIndex = 0;

                UpdateBalanceLabels(selectedAccountIndex);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.INIT", ex);
            }
        }

        private void UpdateBalanceLabels(uint accountIndex)
        {
            decimal balanceTotal = 0;
            decimal balanceUnlocked = 0;

            foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
            {
                if (account.Index == accountIndex)
                {
                    balanceTotal = account.BalanceTotal;
                    balanceUnlocked = account.BalanceUnlocked;
                    break;
                }
            }

            lblBalance.Content = balanceTotal + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
            lblUnlocked.Content = balanceUnlocked + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
        }

        private void Account_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UpdateBalanceLabels((uint)cbxSendFrom.SelectedIndex);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.ASC1", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(cbxSendFrom.SelectedValue?.ToString()) || string.IsNullOrEmpty(tbxSendTo.Text) || string.IsNullOrEmpty(tbxAmount.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "From Address, To Address and Amount are required", true));
                    return;
                }

                if (!decimal.TryParse(tbxAmount.Text, out decimal amount) || amount <= 0)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Amount must be a valid number greater than 0", true));
                    return;
                }

                uint fromAccountIndex = (uint)cbxSendFrom.SelectedIndex;
                string sendToAddress = tbxSendTo.Text;
                string sendPaymentId = tbxPaymentId.Text ?? string.Empty;
                string priority = (string)cbxPriority.SelectedValue!;
                bool isSplit = (bool)cbxSplitTransfer.IsChecked!;

                MessageBoxView confirmWindow = new("Confirm Transfer", "You're about to send " + tbxAmount.Text
                    + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits
                    + ". Once transfer is started, it cannot be stopped. Do you want to continue?",
                    false, true);
                DialogResult? confirmRes = await DialogService.ShowAsync<DialogResult>(confirmWindow);

                if (confirmRes == null || !confirmRes.IsOk)
                {
                    return;
                }

                bool isAuthorized = false;

                if (DateTime.Now > GlobalData.WalletPassProvidedTime.AddMinutes(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes))
                {
                    TextBoxView textWindow = new("Provide Wallet Password", "Please provide wallet password", string.Empty, "Required - Wallet password", true, true);
                    DialogResult? passRes = await DialogService.ShowAsync<DialogResult>(textWindow);

                    if (passRes != null && passRes.IsOk)
                    {
                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                        {
                            if (Hashing.Verify(passRes.TextBoxValue.ToCharArray(), GlobalData.WalletPasswordHash))
                            {
                                isAuthorized = true;
                                GlobalData.WalletPassProvidedTime = DateTime.Now;
                                passRes.TextBoxValue = string.Empty;
                            }
                        }
                        else
                        {
                            UnlockWithPassRequest request = new()
                            {
                                Password = passRes.TextBoxValue.ToCharArray(),
                                TimeoutInSeconds = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes * 60
                            };

                            UnlockWithPassResponse response = await GlobalData.WalletService.UnlockWithPass(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                            if (response.Error.IsError)
                            {
                                Logger.LogError("TFV.OKBC", "Unlock error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                                await DialogService.ShowAsync(new MessageBoxView("Unlock Wallet", "Unlock error\r\n\r\n" + response.Error.Message, true));
                            }
                            else
                            {
                                isAuthorized = true;
                                GlobalData.WalletPassProvidedTime = DateTime.Now;
                            }
                        }
                    }
                }
                else
                {
                    isAuthorized = true;
                }

                if (!isAuthorized)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Authorization failed", true));
                    return;
                }

                btnOk.Content = "Sending...";
                btnOk.IsEnabled = false;
                btnCancel.IsEnabled = false;

                if (isSplit)
                {
                    await MakeTransferSplit(fromAccountIndex, sendToAddress, amount, sendPaymentId, priority);
                }
                else
                {
                    await MakeTransfer(fromAccountIndex, sendToAddress, amount, sendPaymentId, priority);
                }

                UIManager.NavigateToPage(SplitViewPages.Wallet);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("TFV.CNCL", "Transfer funds cancelled");
                UIManager.NavigateToPage(SplitViewPages.Wallet);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.CNCL", ex);
            }
        }

        public void GeneratePaymentId_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                tbxPaymentId.Text = GlobalMethods.GenerateRandomHexString(64);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.GPIC", ex);
            }
        }

        private async System.Threading.Tasks.Task MakeTransfer(uint fromAccountIndex, string sendToAddress, decimal amount, string paymentId, string priority)
        {
            try
            {
                TransferRequest request = new()
                {
                    Destinations = [new() { Amount = amount, Address = sendToAddress }],
                    AccountIndex = fromAccountIndex,
                    Priority = priority,
                    PaymentId = paymentId
                };

                TransferResponse response = await GlobalData.WalletService.Transfer(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("TFV.MKTR", "Transfer error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Transfer error\r\n\r\n" + response.Error.Message, true));
                    });
                }
                else
                {
                    Logger.LogDebug("TFV.MKTR", "Transfer successful");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Transfer successful!", true));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.MKTR", ex);
            }
        }

        private async System.Threading.Tasks.Task MakeTransferSplit(uint fromAccountIndex, string sendToAddress, decimal amount, string paymentId, string priority)
        {
            try
            {
                TransferRequest request = new()
                {
                    Destinations = [new() { Amount = amount, Address = sendToAddress }],
                    AccountIndex = fromAccountIndex,
                    Priority = priority,
                    PaymentId = paymentId
                };

                TransferResponse response = await GlobalData.WalletService.TransferSplit(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("TFV.MTSP", "Split transfer error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Split", "Transfer error\r\n\r\n" + response.Error.Message, true));
                    });
                }
                else
                {
                    Logger.LogDebug("TFV.MTSP", "Transfer Split successful");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Split", "Transfer successful!", true));
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.MTSP", ex);
            }
        }
    }
}
