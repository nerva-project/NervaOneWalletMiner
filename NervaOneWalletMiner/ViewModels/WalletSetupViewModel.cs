using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.ViewModels
{
    internal class WalletSetupViewModel : ViewModelBase
    {
        public record WalletOperationResult(bool IsSuccess, string Title = "", string Message = "");

        #region Reactive Properties

        private string _logLevel = string.Empty;
        public string LogLevel
        {
            get => _logLevel;
            set => this.RaiseAndSetIfChanged(ref _logLevel, value);
        }

        private string _unlockMinutes = string.Empty;
        public string UnlockMinutes
        {
            get => _unlockMinutes;
            set => this.RaiseAndSetIfChanged(ref _unlockMinutes, value);
        }

        #endregion // Reactive Properties

        public WalletSetupViewModel()
        {
            try
            {
                var walletSettings = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin];
                LogLevel = walletSettings.LogLevel.ToString();
                UnlockMinutes = walletSettings.UnlockMinutes.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.CONS", ex);
            }
        }

        public WalletOperationResult CheckPrerequisites(bool requireWallet, string operationTitle)
        {
            try
            {
                WalletOperationResult result = new(true);

                if (!GlobalData.IsCliToolsFound)
                {
                    Logger.LogDebug("WSM.CHKP", operationTitle + " but CLI tools not found");
                    result = new WalletOperationResult(false, operationTitle, "Client tools missing. Cannot perform operation until client tools are downloaded and running.");
                }
                else if (!GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly && !GlobalData.IsInitialDaemonConnectionSuccess)
                {
                    Logger.LogDebug("WSM.CHKP", operationTitle + " but daemon not running");
                    result = new WalletOperationResult(false, operationTitle, "Daemon not running. Cannot perform operation until connection is established.");
                }
                else if (requireWallet && !GlobalData.IsWalletOpen)
                {
                    Logger.LogDebug("WSM.CHKP", operationTitle + " but wallet not open");
                    result = new WalletOperationResult(false, operationTitle, "Please open wallet first.");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.CHKP", ex);
                return new WalletOperationResult(false, operationTitle, ex.Message);
            }
        }

        public WalletOperationResult ValidateAndApplySettings()
        {
            string title = "Save Settings";

            try
            {                
                WalletOperationResult result = new(true);                

                bool isChanged = false;
                var walletSettings = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin];

                if (!int.TryParse(LogLevel, out int logLevel) || logLevel < 0 || logLevel > 5)
                {
                    result = new WalletOperationResult(false, title, "Log level needs to be an integer between 0 and 5");
                }
                else if (walletSettings.LogLevel != logLevel)
                {
                    walletSettings.LogLevel = logLevel;
                    isChanged = true;
                }

                if (result.IsSuccess && (!int.TryParse(UnlockMinutes, out int unlockMinutes) || unlockMinutes < 0 || unlockMinutes > 10000))
                {
                    result = new WalletOperationResult(false, title, "Wallet Unlock Minutes needs to be an integer between 0 and 10000");
                }
                else if (int.TryParse(UnlockMinutes, out unlockMinutes) && walletSettings.UnlockMinutes != unlockMinutes)
                {
                    walletSettings.UnlockMinutes = unlockMinutes;
                    isChanged = true;
                }

                if (isChanged)
                {
                    GlobalMethods.SaveConfig();
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.VAPS", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
        }

        public async Task<WalletOperationResult> CreateNewWallet(string walletName, string walletPassword, string walletLanguage)
        {
            string title = "Create Wallet";
            CreateWalletRequest request;

            try
            {
                request = new()
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
                    Logger.LogError("WSM.CNWL", "Failed to create wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return new WalletOperationResult(false, title, "Error creating " + walletName + " wallet\r\n" + response.Error.Message);
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("WSM.CNWL", "Wallet " + walletName + " created successfully");
                return new WalletOperationResult(true);
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.CNWL", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
            finally
            {
                request = new();
            }
        }

        public async Task<WalletOperationResult> RestoreFromSeed(string seed, string seedOffset, string walletName, string walletPassword, string walletLanguage)
        {
            string title = "Restore from Seed";
            RestoreFromSeedRequest request;

            try
            {
                request = new()
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
                    GlobalMethods.WalletClosedOrErrored();
                    Logger.LogError("WSM.RFSD", "Failed to restore wallet " + walletName + " | Info: " + response.Info + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return new WalletOperationResult(false, title, "Error restoring " + walletName + " wallet\r\n" + response.Error.Message);
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("WSM.RFSD", "Wallet " + walletName + " restored successfully! Info: " + response.Info);
                return new WalletOperationResult(true, title, walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.");
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.RFSD", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
            finally
            {
                seed = walletPassword = string.Empty;
                request = new();
            }
        }

        public async Task<WalletOperationResult> RestoreFromKeys(string walletAddress, string viewKey, string spendKey, string walletName, string walletPassword, string walletLanguage)
        {
            string title = "Restore from Keys";
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
                    Logger.LogError("WSM.RFKY", "Failed to restore wallet " + walletName + " | Info: " + response.Info + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return new WalletOperationResult(false, title, "Error restoring " + walletName + " wallet\r\n" + response.Error.Message);
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("WSM.RFKY", "Wallet " + walletName + " restored successfully! Info: " + response.Info);
                return new WalletOperationResult(true, title, walletName + " wallet restored\r\n\r\nYour new wallet is now open. It will take some time to synchronize your transactions.");
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.RFKY", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
            finally
            {
                viewKey = spendKey = walletPassword = string.Empty;
                request = new();
            }
        }

        public async Task<WalletOperationResult> RescanSpent()
        {
            string title = "Rescan Spent";

            try
            {
                RescanSpentResponse response = await GlobalData.WalletService.RescanSpent(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new RescanSpentRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("WSM.RSPT", "Failed to rescan spent | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return new WalletOperationResult(false, title, "Error rescanning\r\n" + response.Error.Message);
                }

                Logger.LogDebug("WSM.RSPT", "Rescan spent returned successfully");
                return new WalletOperationResult(true, title, "Rescan spent command submitted successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.RSPT", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
        }

        public async Task<WalletOperationResult> RescanBlockchain()
        {
            string title = "Rescan Blockchain";

            try
            {
                RescanBlockchainResponse response = await GlobalData.WalletService.RescanBlockchain(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, new RescanBlockchainRequest());

                if (response.Error.IsError)
                {
                    Logger.LogError("WSM.RSBC", "Failed to rescan Blockchain | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return new WalletOperationResult(false, title, "Error rescanning\r\n" + response.Error.Message);
                }

                Logger.LogDebug("WSM.RSBC", "Rescan Blockchain returned successfully");
                return new WalletOperationResult(true, title, "Rescan Blockchain command submitted successfully.");
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.RSBC", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
        }

        public bool IsPasswordStillValid()
        {
            try
            {
                return DateTime.Now <= GlobalData.WalletPassProvidedTime.AddMinutes(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes);
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.IPSV", ex);
                return false;
            }
        }

        public bool VerifyPasswordLocally(string password)
        {
            try
            {
                return Hashing.Verify(password, GlobalData.WalletPasswordHash);
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.VRPL", ex);
                return false;
            }
        }

        public async Task<WalletOperationResult> UnlockWithPassword(string password)
        {
            string title = "Unlock Wallet";
            UnlockWithPassRequest request = new()
            {
                Password = password,
                TimeoutInSeconds = GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].UnlockMinutes * 60
            };

            try
            {
                UnlockWithPassResponse response = await GlobalData.WalletService.UnlockWithPass(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WSM.ULKP", "Unlock error | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return new WalletOperationResult(false, title, "Unlock error\r\n\r\n" + response.Error.Message);
                }

                GlobalData.WalletPassProvidedTime = DateTime.Now;
                return new WalletOperationResult(true);
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.ULKP", ex);
                return new WalletOperationResult(false, title, ex.Message);
            }
            finally
            {
                request = new();
            }
        }

        public bool ShouldDumpKeysToFile()
        {
            try
            {
                return GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].AreKeysDumpedToFile;
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.SDKT", ex);
                return false;
            }
        }

        // Returns (isSuccess, dumpFilePath). DumpFilePath is only valid when success is true.
        public async Task<(bool IsSuccess, string DumpFilePath)> DumpKeysToFile()
        {
            string dumpFilePath = Path.Combine(GlobalData.ExportsDir, GlobalData.WalletDumpFileName);
            GetPrivateKeysRequest request = new() { DumpFileWithPath = dumpFilePath };

            try
            {
                GlobalMethods.DeleteFileIfExists(dumpFilePath);

                GetPrivateKeysResponse response = await GlobalData.WalletService.GetPrivateKeys(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    Logger.LogError("WSM.DKTF", "Failed to dump keys for " + GlobalData.OpenedWalletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);
                    return (false, string.Empty);
                }

                Logger.LogDebug("WSM.DKTF", "Keys dumped successfully for " + GlobalData.OpenedWalletName);
                return (true, dumpFilePath);
            }
            catch (Exception ex)
            {
                Logger.LogException("WSM.DKTF", ex);
                return (false, string.Empty);
            }
        }
    }
}