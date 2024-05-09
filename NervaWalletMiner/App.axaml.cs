using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.Rpc;
using NervaWalletMiner.ViewModels;
using NervaWalletMiner.Views;
using System;
using System.Configuration;

namespace NervaWalletMiner;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
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
        Logger.LogDebug("App.IC", "Initialization completed");
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Logger.LogDebug("App.AE", "Exiting...");

        Shutdown();
    }

    public static void Shutdown()
    {
        // Prevent the daemon restarting automatically before telling it to stop
        if (GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].StopOnExit)
        {
            //TODO: Call daemon method to exit
            //Daemon.StopDaemon();
            DaemonProcess.ForceClose();
        }

        WalletProcess.ForceClose();

        Logger.LogInfo("App.SD", "PROGRAM TERMINATED");

        Environment.Exit(0);
    }

    public static void SetUpDefaults()
    {
        try
        {
            if (GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads == 0)
            {
                // By default use 50% of threads
                GlobalData.ApplicationSettings.Daemon[GlobalData.ApplicationSettings.ActiveCoin].MiningThreads = GlobalData.CpuThreadCount > 1 ? Convert.ToInt32(Math.Floor(GlobalData.CpuThreadCount / 2.00)) : 1;
            }

            // TODO: Might need to set up other defaults

        }
        catch (Exception ex)
        {
            Logger.LogException("App.SUD", ex);
        }
    }
}