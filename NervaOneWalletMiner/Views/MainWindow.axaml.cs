using Avalonia.Controls;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;
using System.Threading.Tasks;

namespace NervaOneWalletMiner.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
		try
		{
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }
		catch (Exception ex)
		{
            Logger.LogException("MAW.CONS", ex);
        }        
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            MainViewModel vm = (MainViewModel)DataContext!;
            vm.CheckAndGetCliEvent += CheckAndDownloadCliIfNeeded;
            vm.SyncWithQuickSyncEvent += QuickSyncIfWanted;

            CheckAndDownloadCliIfNeeded();
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.MAWL", ex);
        }
    }

    private async void QuickSyncIfWanted(double percentSynced)
    {
        try
        {
            Logger.LogDebug("MAW.QSIW", "Asking user if they want to use QuickSync");
            await Dispatcher.UIThread.Invoke(async () =>
            {
                MessageBoxView window = new("QuickSync", "You're currently only " + percentSynced.ToString("P1") + " synchronized "
                    + "\n\r\n\rWould you like to use QuickSync to synchronize faster?", false);
                await window.ShowDialog(this).ContinueWith(QuickSyncConfirmDialogClosed);
            });
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.QSIW", ex);
        }
    }

    private void QuickSyncConfirmDialogClosed(Task task)
    {
        try
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                GlobalMethods.RestartWithQuickSync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.QSDC", ex);
        }
    }

    public void CheckAndDownloadCliIfNeeded()
    {
        if (GlobalData.IsConfigFound && !GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
        {
            // CLI tools missing. Need to download            
            GlobalData.IsCliToolsFound = false;
            string cliToolsLink = GlobalMethods.GetCliToolsDownloadLink(GlobalData.AppSettings.ActiveCoin);
            Logger.LogDebug("MAW.CDCN", "CLI tools not found. Asking user to confirm download link: " + cliToolsLink);

            var window = new TextBoxView("Get Client Tools", "Client Tools Download Link", cliToolsLink, string.Empty);
            window.ShowDialog(this).ContinueWith(CliToolsLinkDialogClosed);
        }
    }

    private async void CliToolsLinkDialogClosed(Task task)
    {
        try
        {
            DialogResult result = ((DialogResult)((Task<object>)task).Result);
            if (result != null && result.IsOk)
            {
                Logger.LogDebug("MAW.CTLC", "CLI tools not found. Attempting to download from: " + result.TextBoxValue);
                GlobalData.IsCliToolsDownloading = true;

                if (!string.IsNullOrEmpty(result.TextBoxValue))
                {
                    // Download and extract CLI tools
                    GlobalMethods.SetUpCliTools(result.TextBoxValue, GlobalData.CliToolsDir);
                }
            }
            else
            {
                Logger.LogDebug("MAW.CTLC", "CLI tools download cancelled.");
                GlobalData.IsCliToolsDownloading = false;

                await Dispatcher.UIThread.Invoke(async () =>
                {
                    MessageBoxView window = new("Client Tools Missing", "NervaOne cannot run without client tools. Switch coin or restart to download client tools. "
                        + "Alternatively you can put your own client tools in Daemon Setup > Open Client Tools Folder", true);
                    await window.ShowDialog(this);
                });               
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.CTLC", ex);
        }
    }
}