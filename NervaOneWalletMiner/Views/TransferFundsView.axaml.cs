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
        private string _returnPage = SplitViewPages.Wallet;

        public TransferFundsView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                Initialized += TransferFundsView_Initialized;
                Loaded += (_, _) => tbxSendTo.Focus();
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
                _returnPage = DataContext is TransferFundsViewModel vm4 ? vm4.ReturnPage : SplitViewPages.Wallet;

                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle)
                {
                    pnlUnlocked.IsVisible = false;
                }

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPaymentIdSupported)
                {
                    pnlPaymentId.IsVisible = false;
                }

                if (!GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSplitTransferSupported)
                {
                    cbxSplitTransfer.IsVisible = false;
                }

                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSendFromSupported)
                {
                    foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
                    {
                        if (!_accounts.ContainsKey(account.Index))
                        {
                            _accounts.Add(account.Index, string.IsNullOrEmpty(account.Label) ? "No label" + " (" + account.AddressShort + ")" : account.Label + " (" + account.AddressShort + ")");
                        }
                    }

                    cbxSendFrom.ItemsSource = _accounts.Values;
                    cbxSendFrom.SelectedIndex = (int)selectedAccountIndex;
                }
                else
                {
                    pnlSendFrom.IsVisible = false;
                }

                if (!string.IsNullOrEmpty(toAddress))
                {
                    tbxSendTo.Text = toAddress;
                }

                if (!string.IsNullOrEmpty(paymentId))
                {
                    tbxPaymentId.Text = paymentId;
                }

                List<string> priorityList = GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle
                    ? SendPriority.GetBtcPriorityList()
                    : SendPriority.GetXmrPriorityList();

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
            decimal balanceTotal;
            decimal balanceUnlocked;
            string units = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;

            if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSendFromSupported)
            {
                balanceTotal = 0;
                balanceUnlocked = 0;

                foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
                {
                    if (account.Index == accountIndex)
                    {
                        balanceTotal = account.BalanceTotal;
                        balanceUnlocked = account.BalanceUnlocked;
                        break;
                    }
                }
            }
            else
            {
                balanceTotal = GlobalData.WalletStats.BalanceTotal;
                balanceUnlocked = GlobalData.WalletStats.BalanceUnlocked;
            }

            lblBalance.Content = GlobalMethods.FormatAmountFull(balanceTotal) + " " + units;
            lblUnlocked.Content = GlobalMethods.FormatAmountFull(balanceUnlocked) + " " + units;
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
                if(GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle)
                {
                    if (string.IsNullOrEmpty(tbxSendTo.Text) || string.IsNullOrEmpty(tbxAmount.Text))
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Send To Address and Amount are required", true));
                        return;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(cbxSendFrom.SelectedValue?.ToString()) || string.IsNullOrEmpty(tbxSendTo.Text) || string.IsNullOrEmpty(tbxAmount.Text))
                    {
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "From Address, To Address and Amount are required", true));
                        return;
                    }
                }

                if (!decimal.TryParse(tbxAmount.Text, out decimal amount) || amount <= 0)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Amount must be a valid number greater than 0", true));
                    return;
                }

                uint fromAccountIndex = (uint)cbxSendFrom.SelectedIndex;

                decimal availableBalance;
                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsSendFromSupported)
                {
                    availableBalance = GlobalData.WalletStats.Subaddresses.TryGetValue(fromAccountIndex, out var acc) ? acc.BalanceUnlocked : 0;
                }
                else
                {
                    availableBalance = GlobalData.WalletStats.BalanceUnlocked;
                }

                if (amount > availableBalance)
                {
                    await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Amount exceeds available balance of " + availableBalance, true));
                    return;
                }
                string sendToAddress = tbxSendTo.Text;
                string sendPaymentId = tbxPaymentId.Text ?? string.Empty;
                string priority = (string)cbxPriority.SelectedValue!;
                bool isSplit = (bool)cbxSplitTransfer.IsChecked!;
                string units = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
                var walletRpc = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc;

                // Estimate fee for regular transfers; skip for split (transfer_split may produce multiple txs)
                EstimateFeeResponse feeResponse = new();
                if (!isSplit)
                {
                    btnOk.Content = "Calculating...";
                    btnOk.IsEnabled = false;
                    btnCancel.IsEnabled = false;

                    feeResponse = await GlobalData.WalletService.EstimateFee(walletRpc, new TransferRequest
                    {
                        Destinations = [new() { Amount = amount, Address = sendToAddress }],
                        AccountIndex = fromAccountIndex,
                        Priority = priority,
                        PaymentId = sendPaymentId
                    });

                    btnOk.Content = "Transfer";
                    btnOk.IsEnabled = true;
                    btnCancel.IsEnabled = true;

                    if (feeResponse.Error.IsError)
                    {
                        Logger.LogError("TFV.OKBC", "Fee estimation error | " + feeResponse.Error.Message);
                        await DialogService.ShowAsync(new MessageBoxView("Transfer Funds", "Could not estimate fee\r\n\r\n" + feeResponse.Error.Message, true));
                        return;
                    }
                }

                string confirmMessage;
                if (isSplit)
                {
                    confirmMessage = "You're about to send " + tbxAmount.Text + " " + units + ". Once transfer is started, it cannot be stopped. Do you want to continue?";
                }
                else
                {
                    confirmMessage = "Send To: " + GlobalMethods.GetShorterString(sendToAddress, 12) + "\r\n"
                        + "Amount: " + tbxAmount.Text + " " + units + "\r\n"
                        + "Fee: " + feeResponse.Fee + " " + units + "\r\n"
                        + "Total: " + (amount + feeResponse.Fee) + " " + units + "\r\n";

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsWalletBtcStyle)
                    {
                        confirmMessage += "Est. confirmation: " + SendPriority.GetBtcConfirmationTarget(priority) + "\r\n";
                    }

                    confirmMessage += "\r\nOnce transfer is started, it cannot be stopped. Do you want to continue?";
                }

                DialogResult? confirmRes = await DialogService.ShowAsync<DialogResult>(new MessageBoxView("Confirm Transfer", confirmMessage, false, true));

                if (confirmRes == null || !confirmRes.IsOk)
                {
                    return;
                }

                bool isAuthorized = false;

                if (DateTime.Now > GlobalData.WalletPassProvidedTime.AddMinutes(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes))
                {
                    TextBoxView textWindow = new(title: "Provide Wallet Password", labelValue: "Please provide wallet password", textValue: string.Empty, textWatermark: "Required - Wallet password", isTextRequired: true, isTextPassword: true, okButtonText: "Submit");
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

                            UnlockWithPassResponse response = await GlobalData.WalletService.UnlockWithPass(walletRpc, request);

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
                    await MakeTransfer(fromAccountIndex, sendToAddress, amount, sendPaymentId, priority, feeResponse.TxData);
                }

                UIManager.NavigateToPage(_returnPage);
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
                UIManager.NavigateToPage(_returnPage);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.CNCL", ex);
            }
        }

        public async void PickAddress_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                DialogResult? res = await DialogService.ShowAsync<DialogResult>(new AddressPickerView("Pick Address"));

                if (res != null && res.IsOk)
                {
                    tbxSendTo.Text = res.SendToAddress;
                    tbxPaymentId.Text = res.SendPaymentId;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TFV.PABC", ex);
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

        private async System.Threading.Tasks.Task MakeTransfer(uint fromAccountIndex, string sendToAddress, decimal amount, string paymentId, string priority, string txData)
        {
            try
            {
                TransferRequest request = new()
                {
                    Destinations = [new() { Amount = amount, Address = sendToAddress }],
                    AccountIndex = fromAccountIndex,
                    Priority = priority,
                    PaymentId = paymentId,
                    TxData = txData
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
