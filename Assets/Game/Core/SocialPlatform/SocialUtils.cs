#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
public abstract class SocialUtils
{
    public class SocialCache
    {
        public string SocialId
        {
            get { return PersistenceFacade.instance.LocalDriver.Prefs_SocialId; }
            set { PersistenceFacade.instance.LocalDriver.Prefs_SocialId = value; }
        }

        public string ProfileName
        {
            get {
                return PersistencePrefs.Social_ProfileName; }
            set
            {
                PersistencePrefs.Social_ProfileName = value; }
        }

        private Texture2D mProfilePicture;
        public Texture2D ProfilePicture
        {
            get
            {
                return mProfilePicture;
            }

            set
            {
                if (mProfilePicture != null)
                {
                    Texture2D.Destroy(mProfilePicture);
                }

                mProfilePicture = value;

                //Disk_SaveProfilePicture(mProfilePicture);                
            }
        }

        public bool HasBeenUpdatedFromPlatform { get; set; }

        public void Invalidate()
        {
            ProfileName = null;
            SocialId = null;

            ProfilePicture = null;
            Disk_DeleteProfilePicture();

            HasBeenUpdatedFromPlatform = false;
        }

        public void LoadInfo(bool force, Action onDone)
        {
            if (Disk_IsPictureLoaded && !force)
            {
                if (onDone != null)
                {
                    onDone();
                }
            }
            else
            {
                Util.StartCoroutineWithoutMonobehaviour("Disk_LoadProfileImage", Disk_LoadProfileImage(onDone));
            }
        }

        #region disk
        private bool Disk_IsPictureLoaded { get; set; }

        private IEnumerator Disk_LoadProfileImage(Action onDone)
        {            
            Texture2D cachedImage = null;
            FileStream kFile = FileUtils.OpenBinaryFileInDeviceStorage(PROFILE_IMAGE_STORAGE_PATH, FileUtils.FileMode.E_FILE_READ, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
            if (kFile != null)
            {
                byte[] kBytes = new byte[kFile.Length];

                kFile.Read(kBytes, 0, (int)kFile.Length);

                cachedImage = new Texture2D(256, 256);
                cachedImage.LoadImage(kBytes);

                kFile.Close();
            }
            else
            {
                SocialUtils.LogWarning("SocialUtils (Store_LoadProfileImage) :: LoadCachedImage failed");
            }

            Disk_IsPictureLoaded = true;           

            ProfilePicture = cachedImage;
            if (onDone != null)
            {
                onDone();
            }

            yield return null;
        }

        private void Disk_DeleteProfilePicture()
        {
            string path = PROFILE_IMAGE_STORAGE_PATH;
            if (File.Exists(path))
            {
                FileUtils.RemoveFileInDeviceStorage(path, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);                                
                Log("Profile image deleted from " + path);
            }
        }              
        #endregion
    }

    private SocialCache mCache;
    public SocialCache Cache
    {
        get
        {
            if (mCache == null)
            {
                mCache = new SocialCache();
            }

            return mCache;
        }
    }

    private bool mIsEnabled;
    public bool GetIsEnabled()
    {
        return mIsEnabled;
    }

    protected void SetIsEnabled(bool value)
    {
        mIsEnabled = value;
    }

    public abstract string GetPlatformNameTID();

    public abstract void Init(SocialPlatformManager manager);

    public abstract string GetSocialID();

    public abstract string GetAccessToken();

    public abstract string GetUserName();

    public abstract bool IsLoggedIn();

	public abstract bool IsLogInTimeoutEnabled();

	public virtual void OnLogInTimeout() {}

    public static string PROFILE_IMAGE_STORAGE_PATH = "ProfileImg.bytes";

    public class ProfileInfo
    {
        public const string KEY_ID = "id";
        public const string KEY_FIRST_NAME = "first_name";
        public const string KEY_LAST_NAME = "last_name";
        public const string KEY_GENDER = "gender";
        public const string KEY_EMAIL = "email";

        public ProfileInfo()
        {
            Reset();
        }

        private Dictionary<string, string> ValuesAsString { get; set; }

