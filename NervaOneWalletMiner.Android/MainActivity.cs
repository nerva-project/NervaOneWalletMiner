using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;

using Avalonia.Android;
using NervaOneWalletMiner.Android.Services;
using NervaOneWalletMiner.Helpers;
using System;

namespace NervaOneWalletMiner.Android;

[Activity(
    Label = "NervaOneWalletMiner.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@mipmap/Icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        StartForegroundService(new Intent(this, typeof(NervaForegroundService)));
    }

    private void RequestIgnoreBatteryOptimizations()
    {
        try
        {
            PowerManager? powerManager = (PowerManager?)GetSystemService(PowerService);
            if (powerManager != null && !powerManager.IsIgnoringBatteryOptimizations(PackageName))
            {
                Intent intent = new(Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(global::Android.Net.Uri.Parse("package:" + PackageName));
                StartActivity(intent);
            }
        }
        catch (Exception ex)
        {
            Logger.LogException("MNA.RIBO", ex);
        }
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
            RequestIgnoreBatteryOptimizations();
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
