using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace NervaOneWalletMiner.Android.Services;

[Service(ForegroundServiceType = (int)ForegroundService.TypeDataSync, Exported = false)]
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
            .SetSmallIcon(Resource.Drawable.Icon)
            .SetOngoing(true)
            .Build()!;

        StartForeground(NotificationId, notification, ForegroundService.TypeDataSync);
        return StartCommandResult.Sticky;
    }

    private void CreateNotificationChannel()
    {
        NotificationChannel channel = new(ChannelId, "NervaOne Sync", NotificationImportance.Low);
        NotificationManager? notificationManager = (NotificationManager?)GetSystemService(NotificationService);
        notificationManager?.CreateNotificationChannel(channel);
    }
}
