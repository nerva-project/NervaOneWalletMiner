using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;

namespace NervaOneWalletMiner.Android.Services;

[Service(ForegroundServiceType = ForegroundService.TypeDataSync | ForegroundService.TypeSpecialUse, Exported = false)]
public class NervaForegroundService : Service
{
    private const string ChannelId = "nervaone_sync";
    private const int NotificationId = 1001;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        CreateNotificationChannel();

        Notification notification = new Notification.Builder(this, ChannelId)
            .SetContentTitle("NervaOne")
            .SetContentText("Blockchain sync running")
            .SetSmallIcon(Resource.Drawable.ic_notification)
            .SetOngoing(true)
            .Build()!;

        if (OperatingSystem.IsAndroidVersionAtLeast(34))
        {
            StartForeground(NotificationId, notification, ForegroundService.TypeSpecialUse);
        }
        else if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            StartForeground(NotificationId, notification, ForegroundService.TypeDataSync);
        }
        else
        {
#pragma warning disable CA1422 // 2-param overload is the only option on API 26-28
            StartForeground(NotificationId, notification);
#pragma warning restore CA1422
        }

        return StartCommandResult.RedeliverIntent;
    }

    private void CreateNotificationChannel()
    {
        NotificationChannel channel = new(ChannelId, "NervaOne Sync", NotificationImportance.Low);
        NotificationManager? notificationManager = (NotificationManager?)GetSystemService(NotificationService);
        notificationManager?.CreateNotificationChannel(channel);
    }
}
