using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Objects.Constants;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using System;
using System.Threading.Tasks;

namespace NervaWalletMiner.Views
{
    public partial class WalletView : UserControl
    {
        public WalletView()
        {
            InitializeComponent();
        }

        public void OpenCloseWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var btnOpenCloseWallet = this.Get<Button>("btnOpenCloseWallet");

                if (btnOpenCloseWallet.Content!.ToString()!.Equals(StatusWallet.OpenWallet))
                {
                    // Open wallet dialog
                    var window = new OpenWalletView();
                    window.ShowDialog(GetWindow()).ContinueWith(DialogClosed);
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
                Logger.LogException("Hom.SSMC", ex);
            }
        }

        private void DialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if(result != null && result.IsOk)
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

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Open Wallet", walletName + " wallet opened successfully!", ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
        }

        private static async void CloseUserWallet()
        {
            CloseWalletRequest request = new();

            CloseWalletResponse response = await GlobalData.WalletService.CloseWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

            if (response.Error.IsError)
            {
                GlobalData.IsWalletOpen = false;
                GlobalData.IsWalletJustOpened = false;

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

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Close Wallet", "Wallet closed successfully!", ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
        }

        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");
    }
}