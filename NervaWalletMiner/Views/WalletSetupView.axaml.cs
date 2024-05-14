using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Objects;
using NervaWalletMiner.Rpc.Wallet.Requests;
using NervaWalletMiner.Rpc.Wallet.Responses;
using NervaWalletMiner.ViewsDialogs;
using System;
using System.Threading.Tasks;

namespace NervaWalletMiner.Views
{
    public partial class WalletSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public WalletSetupView()
        {
            InitializeComponent();
        }

        public void CreateWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new CreateWalletView();
                window.ShowDialog(GetWindow()).ContinueWith(DialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("Hom.SSMC", ex);
            }
        }

        private void DialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // Open wallet
                if (!string.IsNullOrEmpty(result.WalletName) && !string.IsNullOrEmpty(result.WalletPassword))
                {
                    CreateNewWallet(result.WalletName, result.WalletPassword, result.WalletLanguage);
                }
            }
            else
            {
                // Cancelled or closed. Don't need to do anything

            }
        }

        private static async void CreateNewWallet(string walletName, string walletPassword, string walletLanguage)
        {
            CreateWalletRequest request = new()
            {
                WalletName = walletName,
                Password = walletPassword,
                Language = walletLanguage
            };

            CreateWalletResponse response = await GlobalData.WalletService.CreateWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

            if (response.Error.IsError)
            {
                GlobalData.IsWalletOpen = false;
                GlobalData.IsWalletJustOpened = false;

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Create Wallet", "Error creating " + walletName + " wallet\r\n" + response.Error.Message, ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
            else
            {
                GlobalData.IsWalletOpen = true;
                GlobalData.IsWalletJustOpened = true;

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Create Wallet", walletName + " wallet created successfully!\r\n\r\nYour new wallet is now open. Make sure to save your seed phrase and keys!", ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
        }
    }
}