using Avalonia.Controls;
using Avalonia.Interactivity;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Diagnostics;

namespace NervaOneWalletMiner.Views
{
    public partial class WalletSetupView : UserControl
    {
        Window GetWindow() => TopLevel.GetTopLevel(this) as Window ?? throw new NullReferenceException("Invalid Owner");
        WalletSetupViewModel GetVm() => (WalletSetupViewModel)DataContext!;

        public WalletSetupView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
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
                Process.Start(new ProcessStartInfo { FileName = GlobalData.WalletDir, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.OWFC", ex);
            }
        }

        public async void SweepBelow_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var prereq = GetVm().CheckPrerequisites(true, "Sweep Below");
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                var window = new SweepBelowView();
                DialogResult result = await window.ShowDialog<DialogResult>(GetWindow());

                if (result == null || !result.IsOk)
                {
                    return;
                }

                var opResult = await GetVm().SweepBelow(result.SweepBelowAmount, result.SweepBelowAddress);

                var owner = TopLevel.GetTopLevel(this) as Window;
                if (owner != null)
                {
                    await new MessageBoxView(opResult.Title, opResult.Message, true).ShowDialog(owner);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.SWBC", ex);
            }
        }

        public void OpenWalletExportsFolder_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = GlobalData.ExportsDir, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.OWEC", ex);
            }
        }

        public async void SaveSettings_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var error = GetVm().ValidateAndApplySettings();
                if (!error.IsSuccess)
                {
                    await new MessageBoxView(error.Title, error.Message, true).ShowDialog(GetWindow());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.SSC1", ex);
            }
        }

        #region Create Wallet
        public async void CreateWallet_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var prereq = GetVm().CheckPrerequisites(false, "Create New Wallet");
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                var window = new CreateWalletView();
                DialogResult result = await window.ShowDialog<DialogResult>(GetWindow());

                if (result == null || !result.IsOk)
                {
                    return;
                }

                if (string.IsNullOrEmpty(result.WalletName) || result.WalletPassword.Length == 0)
                {
                    return;
                }

                var opResult = await GetVm().CreateNewWallet(result.WalletName, result.WalletPassword, result.WalletLanguage);

                if (opResult.IsSuccess)
                {
                    await new DisplayKeysSeedView("Wallet created successfully! Your new wallet is now open.\r\n\r\nPlease make sure to save your seed phrase and keys to a safe place. You'll need them if you ever need to restore your wallet. If somebody gets a hold of those, they can steal your funds!").ShowDialog(GetWindow());
                }
                else if (!string.IsNullOrEmpty(opResult.Message))
                {
                    await new MessageBoxView(opResult.Title, opResult.Message, true).ShowDialog(GetWindow());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.CWC1", ex);
            }
        }
        #endregion // Create Wallet

        #region Restore from Seed
        public async void RestoreFromSeed_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var prereq = GetVm().CheckPrerequisites(false, "Restore Wallet from Seed");
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                var window = new RestoreFromSeedView();
                DialogResult result = await window.ShowDialog<DialogResult>(GetWindow());

                if (result == null || !result.IsOk)
                {
                    return;
                }

                if (result.SeedPhrase.Length == 0 || string.IsNullOrEmpty(result.WalletName) || result.WalletPassword.Length == 0)
                {
                    return;
                }

                if (GlobalData.IsWalletOpen)
                {
                    GlobalMethods.ForceWalletClose();
                    GlobalMethods.WalletClosedOrErrored();
                }

                var opResult = await GetVm().RestoreFromSeed(result.SeedPhrase, result.SeedOffset, result.WalletName, result.WalletPassword, result.WalletLanguage);
                await new MessageBoxView(opResult.Title, opResult.Message, true).ShowDialog(GetWindow());
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RFSC", ex);
            }
        }
        #endregion // Restore from Seed

        #region Restore from Keys
        public async void RestoreFromKeys_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var prereq = GetVm().CheckPrerequisites(false, "Restore Wallet from Keys");
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                var window = new RestoreFromKeysView();
                DialogResult result = await window.ShowDialog<DialogResult>(GetWindow());

                if (result == null || !result.IsOk)
                {
                    return;
                }

                if (string.IsNullOrEmpty(result.WalletAddress)
                    || result.ViewKey.Length == 0
                    || result.SpendKey.Length == 0
                    || string.IsNullOrEmpty(result.WalletName)
                    || result.WalletPassword.Length == 0)
                {
                    return;
                }

                if (GlobalData.IsWalletOpen)
                {
                    GlobalMethods.ForceWalletClose();
                    GlobalMethods.WalletClosedOrErrored();
                }

                var opResult = await GetVm().RestoreFromKeys(result.WalletAddress, result.ViewKey, result.SpendKey, result.WalletName, result.WalletPassword, result.WalletLanguage);
                await new MessageBoxView(opResult.Title, opResult.Message, true).ShowDialog(GetWindow());
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RFKC", ex);
            }
        }
        #endregion // Restore from Keys

        #region Rescan Spent
        public async void RescanSpent_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var prereq = GetVm().CheckPrerequisites(true, "Rescan Spent");
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                var opResult = await GetVm().RescanSpent();
                await new MessageBoxView(opResult.Title, opResult.Message, true).ShowDialog(GetWindow());
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RSSP", ex);
            }
        }
        #endregion // Rescan Spent

        #region Rescan Blockchain
        public async void RescanBlockchain_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var prereq = GetVm().CheckPrerequisites(true, "Rescan Blockchain");
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                var opResult = await GetVm().RescanBlockchain();
                await new MessageBoxView(opResult.Title, opResult.Message, true).ShowDialog(GetWindow());
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.RBCC", ex);
            }
        }
        #endregion // Rescan Blockchain

        #region View Keys/Seed
        public async void ViewKeysSeed_Clicked(object sender, RoutedEventArgs args)
        {
            string title = "View Keys and Seed";

            try
            {
                var vm = GetVm();

                var prereq = vm.CheckPrerequisites(true, title);
                if (!prereq.IsSuccess)
                {
                    await new MessageBoxView(prereq.Title, prereq.Message, true).ShowDialog(GetWindow());
                    return;
                }

                bool isAuthorized = false;
                if (vm.IsPasswordStillValid())
                {
                    isAuthorized = true;
                }
                else
                {
                    TextBoxView textWindow = new("Provide Wallet Password", "Please provide wallet password", string.Empty, "Required - Wallet password", true, true);
                    DialogResult passRes = await textWindow.ShowDialog<DialogResult>(GetWindow());

                    if (passRes == null || !passRes.IsOk)
                    {
                        return;
                    }

                    if (GlobalData.CoinSettings[GlobalData.AppSettings.ActiveCoin].IsPassRequiredToOpenWallet)
                    {
                        isAuthorized = vm.VerifyPasswordLocally(passRes.TextBoxValue.ToCharArray());
                        if (isAuthorized)
                        {
                            GlobalData.WalletPassProvidedTime = DateTime.Now;
                        }
                        else
                        {
                            await new MessageBoxView(title, "Incorrect password", true).ShowDialog(GetWindow());
                        }
                    }
                    else
                    {
                        var unlockResult = await vm.UnlockWithPassword(passRes.TextBoxValue);
                        if (!unlockResult.IsSuccess && !string.IsNullOrEmpty(unlockResult.Message))
                        {
                            await new MessageBoxView(unlockResult.Title, unlockResult.Message, true).ShowDialog(GetWindow());
                        }
                        isAuthorized = unlockResult.IsSuccess;
                    }
                }

                if (!isAuthorized)
                {
                    return;
                }

                if (vm.ShouldDumpKeysToFile())
                {
                    var (isSuccess, dumpPath) = await vm.DumpKeysToFile();
                    if (isSuccess)
                    {
                        await new TextBoxView("View Private Keys", "Keys have been exported to below file", dumpPath, string.Empty).ShowDialog(GetWindow());
                    }
                }
                else
                {
                    await new DisplayKeysSeedView("Please make sure to save your seed phrase and keys to a safe place. You'll need them if you ever need to restore your wallet. If somebody gets a hold of those, they can steal your funds!").ShowDialog(GetWindow());
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("WAS.VKSC", ex);
            }
        }
        #endregion // View Keys/Seed
    }
}
