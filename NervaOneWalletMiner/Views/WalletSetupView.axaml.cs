using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class WalletSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public WalletSetupView()
        {
            InitializeComponent();
        }

        #region Create Wallet
        public void CreateWalletClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new CreateWalletView();
                window.ShowDialog(GetWindow()).ContinueWith(CreateWalletDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("WalS.CWC", ex);
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
                GlobalData.OpenedWalletName = string.Empty;

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
                GlobalData.OpenedWalletName = walletName;
                GlobalData.NewestTransactionHeight = 0;

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var box = MessageBoxManager.GetMessageBoxStandard("Create Wallet", walletName + " wallet created successfully!\r\n\r\nYour new wallet is now open. Make sure to save your seed phrase and keys!", ButtonEnum.Ok);
                    _ = await box.ShowAsync();
                });
            }
        }

        private void CreateWalletDialogClosed(Task task)
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
        #endregion // Create Wallet

        #region Restore from Seed
        public void RestoreFromSeedClicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new RestoreFromSeedView();
                window.ShowDialog(GetWindow()).ContinueWith(RestoreFromSeedDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("WalS.CWC", ex);
            }
        }

        private void RestoreFromSeedDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // TODO: Restore wallet
                

            }
            else
            {
                // Cancelled or closed. Don't need to do anything

            }
        }
        #endregion // Restore from Seed
    }
}