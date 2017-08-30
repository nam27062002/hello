﻿public class TrackingPersistenceSystem : PersistenceSystem
{
    private const string PARAM_ACCOUNT_ID = "accID";
    private const string PARAM_ADS_COUNT = "adsCount";
    private const string PARAM_ADS_SESSIONS = "adsSessions";
    private const string PARAM_GAME_ROUND_COUNT = "gameRoundCount";
    private const string PARAM_SESSION_COUNT = "sessionCount";
    private const string PARAM_SOCIAL_ID = "socialID";
    private const string PARAM_SOCIAL_PLATFORM = "socialPlatform";
    private const string PARAM_TOTAL_PLAY_TIME = "totalPlaytime";
    private const string PARAM_TOTAL_PURCHASES = "totalPurchases";
    private const string PARAM_TOTAL_STORE_VISITS = "totalStoreVisits";
    private const string PARAM_USER_ID = "userID";

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

    // Counter of game runs since installation
    public int GameRoundCount
    {
        get
        {
            return Cache_GetInt(PARAM_GAME_ROUND_COUNT);
        }

        set
        {
            Cache_SetInt(PARAM_GAME_ROUND_COUNT, value);
        }
    }

    public int TotalPlaytime
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_PLAY_TIME);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_PLAY_TIME, value);
        }
    }

    public int TotalPurchases
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_PURCHASES);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_PURCHASES, value);
        }
    }

    public int TotalStoreVisits
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_STORE_VISITS);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_STORE_VISITS, value);
        }
    }

    public int AdsCount
    {
        get
        {
            return Cache_GetInt(PARAM_ADS_COUNT);
        }

        set
        {
            Cache_SetInt(PARAM_ADS_COUNT, value);
        }
    }

    public int AdsSessions
    {
        get
        {
            return Cache_GetInt(PARAM_ADS_SESSIONS);
        }

        set
        {
            Cache_SetInt(PARAM_ADS_SESSIONS, value);
        }
    }

    public TrackingPersistenceSystem()
    {
        m_systemName = "Tracking";

        CacheDataInt dataInt;
        CacheDataString dataString = new CacheDataString(PARAM_USER_ID, "");
        Cache_AddData(PARAM_USER_ID, dataString);

        string key = PARAM_SOCIAL_PLATFORM;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

        key = PARAM_SOCIAL_ID;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

        key = PARAM_ACCOUNT_ID;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_SESSION_COUNT;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_GAME_ROUND_COUNT;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_PLAY_TIME;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_PURCHASES;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_STORE_VISITS;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_ADS_COUNT;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_STORE_VISITS;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_ADS_COUNT;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_ADS_SESSIONS;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

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