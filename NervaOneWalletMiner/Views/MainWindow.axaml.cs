using Avalonia.Controls;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;

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
            vm.ShowDaemonTabEvent += ShowDaemonTab;

            CheckAndDownloadCliIfNeeded();

            GlobalMethods.ShowHideDaemonTab();
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
                DialogResult result = await window.ShowDialog<DialogResult>(this);
                if (result != null && result.IsOk)
                {
                    GlobalMethods.RestartWithQuickSync();
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.QSIW", ex);
        }
    }

    public async void CheckAndDownloadCliIfNeeded()
    {
        try
        {
            if (GlobalData.IsConfigFound && !GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
            {
                // CLI tools missing. Need to download
                GlobalData.IsCliToolsFound = false;
                Logger.LogDebug("MAW.CDCN", "CLI tools not found. Opening Coin Setup View.");

                var window = new CoinSetupView();
                DialogResult result = await window.ShowDialog<DialogResult>(this);
                if (result != null && result.IsOk)
                {
                    Logger.LogDebug("MAW.CTLC", "CLI tools not found. Attempting to download from: " + result.TextBoxValue);
                    GlobalData.IsCliToolsDownloading = true;

                    if (!string.IsNullOrEmpty(result.TextBoxValue))
                    {
                        GlobalMethods.SetUpCliTools(result.TextBoxValue, GlobalData.CliToolsDir);
                    }
                }
                else
                {
                    Logger.LogDebug("MAW.CTLC", "CLI tools download cancelled.");
                    GlobalData.IsCliToolsDownloading = false;

                    MessageBoxView msgWindow = new("Client Tools Missing", "NervaOne cannot run without client tools. Switch coin or restart to download client tools. "
                        + "Alternatively you can put your own client tools in Daemon Setup > Open Client Tools Folder", true);
                    await msgWindow.ShowDialog(this);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAW.CTLC", ex);
        }
    }

    public void ShowDaemonTab(bool isVisible)
    {
        try
        {
            if (!isVisible)
            {
                if (MainViewControl.daemon.IsVisible)
                {
                    MainViewControl.daemon.IsVisible = false;
                    if (MainViewControl.daemon.IsSelected)
                    {
                        MainViewControl.wallet.IsSelected = true;
                    }
                }
            }
            else
            {
                if (!MainViewControl.daemon.IsVisible)
                {
                    MainViewControl.daemon.IsVisible = true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.DVIS", ex);
        }
    }
}