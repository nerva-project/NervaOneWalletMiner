using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NervaWalletMiner.Helpers;
using NervaWalletMiner.ViewModels;
using NervaWalletMiner.Views;
using System;

namespace NervaWalletMiner;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
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

        SetupApplication();

        base.OnFrameworkInitializationCompleted();
    }

    public void SetupApplication()
    {
        Logger.SetUpLogger();
        log4net.Config.XmlConfigurator.Configure();
        Logger.LogDebug("App.IC", "Directories and logger initialized");
        Logger.LogInfo("App.IC", "App Version: " + GlobalData.Version + " | OS: " + Environment.OSVersion.Platform + " " + Environment.OSVersion.Version + " | CPUs: " + Environment.ProcessorCount);
    }
}