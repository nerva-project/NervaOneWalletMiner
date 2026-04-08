using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

using Avalonia;
using Avalonia.Android;
using NervaOneWalletMiner.Android.Services;
using NervaOneWalletMiner.Helpers;
using ReactiveUI.Avalonia;

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
            .UseReactiveUI();
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
