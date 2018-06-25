public class TrackingPersistenceSystem : PersistenceSystem
{
    private const string PARAM_SERVER_USER_ID = "accID";
    private const string PARAM_ADS_COUNT = "adsCount";
    private const string PARAM_ADS_SESSIONS = "adsSessions";
    private const string PARAM_GAME_ROUND_COUNT = "gameRoundCount";
    private const string PARAM_SESSION_COUNT = "sessionCount";
    private const string PARAM_SOCIAL_ID = "socialID";
    private const string PARAM_SOCIAL_PLATFORM = "socialPlatform";
    private const string PARAM_TOTAL_PLAY_TIME = "totalPlaytime";
    private const string PARAM_TOTAL_PURCHASES = "totalPurchases";
	private const string PARAM_TOTAL_EGG_PURCHASES = "totalEggPurchases";
    private const string PARAM_TOTAL_EGGS_PURCHASED_WITH_HC = "totalEggsPurchasedWithHC";
    private const string PARAM_TOTAL_EGGS_FOUND = "totalEggsFound";
    private const string PARAM_TOTAL_EGGS_OPENED = "totalEggsOpened";
    private const string PARAM_TOTAL_STORE_VISITS = "totalStoreVisits";
    private const string PARAM_USER_ID = "userID";
    
    /// Whether or not this is the first time the game is loaded ever    
    private const string PARAM_FIRST_LOADING = "firstLoading";
    private const string PARAM_SOCIAL_AUTH_SENT = "socialAuthSent";

    // Amount of times the user has closed the legal popup so far
    private const string PARAM_TOTAL_LEGAL_VISITS = "totalLegalVisits";

    // Events sent that should be sent only once.
    private const string PARAM_EVENTS_ALREADY_SENT = "eventsAlreadySent";

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
    public string ServerUserID
    {
        get
        {
            return Cache_GetString(PARAM_SERVER_USER_ID);
        }

        set
        {
            Cache_SetString(PARAM_SERVER_USER_ID, value);
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

	public int EggPurchases
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_EGG_PURCHASES);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_EGG_PURCHASES, value);
        }
    }

    public int EggSPurchasedWithHC
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_EGGS_PURCHASED_WITH_HC);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_EGGS_PURCHASED_WITH_HC, value);
        }
    }

    public int EggsFound
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_EGGS_FOUND);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_EGGS_FOUND, value);
        }
    }

    public int EggsOpened
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_EGGS_OPENED);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_EGGS_OPENED, value);
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

    public bool IsFirstLoading
    {
        get
        {
            return Cache_GetBool(PARAM_FIRST_LOADING);
        }

        set
        {
            Cache_SetBool(PARAM_FIRST_LOADING, value);
        }
    }

    public bool SocialAuthSent
    {
        get
        {
            return Cache_GetBool(PARAM_SOCIAL_AUTH_SENT);
        }

        set
        {
            Cache_SetBool(PARAM_SOCIAL_AUTH_SENT, value);
        }
    }

    public int TotalLegalVisits
    {
        get
        {
            return Cache_GetInt(PARAM_TOTAL_LEGAL_VISITS);
        }

        set
        {
            Cache_SetInt(PARAM_TOTAL_LEGAL_VISITS, value);
        }
    }

    public bool HasEventAlreadyBeenSent(string e)
    {
        string value = Cache_GetString(PARAM_EVENTS_ALREADY_SENT);
        return (string.IsNullOrEmpty(value)) ? false : value.Contains(e);
    }

    public void NotifyEventSent(string e)
    {
        // Makes sure that it's not already been added
        if (!HasEventAlreadyBeenSent(e))
        {
            string key = PARAM_EVENTS_ALREADY_SENT;
            string value = Cache_GetString(key);
            if (string.IsNullOrEmpty(value))
            {
                value = e;
            }
            else
            {
                value += "," + e;
            }

            Cache_SetString(key, value);
        }
    }

    public TrackingPersistenceSystem()
    {
        m_systemName = "Tracking";

        CacheDataInt dataInt;
        CacheDataBool dataBool;
        CacheDataString dataString = new CacheDataString(PARAM_USER_ID, "");
        Cache_AddData(PARAM_USER_ID, dataString);

        string key = PARAM_SOCIAL_PLATFORM;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

        key = PARAM_SOCIAL_ID;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

        key = PARAM_SERVER_USER_ID;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

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
        
        key = PARAM_ADS_COUNT;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_ADS_SESSIONS;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_FIRST_LOADING;
        dataBool = new CacheDataBool(key, true);
        Cache_AddData(key, dataBool);

        key = PARAM_SOCIAL_AUTH_SENT;
        dataBool = new CacheDataBool(key, false);
        Cache_AddData(key, dataBool);

        key = PARAM_TOTAL_LEGAL_VISITS;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_EGG_PURCHASES;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_EGGS_PURCHASED_WITH_HC;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_EGGS_FOUND;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_TOTAL_EGGS_OPENED;
        dataInt = new CacheDataInt(key, 0);
        Cache_AddData(key, dataInt);

        key = PARAM_EVENTS_ALREADY_SENT;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

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

    public void SetSocialParams(string socialPlatform, string socialID)
    {        
        SocialPlatform = socialPlatform;
        SocialID = socialID;       
    }
}