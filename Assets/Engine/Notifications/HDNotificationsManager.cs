﻿using UnityEngine;
using System;

public class HDNotificationsManager : Singleton<HDNotificationsManager>
{
    private const string HD_NOTIFICATIONS = "HD_NOTIFICATIONS";
    public const string SILENT_FLAG = "Notifications.Silent";
    
    public static bool CanBeUsed()  // This function is here to protect old android not to initialize because of the trilladora
    {
        bool ret = true;
        /*
#if !UNITY_EDITOR && UNITY_ANDROID
            ret = PlatformUtilsAndroidImpl.GetSDKLevel() >= 21;
#endif
        */
        return ret;
    } 

    public void Initialise()
    {
        if (CanBeUsed())
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
                    
                Log("Notifications enabled = " + GetNotificationsEnabled());
            }
        }
    }
#if UNITY_IOS
    public void Update()
    {
		CheckRemoteNotifications();
    }
#endif

#if UNITY_IOS
	private void CheckRemoteNotifications() {
		if ( UnityEngine.iOS.NotificationServices.remoteNotificationCount > 0)
		{
			// CheckAppOpenedBy() needs to be called because it takes into account NotificationServices.remoteNotificationCount,
            // which will be cleared at the end of this method
            DeviceUtilsManager.SharedInstance.CheckAppOpenedBy();
            
            bool debugEnabled = FeatureSettingsManager.IsDebugEnabled;
            if (debugEnabled)
            {
			     ControlPanel.Log(LOG_CHANNEL + "================= ");
			     ControlPanel.Log(LOG_CHANNEL + " Number Of remote notificaions: " + UnityEngine.iOS.NotificationServices.remoteNotificationCount);
			}

            int max = UnityEngine.iOS.NotificationServices.remoteNotificationCount;
			for (int i = 0; i < max; i++)
			{
				UnityEngine.iOS.RemoteNotification notification = UnityEngine.iOS.NotificationServices.remoteNotifications[i];
				
                if (debugEnabled)
                {
                    ControlPanel.Log(LOG_CHANNEL + " Body:" + notification.alertBody);
    				ControlPanel.Log(LOG_CHANNEL + " Sound:" + notification.soundName);
    				ControlPanel.Log(LOG_CHANNEL + " BadgeNumber:" + notification.applicationIconBadgeNumber);
    				ControlPanel.Log(LOG_CHANNEL + " Has Action:" + notification.hasAction);
    				ControlPanel.Log(LOG_CHANNEL + " UserInfo Size:" + notification.userInfo.Count);
    				foreach (System.Collections.DictionaryEntry entry in notification.userInfo)
    				{
    					ControlPanel.Log(LOG_CHANNEL + " \t\t entry: " + entry.Key + " value:" + entry.Value);    
    				}
                }

				// if notification is silent save a flag for the game to know
				if ( IsNotificationSilent( notification ) )
				{
                    if (debugEnabled)
                    {
					   ControlPanel.Log( LOG_CHANNEL + "Is Silent Notification");  
                    }

					PlayerPrefs.SetInt(SILENT_FLAG, 1);
				}
			}

            if (debugEnabled)
            {
			     ControlPanel.Log(LOG_CHANNEL + "================= ");
            }
            
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
        if ( CanBeUsed() )
        {
            Log("SetNotificationsEnabled = " + enabled);
    		int v  = enabled ? 1 : 0;
    		PlayerPrefs.SetInt(HD_NOTIFICATIONS,v);
    
            NotificationsManager.SharedInstance.SetNotificationsEnabled(enabled);
    
    		// Clear all notifications
            NotificationsManager.SharedInstance.CancelAllNotifications();
            
        }
    }   

    private void ScheduleNotificationFromSku(string strSKU, string strAction, int iTimeLeft)
    {
        if (CanBeUsed())
        {
            DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.NOTIFICATIONS, strSKU);
            string body = "";
            if (def != null)
            {
                body = LocalizationManager.SharedInstance.Localize(def.Get("tidName"));
				body = EmojiManager.ReplaceEmojis(body);		// Replace emoji tags! 
            }

            ScheduleNotification(strSKU, body, "Action", iTimeLeft);
        }
    }


    private static int MAX_TIME_LEFT = 60 * 60 * 24 * 2;    //just 2 days
    private void ScheduleNotification(string strSKU, string strBody, string strAction, int iTimeLeft)
    {
        if (CanBeUsed())
        {
            if (iTimeLeft > MAX_TIME_LEFT)
            {
                Debug.LogError("Error: ScheduleNotification time left = " + iTimeLeft + " secs. 2 days maximum allowed. ");
                iTimeLeft = MAX_TIME_LEFT;
            }
            Log("ScheduleNotification enabled = " + GetNotificationsEnabled() + " strSKU = " + strSKU + " strBody = " + strBody + " strAction = " + strAction + " iTimeLeft = " + iTimeLeft);
            NotificationsManager.SharedInstance.ScheduleNotification(strSKU, strBody, strAction, iTimeLeft);
        }
    }

    private void CancelNotification(string strSKU)
    {
        if (CanBeUsed())
        {
            Log("CancelNotification enabled = " + GetNotificationsEnabled() + " strSKU = " + strSKU);
            NotificationsManager.SharedInstance.CancelNotification(strSKU);
        }
    }

	#region game
	// Add here the game related code
	public const int SILENCE_START_HOUR = 23;
	public const int SILENCE_END_HOUR = 8;

	private const string SKU_EGG_HATCHED = "sku.not.01";
    private const string SKU_NEW_MISSIONS = "sku.not.02";
    private const string SKU_NEW_CHESTS = "sku.not.03";
	private const string SKU_DAILY_REWARD = "sku.not.04";
    private const string SKU_REENGAGEMENT = "sku.not.14";
	private const string SKU_FREE_OFFER = "sku.not.15";

    private const string DEFAULT_ACTION = "Action";

	// Eggs
    public void ScheduleEggHatchedNotification(int seconds)
    {
        ScheduleNotificationFromSku(SKU_EGG_HATCHED, DEFAULT_ACTION, seconds);
    }

    public void CancelEggHatchedNotification()
    {
        CancelNotification(SKU_EGG_HATCHED);
    }

	// Missions
    public void ScheduleNewMissionsNotification(int seconds)
    {
        ScheduleNotificationFromSku(SKU_NEW_MISSIONS, DEFAULT_ACTION, seconds);
    }

    public void CancelNewMissionsNotification()
    {
        CancelNotification(SKU_NEW_MISSIONS);
    }

	// Chests
    public void ScheduleNewChestsNotification(int seconds)
    {
        ScheduleNotificationFromSku(SKU_NEW_CHESTS, DEFAULT_ACTION, seconds);
    }

	public void CancelNewChestsNotification() {
		CancelNotification(SKU_NEW_CHESTS);
	}

	// Daily Reward
	public void ScheduleNewDailyReward(int seconds)
    {
        ScheduleNotificationFromSku(SKU_DAILY_REWARD, DEFAULT_ACTION, seconds);
    }

	public void CancelDailyRewardNotification() {
		CancelNotification(SKU_DAILY_REWARD);
	}

	// Free Offer
	public void ScheduleNewFreeOffer(int seconds) {
		ScheduleNotificationFromSku(SKU_FREE_OFFER, DEFAULT_ACTION, seconds);
	}

	public void CancelFreeOfferNotification() {
		CancelNotification(SKU_FREE_OFFER);
	}

	// Reengagement
    public void ScheduleReengagementNotification(int seconds) {
		ScheduleNotificationFromSku(SKU_REENGAGEMENT, DEFAULT_ACTION, seconds);
	}

    public void CancelReengagementNotification(  ){
        CancelNotification(SKU_REENGAGEMENT);
    }
	#endregion

	#region utils
	/// <summary>
	/// Adjust given date time to avoid silent hours.
	/// The date time will be moved forward to the first non-silent hour possible.
	/// </summary>
	/// <param name="_dateTime">Date time to be adjusted.</param>
	/// <returns>Adjusted date time.</returns>
	public static DateTime AvoidSilentHours(DateTime _dateTime) {
		// Add hour by hour until condition is met
		bool adjusted = false;
		while(_dateTime.Hour >= HDNotificationsManager.SILENCE_START_HOUR || _dateTime.Hour < HDNotificationsManager.SILENCE_END_HOUR) {
			_dateTime = _dateTime.AddHours(1);
			adjusted = true;
		}

		// Round to hour
		if(adjusted) {
			_dateTime = new DateTime(_dateTime.Year, _dateTime.Month, _dateTime.Day, _dateTime.Hour, 0, 0, 0, _dateTime.Kind);
		}

		return _dateTime;
	}
	#endregion

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
