/// <summary>
/// This class is responsible to handle any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

using UnityEngine;
using System.Collections.Generic;

public class HDTrackingManagerImp : HDTrackingManager
{    
    private enum EState
    {
        None,
        WaitingForSessionStart,
        SessionStarted,
        Banned
    }

	private FunnelData_Load m_loadFunnel;
	private FunnelData_FirstUX m_firstUXFunnel;

    private EState State { get; set; }    

    private bool IsStartSessionNotified { get; set; }

    private bool AreSDKsInitialised { get; set; }    

    public HDTrackingManagerImp()
    {
		m_loadFunnel = new FunnelData_Load();
		m_firstUXFunnel = new FunnelData_FirstUX();		        
    }

    public override void Init()
    {        
    	base.Init();
        State = EState.WaitingForSessionStart;
        IsStartSessionNotified = false;
        AreSDKsInitialised = false;                

        if (TrackingPersistenceSystem == null)
        {
            TrackingPersistenceSystem = new TrackingPersistenceSystem();
        }
        else
        {
            TrackingPersistenceSystem.Reset();
        }

        Session_Reset();
        m_loadFunnel.Reset();
		m_firstUXFunnel.Reset();

		Messenger.AddListener<string, string, SimpleJSON.JSONNode>(EngineEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
		Messenger.AddListener<string>(EngineEvents.PURCHASE_ERROR, OnPurchaseFailed);
		Messenger.AddListener<string>(EngineEvents.PURCHASE_FAILED, OnPurchaseFailed);
        Messenger.AddListener<bool>(GameEvents.LOGGED, OnLoggedIn);
    }

    public override void Destroy ()
    {
		base.Destroy ();
		Messenger.RemoveListener<string, string, SimpleJSON.JSONNode>(EngineEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
		Messenger.RemoveListener<string>(EngineEvents.PURCHASE_ERROR, OnPurchaseFailed);
		Messenger.RemoveListener<string>(EngineEvents.PURCHASE_FAILED, OnPurchaseFailed);
        Messenger.RemoveListener<bool>(GameEvents.LOGGED, OnLoggedIn);
    }

	private void OnPurchaseSuccessful(string _sku, string _storeTransactionID, SimpleJSON.JSONNode _receipt) 
	{
        StoreManager.StoreProduct product = GameStoreManager.SharedInstance.GetStoreProduct(_sku);
        string moneyCurrencyCode = null;
        float moneyPrice = 0f;            
        if (product != null) {                
            moneyCurrencyCode = product.m_strCurrencyCode;
            moneyPrice = product.m_fLocalisedPriceValue;
        }

        // store transaction ID is also used for houston transaction ID, which is what Migh&Magic game also does
        string houstonTransactionID = _storeTransactionID;
        string promotionType = null; // Not implemented yet            
        Notify_IAPCompleted(_storeTransactionID, houstonTransactionID, _sku, promotionType, moneyCurrencyCode, moneyPrice);

	}

	private void OnPurchaseFailed(string _sku) 
	{
		
	}

    private void OnLoggedIn(bool logged)
    {        
        if (logged)
        {
            // Server uid is stored as soon as log in happens so we'll be able to start TrackingManager when offline
            PersistencePrefs.ServerUserId = GameSessionManager.SharedInstance.GetUID();            
        }

        // We need to reinitialize TrackingManager if it has already been initialized, otherwise we simply do nothing since it will be initialize properly 
        if (IsStartSessionNotified)
        {
            InitTrackingManager();
        }
    }

    private void CheckAndGenerateUserID()
    {
        if (TrackingPersistenceSystem != null)
        {
            // Generate Analytics user ID if not already set, it cannot be done in init function as we don't know the user ID at that point
            if (string.IsNullOrEmpty(TrackingPersistenceSystem.UserID))
            {
                // Generate a GUID so that we can identify users over the course of firing multiple events etc.
                TrackingPersistenceSystem.UserID = System.Guid.NewGuid().ToString();

                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    Log("Generate User ID = " + TrackingPersistenceSystem.UserID);
                }
            }
        }
    }   

    private void StartSession()
    {     
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("StartSession");
        }

        State = EState.SessionStarted;

        CheckAndGenerateUserID();

        InitSDKs();

        Session_IsFirstTime = TrackingPersistenceSystem.IsFirstLoading;

        // It has to be true only in the first loading
        if (Session_IsFirstTime)
        {
            TrackingPersistenceSystem.IsFirstLoading = false;
        }

        // Session counter advanced
        TrackingPersistenceSystem.SessionCount++;

        // Calety needs to be initialized every time a session starts because the session count has changed
        InitTrackingManager();

        // Sends the start session event
        Track_StartSessionEvent();        
    }

    private void InitSDKs()
    {
        if (!AreSDKsInitialised)
        {
            CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
            InitDNA(settingsInstance);
            InitAppsFlyer(settingsInstance);

            AreSDKsInitialised = true;
        }
    }

