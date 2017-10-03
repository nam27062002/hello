using System;
using System.Collections;
using System.Collections.Generic;
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

                Disk_SaveProfilePicture(mProfilePicture);                
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
         
        public void LoadInfo(Action onDone)
        {
            if (Disk_IsPictureLoaded)
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
            string fileName = Disk_GetProfileImagePath(true);

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("Profile image loaded from " + fileName);

            WWW cachedImageLoader = new WWW(fileName);

            yield return cachedImageLoader;

            Disk_IsPictureLoaded = true;

            Texture2D cachedImage = null;
            if (cachedImageLoader.error == null)
            {
                cachedImage = new Texture2D(256, 256);
                cachedImageLoader.LoadImageIntoTexture(cachedImage);
            }
            else
            {
                SocialUtils.LogWarning("SocialUtils (Store_LoadProfileImage) :: LoadCachedImage failed: " + cachedImageLoader.error);
            }

            ProfilePicture = cachedImage;
            if (onDone != null)
            {
                onDone();
            }
        }

        private void Disk_DeleteProfilePicture()
        {
            string path = Disk_GetProfileImagePath();
            if (File.Exists(path))
            {
                File.Delete(path);

                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("Profile image deleted from " + path);
            }
        }

        private void Disk_SaveProfilePicture(Texture2D image)
        {
            if (image == null)
            {
                Disk_DeleteProfilePicture();
            }
            else
            {
                string fileName = Disk_GetProfileImagePath();                
                File.WriteAllBytes(Disk_GetProfileImagePath(), image.EncodeToPNG());

                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("Profile image saved to " + fileName);
            }
        }

        private string Disk_GetProfileImagePath(bool wwwPath = false)
        {
            //return string.Format("{0}{1}/ProfileImg.bytes", (wwwPath ? "file://" : ""), Application.persistentDataPath);
            string prefix = (wwwPath) ? "file:///" : "";
            return System.IO.Path.Combine(prefix + Application.persistentDataPath, "ProfileImg.bytes");
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

    public abstract string GetPlatformNameTID();    

    public abstract void Init(SocialPlatformManager.GameSocialListener listener);

    public abstract string GetSocialID();

    public abstract string GetAccessToken();

    public abstract string GetUserName();

    public abstract bool IsLoggedIn();            

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

        public string Id                    { get { return GetValueAsString(KEY_ID); } }
        public string FirstName             { get { return GetValueAsString(KEY_FIRST_NAME); } }
        public string LastName              { get { return GetValueAsString(KEY_LAST_NAME); } }
        public string Gender                { get { return GetValueAsString(KEY_GENDER); } }
        public string Email                 { get { return GetValueAsString(KEY_EMAIL); } }
        public int YearOfBirth              { get; set; }
    }

    /// <summary>
    /// Request the user's profile information to the social platform. Cached information is not retrieved
    /// </summary>
    /// <param name="onGetProfileInfo"></param>
    public abstract void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo);
    
    protected void GetProfilePicture(string socialID, Action<Texture2D, bool> onGetProfilePicture, int width = 256, int height = 256)
    {
        ExtendedGetProfilePicture(socialID, onGetProfilePicture, width, height);        
    }    

    protected abstract void ExtendedGetProfilePicture(string socialID, Action<Texture2D, bool> onGetProfilePicture, int width = 256, int height = 256);    

    public void OnLoggedIn()
    {
        Profile_LoadInfo();
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
        if (Profile_OnGetInfoDone != null && FeatureSettingsManager.IsDebugEnabled)
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

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("Profile_LoadInfo socialId = " + socialId + " Cache.SocialId = " + Cache.SocialId);

            if (socialId != Cache.SocialId)
            {
                Cache.Invalidate();
                Cache.SocialId = socialId;
            }

            if (Cache.HasBeenUpdatedFromPlatform)
            {
                Cache.LoadInfo(Profile_PerformGetInfoDone);
            }
            else
            {
                Profile_UpdateInfoFromPlatform(Profile_PerformGetInfoDone);
            }
        }
        else
        {
            Cache.LoadInfo(Profile_PerformGetInfoDone);
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
        bool profileIsNameReady = false;
        bool profileIsPictureReady = false;

        Action onReady = delegate ()
        {
            if (profileIsNameReady && profileIsPictureReady && onDone != null)
            {
                Cache.HasBeenUpdatedFromPlatform = true;
                onDone();
            }
        };

        GetProfileInfoFromPlatform(delegate (ProfileInfo profileInfo)
        {           
            Cache.ProfileName = (profileInfo != null) ? profileInfo.FirstName : null;
            profileIsNameReady = true;
            onReady();
        });

        GetProfilePicture(GetSocialID(), delegate (Texture2D profileImage, bool error)
        {
            if (!error)
            {
                Cache.ProfilePicture = profileImage;
            }

            profileIsPictureReady = true;
            onReady();
        });
    }
    #endregion        

    #region log
    private static bool LOG_USE_COLOR = false;
    private static string LOG_CHANNEL = "[SocialUtils] ";
    private static string LOG_CHANNEL_COLOR = "<color=cyan>" + LOG_CHANNEL;

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

    public static void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_CHANNEL + msg);
    }

    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }
    #endregion
}