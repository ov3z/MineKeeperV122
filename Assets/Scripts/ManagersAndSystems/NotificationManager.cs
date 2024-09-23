using UnityEngine;

#if UNITY_ANDROID || PLATFORM_ANDROID
using Unity.Notifications.Android;
#endif

public class NotificationManager : MonoBehaviour
{
    void Start()
    {

#if UNITY_ANDROID || PLATFORM_ANDROID
        AndroidNotificationCenter.CancelAllDisplayedNotifications();

        var channel = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification();
        notification.Title = "Treasures are wainting";
        notification.Text = "Discover the treasures hidden beneath the shadows and save the princess!";
        notification.FireTime = System.DateTime.Now.AddHours(12);

        //notification.LargeIcon = "icon_0";


        var id = AndroidNotificationCenter.SendNotification(notification, "channel_id");

        if (AndroidNotificationCenter.CheckScheduledNotificationStatus(id) == NotificationStatus.Scheduled)
        {
            AndroidNotificationCenter.CancelAllNotifications();
            AndroidNotificationCenter.SendNotification(notification, "channel_id");
        }
#endif
    }
}
