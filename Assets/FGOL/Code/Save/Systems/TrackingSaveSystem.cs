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

    // Platform used to log in. It can be either "facebook" or "weibo"
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

    // Id in the social platform where the user last logged in. It must be "" if the user has never logged in
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

    // Id in our server. It must be 0 if the user has never logged in
    public int AccountID
    {
        get
        {
            return Cache_GetInt(PARAM_ACCOUNT_ID);
        }

        set
        {            
            Cache_SetInt(PARAM_ACCOUNT_ID, value);
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

        CacheDataInt dataInt;
        CacheDataString dataString = new CacheDataString(PARAM_USER_ID, "");
        Cache_AddData(PARAM_USER_ID, dataString);

        dataString = new CacheDataString(PARAM_SOCIAL_PLATFORM, "");
        Cache_AddData(PARAM_SOCIAL_PLATFORM, dataString);

        dataString = new CacheDataString(PARAM_SOCIAL_ID, "");
        Cache_AddData(PARAM_SOCIAL_ID, dataString);

        dataInt = new CacheDataInt(PARAM_ACCOUNT_ID, 0);
        Cache_AddData(PARAM_ACCOUNT_ID, dataInt);

        dataInt = new CacheDataInt(PARAM_SESSION_COUNT, 0);
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

        // Try to convert it to an int value since the parameter of the DNA event requires an int value
        if (string.IsNullOrEmpty(accId))
        {
            AccountID = 0;
        }
        else
        {
            AccountID = int.Parse(accId);
        }
    }
}