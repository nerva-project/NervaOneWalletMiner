using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using NervaOneWalletMiner.Helpers;
using NervaOneWalletMiner.Objects.Constants;
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

            GlobalMethods.SetCoin(GlobalData.AppSettings.ActiveCoin);

            GlobalMethods.DeleteOldLogFiles();

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
        Logger.LogDebug("APP.ONET", "Exiting...");

        GlobalMethods.Shutdown();
    }

    public static void SetUpDefaults()
    {
        try
        {
            GlobalMethods.SetDefaultCoinSpecificSettings();

            // Set theme
            if (GlobalData.AppSettings.Theme == Theme.Default)
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
        }
        catch (Exception ex)
        {
            Logger.LogException("APP.SUDF", ex);
        }
    }
}