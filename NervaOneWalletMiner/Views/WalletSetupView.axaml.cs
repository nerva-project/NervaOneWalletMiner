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
using System.Diagnostics;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class WalletSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public WalletSetupView()
        {
            InitializeComponent();

            tbxLogLevel.Text = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].LogLevel.ToString();
        }

        public void OpenWalletsFolder_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.WalletDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.OCTFC", ex);
            }
        }

        public void SaveSettings_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;
               
                uint logLevel = Convert.ToUInt32(tbxLogLevel.Text);
                if (!string.IsNullOrEmpty(tbxLogLevel.Text) && GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].LogLevel != logLevel)
                {
                    GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].LogLevel = logLevel;
                    isChanged = true;
                }

                if (isChanged)
                {
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.SSC", ex);
            }
        }

        #region Create Wallet
        public void CreateWallet_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new CreateWalletView();
                window.ShowDialog(GetWindow()).ContinueWith(CreateWalletDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.CWC", ex);
            }
        }

        private static async void CreateNewWallet(string walletName, string walletPassword, string walletLanguage)
        {
            try
            {
                CreateWalletRequest request = new()
                {
                    WalletName = walletName,
                    Password = walletPassword,
                    Language = walletLanguage
                };

                CreateWalletResponse response = await GlobalData.WalletService.CreateWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);
                request = new();

                if (response.Error.IsError)
                {
                    GlobalData.IsWalletOpen = false;
                    GlobalData.IsWalletJustOpened = false;
                    GlobalData.OpenedWalletName = string.Empty;

                    Logger.LogError("WalSV.CNW", "Failed to create wallet " + walletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code);
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

                    Logger.LogDebug("WalSV.CNW", "Wallet " + walletName + " created successfully.");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Create Wallet", walletName + " wallet created successfully!\r\n\r\nYour new wallet is now open. Make sure to save your seed phrase and keys!", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.CNW", ex);
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
        }
        #endregion // Create Wallet

        #region Restore from Seed
        public void RestoreFromSeed_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new RestoreFromSeedView();
                window.ShowDialog(GetWindow()).ContinueWith(RestoreFromSeedDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RFSC", ex);
            }
        }

        private void RestoreFromSeedDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // TODO: Change this so you're not storing sensitive date in many places
                if (!string.IsNullOrEmpty(result.SeedPhrase) && !string.IsNullOrEmpty(result.WalletName) && !string.IsNullOrEmpty(result.WalletPassword))
                {
                    RestoreFromSeed(result.SeedPhrase, result.SeedOffset, result.WalletName, result.WalletPassword, result.WalletLanguage);
                }

            }          
        }

        private static async void RestoreFromSeed(string seed, string seedOffset, string walletName, string walletPassword, string walletLanguage)
        {
            try
            {
                RestoreFromSeedRequest request = new()
                {
                    Seed = seed,
                    SeedOffset = seedOffset,
                    WalletName = walletName,
                    Password = walletPassword,
                    Language = walletLanguage
                };

                RestoreFromSeedResponse response = await GlobalData.WalletService.RestoreFromSeed(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalData.IsWalletOpen = false;
                    GlobalData.IsWalletJustOpened = false;
                    GlobalData.OpenedWalletName = string.Empty;

                    Logger.LogError("WalSV.RFS", "Failed to restore wallet " + walletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code + " | Info: " + response.Info);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Restore from Seed", "Error restoring " + walletName + " wallet\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
                else
                {
                    GlobalData.IsWalletOpen = true;
                    GlobalData.IsWalletJustOpened = true;
                    GlobalData.OpenedWalletName = walletName;
                    GlobalData.NewestTransactionHeight = 0;

                    Logger.LogDebug("WalSV.RFS", "Wallet " + walletName + " restored successfully! Info: " + response.Info);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Restore from Seed", walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RFS", ex);
            }
            
        }
        #endregion // Restore from Seed

        #region Restore from Keys
        public void RestoreFromKeys_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new RestoreFromKeysView();
                window.ShowDialog(GetWindow()).ContinueWith(RestoreFromKeysDialogClosed);
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RFKC", ex);
            }
        }

        private void RestoreFromKeysDialogClosed(Task task)
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                // TODO: Change this so you're not storing sensitive date in many places
                if (!string.IsNullOrEmpty(result.WalletAddress) && !string.IsNullOrEmpty(result.ViewKey)  && !string.IsNullOrEmpty(result.SpendKey)
                    && !string.IsNullOrEmpty(result.WalletName) && !string.IsNullOrEmpty(result.WalletPassword))
                {
                    RestoreFromKeys(result.WalletAddress, result.ViewKey, result.SpendKey, result.WalletName, result.WalletPassword, result.WalletLanguage);
                }

            }
        }

        private static async void RestoreFromKeys(string walletAddress, string viewKey, string spendKey, string walletName, string walletPassword, string walletLanguage)
        {
            try
            {
                RestoreFromKeysRequest request = new()
                {
                    WalletAddress = walletAddress,
                    ViewKey = viewKey,
                    SpendKey = spendKey,
                    WalletName = walletName,
                    Password = walletPassword,
                    Language = walletLanguage
                };

                RestoreFromKeysResponse response = await GlobalData.WalletService.RestoreFromKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalData.IsWalletOpen = false;
                    GlobalData.IsWalletJustOpened = false;
                    GlobalData.OpenedWalletName = string.Empty;

                    Logger.LogError("WalSV.RFK", "Failed to restore wallet " + walletName + " | Message: " + response.Error.Message + " | Code: " + response.Error.Code + " | Info: " + response.Info);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Restore from Seed", "Error restoring " + walletName + " wallet\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
                else
                {
                    GlobalData.IsWalletOpen = true;
                    GlobalData.IsWalletJustOpened = true;
                    GlobalData.OpenedWalletName = walletName;
                    GlobalData.NewestTransactionHeight = 0;

                    Logger.LogDebug("WalSV.RFS", "Wallet " + walletName + " restored successfully! Info: " + response.Info);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Restore from Seed", walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RFK", ex);
            }

        }
        #endregion // Restore from Keys

        #region Rescan Spent
        public void RescanSpent_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if(GlobalData.IsWalletOpen)
                {
                    RescanSpent();
                }
                else
                {
                    Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rescan Spent", "Please open wallet first.", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RSP", ex);
            }
        }

        private static async void RescanSpent()
        {
            try
            {
                RescanSpentResponse response = await GlobalData.WalletService.RescanSpent(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new RescanSpentRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("WalSV.CNW", "Failed to rescan spent. Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rescan Spent", "Error rescanning\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
                else
                {
                    Logger.LogDebug("WalSV.CNW", "Rescan spent returned successfully.");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rescan Spent", "Rescan spent command submitted successfully.", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RS", ex);
            }
        }
        #endregion // Rescan Spent

        #region Rescan Blockchain
        public void RescanBlockchain_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (GlobalData.IsWalletOpen)
                {
                    RescanBlockchain();
                }
                else
                {
                    Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rescan Blockchain", "Please open wallet first.", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RSP", ex);
            }
        }

        private static async void RescanBlockchain()
        {
            try
            {
                RescanBlockchainResponse response = await GlobalData.WalletService.RescanBlockchain(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new RescanBlockchainRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("WalSV.RB", "Failed to rescan Blockchain. Message: " + response.Error.Message + " | Code: " + response.Error.Code);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rescan Blockchain", "Error rescanning\r\n" + response.Error.Message, ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
                else
                {
                    Logger.LogDebug("WalSV.RB", "Rescan Blockchain returned successfully.");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard("Rescan Blockchain", "Rescan Blockchain command submitted successfully.", ButtonEnum.Ok);
                        _ = await box.ShowAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.RB", ex);
            }
        }
        #endregion // Rescan Blockchain

        #region View Keys/Seed
        public void ViewKeysSeed_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var window = new DisplayKeysSeedView();
                window.ShowDialog(GetWindow());
            }
            catch (Exception ex)
            {
                Logger.LogException("WalSV.VKSC", ex);
            }
        }
        #endregion // View Keys/Seed
    }
}