        public void SetValueAsString(string key, string value)
        {
            if (ValuesAsString == null)
            {
                ValuesAsString = new Dictionary<string, string>();
            }

            if (ValuesAsString.ContainsKey(key))
            {
                ValuesAsString[key] = value;
            }
            else
            {
                ValuesAsString.Add(key, value);
            }
        }

        public string GetValueAsString(string key)
        {
            return (ValuesAsString != null && ValuesAsString.ContainsKey(key)) ? ValuesAsString[key] : null;
        }

        public void Reset()
        {
            if (ValuesAsString != null)
            {
                ValuesAsString.Clear();
            }

            YearOfBirth = 0;
        }

        public string Id { get { return GetValueAsString(KEY_ID); } }
        public string FirstName { get { return GetValueAsString(KEY_FIRST_NAME); } }
        public string LastName { get { return GetValueAsString(KEY_LAST_NAME); } }
        public string Gender { get { return GetValueAsString(KEY_GENDER); } }
        public string Email { get { return GetValueAsString(KEY_EMAIL); } }
        public int YearOfBirth { get; set; }
    }

    /// <summary>
    /// Request the user's profile information to the social platform. Cached information is not retrieved
    /// </summary>
    /// <param name="onGetProfileInfo"></param>
    public abstract void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo);

