using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views
{
    public partial class WalletSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");

        public WalletSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();

                tbxLogLevel.Text = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].LogLevel.ToString();
                tbxWalletUnlockMinutes.Text = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.CONST", ex);
            }
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
                Logger.LogException("WAS.OWFC", ex);
            }
        }

        public void OpenWalletExportsFolder_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = GlobalData.ExportsDir,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.OWEC", ex);
            }
        }

        public void SaveSettings_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isChanged = false;

                bool isInt = int.TryParse(tbxLogLevel.Text, out int logLevel);
                if(!isInt || logLevel < 0 || logLevel > 5)
                {
                    MessageBoxView window = new("Save Settings", "Log level needs to be an integer between 0 and 5", true);
                    window.ShowDialog(GetWindow());
                }
                else
                {
                    if (GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].LogLevel != logLevel)
                    {
                        GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].LogLevel = logLevel;
                        isChanged = true;
                    }
                }

                isInt = int.TryParse(tbxWalletUnlockMinutes.Text, out int unlockMinutes);
                if (!isInt || unlockMinutes < 0 || unlockMinutes > 10000)
                {
                    MessageBoxView window = new("Save Settings", "Wallet Unlock Minutes needs to be an integer between 0 and 10000", true);
                    window.ShowDialog(GetWindow());
                }
                else
                {
                    if (GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes != unlockMinutes)
                    {
                        GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes = unlockMinutes;
                        isChanged = true;
                    }
                }

                if (isChanged)
                {
                    GlobalMethods.SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.SSC1", ex);
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
                Logger.LogException("WAS.CWC1", ex);
            }
        }

        private async void CreateNewWallet(string walletName, string walletPassword, string walletLanguage)
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
                    GlobalMethods.WalletClosedOrErrored();

                    Logger.LogError("WAS.CNW1", "Failed to create wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Create Wallet", "Error creating " + walletName + " wallet\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    GlobalMethods.WalletJustOpened(walletName);

                    Logger.LogDebug("WAS.CNW1", "Wallet " + walletName + " created successfully");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Create Wallet", walletName + " wallet created successfully!\r\n\r\nYour new wallet is now open. Make sure to save your seed phrase and keys!", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.CNW1", ex);
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
                Logger.LogException("WAS.RFSC", ex);
            }
        }

        private void RestoreFromSeedDialogClosed(Task task)
        {
            DialogResult result;

            try
            {
                result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk)
                {
                    // Close wallet first, if one is opened
                    if (GlobalData.IsWalletOpen)
                    {
                        GlobalMethods.ForceWalletClose();
                        GlobalMethods.WalletClosedOrErrored();
                    }

                    if (!string.IsNullOrEmpty(result.SeedPhrase) && !string.IsNullOrEmpty(result.WalletName) && !string.IsNullOrEmpty(result.WalletPassword))
                    {
                        RestoreFromSeed(result.SeedPhrase, result.SeedOffset, result.WalletName, result.WalletPassword, result.WalletLanguage);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RSDC", ex);
            }
            finally
            {
                result = new DialogResult();
            }
        }

        private async void RestoreFromSeed(string seed, string seedOffset, string walletName, string walletPassword, string walletLanguage)
        {
            RestoreFromSeedRequest request = new()
            {
                Seed = seed,
                SeedOffset = seedOffset,
                WalletName = walletName,
                Password = walletPassword,
                Language = walletLanguage
            };

            try
            {
                RestoreFromSeedResponse response = await GlobalData.WalletService.RestoreFromSeed(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();

                    Logger.LogError("WAS.RFS1", "Failed to restore wallet " + walletName + " | Info: " + response.Info + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Restore from Seed", "Error restoring " + walletName + " wallet\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    GlobalMethods.WalletJustOpened(walletName);

                    Logger.LogDebug("WAS.RFS1", "Wallet " + walletName + " restored successfully! Info: " + response.Info);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Restore from Seed", walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RFS1", ex);
            }
            finally
            {
                seed = walletPassword = string.Empty;
                request = new();
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
                Logger.LogException("WAS.RFKC", ex);
            }
        }

        private void RestoreFromKeysDialogClosed(Task task)
        {
            DialogResult result;

            try
            {
                result = ((DialogResult)((Task<object>)task).Result);
                if (result != null && result.IsOk)
                {
                    // Close wallet first, if one is opened
                    if (GlobalData.IsWalletOpen)
                    {
                        GlobalMethods.ForceWalletClose();
                        GlobalMethods.WalletClosedOrErrored();
                    }

                    if (!string.IsNullOrEmpty(result.WalletAddress) && !string.IsNullOrEmpty(result.ViewKey) && !string.IsNullOrEmpty(result.SpendKey)
                        && !string.IsNullOrEmpty(result.WalletName) && !string.IsNullOrEmpty(result.WalletPassword))
                    {
                        RestoreFromKeys(result.WalletAddress, result.ViewKey, result.SpendKey, result.WalletName, result.WalletPassword, result.WalletLanguage);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RKDC", ex);
            }
            finally
            {
                result = new DialogResult();
            }
        }

        private async void RestoreFromKeys(string walletAddress, string viewKey, string spendKey, string walletName, string walletPassword, string walletLanguage)
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

            try
            {
                RestoreFromKeysResponse response = await GlobalData.WalletService.RestoreFromKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();

                    Logger.LogError("WAS.RFKS", "Failed to restore wallet " + walletName + " | Info: " + response.Info + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Restore from Keys", "Error restoring " + walletName + " wallet\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    GlobalMethods.WalletJustOpened(walletName);

                    Logger.LogDebug("WAS.RFKS", "Wallet " + walletName + " restored successfully! Info: " + response.Info);
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Restore from Keys", walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RFKS", ex);
            }
            finally
            {
                viewKey = spendKey = walletPassword = string.Empty;
                request = new();
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
                    Logger.LogDebug("WAS.RSTC", "Trying to rescan spent but wallet closed");
                    Dispatcher.UIThread.Invoke(async () =>
                    {                        
                        MessageBoxView window = new("Rescan Spent", "Please open wallet first.", true);
                        await window.ShowDialog(GetWindow());
                    });
                }                
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RSSP", ex);
            }
        }

        private async void RescanSpent()
        {
            try
            {
                RescanSpentResponse response = await GlobalData.WalletService.RescanSpent(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new RescanSpentRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("WAS.RSPT", "Failed to rescan spent | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Rescan Spent", "Error rescanning\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAS.RSPT", "Rescan spent returned successfully");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Rescan Spent", "Rescan spent command submitted successfully.", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RSPT", ex);
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
                    Logger.LogDebug("WAS.RSTC", "Trying to rescan blockchain but wallet closed");
                    Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Rescan Blockchain", "Please open wallet first.", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RBCC", ex);
            }
        }

        private async void RescanBlockchain()
        {
            try
            {
                RescanBlockchainResponse response = await GlobalData.WalletService.RescanBlockchain(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new RescanBlockchainRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("WAS.RSBC", "Failed to rescan Blockchain | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    await Dispatcher.UIThread.Invoke(async () =>
                    {
                        MessageBoxView window = new("Rescan Blockchain", "Error rescanning\r\n" + response.Error.Message, true);
                        await window.ShowDialog(GetWindow());
                    });
                }
                else
                {
                    Logger.LogDebug("WAS.RSBC", "Rescan Blockchain returned successfully");
                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        MessageBoxView window = new("Rescan Blockchain", "Rescan Blockchain command submitted successfully.", true);
                        await window.ShowDialog(GetWindow());
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RSBC", ex);
            }
        }
        #endregion // Rescan Blockchain

        #region View Keys/Seed
        public async void ViewKeysSeed_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                bool isAuthorized = false;
                if(!GlobalData.IsWalletOpen)
                {
                    MessageBoxView window = new("View Keys and Seed", "Please open wallet first.", true);
                    await window.ShowDialog(GetWindow());
                }
                else if (DateTime.Now > GlobalData.WalletPassProvidedTime.AddMinutes(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes))
                {
                    // Password required
                    TextBoxView textWindow = new TextBoxView("Provide Wallet Password", "Please provide wallet password", string.Empty, "Required - Wallet password", true, true);
                    DialogResult passRes = await textWindow.ShowDialog<DialogResult>(GetWindow());
                    if (passRes != null && passRes.IsOk)
                    {
                        if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                        {
                            // Self managed lock
                            if (Hashing.Verify(passRes.TextBoxValue, GlobalData.WalletPasswordHash))
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
                                Logger.LogError("WAS.VKSC", "Unlock error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
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

                if (isAuthorized)
                {
                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].AreKeysDumpedToFile)
                    {
                        DumpKeysToFile();
                    }
                    else
                    {
                        var window = new DisplayKeysSeedView();
                        await window.ShowDialog(GetWindow());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.VKSC", ex);
            }
        }

        private async void DumpKeysToFile()
        {
            try
            {
                GetPrivateKeysRequest request = new GetPrivateKeysRequest()
                {
                    DumpFileWithPath = Path.Combine(GlobalData.ExportsDir, GlobalData.WalletDumpFileName)
                };

                GlobalMethods.DeleteFileIfExists(request.DumpFileWithPath);

                GetPrivateKeysResponse response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WAS.GASK", "Failed to dump keys for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                }
                else
                {
                    var window = new TextBoxView("View Private Keys", "Keys have been exported to below file", request.DumpFileWithPath, string.Empty);
                    await window.ShowDialog(GetWindow());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.GASK", ex);
            }
        }
        #endregion // View Keys/Seed
    }
}