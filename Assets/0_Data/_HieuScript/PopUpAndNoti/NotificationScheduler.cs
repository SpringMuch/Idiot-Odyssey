using System;
using UnityEngine;
using UnityEngine.Android;
using System.Collections;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class NotificationScheduler : MonoBehaviour
{
    private const string ChannelId = "idiot_odyssey_channel";

    void Start() {
    #if UNITY_ANDROID && !UNITY_EDITOR
        var channel = new AndroidNotificationChannel()
        {
            Id = "idiot_odyssey_channel",
            Name = "Idiot Odyssey Notifications",
            Importance = Importance.Default,
            Description = "Nhắc người chơi quay lại game",
        };
    AndroidNotificationCenter.RegisterNotificationChannel(channel);

    StartCoroutine(RequestNotifWhenReady());
    #endif
    }

    IEnumerator RequestNotifWhenReady() {
        yield return null;
        float t = 0f;
        while (t < 10f && ConsentManager.Instance != null && !ConsentManager.Instance.IsReady) {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        TryScheduleDailyNotification();
    }

    private void OnConsentDoneThenRequestNotif()
    {
        if (ConsentManager.Instance != null)
            ConsentManager.Instance.OnConsentFlowFinished -= OnConsentDoneThenRequestNotif;

        TryScheduleDailyNotification();
    }
    public void TryScheduleDailyNotification()
    {
#if UNITY_ANDROID
        if (!NotificationPermission.IsGranted())
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += OnPermissionGranted;
            callbacks.PermissionDenied += OnPermissionDenied;
            callbacks.PermissionDeniedAndDontAskAgain += OnPermissionDeniedAndDontAskAgain;

            Permission.RequestUserPermission(NotificationPermission.POST_NOTIFICATIONS, callbacks);
        }
        else
        {
            ScheduleDailyNotification();
        }
#else
        Debug.Log("[NotificationScheduler] Notifications are only supported on Android.");
#endif
    }

#if UNITY_ANDROID
    private void OnPermissionGranted(string permissionName)
    {
        if (permissionName == NotificationPermission.POST_NOTIFICATIONS)
        {
            ScheduleDailyNotification();
            Debug.Log("✅ Notification permission granted — daily reminder scheduled.");
        }
    }

    private void OnPermissionDenied(string permissionName)
    {
        Debug.LogWarning("⚠️ Notification permission denied by user.");
        // Hiển thị AlertDialog native gợi ý mở Settings
        ShowNativeDialog(
            "Notification Permission",
            "To receive reminders, please enable notifications in Settings.",
            "Open Settings",
            "Cancel"
        );
    }

    private void OnPermissionDeniedAndDontAskAgain(string permissionName)
    {
        Debug.LogWarning("🚫 Notification permission permanently denied (Don't ask again).");
        // Hiển thị AlertDialog native gợi ý mở Settings
        ShowNativeDialog(
            "Notifications Disabled",
            "Notifications are disabled. Please enable them manually in Settings.",
            "Open Settings",
            "Close"
        );
    }
#endif
    private void ScheduleDailyNotification()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        var notification = new AndroidNotification
        {
            Title = "Potato misses you 🥔💔",
            Text = "Come back to help Potato overcome new challenges!",
            FireTime = System.DateTime.Now.AddHours(24), // gửi sau 24 giờ
            RepeatInterval = new TimeSpan(1, 0, 0, 0) // lặp mỗi ngày
        };

        AndroidNotificationCenter.SendNotification(notification, ChannelId);
#else
        Debug.Log("[NotificationScheduler] ScheduleDailyNotification skipped (not Android).");
#endif
    }

#if UNITY_ANDROID
    private void ShowNativeDialog(string title, string message, string positiveText, string negativeText)
    {
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    using (var builder = new AndroidJavaObject("android.app.AlertDialog$Builder", activity))
                    {
                        builder.Call<AndroidJavaObject>("setTitle", title);
                        builder.Call<AndroidJavaObject>("setMessage", message);

                        // Positive (Open Settings)
                        builder.Call<AndroidJavaObject>(
                            "setPositiveButton",
                            positiveText,
                            new PositiveButtonListener(activity)
                        );

                        // Negative (Cancel / Close)
                        builder.Call<AndroidJavaObject>("setNegativeButton", negativeText, (AndroidJavaObject)null);

                        using (var dialog = builder.Call<AndroidJavaObject>("create"))
                        {
                            dialog.Call("show");
                        }
                    }
                }));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("ShowNativeDialog failed: " + e.Message);
        }
    }
    private class PositiveButtonListener : AndroidJavaProxy
    {
        private readonly AndroidJavaObject activity;

        public PositiveButtonListener(AndroidJavaObject act)
            : base("android.content.DialogInterface$OnClickListener")
        {
            activity = act;
        }

        // onClick(DialogInterface dialog, int which)
        public void onClick(AndroidJavaObject dialog, int which)
        {
            using (var intent = new AndroidJavaObject("android.content.Intent",
                "android.settings.APP_NOTIFICATION_SETTINGS"))
            {
                intent.Call<AndroidJavaObject>("putExtra", "android.provider.extra.APP_PACKAGE",
                    activity.Call<string>("getPackageName"));
                activity.Call("startActivity", intent);
            }
        }
    }
#endif
}
