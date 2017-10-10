using UnityEngine;
public class HDNotificationsManager : UbiBCN.SingletonMonoBehaviour<HDNotificationsManager>
{
    public void Initialise()
    {
        if (!NotificationsManager.SharedInstance.CheckIfInitialised())
        {
            NotificationsManager.SharedInstance.Initialise();

            // [DGR] TODO: icon has to be created and located in the right folder
#if UNITY_ANDROID
            NotificationsManager.SharedInstance.SetNotificationIcons("", "push_notifications", 0x00000000);
#endif

            //int notificationsEnabled = PlayerPrefs.GetInt(PopupSettings.KEY_SETTINGS_NOTIFICATIONS, 1);
            //NotificationsManager.SharedInstance.SetNotificationsEnabled(notificationsEnabled > 0);

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("Notifications enabled = " + GetNotificationsEnabled());
        }
    }

    public bool GetNotificationsEnabled()
    {
        return NotificationsManager.SharedInstance.GetNotificationsEnabled();
    }

    public void SetNotificationsEnabled(bool enabled)
    {
        Log("SetNotificationsEnabled = " + enabled);
        NotificationsManager.SharedInstance.SetNotificationsEnabled(enabled);
    }

    public void ScheduleNotification(string strSKU, string strBody, string strAction, int iTimeLeft)
    {
        Log("ScheduleNotification enabled = " + GetNotificationsEnabled() + " strSKU = " + strSKU + " strBody = " + strBody + " strAction = " + strAction + " iTimeLeft = " + iTimeLeft);
        NotificationsManager.SharedInstance.ScheduleNotification(strSKU, strBody, strAction, iTimeLeft);
    }

    public void CancelNotification(string strSKU)
    {
		Log("CancelNotification enabled = " + GetNotificationsEnabled() + " strSKU = " + strSKU);
    	NotificationsManager.SharedInstance.CancelNotification(strSKU);
    }

    #region log
    private const string LOG_CHANNEL = "[HDNotificationsManager]";
    private void Log(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.Log(msg);
    }

    private void LogWarning(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogWarning(msg);
    }

    private void LogError(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogError(msg);
    }
    #endregion
}
