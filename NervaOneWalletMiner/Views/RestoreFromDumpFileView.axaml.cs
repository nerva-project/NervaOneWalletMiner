using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc.Wallet.Requests;
using NervaOneWalletMiner.Rpc.Wallet.Responses;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.IO;

namespace NervaOneWalletMiner.Views
{
    public partial class RestoreFromDumpFileView : UserControl
    {
        public RestoreFromDumpFileView()
        {
            try
            {
                InitializeComponent();
                imgCoinIcon.Source = GlobalMethods.GetLogo();
                Loaded += (_, _) => tbxWalletName.Focus();
            }
            catch (Exception ex)
            {
                Logger.LogException("RDF.CONS", ex);
            }
        }

        public async void Browse_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                var files = await TopLevel.GetTopLevel(this)!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Wallet Dump File",
                    AllowMultiple = false
                });

                if (files.Count > 0)
                {
                    tbxDumpFilePath.Text = files[0].Path.LocalPath;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RDF.BRWC", ex);
            }
        }

        public async void OkButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                string walletName = tbxWalletName.Text ?? string.Empty;
                string dumpFilePath = tbxDumpFilePath.Text ?? string.Empty;

                if (string.IsNullOrEmpty(walletName))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", "Wallet Name is required.", true));
                    return;
                }
                else if (walletName.Contains('/') || walletName.Contains('\\') || walletName.Contains(".."))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", "Wallet Name cannot contain path characters.", true));
                    return;
                }

                if (string.IsNullOrEmpty(dumpFilePath))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", "Dump File Path is required.", true));
                    return;
                }

                if (!File.Exists(dumpFilePath))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", "Dump file not found at the specified path.", true));
                    return;
                }

                if (string.IsNullOrEmpty(tbxPassword.Text))
                {
                    await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", "Password is required.", true));
                    return;
                }

                MessageBoxView confirmWindow = new("Restore Wallet", "This will create a new wallet '" + walletName + "' and import all keys from the dump file. A blockchain scan will be performed which may take some time. Continue?", false, true);
                DialogResult? confirmRes = await DialogService.ShowAsync<DialogResult>(confirmWindow);
                if (confirmRes == null || !confirmRes.IsOk)
                {
                    return;
                }

                char[] walletPassword = tbxPassword.Text.ToCharArray();
                tbxPassword.Text = string.Empty;

                btnOk.Content = "Restoring...";
                btnOk.IsEnabled = false;
                btnCancel.IsEnabled = false;
                tbxWalletName.IsEnabled = false;
                tbxDumpFilePath.IsEnabled = false;
                btnBrowse.IsEnabled = false;
                tbxPassword.IsEnabled = false;
                btnShowHidePassword.IsEnabled = false;

                Logger.LogDebug("RDF.OKBC", "Restoring wallet from dump file: " + walletName);

                if (GlobalData.IsWalletOpen)
                {
                    GlobalMethods.ForceWalletClose();
                    GlobalMethods.WalletClosedOrErrored();
                }

                ImportWalletRequest request = new()
                {
                    WalletName = walletName,
                    DumpFileWithPath = dumpFilePath,
                    Password = walletPassword
                };

                ImportWalletResponse response = await GlobalData.WalletService.ImportWallet(GlobalData.AppSettings.Wallet[GlobalData.AppSettings.ActiveCoin].Rpc, request);

                if (response.Error.IsError)
                {
                    GlobalMethods.WalletClosedOrErrored();
                    Logger.LogError("RDF.OKBC", "Failed to restore wallet " + walletName + " | Code: " + response.Error.Code + " | Message: " + response.Error.Message + " | Content: " + response.Error.Content);

                    string errorMessage = "Error restoring " + walletName + "\r\n\r\n" + GlobalMethods.GetRpcErrorMessage(response.Error.Content, response.Error.Message);

                    await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", errorMessage, true));
                    btnOk.Content = "Restore";
                    btnOk.IsEnabled = true;
                    btnCancel.IsEnabled = true;
                    tbxWalletName.IsEnabled = true;
                    tbxDumpFilePath.IsEnabled = true;
                    btnBrowse.IsEnabled = true;
                    tbxPassword.IsEnabled = true;
                    btnShowHidePassword.IsEnabled = true;
                    return;
                }

                GlobalMethods.WalletJustOpened(walletName);
                Logger.LogDebug("RDF.OKBC", "Wallet " + walletName + " restored successfully");
                await DialogService.ShowAsync(new MessageBoxView("Restore From Dump File", walletName + " restored successfully!\r\n\r\nYour wallet is now open with all imported keys.", true));

                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("RDF.OKBC", ex);
            }
        }

        public void CancelButton_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                Logger.LogDebug("RDF.CNCL", "Restore from dump file cancelled");
                UIManager.NavigateToPage(SplitViewPages.WalletSetup);
            }
            catch (Exception ex)
            {
                Logger.LogException("RDF.CNCL", ex);
            }
        }

        public void ShowHidePassword_Clicked(object sender, RoutedEventArgs args)
        {
            try
            {
                if (tbxPassword.RevealPassword)
                {
                    tbxPassword.RevealPassword = false;
                    btnShowHidePassword.Content = "Show";
                }
                else
                {
                    tbxPassword.RevealPassword = true;
                    btnShowHidePassword.Content = "Hide";
                }
            }
            catch (Exception ex)
            {
                Logger.LogException("RDF.SHPC", ex);
            }
        }
    }
}
