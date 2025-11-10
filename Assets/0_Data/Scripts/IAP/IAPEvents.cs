using System;
public static class IAPEvents
{
    public static Action<string> OnInfo;
    public static Action<string> OnError;
    public static Action<string> OnSuccess;
    public static Action<string, Action<bool>> OnConfirm; 
    public static Action<bool> OnNoAdsStatusChanged;
}
