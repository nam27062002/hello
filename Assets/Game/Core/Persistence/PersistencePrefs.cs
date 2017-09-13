/// <summary>
/// This class is responsible for listing all persistence data stored in prefs. There are two reason why you'd want to store a value as a pref instead of in a <c>PersistenceSystem</c>:
/// 1)Sensitive data that need to be protected from corruption
/// 2)Data that need to be ready as soon as possible, typically for tracking. Example: trackingID should be accessible immediately so we can start tracking system and send events
///   as soon as possible
/// </summary>
using System.Collections.Generic;
using UnityEngine;
public class PersistencePrefs
{
    // If you want to add a new key, please remember to add it to the list defined below so all persistece prefs can be deleted when clearing the persistence
    private static string KEY_ACTIVE_PROFILE_NAME = "activeProfileName";

    // We want the cloud save to stores per device instead of per profile
    private static string KEY_CLOUD_SAVE_ENABLED = "cloudSaveEnabled";

    private static string KEY_SOCIAL_PROFILE_NAME = "SocialProfileName";

    // Stored here so TrackingManager can be initialized as soon as possible
    private static string KEY_SERVER_USER_ID = "serverUserId";
    
    private static List<string> KEYS = new List<string>()
    {
        KEY_ACTIVE_PROFILE_NAME,
        KEY_CLOUD_SAVE_ENABLED,
        KEY_SOCIAL_PROFILE_NAME,
        KEY_SERVER_USER_ID,
    };        

    public static bool IsDirty = false;

    public static void Clear()
    {
        int count = KEYS.Count;
        for (int i= 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(KEYS[i]);
        }
    }

    public static void Update()
    {
        if (IsDirty)
        {
            PlayerPrefs.Save();
            IsDirty = false;
        }
    }

    public static string ActiveProfileName
    {
        get { return PlayerPrefs.GetString(KEY_ACTIVE_PROFILE_NAME, PersistenceProfile.DEFAULT_PROFILE); }

		set { SetString(KEY_ACTIVE_PROFILE_NAME, value); }
    }

    public static bool IsCloudSaveEnabled
    {
        get { return PlayerPrefs.GetInt(KEY_CLOUD_SAVE_ENABLED, 1) == 1; }
        set { SetInt(KEY_CLOUD_SAVE_ENABLED, (value ? 1: 0)); }
    }
    
    public static string ServerUserId
    {
        get { return PlayerPrefs.GetString(KEY_SERVER_USER_ID, null);  }
        set { SetString(KEY_SERVER_USER_ID, value); }
    }
        
    #region social
    public static string Social_ProfileName
    {
        get { return PlayerPrefs.GetString(KEY_SOCIAL_PROFILE_NAME, null); }
        set { SetString(KEY_SOCIAL_PROFILE_NAME, value); }
    }
    #endregion

    private static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        IsDirty = true;
    }

    private static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        IsDirty = true;
    }
}