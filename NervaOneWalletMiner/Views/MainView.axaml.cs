using Avalonia.Controls;
using Avalonia.Threading;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.ViewsDialogs;
using System;

namespace NervaOneWalletMiner.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
		try
		{
            InitializeComponent();

            DialogService.SetHost(
                (dialog, callback) => overlayHost.Push(dialog, callback),
                result => overlayHost.Pop(result)
            );

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].IsWalletOnly)
            {
                wallet.IsSelected = true;
            }
            else
            {
                daemon.IsSelected = true;
            }

            ShowHideToolTips();

            Loaded += MainView_Loaded;
        }
		catch (Exception ex)
		{
            Logger.LogException("MAV.CONS", ex);
        }
    }

    private void MainView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
            Logger.LogException("MAV.MAVL", ex);
        }
    }

    private async void QuickSyncIfWanted(double percentSynced)
    {
        try
        {
            Logger.LogDebug("MAV.QSIW", "Asking user if they want to use QuickSync");
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                MessageBoxView window = new("QuickSync", "You're currently only " + percentSynced.ToString("P1") + " synchronized "
                    + "\n\r\n\rWould you like to use QuickSync to synchronize faster?", false);
                DialogResult? result = await DialogService.ShowAsync<DialogResult>(window);
                if (result != null && result.IsOk)
                {
                    await GlobalMethods.RestartWithQuickSync();
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.QSIW", ex);
        }
    }

    public void CheckAndDownloadCliIfNeeded()
    {
        try
        {
            if (GlobalData.IsConfigFound && !GlobalMethods.DirectoryContainsCliTools(GlobalData.CliToolsDir))
            {
                GlobalData.IsCliToolsFound = false;
                Logger.LogDebug("MAV.CDCN", "CLI tools not found. Navigating to Coin Setup View.");
                UIManager.NavigateToCoinSetup();
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.CDCN", ex);
        }
    }

    public void ShowDaemonTab(bool isVisible)
    {
        try
        {
            if (!isVisible)
            {
                if (daemon.IsVisible)
                {
                    daemon.IsVisible = false;
                    if (daemon.IsSelected)
                    {
                        wallet.IsSelected = true;
                    }
                }
            }
            else
            {
                if (!daemon.IsVisible)
                {
                    daemon.IsVisible = true;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.SDTB", ex);
        }
    }

    private void spvMain_PaneOpenedClosed(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            ShowHideToolTips();
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.MPOC", ex);
        }
    }

    private void ShowHideToolTips()
    {
        try
        {
            if (spvMain.IsPaneOpen)
            {
                ToolTip.SetTip(daemon, null);
                ToolTip.SetTip(wallet, null);
                ToolTip.SetTip(transfers, null);
                ToolTip.SetTip(address_book, null);
                ToolTip.SetTip(daemon_setup, null);
                ToolTip.SetTip(wallet_setup, null);
                ToolTip.SetTip(settings, null);
                ToolTip.SetTip(about, null);
                ToolTip.SetTip(btnLines, "Collapse");
            }
            else
            {
                ToolTip.SetTip(daemon, "Daemon");
                ToolTip.SetTip(wallet, "Wallet");
                ToolTip.SetTip(transfers, "Transfers");
                ToolTip.SetTip(address_book, "Address Book");
                ToolTip.SetTip(daemon_setup, "Daemon Setup");
                ToolTip.SetTip(wallet_setup, "Wallet Setup");
                ToolTip.SetTip(settings, "Settings");
                ToolTip.SetTip(about, "About");
                ToolTip.SetTip(btnLines, "Expand");
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MAV.SHTT", ex);
        }
    }
}