    private void InitDNA(CaletySettings settingsInstance)
    {
        // DNA is not initialized in editor because it doesn't work on Windows and it crashes on Mac
#if !UNITY_EDITOR        
        if (settingsInstance != null)
        {
            UbimobileToolkit.UbiservicesEnvironment kDNAEnvironment = UbimobileToolkit.UbiservicesEnvironment.UAT;
            if (settingsInstance.m_iBuildEnvironmentSelected == (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION)
            {
                kDNAEnvironment = UbimobileToolkit.UbiservicesEnvironment.PROD;
            }

#if UNITY_ANDROID
            DNAManager.SharedInstance.Initialise("12e4048c-5698-4e1e-a1d1-c8c2411b2515", settingsInstance.GetClientBuildVersion(), settingsInstance.m_strVersionAndroidGplay, kDNAEnvironment);
#elif UNITY_IOS
			DNAManager.SharedInstance.Initialise ("42cbdf99-63e7-4e80-aae3-d05b9533349e", settingsInstance.GetClientBuildVersion(), settingsInstance.m_strVersionIOS, kDNAEnvironment);
#endif
        }
#endif
    }

    private void InitAppsFlyer(CaletySettings settingsInstance)
    {
        // Init AppsFlyer
#if UNITY_IOS
        string strAppsFlyerPlatformID = "1163163344";
#elif UNITY_ANDROID
        string strAppsFlyerPlatformID = settingsInstance.GetBundleID();
#else
        string strAppsFlyerPlatformID = "";
#endif        
        AppsFlyerManager.SharedInstance.Initialise("m2TXzMjM53e5MCwGasukoW", strAppsFlyerPlatformID, TrackingPersistenceSystem.UserID);

#if UNITY_ANDROID
        AppsFlyerManager.SharedInstance.SetAndroidGCMKey(settingsInstance.m_strGameCenterAppGoogle[settingsInstance.m_iBuildEnvironmentSelected]);
#endif
    }

    private void InitTrackingManager()
    {
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
        if (settingsInstance != null)
        {
            int sessionNumber = TrackingPersistenceSystem.SessionCount;
            string trackingID = TrackingPersistenceSystem.UserID;
            string userID = PersistencePrefs.ServerUserId;
            TrackingManager.ETrackPlatform trackPlatform = (GameSessionManager.SharedInstance.IsLogged()) ? TrackingManager.ETrackPlatform.E_TRACK_PLATFORM_ONLINE : TrackingManager.ETrackPlatform.E_TRACK_PLATFORM_OFFLINE;

            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID + " userId = " + userID + " trackPlatform = " + trackPlatform);
            }

            TrackingManager.TrackingConfig kTrackingConfig = new TrackingManager.TrackingConfig();
            kTrackingConfig.m_eTrackPlatform = trackPlatform;
            kTrackingConfig.m_strJSONConfigFilePath = "Tracking/TrackingEvents";
            kTrackingConfig.m_strStartSessionEventName = "game.start";
			kTrackingConfig.m_strEndSessionEventName = "custom.mobile.stop";
            kTrackingConfig.m_strMergeAccountEventName = "MERGE_ACCOUNTS";
            kTrackingConfig.m_strClientVersion = settingsInstance.GetClientBuildVersion();
            kTrackingConfig.m_strTrackingID = trackingID;
            kTrackingConfig.m_strUserIDOptional = userID;
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
                if (TrackingPersistenceSystem != null && IsStartSessionNotified)
                {
                    // No tracking for hackers because their sessions will be misleading
                    if (UsersManager.currentUser.isHacker)
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

        if (TrackingPersistenceSystem != null && TrackingPersistenceSystem.IsDirty)
        {
            TrackingPersistenceSystem.IsDirty = false;
            PersistenceFacade.instance.Save_Request(false);
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
            if (TrackingPersistenceSystem != null)
            {
                str += " totalPlayTime = " + TrackingPersistenceSystem.TotalPlaytime;
            }

            Log(str);
        }
        
        if (TrackingPersistenceSystem != null)
        {
            // Current session play time is added up to the total
            int sessionTime = (int)Session_PlayTime;
            TrackingPersistenceSystem.TotalPlaytime += sessionTime;
        }            

        Track_ApplicationEndEvent(reason.ToString());

        // It needs to be reseted after tracking the event because the end application event needs to send the session play time
        Session_PlayTime = 0f;        
    }

    /// <summary>
    /// Called when the user starts a round
    /// </summary>    
    public override void Notify_RoundStart(int dragonXp, int dragonProgression, string dragonSkin, List<string> pets)
    {
        // custom.game.start has to be send just the first time
        if (!Session_AnyRoundsStarted)
        {
            Session_AnyRoundsStarted = true;
            Track_MobileStartEvent();
        }

        // Resets the amount of runs in the current round because a new round has just started
        Session_RunsAmountInCurrentRound = 0;

        // One more game round
        TrackingPersistenceSystem.GameRoundCount++;

        // Notifies that one more round has started
        Track_RoundStart(dragonXp, dragonProgression, dragonSkin, pets);
    }

    public override void Notify_RoundEnd(int dragonXp, int deltaXp, int dragonProgression, int timePlayed, int score, int chestsFound, int eggFound, 
        float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive, int scGained, int hcGained)
    {
        // Last deathType, deathSource and deathCoordinates are used since this information is provided when Notify_RunEnd() is called
        Track_RoundEnd(dragonXp, deltaXp, dragonProgression, timePlayed, score, Session_LastDeathType, Session_LastDeathSource, Session_LastDeathCoordinates,
            chestsFound, eggFound, highestMultiplier, highestBaseMultiplier, furyRushNb, superFireRushNb, hcRevive, adRevive, scGained, hcGained);
    }

    public override void Notify_RunEnd(int dragonXp, int timePlayed, int score, string deathType, string deathSource, Vector3 deathCoordinates)
    {
        Session_RunsAmountInCurrentRound++;

        string deathCoordinatesAsString = Track_CoordinatesToString(deathCoordinates);

        // Death information is stored at this point so it can be used easily when Notify_RoundEnd() is called
        Session_LastDeathType = deathType;
        Session_LastDeathSource = deathSource;
        Session_LastDeathCoordinates = deathCoordinatesAsString;

        // Actual track
        Track_RunEnd(dragonXp, timePlayed, score, deathType, deathSource, deathCoordinatesAsString);    
    }

    /// <summary>
    /// Called when the user opens the app store
    /// </summary>
    public override void Notify_StoreVisited()
    {
        if (TrackingPersistenceSystem != null)
        {
            TrackingPersistenceSystem.TotalStoreVisits++;
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

        if (TrackingPersistenceSystem != null)
        {
            TrackingPersistenceSystem.TotalPurchases++;
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
    /// <param name="amountBalance">Amount of this currency after the transaction was performed</param>
    public override void Notify_PurchaseWithResourcesCompleted(EEconomyGroup economyGroup, string itemID, string promotionType, 
        UserProfile.Currency moneyCurrency, int moneyPrice, int amountBalance)
    {
        Track_PurchaseWithResourcesCompleted(EconomyGroupToString(economyGroup), itemID, 1, promotionType, Track_UserCurrencyToString(moneyCurrency), moneyPrice, amountBalance);
    }

    /// <summary>
    /// Called when the user earned some resources
    /// </summary>
    /// <param name="economyGroup">ID used to identify the type of item the user has earned. Example UNLOCK_DRAGON</param>        
    /// <param name="moneyCurrencyCode">Currency type earned</param>
    /// <param name="amountDelta">Amount of the currency earned</param>
    /// <param name="amountBalance">Amount of this currency after the transaction was performed</param>
    public override void Notify_EarnResources(EEconomyGroup economyGroup, UserProfile.Currency moneyCurrencyCode, int amountDelta, int amountBalance)
    {       
        Track_EarnResources(EconomyGroupToString(economyGroup), Track_UserCurrencyToString(moneyCurrencyCode), amountDelta, amountBalance);
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
        if (adIsLoaded && TrackingPersistenceSystem != null)
        {
            TrackingPersistenceSystem.AdsCount++;

            if (!Session_IsAdSession)
            {
                Session_IsAdSession = true;
                TrackingPersistenceSystem.AdsSessions++;
            }
        }

        Track_AdFinished(adType, adIsLoaded, maxReached, adViewingDuration, provider);
    }

	/// <summary>
	/// The game has reached a step in the loading funnel.
	/// </summary>
	/// <param name="_step">Step to notify.</param>
	public override void Notify_Funnel_Load(FunnelData_Load.Steps _step) {
		Track_Funnel(m_loadFunnel.name, m_loadFunnel.GetStepName(_step), m_loadFunnel.GetStepDuration(_step), m_loadFunnel.GetStepTotalTime(_step), Session_IsFirstTime);
	}

	/// <summary>
	/// The game has reached a step in the firts user experience funnel.
	/// </summary>
	/// <param name="_step">Step to notify.</param>
	public override void Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps _step) {
		Track_Funnel(m_firstUXFunnel.name, m_firstUXFunnel.GetStepName(_step), m_firstUXFunnel.GetStepDuration(_step), m_firstUXFunnel.GetStepTotalTime(_step), Session_IsFirstTime);
	}
#endregion

#region track	
    private void Track_StartSessionEvent()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_StartSessionEvent");
        }        
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("game.start");
        if (e != null)
        {
            Track_AddParamSubVersion(e);
            Track_AddParamProviderAuth(e);
            Track_AddParamPlayerID(e);
            Track_AddParamServerAccID(e);
            // "" is sent because Calety doesn't support this yet
            Track_AddParamString(e, TRACK_PARAM_TYPE_NOTIF, "");
            Track_SendEvent(e);
        }
    }    

    private void Track_ApplicationEndEvent(string stopCause)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_ApplicationEndEvent " + stopCause);
        }        
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.mobile.stop");
        if (e != null)
        {
            Track_AddParamBool(e, TRACK_PARAM_IS_PAYING_SESSION, Session_IsPayingSession);
            Track_AddParamPlayerProgress(e);
            e.SetParameterValue(TRACK_PARAM_SESSION_PLAY_TIME, (int)Session_PlayTime);
            Track_AddParamString(e, TRACK_PARAM_STOP_CAUSE, stopCause);
            Track_AddParamTotalPlaytime(e);
			Track_SendEvent(e);
        }
    }

    private void Track_MobileStartEvent()
    {        
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_MobileStartEvent");
        }
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.mobile.start");
        if (e != null)
        {
            Track_AddParamAbTesting(e);
            Track_AddParamPlayerProgress(e);         
			Track_SendEvent(e);
        }
    }

    private void Track_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_IAPCompleted storeTransactionID = " + storeTransactionID + " houstonTransactionID = " + houstonTransactionID + 
                " itemID = " + itemID + " promotionType = " + promotionType + " moneyCurrencyCode = " + moneyCurrencyCode + " moneyPrice = " + moneyPrice);
        }        
                
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

            if (TrackingPersistenceSystem != null)
            {
                Track_AddParamTotalPurchases(e);
                e.SetParameterValue(TRACK_PARAM_TOTAL_STORE_VISITS, TrackingPersistenceSystem.TotalStoreVisits);
            }

			Track_SendEvent(e);
        }
    }

    private void Track_PurchaseWithResourcesCompleted(string economyGroup, string itemID, int itemQuantity, string promotionType, 
        string moneyCurrency, float moneyPrice, int amountBalance)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_PurchaseWithResourcesCompleted economyGroup = " + economyGroup + " itemID = " + itemID + " promotionType = " + promotionType + 
                " moneyCurrency = " + moneyCurrency + " moneyPrice = " + moneyPrice + " amountBalance = " + amountBalance);
        }
        
        // HQ event
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

			Track_SendEvent(e);
        }

        // Game event
        e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.eco.sink");
        if (e != null)
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, moneyCurrency);
            e.SetParameterValue(TRACK_PARAM_AMOUNT_DELTA, (int)moneyPrice);
            e.SetParameterValue(TRACK_PARAM_AMOUNT_BALANCE, amountBalance);
            Track_AddParamString(e, TRACK_PARAM_ECO_GROUP, economyGroup);
            Track_AddParamString(e, TRACK_PARAM_ITEM, itemID);            

			Track_SendEvent(e);
        }

    }

    private void Track_EarnResources(string economyGroup, string moneyCurrency, int amountDelta, int amountBalance)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_EarnResources economyGroup = " + economyGroup + " moneyCurrency = " + moneyCurrency + " moneyPrice = " + amountDelta + " amountBalance = " + amountBalance);
        }
        
        // Game event
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.eco.source");
        if (e != null)
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, moneyCurrency);
            e.SetParameterValue(TRACK_PARAM_AMOUNT_DELTA, (int)amountDelta);
            e.SetParameterValue(TRACK_PARAM_AMOUNT_BALANCE, amountBalance);
            Track_AddParamString(e, TRACK_PARAM_ECO_GROUP, economyGroup);            

			Track_SendEvent(e);
        }
    }

    private void Track_CustomerSupportRequested()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_CustomerSupportRequested");
        }
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.game.cs");
        if (e != null)
        {                                    
            Track_AddParamTotalPurchases(e);
            Track_AddParamSessionsCount(e);
            Track_AddParamPlayerProgress(e);
            // Always 0 since there's no pvp in the game
            e.SetParameterValue(TRACK_PARAM_PVP_MATCHES_PLAYED, 0);            

			Track_SendEvent(e);
        }
    }

    private void Track_AdStarted(string adType, string rewardType, bool adIsAvailable, string provider = null)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_AdStarted adType = " + adType + " rewardType = " + rewardType + " adIsAvailable = " + adIsAvailable + " provider = " + provider);
        }
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.game.ad.start");
        if (e != null)
        {
            if (TrackingPersistenceSystem != null)
            {
                e.SetParameterValue(TRACK_PARAM_NB_ADS_LTD, TrackingPersistenceSystem.AdsCount);
                e.SetParameterValue(TRACK_PARAM_NB_ADS_SESSION, TrackingPersistenceSystem.AdsSessions);
            }

            Track_AddParamBool(e, TRACK_PARAM_AD_IS_AVAILABLE, adIsAvailable);
            Track_AddParamString(e, TRACK_PARAM_REWARD_TYPE, rewardType);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);
            Track_AddParamString(e, TRACK_PARAM_ADS_TYPE, adType);

			Track_SendEvent(e);
        }
    }

    private void Track_AdFinished(string adType, bool adIsLoaded, bool maxReached, int adViewingDuration, string provider)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_AdFinished adType = " + adType + " adIsLoaded = " + adIsLoaded + " maxReached = " + maxReached + 
                " adViewingDuration = " + adViewingDuration + " provider = " + provider);
        }
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.game.ad.finished");
        if (e != null)
        {            
            Track_AddParamBool(e, TRACK_PARAM_IS_LOADED, adIsLoaded);
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);            
            e.SetParameterValue(TRACK_PARAM_AD_VIEWING_DURATION, adViewingDuration);
            Track_AddParamBool(e, TRACK_PARAM_MAX_REACHED, maxReached);            
            Track_AddParamString(e, TRACK_PARAM_ADS_TYPE, adType);

			Track_SendEvent(e);
        }
    }

    private void Track_RoundStart(int dragonXp, int dragonProgression, string dragonSkin, List<string> pets)
    {
        if(FeatureSettingsManager.IsDebugEnabled)
        {
            string str = "Track_RoundStart dragonXp = " + dragonXp + " dragonProgression = " + dragonProgression + " dragonSkin = " + dragonSkin;
            if (pets != null)
            {
                int count = pets.Count;
                for (int i = 0; i < count; i++)
                {
                    str += " pet[" + i + "] = " + pets[i];
                }
            }

            Log(str);
        }
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.gameplay.start");
        if (e != null)
        {            
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            e.SetParameterValue(TRACK_PARAM_XP, dragonXp);
            e.SetParameterValue(TRACK_PARAM_DRAGON_PROGRESSION, dragonProgression);            
            Track_AddParamString(e, TRACK_PARAM_DRAGON_SKIN, dragonSkin);
            Track_AddParamPets(e, pets);            
			Track_SendEvent(e);
        }
    }

    public void Track_RoundEnd(int dragonXp, int deltaXp, int dragonProgression, int timePlayed, int score, 
        string deathType, string deathSource, string deathCoordinates, int chestsFound, int eggFound,
        float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive,
        int scGained, int hcGained)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_RoundEnd dragonXp = " + dragonXp + " deltaXp = " + deltaXp + " dragonProgression = " + dragonProgression + 
                " timePlayed = " + timePlayed + " score = " + score +
                " deathType = " + deathType + " deathSource = " + deathSource + " deathCoor = " + deathCoordinates + 
                " chestsFound = " + chestsFound + " eggFound = " + eggFound + 
                " highestMultiplier = " + highestMultiplier + " highestBaseMultiplier = " + highestBaseMultiplier + 
                " furyRushNb = " + furyRushNb + " superFireRushNb = " + superFireRushNb + " hcRevive = " + hcRevive + " adRevive = " + adRevive + 
                " scGained = " + scGained + " hcGained = " + hcGained);
        }

        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.gameplay.end");
        if (e != null)
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamRunsAmount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            e.SetParameterValue(TRACK_PARAM_XP, dragonXp);
            e.SetParameterValue(TRACK_PARAM_DELTA_XP, deltaXp);            
            e.SetParameterValue(TRACK_PARAM_DRAGON_PROGRESSION, dragonProgression);
            e.SetParameterValue(TRACK_PARAM_TIME_PLAYED, timePlayed);
            e.SetParameterValue(TRACK_PARAM_SCORE, score);
            Track_AddParamString(e, TRACK_PARAM_DEATH_TYPE, deathType);
            Track_AddParamString(e, TRACK_PARAM_DEATH_CAUSE, deathSource);
            Track_AddParamString(e, TRACK_PARAM_DEATH_COORDINATES, deathCoordinates);
            e.SetParameterValue(TRACK_PARAM_CHESTS_FOUND, chestsFound);
            e.SetParameterValue(TRACK_PARAM_EGG_FOUND, eggFound);
            e.SetParameterValue(TRACK_PARAM_HIGHEST_MULTIPLIER, highestMultiplier);
            e.SetParameterValue(TRACK_PARAM_HIGHEST_BASE_MULTIPLIER, highestBaseMultiplier);
            e.SetParameterValue(TRACK_PARAM_FIRE_RUSH_NB, furyRushNb);
            e.SetParameterValue(TRACK_PARAM_SUPER_FIRE_RUSH_NB, superFireRushNb);
            e.SetParameterValue(TRACK_PARAM_HC_REVIVE, hcRevive);
            e.SetParameterValue(TRACK_PARAM_AD_REVIVE, adRevive);
            e.SetParameterValue(TRACK_PARAM_SC_EARNED, scGained);
            e.SetParameterValue(TRACK_PARAM_HC_EARNED, hcGained);

            Track_SendEvent(e);
        }
    }

    private void Track_RunEnd(int dragonXp, int timePlayed, int score, string deathType, string deathSource, string deathCoordinates)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_RunEnd dragonXp = " + dragonXp + " timePlayed = " + timePlayed + " score = " + score + 
                " deathType = " + deathType + " deathSource = " + deathSource + " deathCoor = " + deathCoordinates);
        }
        
        TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent("custom.gameplay.dead");
        if (e != null)
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamRunsAmount(e);
            e.SetParameterValue(TRACK_PARAM_XP, dragonXp);
            e.SetParameterValue(TRACK_PARAM_TIME_PLAYED, timePlayed);
            e.SetParameterValue(TRACK_PARAM_SCORE, score);
            Track_AddParamString(e, TRACK_PARAM_DEATH_TYPE, deathType);
            Track_AddParamString(e, TRACK_PARAM_DEATH_CAUSE, deathSource);
            Track_AddParamString(e, TRACK_PARAM_DEATH_COORDINATES, deathCoordinates);

			Track_SendEvent(e);
        }
    }

	private void Track_Funnel(string _event, string _step, int _stepDuration, int _totalDuration, bool _fistLoad)
	{
		if (FeatureSettingsManager.IsDebugEnabled)
		{
			Log("Track_Funnel eventID = " + _event + " stepName = " + _step + " stepDuration = " + _stepDuration + " totalDuration = " + _totalDuration + " firstLoad = " + _fistLoad);
		}

		TrackingManager.TrackingEvent e = TrackingManager.SharedInstance.GetNewTrackingEvent(_event);
		if (e != null)
		{
			e.SetParameterValue(TRACK_PARAM_STEP_NAME, _step);
			e.SetParameterValue(TRACK_PARAM_STEP_DURATION, _stepDuration);
			e.SetParameterValue(TRACK_PARAM_TOTAL_DURATION, _totalDuration);
			e.SetParameterValue(TRACK_PARAM_FIRST_LOAD, _fistLoad);	
			
			Track_SendEvent(e);
		}
	}

    // -------------------------------------------------------------
    // Params
    // -------------------------------------------------------------    
    private const string TRACK_PARAM_AB_TESTING                 = "abtesting";
    private const string TRACK_PARAM_AD_IS_AVAILABLE            = "adIsAvailable";
    private const string TRACK_PARAM_AD_REVIVE                  = "adRevive";
    private const string TRACK_PARAM_ADS_TYPE                   = "adsType";
    private const string TRACK_PARAM_AD_VIEWING_DURATION        = "adViewingDuration";
    private const string TRACK_PARAM_AMOUNT_BALANCE             = "amountBalance";
    private const string TRACK_PARAM_AMOUNT_DELTA               = "amountDelta";                
    private const string TRACK_PARAM_CURRENCY                   = "currency";
    private const string TRACK_PARAM_CHESTS_FOUND               = "chestsFound";
    private const string TRACK_PARAM_DEATH_CAUSE                = "deathCause";
    private const string TRACK_PARAM_DEATH_COORDINATES          = "deathCoordinates";
    private const string TRACK_PARAM_DEATH_IN_CURRENT_RUN_NB    = "deathInCurrentRunNb";
    private const string TRACK_PARAM_DEATH_TYPE                 = "deathType";
    private const string TRACK_PARAM_DELTA_XP                   = "deltaXp";
    private const string TRACK_PARAM_DRAGON_PROGRESSION         = "dragonProgression";
    private const string TRACK_PARAM_DRAGON_SKIN                = "dragonSkin";
    private const string TRACK_PARAM_ECO_GROUP                  = "ecoGroup";
    private const string TRACK_PARAM_ECONOMY_GROUP              = "economyGroup";
    private const string TRACK_PARAM_EGG_FOUND                  = "eggFound";
    private const string TRACK_PARAM_FIRST_LOAD                 = "firstLoad";
    private const string TRACK_PARAM_FIRE_RUSH_NB               = "fireRushNb";
    private const string TRACK_PARAM_GAME_RUN_NB                = "gameRunNb";
    private const string TRACK_PARAM_HC_EARNED                  = "hcEarned";
    private const string TRACK_PARAM_HC_REVIVE                  = "hcRevive";
    private const string TRACK_PARAM_HIGHEST_BASE_MULTIPLIER    = "highestBaseMultiplier";
    private const string TRACK_PARAM_HIGHEST_MULTIPLIER         = "highestMultiplier";
    private const string TRACK_PARAM_HOUSTON_TRANSACTION_ID     = "houstonTransactionID";
    private const string TRACK_PARAM_IN_GAME_ID                 = "InGameId";
    private const string TRACK_PARAM_IS_LOADED                  = "isLoaded";
    private const string TRACK_PARAM_IS_PAYING_SESSION          = "isPayingSession";
    private const string TRACK_PARAM_ITEM                       = "item";
    private const string TRACK_PARAM_ITEM_ID                    = "itemID";
    private const string TRACK_PARAM_ITEM_QUANTITY              = "itemQuantity";
    private const string TRACK_PARAM_MAX_REACHED                = "maxReached";
    private const string TRACK_PARAM_MAX_XP                     = "maxXp";
    private const string TRACK_PARAM_MONEY_CURRENCY             = "moneyCurrency";
    private const string TRACK_PARAM_MONEY_IAP                  = "moneyIAP";
    private const string TRACK_PARAM_NB_ADS_LTD                 = "nbAdsLtd";
    private const string TRACK_PARAM_NB_ADS_SESSION             = "nbAdsSession";
    private const string TRACK_PARAM_PET1                       = "pet1";
    private const string TRACK_PARAM_PET2                       = "pet2";
    private const string TRACK_PARAM_PET3                       = "pet3";
    private const string TRACK_PARAM_PET4                       = "pet4";
    private const string TRACK_PARAM_PLAYER_ID                  = "playerID";
    private const string TRACK_PARAM_PLAYER_PROGRESS            = "playerProgress";
    private const string TRACK_PARAM_PROMOTION_TYPE             = "promotionType";    
    private const string TRACK_PARAM_PROVIDER                   = "provider";
    private const string TRACK_PARAM_PROVIDER_AUTH              = "providerAuth";
    private const string TRACK_PARAM_PVP_MATCHES_PLAYED         = "pvpMatchesPlayed";
    private const string TRACK_PARAM_REWARD_TYPE                = "rewardType";
    private const string TRACK_PARAM_SC_EARNED                  = "scEarned";
    private const string TRACK_PARAM_SCORE                      = "score";
    private const string TRACK_PARAM_SESSION_PLAY_TIME          = "sessionPlaytime";
    private const string TRACK_PARAM_SESSIONS_COUNT             = "sessionsCount";    
	private const string TRACK_PARAM_STEP_DURATION              = "stepDuration";
	private const string TRACK_PARAM_STEP_NAME	                = "stepName";
	private const string TRACK_PARAM_STOP_CAUSE                 = "stopCause";
    private const string TRACK_PARAM_STORE_TRANSACTION_ID       = "storeTransactionID";
    private const string TRACK_PARAM_SUBVERSION                 = "SubVersion";
    private const string TRACK_PARAM_SUPER_FIRE_RUSH_NB         = "superFireRushNb";    
    private const string TRACK_PARAM_TIME_PLAYED                = "timePlayed";
    private const string TRACK_PARAM_TOTAL_DURATION             = "totalDuration";
    private const string TRACK_PARAM_TOTAL_PLAYTIME             = "totalPlaytime";
    private const string TRACK_PARAM_TOTAL_PURCHASES            = "totalPurchases";
    private const string TRACK_PARAM_TOTAL_STORE_VISITS         = "totalStoreVisits";
    private const string TRACK_PARAM_TYPE_NOTIF                 = "typeNotif";
    private const string TRACK_PARAM_XP                         = "xp";

	private void Track_SendEvent(TrackingManager.TrackingEvent e)
	{
		// Events are not sent in UNITY_EDITOR because DNA crashes on Mac
		#if !UNITY_EDITOR
		TrackingManager.SharedInstance.SendEvent(e);
		#endif
	}

    private void Track_AddParamSubVersion(TrackingManager.TrackingEvent e)
    {
        // "SoftLaunch" is sent so far. It will be changed wto "HardLaunch" after WWL
        Track_AddParamString(e, TRACK_PARAM_SUBVERSION, "SoftLaunch");
    }

    private void Track_AddParamProviderAuth(TrackingManager.TrackingEvent e)
    {
        string value = null;
        if (TrackingPersistenceSystem != null && !string.IsNullOrEmpty(TrackingPersistenceSystem.SocialPlatform))
        {
            value = TrackingPersistenceSystem.SocialPlatform;
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
        if (TrackingPersistenceSystem != null && !string.IsNullOrEmpty(TrackingPersistenceSystem.SocialID))
        {
            value = TrackingPersistenceSystem.SocialID;
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
        if (TrackingPersistenceSystem != null)
        {
            value = TrackingPersistenceSystem.AccountID;
        }

        e.SetParameterValue(TRACK_PARAM_IN_GAME_ID, value);
    }    

    private void Track_AddParamAbTesting(TrackingManager.TrackingEvent e)
    {        
        e.SetParameterValue(TRACK_PARAM_AB_TESTING, "");
    }

    private void Track_AddParamHighestDragonXp(TrackingManager.TrackingEvent e)
    {
        int value = 0;
        if (UsersManager.currentUser != null)
        {
            DragonData highestDragon = UsersManager.currentUser.GetHighestDragon();
            if (highestDragon != null && highestDragon.progression != null)
            {
                value = (int)highestDragon.progression.xp;
            }
        }

        e.SetParameterValue(TRACK_PARAM_MAX_XP, value);
    }

    private void Track_AddParamPlayerProgress(TrackingManager.TrackingEvent e)
    {
        int value = (UsersManager.currentUser != null) ? UsersManager.currentUser.GetPlayerProgress() : 0;
        Track_AddParamString(e, TRACK_PARAM_PLAYER_PROGRESS, value + "");
    }    

    private void Track_AddParamSessionsCount(TrackingManager.TrackingEvent e)
    {
        int value = (TrackingPersistenceSystem != null) ? TrackingPersistenceSystem.SessionCount : 0;
        e.SetParameterValue(TRACK_PARAM_SESSIONS_COUNT, value);
    }

    private void Track_AddParamGameRoundCount(TrackingManager.TrackingEvent e)
    {
        int value = (TrackingPersistenceSystem != null) ? TrackingPersistenceSystem.GameRoundCount : 0;
        e.SetParameterValue(TRACK_PARAM_GAME_RUN_NB, value);
    }

    private void Track_AddParamTotalPlaytime(TrackingManager.TrackingEvent e)
    {
        int value = (TrackingPersistenceSystem != null) ? TrackingPersistenceSystem.TotalPlaytime : 0;
        e.SetParameterValue(TRACK_PARAM_TOTAL_PLAYTIME, value);
    }

    private void Track_AddParamPets(TrackingManager.TrackingEvent e, List<string> pets)
    {
        // 4 pets are currently supported
        string pet1 = null;
        string pet2 = null;
        string pet3 = null;
        string pet4 = null;
        if (pets != null)
        {
            int count = pets.Count;
            if (count > 0)
            {
                pet1 = pets[0];
            }

            if (count > 1)
            {
                pet2 = pets[1];
            }

            if (count > 2)
            {
                pet3 = pets[2];
            }

            if (count > 3)
            {
                pet4 = pets[3];
            }
        }

        Track_AddParamString(e, TRACK_PARAM_PET1, pet1);
        Track_AddParamString(e, TRACK_PARAM_PET2, pet2);
        Track_AddParamString(e, TRACK_PARAM_PET3, pet3);
        Track_AddParamString(e, TRACK_PARAM_PET4, pet4);
    }

    private void Track_AddParamTotalPurchases(TrackingManager.TrackingEvent e)
    {
        if (TrackingPersistenceSystem != null)
        {
            e.SetParameterValue(TRACK_PARAM_TOTAL_PURCHASES, TrackingPersistenceSystem.TotalPurchases);
        }        
    }   

    private void Track_AddParamRunsAmount(TrackingManager.TrackingEvent e)
    {
        if (TrackingPersistenceSystem != null)
        {
            e.SetParameterValue(TRACK_PARAM_DEATH_IN_CURRENT_RUN_NB, Session_RunsAmountInCurrentRound);
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

    private string Track_CoordinatesToString(Vector3 coord)
    {
        return "x=" + coord.x.ToString("0.0") + ", y=" + coord.y.ToString("0.0");
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
    /// This flag states whether or not the user has started any rounds. This is used to start DNA session. DNA session starts when the user 
    /// starts the first round since the application started
    /// </summary>
    private bool Session_AnyRoundsStarted { get; set; }

    /// <summary>
    /// Amount of runs played by the user in the current round so far
    /// </summary>
    private int Session_RunsAmountInCurrentRound { get; set; }

    private string Session_LastDeathType { get; set; }
    private string Session_LastDeathSource { get; set; }
    private string Session_LastDeathCoordinates { get; set; }

    /// <summary>
    /// Whether or not this is the first session since installation. It's required as a parameter for some events.
    /// </summary>
    private bool Session_IsFirstTime { get; set; }

    private void Session_Reset()
    {
        Session_IsPayingSession = false;
        Session_IsAdSession = false;
        Session_PlayTime = 0f;        
        Session_AnyRoundsStarted = false;
        Session_RunsAmountInCurrentRound = 0;
        Session_LastDeathType = null;
        Session_LastDeathSource = null;
        Session_LastDeathCoordinates = null;
        Session_IsFirstTime = false;
     }
#endregion

#region debug
    private const bool Debug_IsEnabled = true;

    private void Debug_Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            //Notify_RoundStart(0, 0, null, null);
            Debug.Log("gamRoundCount = " + TrackingPersistenceSystem.GameRoundCount + " Session_PlayTime = " + Session_PlayTime);
        }        
    }
#endregion
}

