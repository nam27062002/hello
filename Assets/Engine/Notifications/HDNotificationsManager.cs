﻿using UnityEngine;
public class HDNotificationsManager : UbiBCN.SingletonMonoBehaviour<HDNotificationsManager>
{
	private const string HD_NOTIFICATIONS = "HD_NOTIFICATIONS";
	public const string SILENT_FLAG = "Notifications.Silent";

    public void Initialise()
    {
        if (!NotificationsManager.SharedInstance.CheckIfInitialised())
        {
			/*
			#if UNITY_IOS
			UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound, true);
			CheckRemoteNotifications();
			#endif
			*/

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

    public void Update()
    {
#if UNITY_IOS    
		CheckRemoteNotifications();
#endif        
    }

	#if UNITY_IOS
	private void CheckRemoteNotifications() {
		if ( UnityEngine.iOS.NotificationServices.remoteNotificationCount > 0)
		{
			ControlPanel.Log(LOG_CHANNEL + "================= ");
			ControlPanel.Log(LOG_CHANNEL + " Number Of remote notificaions: " + UnityEngine.iOS.NotificationServices.remoteNotificationCount);
			int max = UnityEngine.iOS.NotificationServices.remoteNotificationCount;
			for (int i = 0; i < max; i++)
			{
				UnityEngine.iOS.RemoteNotification notification = UnityEngine.iOS.NotificationServices.remoteNotifications[i];
				ControlPanel.Log(LOG_CHANNEL + " Body:" + notification.alertBody);
				ControlPanel.Log(LOG_CHANNEL + " Sound:" + notification.soundName);
				ControlPanel.Log(LOG_CHANNEL + " BadgeNumber:" + notification.applicationIconBadgeNumber);
				ControlPanel.Log(LOG_CHANNEL + " Has Action:" + notification.hasAction);
				ControlPanel.Log(LOG_CHANNEL + " UserInfo Size:" + notification.userInfo.Count);
				foreach (System.Collections.DictionaryEntry entry in notification.userInfo)
				{
					ControlPanel.Log(LOG_CHANNEL + " \t\t entry: " + entry.Key + " value:" + entry.Value);    
				}

				// if notification is silent save a flag for the game to know
				if ( IsNotificationSilent( notification ) )
				{
					ControlPanel.Log( LOG_CHANNEL + "Is Silent Notification");  
					PlayerPrefs.SetInt(SILENT_FLAG, 1);
				}
			}
			ControlPanel.Log(LOG_CHANNEL + "================= ");
			UnityEngine.iOS.NotificationServices.ClearRemoteNotifications();
		}
	}	

	public bool IsNotificationSilent( UnityEngine.iOS.RemoteNotification _notification )
	{
		bool ret = false;
		if(string.IsNullOrEmpty(_notification.alertBody) && string.IsNullOrEmpty(_notification.soundName)) {
			ret = true;
		}
		return ret;
	}
    #endif

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
