using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
using NervaOneWalletMiner.Rpc;
using NervaOneWalletMiner.Rpc.Daemon.Requests;
using NervaOneWalletMiner.ViewModels;
using NervaOneWalletMiner.Views;
using System;

namespace NervaOneWalletMiner;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            // Want those before UI loads
            GlobalMethods.LoadConfig();

            SetUpDefaults();

            Logger.SetUpLogger();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += OnExit;
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewModel()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
            Logger.LogDebug("APP.OFIC", "Initialization completed");
        }
        catch (Exception ex)
        {
            Logger.LogException("APP.OFIC", ex);
        }       
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Logger.LogDebug("APP.OE01", "Exiting...");

        Shutdown();
    }

    public static void Shutdown()
    {
        try
        {            
            if(GlobalData.IsWalletOpen)
            {
                ForceWalletClose();
            }

            Logger.LogDebug("APP.SD01", "Forcing wallet process close.");
            WalletProcess.ForceClose();

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
            {
                ForceDaemonStop();

                Logger.LogDebug("APP.SD01", "Forcing daemon process close.");
                DaemonProcess.ForceClose();
            }            
        }
        catch (Exception ex)
        {
            Logger.LogException("APP.SD01", ex);
        }

        Logger.LogInfo("APP.SD01", "PROGRAM TERMINATED");
        Environment.Exit(0);
    }

    public static async void ForceWalletClose()
    {
        try
        {
            Logger.LogDebug("APP.FWC1", "Closing wallet: " + GlobalData.OpenedWalletName);
            _ = await GlobalData.WalletService.CloseWallet(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new Rpc.Wallet.Requests.CloseWalletRequest());
        }
        catch (Exception ex)
        {
            Logger.LogException("APP.FWC1", ex);
        }
    }

    public static async void ForceDaemonStop()
    {
        try
        {
            Logger.LogDebug("APP.FDS1", "Stopping Daemon.");
            _ = await GlobalData.DaemonService.StopDaemon(GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].Rpc, new StopDaemonRequest());
        }
        catch (Exception ex)
        {
            Logger.LogException("APP.FDS1", ex);
        }
    }

    public static void SetUpDefaults()
    {
        try
        {
            // Set theme
            if(GlobalData.AppSettings.Theme == Theme.Default)
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Default;
            }
            else if(GlobalData.AppSettings.Theme == Theme.Light)
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            }
            else
            {
                Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            }
            

            if (GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads == 0)
            {
                // By default use 50% of threads
                GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].MiningThreads = GlobalData.CpuThreadCount > 1 ? Convert.ToInt32(Math.Floor(GlobalData.CpuThreadCount / 2.00)) : 1;
            }

            GlobalMethods.SetCoin(GlobalData.AppSettings.ActiveCoin);

            // TODO: Might need to set up other defaults
        }
        catch (Exception ex)
        {
            Logger.LogException("APP.SUD1", ex);
        }
    }
}