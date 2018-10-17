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

    // Social platform used: Facebook, Weibo
    private static string KEY_SOCIAL_PLATFORM_KEY = "socialPlatform";

    private static string KEY_SOCIAL_ID = "socialId";

    private static string KEY_SOCIAL_PROFILE_NAME = "socialProfileName";

    // Whether or not the user was logged in the social platform when she quit
    private static string KEY_SOCIAL_LOGGED_IN_WHEN_QUIT = "socialWasLoggedInWhenQuit";

    // Stored here so TrackingManager can be initialized as soon as possible
    private static string KEY_SERVER_USER_ID = "serverUserId";

    // Index to the latest save path stored.
    private static string KEY_SAVEPATHS_LATEST_INDEX = "savePathsLatestIndex";

    // User's profile name. The name instead of the level is stored in order to prevent the wrong profile from being applied if we happen to add/remove profile levels
    private static string KEY_USER_PROFILE_NAME = "userProfileLevel";

    // User's language set in server
    private static string KEY_SERVER_LANGUAGE = "serverLanguage";

    // Latest marketing id notified
    private static string KEY_LATEST_MARKETING_ID_NOTIFIED = "latestMarketingIdNotified";

    // Marketing id. It's stored in prefs when it's retrieved successfully since it may take time to get it from device
    private static string KEY_MARKETING_ID = "marketingId";

    // Timestamp of the latest time a cp2 interstial was played at
    private static string KEY_CP2_INTERTITIAL_LATEST_AT_ID = "cp2InterstitialLatestAt";

    private static List<string> KEYS = new List<string>()
    {
        KEY_ACTIVE_PROFILE_NAME,
        KEY_CLOUD_SAVE_ENABLED,
        KEY_SOCIAL_PLATFORM_KEY,
        KEY_SOCIAL_ID,
        KEY_SOCIAL_PROFILE_NAME,
        KEY_SOCIAL_LOGGED_IN_WHEN_QUIT,
        KEY_SERVER_USER_ID,
        KEY_SAVEPATHS_LATEST_INDEX,
        KEY_USER_PROFILE_NAME,
        KEY_SERVER_LANGUAGE,
        KEY_LATEST_MARKETING_ID_NOTIFIED,
        KEY_MARKETING_ID,
        KEY_CP2_INTERTITIAL_LATEST_AT_ID
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
        get
        {
#if UNITY_EDITOR
            return PlayerPrefs.GetString(KEY_ACTIVE_PROFILE_NAME, PersistenceProfile.DEFAULT_PROFILE);
#else
            // It always returns DEFAULT_PROFILE to make sure that the file with the progress will be found
            return PersistenceProfile.DEFAULT_PROFILE;
#endif
        }

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

    public static int ServerUserIdAsInt
    {
        get
        {
            int returnValue = 0;
            string userId = ServerUserId;
            if (!string.IsNullOrEmpty(userId))
            {
                if (!int.TryParse(userId, out returnValue))
                {
                    returnValue = 0;
                }
            }

            return returnValue;
        }        
    }

    public static int SavePathsLatestIndex
    {
        get { return PlayerPrefs.GetInt(KEY_SAVEPATHS_LATEST_INDEX, 0); }
        set { SetInt(KEY_SAVEPATHS_LATEST_INDEX, value); }
    }
    
    public static string GetUserProfileName()
    {
        return PlayerPrefs.GetString(KEY_USER_PROFILE_NAME, null);
    }

    public static void SetUserProfileName(string value)
    {
        SetString(KEY_USER_PROFILE_NAME, value);
    }

    public static string GetServerLanguage()
    {
        return PlayerPrefs.GetString(KEY_SERVER_LANGUAGE, null);        
    }

    public static void SetServerLanguage(string value)
    {
        SetString(KEY_SERVER_LANGUAGE, value);
    }

    public static string GetLatestMarketingIdNotified()
    {
        return PlayerPrefs.GetString(KEY_LATEST_MARKETING_ID_NOTIFIED, null);
    }

    public static void SetLatestMarketingIdNotified(string value)
    {
        SetString(KEY_LATEST_MARKETING_ID_NOTIFIED, value);
    }

    public static string GetMarketingId()
    {
        return PlayerPrefs.GetString(KEY_MARKETING_ID, null);
    }

    public static void SetMarketingId(string value)
    {
        SetString(KEY_MARKETING_ID, value);
    }

    public static void SetCp2InterstitialLatestAt(long value)
    {
        SetLong(KEY_CP2_INTERTITIAL_LATEST_AT_ID, value);
    }

    public static long GetCp2InterstitialLatestAt()
    {
        return GetLong(KEY_CP2_INTERTITIAL_LATEST_AT_ID);
    }

#region social
    public static string Social_PlatformKey
    {
        get { return PlayerPrefs.GetString(KEY_SOCIAL_PLATFORM_KEY, null); }
        set { SetString(KEY_SOCIAL_PLATFORM_KEY, value); }
    }

    public static string Social_Id
    {
        get { return PlayerPrefs.GetString(KEY_SOCIAL_ID, null); }
        set { SetString(KEY_SOCIAL_ID, value); }
    }

    public static string Social_ProfileName
    {
        get { return PlayerPrefs.GetString(KEY_SOCIAL_PROFILE_NAME, null); }
        set { SetString(KEY_SOCIAL_PROFILE_NAME, value); }
    }

    public static bool Social_WasLoggedInWhenQuit
    {
        get { return PlayerPrefs.GetInt(KEY_SOCIAL_LOGGED_IN_WHEN_QUIT, 1) == 1; }
        set { SetInt(KEY_SOCIAL_LOGGED_IN_WHEN_QUIT, (value ? 1 : 0)); }
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

    private static void SetLong(string key, long value)
    {
        PlayerPrefs.SetString(key, value + "");        
    }

    private static long GetLong(string key)
    {
        long returnValue = 0;
        string value = PlayerPrefs.GetString(key);        
        if (!string.IsNullOrEmpty(value))
        {
            long.TryParse(value, out returnValue);
        }

        return returnValue;
    }
}