    protected void GetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256)
    {
        ExtendedGetProfilePicture(socialID, storagePath, onGetProfilePicture, width, height);
    }

    protected abstract void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256);

    public enum EPlatform
    {
        None,
        Facebook,
        Weibo
    };

    private static string[] sm_platformKeys;

    public static string EPlatformToKey(EPlatform value)
    {
        return value.ToString();
    }

    public static EPlatform KeyToEPlatform(string value)
    {
        if (sm_platformKeys == null)
        {
            sm_platformKeys = Enum.GetNames(typeof(EPlatform));            
        }

        int count = sm_platformKeys.Length;
        for (int i = 0; i < count; i++)
        {
            if (sm_platformKeys[i] == value)
            {
                return (EPlatform)i;
            }
        }

        return EPlatform.None;
    }

    private EPlatform m_platform;
    public EPlatform GetPlatform()
    {
        return m_platform;
    }

    private void SetPlatform(EPlatform value)
    {
        m_platform = value;
    }

    public string GetPlatformKey()
    {
        return EPlatformToKey(GetPlatform());
    }

    public SocialUtils(EPlatform platform)
    {
        SetIsEnabled(true);
        SetPlatform(platform);
    }

    public virtual void Login(bool isAppInit)
    {
        GameSessionManager.SharedInstance.LogInToSocialPlatform(isAppInit);
    }

    public void OnLoggedIn()
    {
        Profile_LoadInfo();
    }

    public void Update()
    {
        Profile_Update();
    }

    #region profile
    private enum EProfileState
    {
        NotLoaded,
        Loading,
        Loaded
    }

    private EProfileState Profile_State { get; set; }

    private Action<string, Texture2D> Profile_OnGetInfoDone { get; set; }
    
    private bool m_profileUpdateInfoFromPlatformReady;
    private bool m_profileNeedsToReloadInfo;
    private string m_profileNameFromPlatform;
    private Action m_profileUpdateInfoFromPlatformOnDone;

    private void Profile_ResetUpdateInfoFromPlatform()
    {
        m_profileUpdateInfoFromPlatformReady = false;
        m_profileNeedsToReloadInfo = false;
        m_profileUpdateInfoFromPlatformOnDone = null;
        m_profileNameFromPlatform = null;
    }


    public bool Profile_NeedsInfoToBeUpdated()
    {
        bool returnValue = false;
        if (IsLoggedIn())
        {
            returnValue = Profile_NeedsSocialIdToBeUpdated() || !Cache.HasBeenUpdatedFromPlatform;
        }

        return returnValue;
    }

    public bool Profile_NeedsSocialIdToBeUpdated()
    {
        bool returnValue = false;
        if (IsLoggedIn())
        {
            returnValue = GetSocialID() != Cache.SocialId;
        }

        return returnValue;
    }

    /// <summary>
    /// Returns user's first name and picture. If these data are cached then they are used instead of requesting them to the social platform.
    /// </summary>
    /// <param name="onDone"></param>
    public void Profile_GetSimpleInfo(Action<string, Texture2D> onDone)
    {
        // Only one request should be processed
        if (Profile_OnGetInfoDone != null)
            LogWarning("Only one Profile_GetInfo can be requested simmultaneously");

        Profile_OnGetInfoDone = onDone;

        // If it needs to be updated then the profile is forced to be loaded
        if (Profile_NeedsInfoToBeUpdated())
        {
            Profile_State = EProfileState.NotLoaded;
        }

        switch (Profile_State)
        {
            case EProfileState.NotLoaded:
                Profile_LoadInfo();
                break;

            case EProfileState.Loaded:
                Profile_PerformGetInfoDone();
                break;
        }
    }

    private void Profile_LoadInfo()
    {
        Profile_State = EProfileState.Loading;

        if (IsLoggedIn())
        {
            string socialId = GetSocialID();
            
            Log("Profile_LoadInfo socialId = " + socialId + " Cache.SocialId = " + Cache.SocialId);

            if (socialId != Cache.SocialId)
            {
                Cache.Invalidate();
                Cache.SocialId = socialId;
            }

            if (Cache.HasBeenUpdatedFromPlatform)
            {
                Cache.LoadInfo(false, Profile_PerformGetInfoDone);
            }
            else
            {
                Profile_UpdateInfoFromPlatform(Profile_PerformGetInfoDone);
            }
        }
        else
        {
            Cache.LoadInfo(false, Profile_PerformGetInfoDone);
        }
    }

    private void Profile_PerformGetInfoDone()
    {
        if (Profile_OnGetInfoDone != null)
        {
            Profile_OnGetInfoDone(Cache.ProfileName, Cache.ProfilePicture);
            Profile_OnGetInfoDone = null;
        }

        Profile_State = EProfileState.Loaded;
    }

    private void Profile_UpdateInfoFromPlatform(Action onDone)
    {
        Profile_ResetUpdateInfoFromPlatform();
        m_profileUpdateInfoFromPlatformOnDone = onDone;
                    
        bool profileIsNameReady = false;
        bool profileIsPictureReady = false;

        Action onReady = delegate ()
        {
            if (profileIsNameReady && profileIsPictureReady && onDone != null)
            {
                m_profileUpdateInfoFromPlatformReady = true;
            }
        };

        GetProfileInfoFromPlatform(delegate (ProfileInfo profileInfo)
        {
            m_profileNameFromPlatform = (profileInfo != null) ? profileInfo.FirstName : null;            
            profileIsNameReady = true;
            onReady();
        });

        GetProfilePicture(GetSocialID(), PROFILE_IMAGE_STORAGE_PATH, delegate(bool error)
        {
            Action onPictureDone = delegate ()
            {
                profileIsPictureReady = true;
                onReady();
            };

            if (!error)
            {
                m_profileNeedsToReloadInfo = true;
                onPictureDone();               
            }
            else
            {
                onPictureDone();
            }
        });
    }

    private void Profile_Update()
    {
        if (m_profileUpdateInfoFromPlatformReady)
        {            
            Log("Profile_Update name = " + m_profileNameFromPlatform + " m_profileNeedsToReloadInfo = " + m_profileNeedsToReloadInfo);

            m_profileUpdateInfoFromPlatformReady = false;

            Action onDone = delegate ()
            {
                Cache.HasBeenUpdatedFromPlatform = true;
                if (m_profileUpdateInfoFromPlatformOnDone != null)
                {
                    m_profileUpdateInfoFromPlatformOnDone();
                }
            };

            Cache.ProfileName = m_profileNameFromPlatform;

            if (m_profileNeedsToReloadInfo)
            {
                Cache.LoadInfo(true, onDone);
            }
            else
            {
                onDone();
            }
        }
    }    
    #endregion        

    #region log
    private static bool LOG_USE_COLOR = false;
    private static string LOG_CHANNEL = "[SocialUtils] ";
    private static string LOG_CHANNEL_COLOR = "<color=cyan>" + LOG_CHANNEL;

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void Log(string msg)
    {
        if (LOG_USE_COLOR)
        {
            msg = LOG_CHANNEL_COLOR + msg + " </color>";
        }
        else
        {
            msg = LOG_CHANNEL + msg;
        }

        Debug.Log(msg);        
    }

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_CHANNEL + msg);
    }

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }
    #endregion
}