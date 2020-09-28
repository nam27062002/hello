using System;

public class TrackingPersistenceSystem : PersistenceSystem
{
    private const string PARAM_SERVER_USER_ID = "accID";
	private const string PARAM_USER_ID = "userID";
    
	private const string PARAM_ADS_COUNT = "adsCount";
    private const string PARAM_ADS_SESSIONS = "adsSessions";
    
	private const string PARAM_TOTAL_PLAY_TIME = "totalPlaytime";
	private const string PARAM_GAME_ROUND_COUNT = "gameRoundCount";
    private const string PARAM_SESSION_COUNT = "sessionCount";
    
	private const string PARAM_SOCIAL_ID = "socialID";
    private const string PARAM_SOCIAL_PLATFORM = "socialPlatform";
    
	private const string PARAM_TOTAL_STORE_VISITS = "totalStoreVisits";
	private const string PARAM_TOTAL_PURCHASES = "totalPurchases";
	private const string PARAM_TOTAL_SPENT = "totalSpent";  // Cents of US Dollar (USD * 100)
	private const string PARAM_MAX_PURCHASE_PRICE = "maxPurchasePrice";  // Cents of US Dollar (USD * 100)
	private const string PARAM_LAST_PURCHASE_PRICE = "lastPurchasePrice";  // Cents of US Dollar (USD * 100)
	private const string PARAM_LAST_PURCHASE_TIMESTAMP = "lastPurchaseTimestamp";   // Unix timestamp (seconds since 1970)
	private const string PARAM_LAST_PURCHASE_ITEM_TYPE = "lastPurchaseItemType";  // Type of the first item of the offer pack, or currency if money pack
	private const string PARAM_LAST_PURCHASE_ITEM_CONTENT = "lastPurchaseItemContent";  // Sku of the first item of the offer pack, or amount if money pack

	private const string PARAM_TOTAL_EGG_PURCHASES = "totalEggPurchases";
    private const string PARAM_TOTAL_EGGS_PURCHASED_WITH_HC = "totalEggsPurchasedWithHC";
    private const string PARAM_TOTAL_EGGS_FOUND = "totalEggsFound";
    private const string PARAM_TOTAL_EGGS_OPENED = "totalEggsOpened";
    
    /// Whether or not this is the first time the game is loaded ever    
    private const string PARAM_FIRST_LOADING = "firstLoading";

    // Whether or not the user has ever logged in to a social platform. This is kept for backward compatibility reasons.
    // This used to be the variable used to know that the user had logged in when there was support for only one social platform
    // PARAM_SOCIAL_AUTH_SENT_LIST is used instead since multiplatform support was added
    private const string PARAM_SOCIAL_AUTH_SENT = "socialAuthSent";

    // List of social platform keys that the user has ever logged in. Required when social multiplatform support was added
    private const string PARAM_SOCIAL_AUTH_SENT_LIST = "socialAuthSentList";

    // Amount of times the user has closed the legal popup so far
    private const string PARAM_TOTAL_LEGAL_VISITS = "totalLegalVisits";

    // Events sent that should be sent only once.
    private const string PARAM_EVENTS_ALREADY_SENT = "eventsAlreadySent";

    // XPromo
    private const string PARAM_XPROMO_EXPERIMENT_NAME = "xpromoExperimentName";    // Unique identifier of the last xpromo experiment activated on for this player
    private const string PARAM_XPROMO_ACTIVATION_DATE = "xpromoActivationDate"; // The date the xpromo was activated for this player

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

	// Cents of US Dollar (USD * 100)
	public int TotalSpent {
		get {
			return Cache_GetInt(PARAM_TOTAL_SPENT);
		}

		set {
			Cache_SetInt(PARAM_TOTAL_SPENT, value);
		}
	}

	// Cents of US Dollar (USD * 100)
	public int MaxPurchasePrice {
		get {
			return Cache_GetInt(PARAM_MAX_PURCHASE_PRICE);
		}

		set {
			Cache_SetInt(PARAM_MAX_PURCHASE_PRICE, value);
		}
	}

	// Cents of US Dollar (USD * 100)
	public int LastPurchasePrice {
		get {
			return Cache_GetInt(PARAM_LAST_PURCHASE_PRICE);
		}

		set {
			Cache_SetInt(PARAM_LAST_PURCHASE_PRICE, value);
		}
	}

	// Seconds since 1970
	public long LastPurchaseTimestamp {
		get {
			return Cache_GetLong(PARAM_LAST_PURCHASE_TIMESTAMP);
		}

		set {
			Cache_SetLong(PARAM_LAST_PURCHASE_TIMESTAMP, value);
		}
	}

