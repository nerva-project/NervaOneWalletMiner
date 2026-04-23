using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using ReactiveUI.Avalonia;

namespace NervaOneWalletMiner.Android;

[Application]
public class NervaApplication : AvaloniaAndroidApplication<App>
{
    public NervaApplication(nint javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        => base.CustomizeAppBuilder(builder)
            .WithInterFont()
            .UseReactiveUI(_ => { });
}
