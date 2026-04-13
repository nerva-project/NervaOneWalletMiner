using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Avalonia;
using Avalonia.Android;
using NervaOneWalletMiner.Android.Services;
using NervaOneWalletMiner.Helpers;
using ReactiveUI.Avalonia;
using System;

namespace NervaOneWalletMiner.Android;

[Activity(
    Label = "NervaOneWalletMiner.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        StartForegroundService(new Intent(this, typeof(NervaForegroundService)));
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI()
            // EGL (GPU) mode renders at the display refresh rate via Choreographer even when
            // the UI is idle, causing constant background CPU/battery drain. Software mode
            // renders only on dirty regions, matching the idle behavior of Avalonia 11.0.x.
            .With(new AndroidPlatformOptions
            {
                RenderingMode = [AndroidRenderingMode.Software]
            });
    }

    protected override void OnPause()
    {
        base.OnPause();

        try
        {
            // Activity is no longer visible (screen off, app switched, etc.).
            // Skip all UI updates and RPC calls until the user returns — only
            // daemon keep-alive checks continue to run in the background.
            Logger.LogDebug("MNA.ONPS", "Entering background mode.");
            MasterProcess.EnterBackgroundMode();
        }
        catch (Exception ex)
        {
            Logger.LogException("MNA.ONPS", ex);
        }
    }

    protected override void OnResume()
    {
        base.OnResume();

        try
        {
            Logger.LogDebug("MNA.ONRS", "Exiting background mode.");
            MasterProcess.ExitBackgroundMode();
        }
        catch (Exception ex)
        {
            Logger.LogException("MNA.ONRS", ex);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (IsFinishing && GlobalData.AppSettings.Daemon[GlobalData.AppSettings.ActiveCoin].StopOnExit)
        {
            StopService(new Intent(this, typeof(NervaForegroundService)));
            GlobalMethods.Shutdown();
        }
    }
}
