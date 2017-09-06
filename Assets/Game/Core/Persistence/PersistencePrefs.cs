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
    public static string KEY_ACTIVE_PROFILE_NAME = "activeProfileName";

    public static List<string> KEYS = new List<string>()
    {
        KEY_ACTIVE_PROFILE_NAME
    };

    public enum EKeys
    {
        activeProfileName,
    }    

    public static void Clear()
    {
        int count = KEYS.Count;
        for (int i= 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(KEYS[i]);
        }
    }

    public static string ActiveProfileName
    {        
        get { return PlayerPrefs.GetString(KEY_ACTIVE_PROFILE_NAME, PersistenceProfile.DEFAULT_PROFILE); }
        set { PlayerPrefs.SetString(KEY_ACTIVE_PROFILE_NAME, value); }
    }
}