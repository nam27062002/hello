﻿using UnityEngine;
public class HDNotificationsManager : UbiBCN.SingletonMonoBehaviour<HDNotificationsManager>
{
	private const string HD_NOTIFICATIONS = "HD_NOTIFICATIONS";


    public void Initialise()
    {
        if (!NotificationsManager.SharedInstance.CheckIfInitialised())
        {
#if UNITY_ANDROID
			NotificationsManager.NotificationChannelConfig kNotificationsConfig = new NotificationsManager.NotificationChannelConfig ();
			kNotificationsConfig.m_strResSmallIconName = "push_notifications";
			kNotificationsConfig.m_bEnableLights = true;
			kNotificationsConfig.m_bEnableVibration = true;
			kNotificationsConfig.m_iIconColorARGB = 0x00000000;
			kNotificationsConfig.m_iLightColorARGB = 0xFFFF8C00;
			NotificationsManager.SharedInstance.Initialise(kNotificationsConfig);			          
#else
			NotificationsManager.SharedInstance.Initialise();			          
#endif

            int notificationsEnabled = PlayerPrefs.GetInt(HD_NOTIFICATIONS, 1);
            NotificationsManager.SharedInstance.SetNotificationsEnabled(notificationsEnabled > 0);

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

		int v  = enabled ? 1 : 0;
		PlayerPrefs.SetInt(HD_NOTIFICATIONS,v);

        NotificationsManager.SharedInstance.SetNotificationsEnabled(enabled);

		// Clear all notifications
        NotificationsManager.SharedInstance.CancelAllNotifications();

        // If enabled reschedule all notifications
		if (enabled){
			if ( UsersManager.currentUser != null && EggManager.incubatingEgg != null){
				EggManager.incubatingEgg.ScheduleEggNotification();
	        }
        }
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