	// Type of the first item of the offer pack, or currency if money pack
	public string LastPurchaseItemType {
		get {
			return Cache_GetString(PARAM_LAST_PURCHASE_ITEM_TYPE);
		}

		set {
			Cache_SetString(PARAM_LAST_PURCHASE_ITEM_TYPE, value);
		}
	}

	// Sku of the first item of the offer pack, or amount if money pack
	public string LastPurchaseItemContent {
		get {
			return Cache_GetString(PARAM_LAST_PURCHASE_ITEM_CONTENT);
		}

		set {
			Cache_SetString(PARAM_LAST_PURCHASE_ITEM_CONTENT, value);
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

    private bool SocialAuthSent
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

    public bool HasSocialAuthSent(string socialPlatformKey)
    {                
        // Workaround for backward compatibility. PARAM_SOCIAL_AUTH_SENT was used to know whether or not the user had ever logged in
        // to the social network before multiplatform support was implemented, so if it's enabled then we need to make sure
        // the social network is added to PARAM_SOCIAL_AUTH_SENT_LIST, which is where this is stuff is stored after multiplatform
        // support was included
        if (SocialAuthSent)
        {
            string defaultSocialPlatformKey = SocialUtils.EPlatformToKey(FlavourManager.Instance.GetCurrentFlavour().SocialPlatformASSocialUtilsEPlatform);
            AddSocialPlatformKeyToAuthSentList(defaultSocialPlatformKey);

            // Set to false because persistence has already been adapted to the new format and we don't need to do it again
            SocialAuthSent = false;
        }

        string socialPlatformList = Cache_GetString(PARAM_SOCIAL_AUTH_SENT_LIST);
        return (string.IsNullOrEmpty(socialPlatformList)) ? false : socialPlatformList.Contains(socialPlatformKey);
    }

    public void AddSocialPlatformKeyToAuthSentList(string socialPlatformKey)
    {
        string socialPlatformList = Cache_GetString(PARAM_SOCIAL_AUTH_SENT_LIST);
        bool needsToSave = true;

        if (string.IsNullOrEmpty(socialPlatformList))
        {
            socialPlatformList = socialPlatformKey;
        }
        else if (socialPlatformList.Contains(socialPlatformKey))
        {
            needsToSave = false;
        }
        else
        {
            socialPlatformList += "," + socialPlatformKey;
        }

        if (needsToSave)
        {
            Cache_SetString(PARAM_SOCIAL_AUTH_SENT_LIST, socialPlatformList);
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

    // XPromo
    public string XPromoExperimentName {
        get { return Cache_GetString(PARAM_XPROMO_EXPERIMENT_NAME); }
        set { Cache_SetString(PARAM_XPROMO_EXPERIMENT_NAME, value); }
	}

    public DateTime XPromoActivationDate {
        get {
            DateTime date = DateTime.MaxValue;
            if(PersistenceUtils.SafeTryParse<DateTime>(Cache_GetString(PARAM_XPROMO_ACTIVATION_DATE), out date)) {
                return date;
			} else {
                return DateTime.MaxValue;
			}
		}

        set {
            Cache_SetString(PARAM_XPROMO_ACTIVATION_DATE, PersistenceUtils.SafeToString(value));
		}
	}

    public TrackingPersistenceSystem()
    {
        m_systemName = "Tracking";

        CacheDataInt dataInt;
		CacheDataLong dataLong;
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

		key = PARAM_TOTAL_SPENT;
		dataInt = new CacheDataInt(key, 0);
		Cache_AddData(key, dataInt);

		key = PARAM_MAX_PURCHASE_PRICE;
		dataInt = new CacheDataInt(key, 0);
		Cache_AddData(key, dataInt);

		key = PARAM_LAST_PURCHASE_PRICE;
		dataInt = new CacheDataInt(key, 0);
		Cache_AddData(key, dataInt);

		key = PARAM_LAST_PURCHASE_TIMESTAMP;
		dataLong = new CacheDataLong(key, 0);
		Cache_AddData(key, dataLong);

		key = PARAM_LAST_PURCHASE_ITEM_TYPE;
		dataString = new CacheDataString(key, string.Empty);
		Cache_AddData(key, dataString);

		key = PARAM_LAST_PURCHASE_ITEM_CONTENT;
		dataString = new CacheDataString(key, string.Empty);
		Cache_AddData(key, dataString);

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

        key = PARAM_SOCIAL_AUTH_SENT_LIST;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

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

        key = PARAM_XPROMO_EXPERIMENT_NAME;
        dataString = new CacheDataString(key, "");
        Cache_AddData(key, dataString);

        key = PARAM_XPROMO_ACTIVATION_DATE;
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