using System;
using System.Collections.Generic;
using FGOL.Save;
using FGOL.Server;
public class TrackingSaveSystem : SaveSystem
{
    private const string PARAM_USER_ID = "userID";
    private const string PARAM_SOCIAL_PLATFORM = "socialPlatform";
    private const string PARAM_SOCIAL_ID = "socialID";
    private const string PARAM_ACCOUNT_ID = "accID";
    private const string PARAM_SESSION_COUNT = "sessionCount";          

    // Tracking user ID generated upon first time session is started, uses GUID as we don't have server at this point
    public string UserID
    {
        get
        {
            return Cache_GetString(PARAM_USER_ID);
        }

        set
        {
            Cache_SetString(PARAM_USER_ID, value);            
        }
    }

    // Platform used to log in. It can be either Facebook or Weibo
    public string SocialPlatform
    {
        get
        {
            return Cache_GetString(PARAM_SOCIAL_PLATFORM);
        }

        set
        {
            Cache_SetString(PARAM_SOCIAL_PLATFORM, value);            
        }
    }

    // Id in the social platform where the user last logged in. It's null if the user has never logged in
    public string SocialID
    {
        get
        {
            return Cache_GetString(PARAM_SOCIAL_ID);
        }

        set
        {            
            Cache_SetString(PARAM_SOCIAL_ID, value);
        }
    }

    // Id in our server. It's null if the user has never logged in
    public string AccountID
    {
        get
        {
            return Cache_GetString(PARAM_ACCOUNT_ID);
        }

        set
        {            
            Cache_SetString(PARAM_ACCOUNT_ID, value);
        }
    }

    // Counter of sessions since installation
    public int SessionCount
    {
        get
        {
            return Cache_GetInt(PARAM_SESSION_COUNT);
        }

        set
        {
            Cache_SetInt(PARAM_SESSION_COUNT, value);
        }       
    }    

    public TrackingSaveSystem()
    {
        m_systemName = "Tracking";

        CacheDataString dataString = new CacheDataString(PARAM_USER_ID, null);
        Cache_AddData(PARAM_USER_ID, dataString);

        dataString = new CacheDataString(PARAM_SOCIAL_PLATFORM, null);
        Cache_AddData(PARAM_SOCIAL_PLATFORM, dataString);

        dataString = new CacheDataString(PARAM_SOCIAL_ID, null);
        Cache_AddData(PARAM_SOCIAL_ID, dataString);

        dataString = new CacheDataString(PARAM_ACCOUNT_ID, null);
        Cache_AddData(PARAM_ACCOUNT_ID, dataString);

        CacheDataInt dataInt = new CacheDataInt(PARAM_SESSION_COUNT, 0);
        Cache_AddData(PARAM_SESSION_COUNT, dataInt);

        Reset();
    }

    public override void Reset()
    {
        Cache_Reset();                                
    }

    public override void Load()
    {
        Cache_Load();
    }

    public override void Save()
    {
        Cache_Save();
    }

    public override bool Upgrade()
    {
        return false;
    }

    public override void Downgrade()
    {
    }

    public void SetSocialParams(string socialPlatform, string socialID, string accId)
    {
        SocialPlatform = socialPlatform;
        SocialID = socialID;
        AccountID = accId;
    }
}