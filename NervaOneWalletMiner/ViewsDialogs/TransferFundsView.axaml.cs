using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.DataGrid;
using NervaOneWalletMiner.Rpc.Common;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using System;
using System.Collections.Generic;

namespace NervaOneWalletMiner.ViewsDialogs
{
    public partial class TransferFundsView : Window
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        Dictionary<uint, string> _accounts = [];

        // Not used but designer will complain without it
        public TransferFundsView()
        {
            InitializeComponent();
        }

        public TransferFundsView(uint selectedAccountIndex, string toAddress = "", string paymentId = "")
        {
            try
            {
                InitializeComponent();
                Icon = GlobalMethods.GetWindowIcon();

                foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
                {
                    if (!_accounts.ContainsKey(account.Index))
                    {
                        _accounts.Add(account.Index, string.IsNullOrEmpty(account.Label) ? "No label" + " (" + account.AddressShort + ")" : account.Label + " (" + account.AddressShort + ")");
                    }
                }

                cbxSendFrom.ItemsSource = _accounts.Values;
                cbxSendFrom.SelectedIndex = (int)selectedAccountIndex;

                if(!string.IsNullOrEmpty(toAddress))
                {
                    tbxSendTo.Text = toAddress;
                }

                if (!string.IsNullOrEmpty(paymentId))
                {
                    tbxPaymentId.Text = paymentId;
                }

                // Can change this based on coin and what priories it has
                List<string> priorityList =
                [
                    SendPriority.Default,
                    SendPriority.Low,
                    SendPriority.Medium,
                    SendPriority.High,
                ];

                cbxPriority.ItemsSource = priorityList;
                cbxPriority.SelectedIndex = 0;

                decimal balanceTotal = 0;
                decimal balanceUnlocked = 0;

                foreach(Account account in GlobalData.WalletStats.Subaddresses.Values)
                {
                    if(account.Index == selectedAccountIndex)
                    {
                        balanceTotal = account.BalanceTotal;
                        balanceUnlocked = account.BalanceUnlocked;
                        break;
                    }
                }

                lblBalance.Content = balanceTotal + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
                lblUnlocked.Content = balanceUnlocked + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
            }
            catch (Exception ex)
            {
                Logger.LogException("TFD.CONS", ex); ;
            }            
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var cbxSendFrom = this.Get<ComboBox>("cbxSendFrom");
                var tbxSendTo = this.Get<TextBox>("tbxSendTo");
                var tbxAmount = this.Get<TextBox>("tbxAmount");
                var tbxPaymentId = this.Get<TextBox>("tbxPaymentId");

                if (string.IsNullOrEmpty(cbxSendFrom.SelectedValue!.ToString()) || string.IsNullOrEmpty(tbxSendTo.Text) || string.IsNullOrEmpty(tbxAmount.Text))
                {
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Transfer Funds", "From Address, To Address and Amount are required", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    uint fromAccountIndex = 0;
                    string fromAddress = cbxSendFrom.SelectedValue.ToString()!;

                    // TODO: Since addresses are shortened, you could potentally have 2 of the same ones
                    foreach (uint index in _accounts.Keys)
                    {
                        if (_accounts[index].Equals(fromAddress))
                        {
                            fromAccountIndex = index;
                        }
                    }

                    MessageBoxView confirmWindow = new MessageBoxView("Confirm Transfer", "You're about to send " + tbxAmount.Text
                        + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits
                        + ". Once transfer is started, it cannot be stopped. Do you want to continue?",
                        false);
                    DialogResult confirmRes = await confirmWindow.ShowDialog<DialogResult>(this);

                    if (confirmRes != null && confirmRes.IsOk)
                    {
                        bool isAuthorized = false;

                        if (DateTime.Now > GlobalData.WalletPassProvidedTime.AddMinutes(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes))
                        {
                            // Password required
                            TextBoxView textWindow = new TextBoxView("Provide Wallet Password", "Please provide wallet password", string.Empty, "Required - Wallet password", true, true);
                            DialogResult passRes = await textWindow.ShowDialog<DialogResult>(GetWindow());
                            if (passRes != null && passRes.IsOk)
                            {
                                if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                                {
                                    // Self managed lock
                                    if (passRes.TextBoxValue.Equals(GlobalData.WalletPassword))
                                    {
                                        isAuthorized = true;
                                        GlobalData.WalletPassProvidedTime = DateTime.Now;
                                    }                                    
                                }
                                else
                                {
                                    // Lock managed by wallet
                                    UnlockWithPassRequest request = new()
                                    {
                                        Password = passRes.TextBoxValue,
                                        TimeoutInSeconds = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes * 60
                                    };

                                    UnlockWithPassResponse response = await GlobalData.WalletService.UnlockWithPass(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                                    if (response.Error.IsError)
                                    {
                                        Logger.LogError("TFD.OKBC", "Unlock error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                                        await Dispatcher.UIThread.Invoke(async () =>
                                        {
                                            MessageBoxView window = new("Unlock Wallet", "Unlock error\r\n\r\n" + response.Error.Message, true);
                                            await window.ShowDialog(GetWindow());
                                        });
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

                        if(isAuthorized)
                        {
                            DialogResult result = new()
                            {
                                IsOk = true,
                                SendFromAddress = fromAddress,
                                SendFromAddressIndex = fromAccountIndex,
                                SendToAddress = tbxSendTo.Text,
                                SendAmount = Convert.ToDecimal(tbxAmount.Text),
                                SendPaymentId = tbxPaymentId.Text!,
                                Priority = (string)cbxPriority.SelectedValue!,
                                IsSplitTranfer = (bool)cbxSplitTransfer.IsChecked!
                            };

                            Close(result);
                        }
                        else
                        {
                            MessageBoxView window = new("Transfer Funds", "Autherization failed", true);
                            await window.ShowDialog(GetWindow());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("TFD.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                DialogResult result = new()
                {
                    IsCancel = true
                };

                Close(result);
            }
            catch (Exception ex)
            {
                Logger.LogException("TFD.CLBC", ex);
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
                Logger.LogException("TFD.GPIC", ex);
            }
        }

        private void Account_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // TODO: Change this to something easier
                uint fromAccountIndex = 0;
                string fromAddress = cbxSendFrom.SelectedValue!.ToString()!;

                foreach (uint index in _accounts.Keys)
                {
                    if (_accounts[index].Equals(fromAddress))
                    {
                        fromAccountIndex = index;
                    }
                }

                decimal balanceTotal = 0;
                decimal balanceUnlocked = 0;

                foreach (Account account in GlobalData.WalletStats.Subaddresses.Values)
                {
                    if (account.Index == fromAccountIndex)
                    {
                        balanceTotal = account.BalanceTotal;
                        balanceUnlocked = account.BalanceUnlocked;
                        break;
                    }
                }

                lblBalance.Content = balanceTotal + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
                lblUnlocked.Content = balanceUnlocked + " " + GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].DisplayUnits;
            }
            catch (Exception ex)
            {
                Logger.LogException("TFD.ASC1", ex);
            }
        }
    }
}
