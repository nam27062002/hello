/// <summary>
/// This class is responsible to handle any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
public class HDTrackingManagerImp : HDTrackingManager
{    
    private enum EState
    {
        WaitingForSessionStart,
        SessionStarted,
        Banned
    }

    private EState State { get; set; }    

    private bool IsStartSessionNotified { get; set; }

    private bool IsDNAInitialised { get; set; }    

    public HDTrackingManagerImp()
    {
        Reset();
    }

    private void Reset()
    {        
        State = EState.WaitingForSessionStart;
        IsStartSessionNotified = false;
        IsDNAInitialised = false;

        if (TrackingSaveSystem == null)
        {
            TrackingSaveSystem = new TrackingSaveSystem();
        }
        else
        {
            TrackingSaveSystem.Reset();
        }        
    }

    private void CheckAndGenerateUserID()
    {
        if (TrackingSaveSystem != null)
        {
            // Generate Analytics user ID if not already set, it cannot be done in init function as we don't know the user ID at that point
            if (string.IsNullOrEmpty(TrackingSaveSystem.UserID))
            {
                // Generate a GUID so that we can identify users over the course of firing multiple events etc.
                TrackingSaveSystem.UserID = System.Guid.NewGuid().ToString();

                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    Log("Generate User ID = " + TrackingSaveSystem.UserID);
                }
            }
        }
    }   

    private void StartSession()
    {     
        if (!IsDNAInitialised)
        {
            InitDNA();
            IsDNAInitialised = true;
        }

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("StartSession");
        }

        State = EState.SessionStarted;

        CheckAndGenerateUserID();

        // Session counter advanced
        TrackingSaveSystem.SessionCount++;              

        // Calety needs to be initialized every time a session starts because the session count has changed
        StartCaletySession();

        // Sends the start session event
        Track_StartSessionEvent();        
    }

    private void InitDNA()
    {
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
        if (settingsInstance != null)
        {
            UbimobileToolkit.UbiservicesEnvironment kDNAEnvironment = UbimobileToolkit.UbiservicesEnvironment.UAT;
            if (settingsInstance.m_iBuildEnvironmentSelected == (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION)
            {
                kDNAEnvironment = UbimobileToolkit.UbiservicesEnvironment.PROD;
            }

#if UNITY_ANDROID
            DNAManager.SharedInstance.Initialise("12e4048c-5698-4e1e-a1d1-c8c2411b2515", settingsInstance.m_strVersionAndroidGplay, kDNAEnvironment);
#elif UNITY_IOS
			DNAManager.SharedInstance.Initialise ("42cbdf99-63e7-4e80-aae3-d05b9533349e", settingsInstance.m_strVersionIOS, kDNAEnvironment);
#endif            
        }
    }

    private void StartCaletySession()
    {
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
        if (settingsInstance != null)
        {
            int sessionNumber = TrackingSaveSystem.SessionCount;
            string trackingID = TrackingSaveSystem.UserID;
            string userID = (Authenticator.Instance.User != null) ? Authenticator.Instance.User.ID : "";
            string socialUserID = SocialFacade.Instance.GetSocialIDFromHighestPrecedenceNetwork();

            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID + " userId = " + userID + " socialUserID = " + socialUserID);
            }

            TrackingManager.TrackingConfig kTrackingConfig = new TrackingManager.TrackingConfig();
            kTrackingConfig.m_eTrackPlatform = TrackingManager.ETrackPlatform.E_TRACK_PLATFORM_OFFLINE;
            kTrackingConfig.m_strJSONConfigFilePath = "Tracking/TrackingEvents";
            kTrackingConfig.m_strStartSessionEventName = "01_START_SESSION";
            kTrackingConfig.m_strEndSessionEventName = "02_END_SESSION";
            kTrackingConfig.m_strMergeAccountEventName = "MERGE_ACCOUNTS";
            kTrackingConfig.m_strClientVersion = settingsInstance.GetClientBuildVersion();
            kTrackingConfig.m_strTrackingID = trackingID;
            kTrackingConfig.m_iSessionNumber = sessionNumber;
            kTrackingConfig.m_iMaxCachedLoggedDays = 3;

            TrackingManager.SharedInstance.Initialise(ref kTrackingConfig);
        }
    }    

    public override void Update()
    {
        switch (State)
        {
            case EState.WaitingForSessionStart:                
                if (TrackingSaveSystem != null && IsStartSessionNotified)
                {
                    // No tracking for hackers because their sessions will be misleading
                    if (SaveFacade.Instance.userSaveSystem.isHacker)
                    {
                        State = EState.Banned;
                    }
                    else
                    {
                        StartSession();
                    }
                }
                break;
        }

        if (TrackingSaveSystem != null && TrackingSaveSystem.IsDirty)
        {
            TrackingSaveSystem.IsDirty = false;
            SaveFacade.Instance.Save();
        }

        if (Session_AnyRoundsStarted)
        {
            Session_PlayTime += Time.deltaTime;
        }

        if (Debug_IsEnabled)
        {
            Debug_Update();
        }
    }

    #region notify
    public override void Notify_ApplicationStart()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_StartSession");
        }

        if (State == EState.WaitingForSessionStart)
        {
            IsStartSessionNotified = true;
        }        
    }

    public override void Notify_ApplicationEnd()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_ApplicationEnd");
        }

        Notify_SessionEnd(ESeassionEndReason.app_closed);

        IsStartSessionNotified = false;
        State = EState.WaitingForSessionStart;       
    }

    public override void Notify_ApplicationPaused()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_ApplicationPaused");
        }

        Notify_SessionEnd(ESeassionEndReason.no_activity);
    }

    public override void Notify_ApplicationResumed()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Notify_ApplicationResumed");
        }

        // If the dna session had been started then it has to be restarted
        if (Session_AnyRoundsStarted)
        {            
            Track_MobileStartEvent();
        }
    }

    private void Notify_SessionEnd(ESeassionEndReason reason)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            string str = "Notify_SessionEnd reason = " + reason + " sessionPlayTime = " + Session_PlayTime;
            if (TrackingSaveSystem != null)
            {
                str += " totalPlayTime = " + TrackingSaveSystem.TotalPlaytime;
            }

            Log(str);
        }
        
        if (TrackingSaveSystem != null)
        {
            // Current session play time is added up to the total
            int sessionTime = (int)Session_PlayTime;
            TrackingSaveSystem.TotalPlaytime += sessionTime;
        }            

        Track_ApplicationEndEvent(reason.ToString());

        // It needs to be reseted after tracking the event because the end application event needs to send the session play time
        Session_PlayTime = 0f;        
    }

    /// <summary>
    /// Called when the user starts a round
    /// </summary>    
    public override void Notify_RoundStart()
    {
        if (!Session_AnyRoundsStarted)
        {
            Session_AnyRoundsStarted = true;
            Track_MobileStartEvent();
        }
    }    

    /// <summary>
    /// Called when the user opens the app store
    /// </summary>
    public override void Notify_StoreVisited()
    {
        if (TrackingSaveSystem != null)
        {
            TrackingSaveSystem.TotalStoreVisits++;
        }
    }

    /// <summary>
    /// /// Called when the user completed an in app purchase.    
    /// </summary>
    /// <param name="storeTransactionID">transaction ID returned by the platform</param>
    /// <param name="houstonTransactionID">transaction ID returned by houston</param>
    /// <param name="itemID">ID of the item purchased</param>
    /// <param name="promotionType">Promotion type if there was one</param>
    /// <param name="moneyCurrencyCode">Code of the currency that the user used to pay for the item</param>
    /// <param name="moneyPrice">Price paid by the user in her currency</param>
    public override void Notify_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice)
    {
        Session_IsPayingSession = true;

        if (TrackingSaveSystem != null)
        {
            TrackingSaveSystem.TotalPurchases++;
        }

        Track_IAPCompleted(storeTransactionID, houstonTransactionID, itemID, promotionType, moneyCurrencyCode, moneyPrice);
    }

    /// <summary>
    /// Called when the user completed a purchase by using game resources (either soft currency or hard currency)
    /// </summary>
    /// <param name="economyGroup">ID used to identify the type of item the user has bought. Example UNLOCK_DRAGON</param>
    /// <param name="itemID">ID used to identify the item that the user bought. Example: sku of the dragon unlocked</param>
    /// <param name="promotionType">Promotion type if there is one, otherwise <c>null</c></param>
    /// <param name="moneyCurrency">Currency type used</param>
    /// <param name="moneyPrice">Amount of the currency paid</param>
    public override void Notify_PurchaseWithResourcesCompleted(EEconomyGroup economyGroup, string itemID, string promotionType, UserProfile.Currency moneyCurrency, int moneyPrice)
    {
        Track_PurchaseWithResourcesCompleted(EconomyGroupToString(economyGroup), itemID, 1, promotionType, Track_UserCurrencyToString(moneyCurrency), moneyPrice);
    }

    /// <summary>
    /// Called when the user clicks on the button to request a customer support ticked
    /// </summary>
    public override void Notify_CustomerSupportRequested()
    {
        Track_CustomerSupportRequested();
    }

    /// <summary>
    /// Called when an ad has been requested by the user. 
    /// <param name="adType">Ad Type.</param>
    /// <param name="rewardType">Type of reward given for watching the ad.</param>
    /// <param name="adIsAvailable"><c>true</c>c> if the ad is available, <c>false</c> otherwise.</param>
    /// <param name="provider">Ad Provider. Optional.</param>    
    /// </summary>
    public override void Notify_AdStarted(string adType, string rewardType, bool adIsAvailable, string provider=null)
    {
        Track_AdStarted(adType, rewardType, adIsAvailable, provider);
    }

    /// <summary>
    /// Called then the ad requested by the user has finished
    /// <param name="adType">Ad Type.</param>    
    /// <param name="adIsLoaded"><c>true</c>c> if the ad was effectively viewed, <c>false</c> otherwise.</param>
    /// <param name="maxReached"><c>true</c> if the user has reached the limit of ad viewing authorized by the app. Used for reward ads</param>
    /// <param name="adViewingDuration">Duration in seconds of the ad viewing.</param>
    /// <param name="provider">Ad Provider. Optional.</param>    
    /// </summary>
    public override void Notify_AdFinished(string adType, bool adIsLoaded, bool maxReached, int adViewingDuration = 0, string provider = null)
    {
        if (adIsLoaded && TrackingSaveSystem != null)
        {
            TrackingSaveSystem.AdsCount++;

            if (!Session_IsAdSession)
            {
                Session_IsAdSession = true;
                TrackingSaveSystem.AdsSessions++;
            }
        }

        Track_AdFinished(adType, adIsLoaded, maxReached, adViewingDuration, provider);
    }
    #endregion

    #region track
    private void Track_StartSessionEvent()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_StartSessionEvent");
        }        

        // DNA
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("game.start");
        if (e != null)
        {
            Track_AddParamSubVersion(e);
            Track_AddParamProviderAuth(e);
            Track_AddParamPlayerID(e);
            Track_AddParamServerAccID(e);
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }    

    private void Track_ApplicationEndEvent(string stopCause)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_ApplicationEndEvent " + stopCause);
        }        

        // DNA
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.mobile.stop");
        if (e != null)
        {
            Track_AddParamBool(e, TRACK_PARAM_IS_PAYING_SESSION, Session_IsPayingSession);
            Track_AddParamPlayerProgress(e);
            e.SetParameterValue(TRACK_PARAM_SESSION_PLAY_TIME, (int)Session_PlayTime);
            Track_AddParamString(e, TRACK_PARAM_STOP_CAUSE, stopCause);
            Track_AddParamTotalPlaytime(e);
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_MobileStartEvent()
    {        
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_MobileStartEvent");
        }

        // DNA
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.mobile.start");
        if (e != null)
        {
            Track_AddParamAbTesting(e);
            Track_AddParamPlayerProgress(e);         
            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_IAPCompleted storeTransactionID = " + storeTransactionID + " houstonTransactionID = " + houstonTransactionID + 
                " itemID = " + itemID + " promotionType = " + promotionType + " moneyCurrencyCode = " + moneyCurrencyCode + " moneyPrice = " + moneyPrice);
        }        

        // DNA
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.player.iap");
        if (e != null)
        {            
            Track_AddParamString(e, TRACK_PARAM_STORE_TRANSACTION_ID, storeTransactionID);
            Track_AddParamString(e, TRACK_PARAM_HOUSTON_TRANSACTION_ID, houstonTransactionID);
            Track_AddParamString(e, TRACK_PARAM_ITEM_ID, itemID);
            Track_AddParamString(e, TRACK_PARAM_PROMOTION_TYPE, promotionType);
            Track_AddParamString(e, TRACK_PARAM_MONEY_CURRENCY, moneyCurrencyCode);            
            e.SetParameterValue(TRACK_PARAM_MONEY_IAP, moneyPrice);

            Track_AddParamPlayerProgress(e);

            if (TrackingSaveSystem != null)
            {
                Track_AddParamTotalPurchases(e);
                e.SetParameterValue(TRACK_PARAM_TOTAL_STORE_VISITS, TrackingSaveSystem.TotalStoreVisits);
            }

            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_PurchaseWithResourcesCompleted(string economyGroup, string itemID, int itemQuantity, string promotionType, string moneyCurrency, float moneyPrice)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_PurchaseWithResourcesCompleted economyGroup = " + economyGroup + " itemID = " + itemID + " promotionType = " + promotionType + 
                " moneyCurrency = " + moneyCurrency + " moneyPrice = " + moneyPrice);
        }

        // DNA
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.player.iap.secondaryStore");
        if (e != null)
        {
            Track_AddParamString(e, TRACK_PARAM_ECONOMY_GROUP, economyGroup);
            Track_AddParamString(e, TRACK_PARAM_ITEM_ID, itemID);
            e.SetParameterValue(TRACK_PARAM_ITEM_QUANTITY, itemQuantity);
            Track_AddParamString(e, TRACK_PARAM_PROMOTION_TYPE, promotionType);
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, moneyCurrency);            
            e.SetParameterValue(TRACK_PARAM_MONEY_IAP, moneyPrice);

            Track_AddParamPlayerProgress(e);
            Track_AddParamTotalPurchases(e);            

            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_CustomerSupportRequested()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_CustomerSupportRequested");
        }

        // DNA custom.mobile.stop
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.game.cs");
        if (e != null)
        {                                    
            Track_AddParamTotalPurchases(e);
            Track_AddParamSessionsCount(e);
            Track_AddParamPlayerProgress(e);
            // Always 0 since there's no pvp in the game
            e.SetParameterValue(TRACK_PARAM_PVP_MATCHES_PLAYED, 0);            

            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_AdStarted(string adType, string rewardType, bool adIsAvailable, string provider = null)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_AdStarted");
        }

        // DNA custom.mobile.stop
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.game.ad.start");
        if (e != null)
        {
            if (TrackingSaveSystem != null)
            {
                e.SetParameterValue(TRACK_PARAM_NB_ADS_LTD, TrackingSaveSystem.AdsCount);
                e.SetParameterValue(TRACK_PARAM_NB_ADS_SESSION, TrackingSaveSystem.AdsSessions);
            }

            Track_AddParamBool(e, TRACK_PARAM_AD_IS_AVAILABLE, adIsAvailable);
            Track_AddParamString(e, TRACK_PARAM_REWARD_TYPE, rewardType);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);
            Track_AddParamString(e, TRACK_PARAM_ADS_TYPE, adType);

            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    private void Track_AdFinished(string adType, bool adIsLoaded, bool maxReached, int adViewingDuration, string provider)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_AdFinished");
        }

        // DNA custom.mobile.stop
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.game.ad.finished");
        if (e != null)
        {            
            Track_AddParamBool(e, TRACK_PARAM_IS_LOADED, adIsLoaded);
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);            
            e.SetParameterValue(TRACK_PARAM_AD_VIEWING_DURATION, adViewingDuration);
            Track_AddParamBool(e, TRACK_PARAM_MAX_REACHED, maxReached);            
            Track_AddParamString(e, TRACK_PARAM_ADS_TYPE, adType);

            TrackingManager.SharedInstance.SendEvent(e);
        }
    }

    // -------------------------------------------------------------
    // Params
    // -------------------------------------------------------------    
    private const string TRACK_PARAM_AB_TESTING                 = "abtesting";
    private const string TRACK_PARAM_AD_IS_AVAILABLE            = "adIsAvailable";
    private const string TRACK_PARAM_AD_VIEWING_DURATION        = "adViewingDuration";
    private const string TRACK_PARAM_ADS_TYPE                   = "adsType";
    private const string TRACK_PARAM_CURRENCY                   = "currency";
    private const string TRACK_PARAM_ECONOMY_GROUP              = "economyGroup";
    private const string TRACK_PARAM_HOUSTON_TRANSACTION_ID     = "houstonTransactionID";
    private const string TRACK_PARAM_IN_GAME_ID                 = "InGameId";
    private const string TRACK_PARAM_IS_LOADED                  = "isLoaded";
    private const string TRACK_PARAM_IS_PAYING_SESSION          = "isPayingSession";
    private const string TRACK_PARAM_ITEM_ID                    = "itemID";
    private const string TRACK_PARAM_ITEM_QUANTITY              = "itemQuantity";
    private const string TRACK_PARAM_MAX_REACHED                = "maxReached";
    private const string TRACK_PARAM_MONEY_CURRENCY             = "moneyCurrency";
    private const string TRACK_PARAM_MONEY_IAP                  = "moneyIAP";
    private const string TRACK_PARAM_NB_ADS_LTD                 = "nbAdsLtd";
    private const string TRACK_PARAM_NB_ADS_SESSION             = "nbAdsSession";
    private const string TRACK_PARAM_PLAYER_ID                  = "playerID";
    private const string TRACK_PARAM_PLAYER_PROGRESS            = "playerProgress";
    private const string TRACK_PARAM_PROMOTION_TYPE             = "promotionType";    
    private const string TRACK_PARAM_PROVIDER                   = "provider";
    private const string TRACK_PARAM_PROVIDER_AUTH              = "providerAuth";
    private const string TRACK_PARAM_PVP_MATCHES_PLAYED         = "pvpMatchesPlayed";
    private const string TRACK_PARAM_REWARD_TYPE                = "rewardType";    
    private const string TRACK_PARAM_SESSION_PLAY_TIME          = "sessionPlaytime";
    private const string TRACK_PARAM_SESSIONS_COUNT             = "sessionsCount";
    private const string TRACK_PARAM_STOP_CAUSE                 = "stopCause";
    private const string TRACK_PARAM_SUBVERSION                 = "SubVersion";
    private const string TRACK_PARAM_STORE_TRANSACTION_ID       = "storeTransactionID";
    private const string TRACK_PARAM_TOTAL_PLAYTIME             = "totalPlaytime";
    private const string TRACK_PARAM_TOTAL_PURCHASES            = "totalPurchases";
    private const string TRACK_PARAM_TOTAL_STORE_VISITS         = "totalStoreVisits";    


    private void Track_AddParamSubVersion(TrackingManager.TrackingEvent e)
    {
        // "SoftLaunch" is sent so far. It will be changed wto "HardLaunch" after WWL
        Track_AddParamString(e, TRACK_PARAM_SUBVERSION, "SoftLaunch");
    }

    private void Track_AddParamProviderAuth(TrackingManager.TrackingEvent e)
    {
        string value = null;
        if (TrackingSaveSystem != null && !string.IsNullOrEmpty(TrackingSaveSystem.SocialPlatform))
        {
            value = TrackingSaveSystem.SocialPlatform;
        }

        if (string.IsNullOrEmpty(value))
        {
            value = "SilentLogin";
        }

        Track_AddParamString(e, TRACK_PARAM_PROVIDER_AUTH, value);
    }

    private void Track_AddParamPlayerID(TrackingManager.TrackingEvent e)
    {
        string value = null;
        if (TrackingSaveSystem != null && !string.IsNullOrEmpty(TrackingSaveSystem.SocialID))
        {
            value = TrackingSaveSystem.SocialID;
        }

        if (string.IsNullOrEmpty(value))
        {
            value = "NotDefined";
        }

        Track_AddParamString(e, TRACK_PARAM_PLAYER_ID, value);
    }

    private void Track_AddParamServerAccID(TrackingManager.TrackingEvent e)
    {
        int value = 0;
        if (TrackingSaveSystem != null)
        {
            value = TrackingSaveSystem.AccountID;
        }

        e.SetParameterValue(TRACK_PARAM_IN_GAME_ID, value);
    }    

    private void Track_AddParamAbTesting(TrackingManager.TrackingEvent e)
    {        
        e.SetParameterValue(TRACK_PARAM_AB_TESTING, "");
    }

    private void Track_AddParamPlayerProgress(TrackingManager.TrackingEvent e)
    {
        int value = (UsersManager.currentUser != null) ? UsersManager.currentUser.GetPlayerProgress() : 0;
        Track_AddParamString(e, TRACK_PARAM_PLAYER_PROGRESS, value + "");
    }    

    private void Track_AddParamSessionsCount(TrackingManager.TrackingEvent e)
    {
        int value = (TrackingSaveSystem != null) ? TrackingSaveSystem.SessionCount : 0;
        e.SetParameterValue(TRACK_PARAM_SESSIONS_COUNT, value);
    }

    private void Track_AddParamTotalPlaytime(TrackingManager.TrackingEvent e)
    {
        int value = (TrackingSaveSystem != null) ? TrackingSaveSystem.TotalPlaytime : 0;
        e.SetParameterValue(TRACK_PARAM_TOTAL_PLAYTIME, value);
    }

    private void Track_AddParamTotalPurchases(TrackingManager.TrackingEvent e)
    {
        if (TrackingSaveSystem != null)
        {
            e.SetParameterValue(TRACK_PARAM_TOTAL_PURCHASES, TrackingSaveSystem.TotalPurchases);
        }        
    }   

    private void Track_AddParamString(TrackingManager.TrackingEvent e, string paramName, string value)
    {
        // null is not a valid value for Calety
        if (value == null)
        {
            value = "";
        }

        e.SetParameterValue(paramName, value);
    }

    private void Track_AddParamBool(TrackingManager.TrackingEvent e, string paramName, bool value)
    {
        int valueToSend = (value) ? 1 : 0;
        e.SetParameterValue(paramName, valueToSend);
    }

    private string Track_UserCurrencyToString(UserProfile.Currency currency)
    {
        string returnValue = "";
        switch (currency)
        {
            case UserProfile.Currency.HARD:
                returnValue = "HardCurrency";
                break;

            case UserProfile.Currency.SOFT:
                returnValue = "SoftCurrency_Coins";
                break;

            case UserProfile.Currency.GOLDEN_FRAGMENTS:
                returnValue = "SoftCurrency_GoldenFragments";
                break;

            case UserProfile.Currency.KEYS:
                returnValue = "SoftCurrency_Keys";
                break;

            case UserProfile.Currency.REAL:
                returnValue = "Real";
                break;

        }

        return returnValue;
    }
    #endregion

    #region session
    // This region is responsible for storing data generated during the current session. These data don't need to be persisted.

    /// <summary>
    /// Enum with the reason why the session has ended. These names have to match the supported values defined for "stopCause" parameter of
    /// "custom.mobile.stop" event described at https://mdc-web-tomcat17.ubisoft.org/confluence/display/dna/Events+Guidelines+for+Mobile+Games#EventsGuidelinesforMobileGames-custom.mobile.start
    /// </summary>
    private enum ESeassionEndReason
    {
        app_closed,
        no_activity
    }

    /// <summary>
    /// Whether or not the user has paid (actual purchase) during the current session
    /// </summary>
    private bool Session_IsPayingSession { get; set; }

    /// <summary>
    /// Whether or not the user has already watched an ad during the current session
    /// </summary>
    private bool Session_IsAdSession { get; set; }

    /// <summary>
    /// Current session duration (in seconds) so far. It has to start being accumulated after the first game round
    /// </summary>
    private float Session_PlayTime { get; set; }

    /// <summary>
    /// Sessions amount, including this one, played by the user since installation
    /// </summary>
    private int Session_Count { get; set; }

    /// <summary>
    /// This flag states whether or not the user has started any rounds. This is used to start DNA session. DNA session starts when the user 
    /// starts the first round since the application started
    /// </summary>
    private bool Session_AnyRoundsStarted { get; set; }

    private void Session_Reset()
    {
        Session_IsPayingSession = false;
        Session_IsAdSession = false;
        Session_PlayTime = 0f;
        Session_Count = 0;
        Session_AnyRoundsStarted = false;
    }
    #endregion

    #region debug
    private const bool Debug_IsEnabled = true;

    private void Debug_Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Notify_RoundStart();
        }        
    }
    #endregion
}

