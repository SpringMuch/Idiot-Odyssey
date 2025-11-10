using UnityEngine;
public static class NotificationPermission
{
    public const string POST_NOTIFICATIONS = "android.permission.POST_NOTIFICATIONS";
    public static bool IsGranted()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var permissionChecker = new AndroidJavaClass("androidx.core.app.ActivityCompat"))
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                int result = permissionChecker.CallStatic<int>("checkSelfPermission", activity, POST_NOTIFICATIONS);
                return result == 0;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("NotificationPermission check failed: " + e.Message);
            return false;
        }
#else
        return true;
#endif
    }
}
