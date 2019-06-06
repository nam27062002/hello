/// <summary>
/// This class is responsible to handle any Hungry Dragon related stuff needed for tracking. It uses Calety Tracking support to send tracking events
/// </summary>

#if UNITY_EDITOR
//Comment to allow event debugging in windows. WARNING! this code doesn't work in Mac
//#define EDITOR_MODE
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Calety.Tracking;

public class HDTrackingManagerImp : HDTrackingManager {
    private enum EState {
        None,
        WaitingForSessionStart,
        SessionStarting,
        SessionStarted,
        Banned
    }


    //---[Event storage]------------------------------------------------------//
    private class HDTrackingEvent {
        public string name;
        public Dictionary<string, object> data;

        public HDTrackingEvent(string _name) {
            name = _name;
            data = new Dictionary<string, object>();
        }

        /// <summary>
        /// Method to do some stuff when we're sure the event has been sent
        /// </summary>
        public void OnSent() {            
        }
    }
    private Queue<HDTrackingEvent> m_eventQueue = new Queue<HDTrackingEvent>();
    //------------------------------------------------------------------------//


    // Load funnel events are tracked by two different apis (Calety and Razolytics).
    private FunnelData_Load m_loadFunnelCalety;
    private FunnelData_LoadRazolytics m_loadFunnelRazolytics;

    private FunnelData_FirstUX m_firstUXFunnel;

    private EState State { get; set; }

    private bool IsStartSessionNotified { get; set; }

    private bool AreSDKsInitialised { get; set; }

    private enum EPlayingMode {
        NONE,
        TUTORIAL,
        PVE,
        SETTINGS
    };

    private EPlayingMode m_playingMode = EPlayingMode.NONE;
    private float m_playingModeStartTime;

    private const bool SESSION_RETRIES_ENABLED = true;

    public HDTrackingManagerImp() {
        m_loadFunnelCalety = new FunnelData_Load();
        m_loadFunnelRazolytics = new FunnelData_LoadRazolytics();
        m_firstUXFunnel = new FunnelData_FirstUX();
    }

    private const float BYTES_TO_MB = 1f / (1024 * 1024);
    private const float BYTES_TO_KB = 1f / 1024;

    private float GetSizeInMb(float sizeInBytes)
    {
        return sizeInBytes * BYTES_TO_MB;
    }

    private float GetSizeInKb(float sizeInBytes)
    {
        return sizeInBytes * BYTES_TO_KB;
    }

    public override void Init() {
        base.Init();

        Reset();

		// We need to track all events that have to be sent right after the session is created. We need to do it here in order to make sure they will be tracked at the very beginning
		// The session will be started later on because we need to wait for persistence to be loaded (since it may contain the trackind id required to start the session) and some
		// events may be reported before the persistence is loaded
		Track_StartSessionEvent();

        Messenger.AddListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
        Messenger.AddListener<string>(MessengerEvents.PURCHASE_ERROR, OnPurchaseFailed);
        Messenger.AddListener<string>(MessengerEvents.PURCHASE_FAILED, OnPurchaseFailed);
        Messenger.AddListener<string>(MessengerEvents.PURCHASE_CANCELLED, OnPurchaseFailed);
        Messenger.AddListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
    }

    protected override void Reset()
    {
        base.Reset();

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

        Performance_Reset();       
        m_loadFunnelCalety.Reset();
        m_loadFunnelRazolytics.Reset();
        m_firstUXFunnel.Reset();
    }

    public override void Destroy() {
        FlushEventQueue();

        base.Destroy();
        Messenger.RemoveListener<string, string, SimpleJSON.JSONNode>(MessengerEvents.PURCHASE_SUCCESSFUL, OnPurchaseSuccessful);
        Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_ERROR, OnPurchaseFailed);
        Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_FAILED, OnPurchaseFailed);
        Messenger.RemoveListener<string>(MessengerEvents.PURCHASE_CANCELLED, OnPurchaseFailed);
        Messenger.RemoveListener<bool>(MessengerEvents.LOGGED, OnLoggedIn);
        Reset();
    }

    public override string GetTrackingID() {
        string returnValue = null;
        if (TrackingPersistenceSystem != null) {
            returnValue = TrackingPersistenceSystem.UserID;
        }

        return returnValue;
    }

    public override string GetDNAProfileID() {
#if !EDITOR_MODE
        return DNAManager.SharedInstance.GetProfileID();
#else
        return null;
#endif
    }

    public override void GoToGame() {
        // Unsent events are stored during the loading because it can be a heavy stuff
        SaveOfflineUnsentEvents();

        if (SESSION_RETRIES_ENABLED) {
            // Session is not allowed to be recreated during game because it could slow it down
            SetRetrySessionCreationIsEnabled(false);
        }
    }

    public override void GoToMenu() {
        // Unsent events are stored during the loading because it can be a heavy stuff
        SaveOfflineUnsentEvents();

        if (SESSION_RETRIES_ENABLED) {
            SetRetrySessionCreationIsEnabled(true);
        }
    }

    private void SetRetrySessionCreationIsEnabled(bool value) {
        // UbiservicesManager is not called from the editor because it doesn’t work on Mac
#if !EDITOR_MODE
		UbiservicesManager.SharedInstance.SetStartSessionRetryBehaviour(value);
#endif
    }

    private bool IsSaveOfflineUnsentEventsAllowed {
        get {
#if EDITOR_MODE
            // Disabled in Editor because it causes a crash on Mac
            return false;
#else
            return FeatureSettingsManager.instance.IsTrackingOfflineCachedEnabled;
#endif
        }
    }

    protected override void SaveOfflineUnsentEventsExtended() {
        if (IsSaveOfflineUnsentEventsAllowed) {
            // We need to send all events enqueued so they can be either sent or stored if they couldn't be sent
            FlushEventQueue();
            DNAManager.SharedInstance.SaveOfflineUnsentEvents();
        }
    }

    private void OnPurchaseSuccessful(string _sku, string _storeTransactionID, SimpleJSON.JSONNode _receipt) {
        StoreManager.StoreProduct product = GameStoreManager.SharedInstance.GetStoreProduct(_sku);
        string moneyCurrencyCode = null;
        float moneyPrice = 0f;
        if (product != null) {
            moneyCurrencyCode = product.m_strCurrencyCode;
            moneyPrice = product.m_fLocalisedPriceValue;
        }

        int moneyUSD = 0;
        bool isSpecialOffer = false;
        string promotionType = null;
        if (!string.IsNullOrEmpty(_sku)) {
            DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, _sku);
            if (def != null) {
                moneyUSD = Convert.ToInt32(def.GetAsFloat("price") * 100f);
                isSpecialOffer = def.GetAsString("type", "").Equals("offer");
                promotionType = def.GetAsString("promotionType");
            }
        }

        // store transaction ID is also used for houston transaction ID, which is what Migh&Magic game also does
        string houstonTransactionID = _storeTransactionID;        
        Notify_IAPCompleted(_storeTransactionID, houstonTransactionID, _sku, promotionType, moneyCurrencyCode, moneyPrice, moneyUSD, isSpecialOffer);

        Session_IsNotifyOnPauseEnabled = true;
    }

    private void OnPurchaseFailed(string _sku) {
        Session_IsNotifyOnPauseEnabled = true;
    }

    private void OnLoggedIn(bool logged) {
        if (logged) {
            // Server uid is stored as soon as log in happens so we'll be able to start TrackingManager when offline
            PersistencePrefs.ServerUserId = GameSessionManager.SharedInstance.GetUID();
            if (TrackingPersistenceSystem != null) {
                TrackingPersistenceSystem.ServerUserID = PersistencePrefs.ServerUserId;
            }
        }

        // We need to reinitialize TrackingManager if it has already been initialized, otherwise we simply do nothing since it will be initialize properly
        if (IsStartSessionNotified) {
            //InitTrackingManager();
        }
    }

    private void CheckAndGenerateUserID() {
        if (TrackingPersistenceSystem != null) {
            // Generate Analytics user ID if not already set, it cannot be done in init function as we don't know the user ID at that point
            if (string.IsNullOrEmpty(TrackingPersistenceSystem.UserID)) {
                // Generate a GUID so that we can identify users over the course of firing multiple events etc.
                TrackingPersistenceSystem.UserID = System.Guid.NewGuid().ToString();

                if (FeatureSettingsManager.IsDebugEnabled) {
                    Log("Generate User ID = " + TrackingPersistenceSystem.UserID);
                }
            }
        }
    }

    private void StartSession() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("StartSession");
        }

        State = EState.SessionStarting;

        CheckAndGenerateUserID();

        Session_IsFirstTime = TrackingPersistenceSystem.IsFirstLoading;

        // It has to be true only in the first loading
        if (Session_IsFirstTime) {
            TrackingPersistenceSystem.IsFirstLoading = false;
        }

        // Session counter advanced
        TrackingPersistenceSystem.SessionCount++;

        // Calety needs to be initialized every time a session starts because the session count has changed
        InitTrackingManager();

        InitSDKs();

        //-------------------------------
        // Start Tracking manager
        // Sends the start session event
        Track_GameStart();

		// We need to wait until this method is called to send this event because it has a parameter that needs persistence to be loaded
		Track_MobileStartEvent();

		HDTrackingEvent e = new HDTrackingEvent("custom.session.started");
		{
			string fullClientVersion = GameSettings.internalVersion.ToString() + "." + ServerManager.SharedInstance.GetRevisionVersion();
			Track_AddParamString(e, TRACK_PARAM_VERSION_REVISION, fullClientVersion);
		}
		m_eventQueue.Enqueue(e);
        //-------------------------------

        if (Session_IsFirstTime) {
            Track_StartPlayingMode(EPlayingMode.TUTORIAL);
        }

        // We need to wait for the session to be started to send the first Calety funnel step
        Notify_Calety_Funnel_Load(FunnelData_Load.Steps._02_persistance);
    }

    private void InitSDKs() {
        if (!AreSDKsInitialised) {
            CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
            InitDNA(settingsInstance);
            InitAppsFlyer(settingsInstance);            

            AreSDKsInitialised = true;
        }
    }

    private void InitDNA(CaletySettings settingsInstance) {
        // DNA is not initialized in editor because it doesn't work on Windows and it crashes on Mac
#if !EDITOR_MODE
        string clientVersion = GameSettings.internalVersion.ToString();

        if (!SESSION_RETRIES_ENABLED)
        {
            SetRetrySessionCreationIsEnabled(false);
        }

		if (settingsInstance != null)
		{
		string strDNAGameVersion = "UAT";
		if (settingsInstance.m_iBuildEnvironmentSelected == (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION)
		{
		    strDNAGameVersion = "Full";
            clientVersion += "_PROD";
		}

		List<string> kEventNameFilters = new List<string> ();
		kEventNameFilters.Add ("custom");

		List<string> kDNACachedEventIDs = TrackingManager.SharedInstance.GetEventIDsByAPI (ETrackAPIs.E_TRACK_API_DNA, kEventNameFilters);

#if UNITY_ANDROID
		DNAManager.SharedInstance.Initialise("12e4048c-5698-4e1e-a1d1-c8c2411b2515", clientVersion, strDNAGameVersion, kDNACachedEventIDs);
#elif UNITY_IOS
		DNAManager.SharedInstance.Initialise ("42cbdf99-63e7-4e80-aae3-d05b9533349e", clientVersion, strDNAGameVersion, kDNACachedEventIDs);
#endif
		}
#endif
    }

    private void InitAppsFlyer(CaletySettings settingsInstance) {
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

    private void InitTrackingManager() {
        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
        if (settingsInstance != null) {
            int sessionNumber = TrackingPersistenceSystem.SessionCount;
            string trackingID = TrackingPersistenceSystem.UserID;
            string userID = PersistencePrefs.ServerUserId;
            //ETrackPlatform trackPlatform = (GameSessionManager.SharedInstance.IsLogged()) ? ETrackPlatform.E_TRACK_PLATFORM_ONLINE : ETrackPlatform.E_TRACK_PLATFORM_OFFLINE;
            ETrackPlatform trackPlatform = ETrackPlatform.E_TRACK_PLATFORM_ONLINE;
            //ETrackPlatform trackPlatform = ETrackPlatform.E_TRACK_PLATFORM_OFFLINE;

            if (FeatureSettingsManager.IsDebugEnabled) {
                Log("SessionNumber = " + sessionNumber + " trackingID = " + trackingID + " userId = " + userID + " trackPlatform = " + trackPlatform);
            }

            Session_BuildVersion = settingsInstance.GetClientBuildVersion();

            TrackingConfig kTrackingConfig = new TrackingConfig();
            kTrackingConfig.m_eTrackPlatform = trackPlatform;
            kTrackingConfig.m_strJSONConfigFilePath = "Tracking/TrackingEvents";
            kTrackingConfig.m_strStartSessionEventName = "custom.etl.session.start";
            kTrackingConfig.m_strEndSessionEventName = "custom.etl.session.end";
            kTrackingConfig.m_strMergeAccountEventName = "MERGE_ACCOUNTS";
            kTrackingConfig.m_strClientVersion = Session_BuildVersion;
            kTrackingConfig.m_strTrackingID = trackingID;
            kTrackingConfig.m_strUserIDOptional = userID;
            kTrackingConfig.m_iSessionNumber = sessionNumber;
            kTrackingConfig.m_iMaxCachedLoggedDays = 3;

            TrackingManager.SharedInstance.Initialise(kTrackingConfig);

            // This needs to be done here because TrackingManager.Initialise() adds a listener to GDPRManager that we need to be called so events will be tracked on 
            // Appsflyer and Facebook Analytics. At this point of the flow we're sure that the user has already seed consent popup so this is a safe place to do this
            GDPRManager.SharedInstance.ProceedWithGDPRDependantAPIs();
        }
    }    

    public override void Update() {
        switch (State) {
            case EState.WaitingForSessionStart:
            if (TrackingPersistenceSystem != null && IsStartSessionNotified) {
                // We need to start session here in Update() so GameCenterManager has time to get the acq_marketing_id, otherwise
                // that field will be empty in "custom.player.info" event
                StartSession();
            }
            break;

            case EState.SessionStarting:
#if UNITY_EDITOR
            if (Time.realtimeSinceStartup > 5f)
#else
            if (UbiservicesManager.SharedInstance.IsSessionCreated())
            //if (UbiservicesManager.SharedInstance.GetUbiServicesFacade() != null)
#endif
            {
                State = EState.SessionStarted;

				// CP2 needs session in ubiservices to be created before being initialised
				HDCP2Manager.Instance.Initialise();
            }
            break;

            case EState.SessionStarted:
            FlushEventQueue();
            break;
        }

        if (TrackingPersistenceSystem != null && TrackingPersistenceSystem.IsDirty) {
            TrackingPersistenceSystem.IsDirty = false;
            PersistenceFacade.instance.Save_Request(false);
        }

        Session_PlayTime += Time.deltaTime;

#if EDITOR_MODE
        Debug_Update();
#endif

        if (Performance_IsTrackingEnabled) {
            Performance_Tracker();
        }

    }

    private void FlushEventQueue() {
        // Makes sure that events can be sent (game.start events has already been sent and Ubiservices session has been created)
        if (Session_GameStartSent && State == EState.SessionStarted) {
            HDTrackingEvent e;
            while (m_eventQueue.Count > 0) {
                e = m_eventQueue.Dequeue();
                e.OnSent();
                Track_SendEvent(e);
            }
        }
    }

    #region notify
    private bool Notify_MeetsEventRequirements(string e) {
        bool returnValue = false;
        switch (e) {
            case TRACK_EVENT_TUTORIAL_COMPLETION:
            // We need to check whether or not the event has already been sent because TrackingPersistenceSystem.GameRoundCount is advanced when a run starts
            // but this condition is checked when the run finishes so the event is still sent even though a crash happened between the run start and the run end
            returnValue = TrackingPersistenceSystem.GameRoundCount >= 2 && !Track_HasEventBeenSent(e);
            break;

            case TRACK_EVENT_FIRST_10_RUNS_COMPLETED:
            // We need to check whether or not the event has already been sent because TrackingPersistenceSystem.GameRoundCount is advanced when a run starts
            // but this condition is checked when the run finishes so the event is still sent even though a crash happened between the run start and the run end
            returnValue = TrackingPersistenceSystem.GameRoundCount >= 10 && !Track_HasEventBeenSent(e);
            break;
        }

        return returnValue;
    }

    private void Notify_ProcessEvent(string e) {
        switch (e) {
            case TRACK_EVENT_TUTORIAL_COMPLETION:
            // tutorial completion
            Track_TutorialCompletion();
            TrackingPersistenceSystem.NotifyEventSent(e);
            break;

            case TRACK_EVENT_FIRST_10_RUNS_COMPLETED:
            // first 10 runs completed
            Track_First10RunsCompleted();
            TrackingPersistenceSystem.NotifyEventSent(e);
            break;
        }
    }

    private void Notify_CheckAndProcessEvent(string e) {
        if (Notify_MeetsEventRequirements(e)) {
            Notify_ProcessEvent(e);
        }
    }

    public override void Notify_ApplicationStart() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_StartSession");
        }

        if (State == EState.WaitingForSessionStart) {
            IsStartSessionNotified = true;
        }
    }

    public override void Notify_ApplicationEnd() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_ApplicationEnd");
        }

        Notify_SessionEnd(ESeassionEndReason.app_closed);
        Track_EtlEndEvent();

        // We need to make sure all events enqueued are sent to Calety TrackingManager in order to give them a chance to either be sent or stored if they couldn't be sent
        FlushEventQueue();

        // Last chance to cache pending events to be sent are stored
        // Not lazy approach is used to guarantee events are stored
        DNAManager.SharedInstance.SaveOfflineUnsentEvents(false);


        IsStartSessionNotified = false;
        State = EState.WaitingForSessionStart;
    }

    public override void Notify_ApplicationPaused() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_ApplicationPaused Session_IsNotifyOnPauseEnabled = " + Session_IsNotifyOnPauseEnabled + " State = " + State);
        }

        if (State == EState.SessionStarted) {
            if (Session_IsNotifyOnPauseEnabled) {
                Notify_SessionEnd(ESeassionEndReason.no_activity);
            }

            Track_EtlEndEvent();
        }

        // We need to make sure all events enqueued are sent to Calety TrackingManager in order to give them a chance to be sent before the game is sent to background, just in case the user doesn't resume it
        FlushEventQueue();
    }

    public override void Notify_ApplicationResumed() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_ApplicationResumed Session_IsNotifyOnPauseEnabled = " + Session_IsNotifyOnPauseEnabled);
        }

        if (State == EState.SessionStarted) {
            Track_EtlStartEvent();

            if (Session_IsNotifyOnPauseEnabled) {
                Track_MobileStartEvent();
            }
        }

        mSession_IsNotifyOnPauseEnabled = true;
    }

    private void Notify_SessionEnd(ESeassionEndReason reason) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            string str = "Notify_SessionEnd reason = " + reason + " sessionPlayTime = " + Session_PlayTime;
            if (TrackingPersistenceSystem != null) {
                str += " totalPlayTime = " + TrackingPersistenceSystem.TotalPlaytime;
            }

            Log(str);
        }

        if (m_playingMode != EPlayingMode.NONE) {
            Track_EndPlayingMode(false);
        }

        if (TrackingPersistenceSystem != null) {
            // Current session play time is added up to the total
            int sessionTime = (int)Session_PlayTime;
            TrackingPersistenceSystem.TotalPlaytime += sessionTime;
        }

        Track_ApplicationEndEvent(reason.ToString());

        // It needs to be reseted after tracking the event because the end application event needs to send the session play time
        Session_PlayTime = 0f;
    }

    private const string MARKETING_ID_NOT_AVAILABLE = "NotAvailable";

    public override void Notify_MarketingID(EMarketingIdFrom from) {
        // Gets marketing id. It tries to get it from prefs first because it's immediate. If it's empty then it tries to get it from the device
        string marketingId = PersistencePrefs.GetMarketingId();
        if (string.IsNullOrEmpty(marketingId)) {
            marketingId = GameSessionManager.SharedInstance.GetDeviceMarketingID();

            if (string.IsNullOrEmpty(marketingId)) {
                marketingId = MARKETING_ID_NOT_AVAILABLE;
            } else {
                // Marketing id is stored in prefs once retrieved successfully from device in order to be able to use it immediately next time it's required
                // since retrieving it from device may take a while
                PersistencePrefs.SetMarketingId(marketingId);
            }
        }

        // Specification has changed and now marketing id has to be sent only from first loading and in this case it has to be notified only 
        // when no marketing id has been notified yet or first time a valid marketing id is retrieved
        if (from == EMarketingIdFrom.FirstLoading) {
            string latestIdNotified = PersistencePrefs.GetLatestMarketingIdNotified();
            bool needsToNotify = string.IsNullOrEmpty(latestIdNotified) ||
                     (latestIdNotified == MARKETING_ID_NOT_AVAILABLE && latestIdNotified != marketingId);
             if ( needsToNotify )
             {
                Track_MarketingID(marketingId);         
             }
            

            if (FeatureSettingsManager.IsDebugEnabled) {
                Log("Notify_MarketingID id = " + marketingId + " needsToNotify = " + needsToNotify + " from = " + from + " latestIdNotified = " + PersistencePrefs.GetLatestMarketingIdNotified());
            }
        }                       
    }

    /// <summary>
    /// Called when the user starts a round
    /// </summary>
    public override void Notify_RoundStart(int dragonXp, int dragonProgression, string dragonSkin, List<string> pets) {
        // Resets the amount of runs in the current round because a new round has just started
        Session_RunsAmountInCurrentRound = 0;
        Session_HungryLettersCount = 0;

        // One more game round
        TrackingPersistenceSystem.GameRoundCount++;

        if (m_playingMode == EPlayingMode.NONE) {
            Track_StartPlayingMode(EPlayingMode.PVE);
        }

        // Notifies that one more round has started
        Track_RoundStart(dragonXp, dragonProgression, dragonSkin, pets);

        Session_NotifyRoundStart();
        //        Notify_StartPerformanceTracker();
    }

    public override void Notify_RoundEnd(int dragonXp, int deltaXp, int dragonProgression, int timePlayed, int score, int chestsFound, int eggFound,
        float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive, int scGained, int hcGained, float boostTime, int mapUsage) {
        Notify_CheckAndProcessEvent(TRACK_EVENT_TUTORIAL_COMPLETION);
        Notify_CheckAndProcessEvent(TRACK_EVENT_FIRST_10_RUNS_COMPLETED);

        if (m_playingMode == EPlayingMode.PVE) {
            Track_EndPlayingMode(true);
        }

        if (TrackingPersistenceSystem != null) {
            TrackingPersistenceSystem.EggsFound += eggFound;
        }

        // Last deathType, deathSource and deathCoordinates are used since this information is provided when Notify_RunEnd() is called
        Track_RoundEnd(dragonXp, deltaXp, dragonProgression, timePlayed, score, Session_LastDeathType, Session_LastDeathSource, Session_LastDeathCoordinates,
            chestsFound, eggFound, highestMultiplier, highestBaseMultiplier, furyRushNb, superFireRushNb, hcRevive, adRevive, scGained, hcGained, (int)(boostTime * 1000.0f), mapUsage);

        Session_NotifyRoundEnd();
    }

    public override void Notify_RunEnd(int dragonXp, int timePlayed, int score, string deathType, string deathSource, Vector3 deathCoordinates) {
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
    public override void Notify_StoreVisited( string origin ) {
        if (TrackingPersistenceSystem != null) {
            TrackingPersistenceSystem.TotalStoreVisits++;
        }
        Track_OpenShop( origin );
    }
    
    public override void Notify_StoreSection( string section) {
        Track_ShopSection( section );
    }
    
    public override void Notify_StoreItemView( string id) {
        Track_ShopItemView( id );
    }

    public override void Notify_IAPStarted() {
        // The app is paused when the iap popup is shown. According to BI session closed event shouldn't be sent when the app is paused to perform an iap and
        // session started event shouldn't be sent when the app is resumed once the iap is completed
        Session_IsNotifyOnPauseEnabled = false;
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
    /// <param name="moneyUSD">Price paid by the user in cents of dollar</param>
    /// <param name="isOffer"><c>true</c> if it's an offer. <c>false</c> otherwise</param>
    public override void Notify_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice, int moneyUSD, bool isOffer) {
        Session_IsPayingSession = true;

        if (TrackingPersistenceSystem != null) {
            TrackingPersistenceSystem.TotalPurchases++;

            // first purchase
            if (TrackingPersistenceSystem.TotalPurchases == 1) {
                Track_FirstPurchase();
            }

            TrackingPersistenceSystem.TotalSpent += moneyUSD;

            TrackingPersistenceSystem.LastPurchaseTimestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() / 1000L;	// Millis to Seconds
        }

        Track_IAPCompleted(storeTransactionID, houstonTransactionID, itemID, promotionType, moneyCurrencyCode, moneyPrice, moneyUSD, isOffer);
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
        UserProfile.Currency moneyCurrency, int moneyPrice, int amountBalance) {
        if (economyGroup == EEconomyGroup.BUY_EGG) {
            if (TrackingPersistenceSystem != null) {
                TrackingPersistenceSystem.EggPurchases++;

                if (moneyCurrency == UserProfile.Currency.HARD) {
                    TrackingPersistenceSystem.EggSPurchasedWithHC++;
                }

                if (TrackingPersistenceSystem.EggPurchases == 1) {
                    // 1 egg bought
                    Track_1EggBought();
                } else if (TrackingPersistenceSystem.EggPurchases == 5) {
                    // 5 eggs bought
                    Track_5EggBought();
                }
            }
        }

        Track_PurchaseWithResourcesCompleted(EconomyGroupToString(economyGroup), itemID, 1, promotionType, moneyCurrency, moneyPrice, amountBalance);
    }

    /// <summary>
    /// Called when the user earned some resources
    /// </summary>
    /// <param name="economyGroup">ID used to identify the type of item the user has earned. Example UNLOCK_DRAGON</param>
    /// <param name="moneyCurrencyCode">Currency type earned</param>
    /// <param name="amountDelta">Amount of the currency earned</param>
    /// <param name="amountBalance">Amount of this currency after the transaction was performed</param>
    /// <param name="_paid">The user recieved this by paying
    public override void Notify_EarnResources(EEconomyGroup economyGroup, UserProfile.Currency moneyCurrencyCode, int amountDelta, int amountBalance, bool paid) {
        // All currencies earned during a round should be collected so a single event with the accumulated amount is sent at the end of the round in order to avoid spamming tracking
        if (economyGroup == EEconomyGroup.REWARD_RUN && Session_IsARoundRunning) {
            Session_AccumRewardInRun(moneyCurrencyCode, amountDelta, paid);
        } else {
            Track_EarnResources(EconomyGroupToString(economyGroup), moneyCurrencyCode, amountDelta, amountBalance, paid);
        }
    }

    /// <summary>
    /// Called when the user clicks on the button to request a customer support ticked
    /// </summary>
    public override void Notify_CustomerSupportRequested() {
        Track_CustomerSupportRequested();
    }

    /// <summary>
    /// Called when an ad has been requested by the user.
    /// <param name="adType">Ad Type.</param>
    /// <param name="rewardType">Type of reward given for watching the ad.</param>
    /// <param name="adIsAvailable"><c>true</c>c> if the ad is available, <c>false</c> otherwise.</param>
    /// <param name="provider">Ad Provider. Optional.</param>
    /// </summary>
    public override void Notify_AdStarted(string adType, string rewardType, bool adIsAvailable, string provider = null) {
        Track_AdStarted(adType, rewardType, adIsAvailable, provider);

        // The app is paused when an ad is played. According to BI session closed event shouldn't be sent when the app is paused to play an ad and
        // session started event shouldn't be sent when the app is resumed once the ad is over
        Session_IsNotifyOnPauseEnabled = false;
    }

    /// <summary>
    /// Called then the ad requested by the user has finished
    /// <param name="adType">Ad Type.</param>
    /// <param name="adIsLoaded"><c>true</c>c> if the ad was effectively viewed, <c>false</c> otherwise.</param>
    /// <param name="maxReached"><c>true</c> if the user has reached the limit of ad viewing authorized by the app. Used for reward ads</param>
    /// <param name="adViewingDuration">Duration in seconds of the ad viewing.</param>
    /// <param name="provider">Ad Provider. Optional.</param>
    /// </summary>
    public override void Notify_AdFinished(string adType, bool adIsLoaded, bool maxReached, int adViewingDuration = 0, string provider = null) {
        if (adIsLoaded && TrackingPersistenceSystem != null) {
            TrackingPersistenceSystem.AdsCount++;

            if (!Session_IsAdSession) {
                Session_IsAdSession = true;
                TrackingPersistenceSystem.AdsSessions++;
            }

            if (TrackingPersistenceSystem.AdsCount == 1) {
                // first ad shown
                Track_FirstAdShown();
            }
        }

        Track_AdFinished(adType, adIsLoaded, maxReached, adViewingDuration, provider);

        Session_IsNotifyOnPauseEnabled = true;
    }

    public override void Notify_MenuLoaded() {
        if (!Session_HasMenuEverLoaded) {
            Session_HasMenuEverLoaded = true;
            HDTrackingManager.Instance.Notify_Razolytics_Funnel_Load(FunnelData_LoadRazolytics.Steps._02_game_loaded);
            HDTrackingManager.Instance.Notify_Calety_Funnel_Load(FunnelData_Load.Steps._03_game_loaded);

            HDTrackingManager.Instance.Notify_DeviceStats();
        }
    }

    /// <summary>
    /// The game has reached a step in the loading funnel.
    /// </summary>
    /// <param name="_step">Step to notify.</param>
    public override void Notify_Calety_Funnel_Load(FunnelData_Load.Steps _step) {
        // Calety funnel, unlike Razolytics funnel, sends all steps for all devices even for those that are not supported by the game. This is done because we can filter out those devices when  checking
        // the loading funnel on DNA
        Track_Funnel(m_loadFunnelCalety.name, m_loadFunnelCalety.GetStepName(_step), m_loadFunnelCalety.GetStepDuration(_step), m_loadFunnelCalety.GetStepTotalTime(_step), Session_IsFirstTime);
    }

    public override void Notify_Razolytics_Funnel_Load(FunnelData_LoadRazolytics.Steps _step) {
        // Makes sure that the device is fully supported by the game. If we didn't do this then the last funnel step would never be sent because that step is sent when the main menu is loaded. This'd be misleading
        // because it could make us think there's a crash when loading the game because we can't filter the unsupported devices out in Razolytics analytics
        if (FeatureSettingsManager.instance.Device_IsSupported() && !FeatureSettingsManager.instance.Device_SupportedWarning()) {
            int _sessionsCount = (TrackingPersistenceSystem == null) ? 0 : TrackingPersistenceSystem.SessionCount;
            string _stepName = m_loadFunnelRazolytics.GetStepName(_step);
            int _stepDuration = m_loadFunnelRazolytics.GetStepDuration(_step);

            GameServerManager.SharedInstance.SendTrackLoading(m_loadFunnelRazolytics.GetStepName(_step), _stepDuration, Session_IsFirstTime, _sessionsCount, null);

            if (FeatureSettingsManager.IsDebugEnabled)
                Log("Notify_Razolytics_Funnel_Load " + _stepName + " duration = " + _stepDuration + " isFirstTime = " + Session_IsFirstTime + " sessionsCount = " + _sessionsCount);
        }
    }

    /// <summary>
    /// The game has reached a step in the firts user experience funnel.
    /// </summary>
    /// <param name="_step">Step to notify.</param>
    public override void Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps _step) {
        // This step has to be sent only within the first session
        if (TrackingPersistenceSystem.SessionCount == 1) {
            Log("FTUX Funnel - step: " + m_firstUXFunnel.GetStepName(_step) + ", duration: " + m_firstUXFunnel.GetStepDuration(_step));
            Track_Funnel(m_firstUXFunnel.name, m_firstUXFunnel.GetStepName(_step), m_firstUXFunnel.GetStepDuration(_step), m_firstUXFunnel.GetStepTotalTime(_step), Session_IsFirstTime);
        }

        if (_step == FunnelData_FirstUX.Steps.Count - 1 && m_playingMode == EPlayingMode.TUTORIAL) {
            Track_EndPlayingMode(true);
        }
    }

    public override void Notify_SocialAuthentication() {
        // This event has to be send only once per user
        if (TrackingPersistenceSystem != null && !TrackingPersistenceSystem.SocialAuthSent) {
            Action<SocialUtils.ProfileInfo> onDone = delegate (SocialUtils.ProfileInfo info) {
                if (info != null) {
                    string provider = SocialPlatformManager.SharedInstance.GetPlatformName();
                    string gender = info.Gender;
                    int birthday = info.YearOfBirth;
                    TrackingPersistenceSystem.SocialAuthSent = true;
                    Track_SocialAuthentication(provider, birthday, gender);
                }
            };

            SocialPlatformManager.SharedInstance.GetProfileInfo(onDone);
        }
    }

    public override void Notify_ConsentPopupDisplay(bool _sourceSettings) {
        Track_ConsentPopupDisplay((_sourceSettings) ? "Settings_Page" : "Homepage");
    }

    public override void Notify_ConsentPopupAccept(int _age, bool _enableAnalytics, bool _enableMarketing, string _modVersion, int _duration) {
        Track_ConsentPopupAccept(_age, _enableAnalytics, _enableMarketing, _modVersion, _duration);
    }

    public override void Notify_Pet(string _sku, string _source) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_Pet " + _sku + " from " + _source);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.player.pet");
        {
            string rarity = null;
            string category = null;
            if (!string.IsNullOrEmpty(_sku)) {
                DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _sku);
                if (petDef != null) {
                    rarity = petDef.Get("rarity");
                    category = petDef.Get("category");
                }
            }

            string trackingName = Translate_PetSkuToTrackingName(_sku);
            Track_AddParamString(e, TRACK_PARAM_PETNAME, trackingName);
            Track_AddParamString(e, TRACK_PARAM_SOURCE_OF_PET, _source);
            Track_AddParamString(e, TRACK_PARAM_RARITY, rarity);
            Track_AddParamString(e, TRACK_PARAM_CATEGORY, category);
            Track_AddParamEggsPurchasedWithHC(e);
            Track_AddParamEggsFound(e);
            Track_AddParamEggsOpened(e);
        }
        m_eventQueue.Enqueue(e);
    }

    public override void Notify_DragonUnlocked(string dragon_sku, int order) {
        // Track af_X_dragon_unlocked where X is the dragon level (dragon level is order + 1). Only dragon levels between 2 to 7 have to be tracked
        if (order >= 1 && order <= 6) {
            Track_DragonUnlocked(order + 1);
        }
    }

    public override void Notify_LoadingGameplayStart() {
        // TODO: Track
    }

    public override void Notify_LoadingGameplayEnd(float loading_duration) {
        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.loadGameplay");
        {
            e.data.Add(TRACK_PARAM_LOADING_TIME, (int)(loading_duration * 1000.0f));
        }
        m_eventQueue.Enqueue(e);
    }

    public override void Notify_LoadingAreaStart(string original_area, string destination_area) {
        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.loadArea");
        {
            Track_AddParamString(e, TRACK_PARAM_ORIGINAL_AREA, original_area);
            Track_AddParamString(e, TRACK_PARAM_NEW_AREA, destination_area);
            Track_AddParamString(e, TRACK_PARAM_ACTION, "started");
            e.data.Add(TRACK_PARAM_LOADING_TIME, 0);
        }
        m_eventQueue.Enqueue(e);
    }

    public override void Notify_LoadingAreaEnd(string original_area, string destination_area, float area_loading_duration) {
        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.loadArea");
        {
            Track_AddParamString(e, TRACK_PARAM_ORIGINAL_AREA, original_area);
            Track_AddParamString(e, TRACK_PARAM_NEW_AREA, destination_area);
            Track_AddParamString(e, TRACK_PARAM_ACTION, "finished");
            e.data.Add(TRACK_PARAM_LOADING_TIME, (int)(area_loading_duration * 1000.0f));
        }
        m_eventQueue.Enqueue(e);
    }

    /// <summary>
    /// The player has opened an info popup.
    /// </summary>
    /// <param name="_popupName">Name of the opened popup. Prefab name.</param>
    /// <param name="_action">How was this popup opened? One of "automatic", "info_button" or "settings".</param>
    override public void Notify_InfoPopup(string _popupName, string _action) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Info Popup - popup: " + _popupName + ", action: " + _action);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.player.infopopup");
        if (e != null) {
            Track_AddParamString(e, TRACK_PARAM_POPUP_NAME, _popupName);
            Track_AddParamString(e, TRACK_PARAM_ACTION, _action);
            m_eventQueue.Enqueue(e);
        }
    }

    public override void Notify_Missions(Mission _mission, EActionsMission _action) {
    
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_Missions " + _action.ToString());
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.player.missions");
        if (e != null) {
            Track_AddParamString(e, TRACK_PARAM_MISSION_TYPE, _mission.def.Get("type"));
            Track_AddParamString(e, TRACK_PARAM_MISSION_TARGET, _mission.def.Get("params"));
            string difficulty = _mission.difficulty.ToString();
            if (MissionManager.IsSpecial(_mission))
            {
                difficulty = "LAB_" + difficulty;
            }
            Track_AddParamString(e, TRACK_PARAM_MISSION_DIFFICULTY, difficulty);
            Track_AddParamString(e, TRACK_PARAM_MISSION_VALUE, StringUtils.FormatBigNumber(_mission.objective.targetValue));
            Track_AddParamString(e, TRACK_PARAM_ACTION, _action.ToString());
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            m_eventQueue.Enqueue(e);
        }
    }

    public override void Notify_SettingsOpen(string zone) {
        if (m_playingMode == EPlayingMode.NONE)
            Track_StartPlayingMode(EPlayingMode.SETTINGS);

        // Track popup settings
        Track_GameSettings( zone );
    }

    public override void Notify_SettingsClose() {
        if (m_playingMode == EPlayingMode.SETTINGS)
            Track_EndPlayingMode(true);
    }
    
    /// <summary>
    /// Notify the tracking when the pause popup appears, used to send custom.game.settings while in game
    /// </summary>
    public override void NotifyIngamePause() {
        // Track popup settings
        Track_GameSettings( "In_game" );
    }
    

    public override void Notify_GlobalEventRunDone(int _eventId, string _eventType, int _runScore, int _score, EEventMultiplier _mulitplier) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_GlobalEventRunDone");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.global.event.rundone");
        if (e != null) {
            Track_AddParamString(e, TRACK_PARAM_GLOBAL_EVENT_ID, _eventId.ToString());
            Track_AddParamString(e, TRACK_PARAM_GLOBAL_EVENT_TYPE, _eventType);
            // Track_AddParamString(e, TRACK_PARAM_EVENT_SCORE_RUN, _runScore.ToString());
            e.data.Add(TRACK_PARAM_EVENT_SCORE_RUN, _runScore);
            // Track_AddParamString(e, TRACK_PARAM_EVENT_SCORE_TOTAL, _score.ToString());
            e.data.Add(TRACK_PARAM_EVENT_SCORE_TOTAL, _score);
            Track_AddParamString(e, TRACK_PARAM_EVENT_MULTIPLIER, _mulitplier.ToString());
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            m_eventQueue.Enqueue(e);
        }
    }

    public override void Notify_GlobalEventReward(int _eventId, string _eventType, int _rewardTier, int _score, bool _topContributor) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_GlobalEventReward eventId: " + _eventId + " eventType: " + _eventType + " rewardTier: " + _rewardTier + " score: " + _score);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.global.event.reward");
        if (e != null) {
            Track_AddParamString(e, TRACK_PARAM_GLOBAL_EVENT_ID, _eventId.ToString());
            Track_AddParamString(e, TRACK_PARAM_GLOBAL_EVENT_TYPE, _eventType);
            Track_AddParamString(e, TRACK_PARAM_REWARD_TIER, _rewardTier.ToString());
            // Track_AddParamString(e, TRACK_PARAM_EVENT_SCORE_TOTAL, _score.ToString());
            e.data.Add(TRACK_PARAM_EVENT_SCORE_TOTAL, _score);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_GLOBAL_TOP_CONTRIBUTOR, _topContributor);

            // Common stuff
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            m_eventQueue.Enqueue(e);
        }
    }

    public override void Notify_Hacker() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Notify_Hacker");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.player.hacker");
        if (e != null) {
            m_eventQueue.Enqueue(e);
        }
    }

    /// <summary>
    /// Notifies the start of performance track every X seconds
    /// </summary>
    public override void Notify_StartPerformanceTracker() {
        Reset_Performance_Tracker();
    }

    /// <summary>
    /// Notifies the stop of performance track every X seconds
    /// </summary>
    public override void Notify_StopPerformanceTracker() {
        Performance_IsTrackingEnabled = false;
    }

    public override void Notify_PopupSurveyShown(EPopupSurveyAction action) {
        Track_PopupSurveyShown(action);
    }

    public override void Notify_PopupUnsupportedDeviceAction(EPopupUnsupportedDeviceAction action) {
        Track_PopupUnsupportedDevice(action);
    }

    public override void Notify_DeviceStats() {
        Track_DeviceStats();
    }

    public override void Notify_HungryLetterCollected() {
        Session_HungryLettersCount++;
    }

    public override void Notify_Crash(bool isFatal, string errorType, string errorMessage) {
        // Marked as deprecated for now
        //Track_Crash(isFatal, errorType, errorMessage);
    }

    public override void Notify_OfferShown(bool onDemand, string itemID, string offerName, string offerType) {
        string action = (onDemand) ? "Opened" : "Shown";
        Track_OfferShown(action, itemID, offerName, offerType);
    }

    public override void Notify_EggOpened() {
        if (TrackingPersistenceSystem != null) {
            TrackingPersistenceSystem.EggsOpened++;
        }
    }

    /// <summary>
    /// Called when the user clicks on tournament button on main screen
    /// <param name="tournamentSku">Sku of the currently available tournament</param>
    /// </summary>
    public override void Notify_TournamentClickOnMainScreen(string tournamentSku) {
        Track_TournamentStep(tournamentSku, "MainScreen", null);
    }

    /// <summary>
    /// Called when the user clicks on next button on tournament description screen
    /// </summary>
    /// <param name="tournamentSku">Sku of the currently available tournament</param>
    public override void Notify_TournamentClickOnNextOnDetailsScreen(string tournamentSku) {
        Track_TournamentStep(tournamentSku, "Next", null);
    }

    /// <summary>
    /// Called when the user clickes on enter tournament button
    /// </summary>
    /// <param name="tournamentSku">Sku of the currently available tournament</param>
    /// <param name="currency"><c>NONE</c> if the tournament is for free, otherwise the currency name used to enter the tournament</param>
    public override void Notify_TournamentClickOnEnter(string tournamentSku, UserProfile.Currency currency) {
        Track_TournamentStep(tournamentSku, "Enter", Track_UserCurrencyToString(currency));
    }

    public override void Notify_RateThisApp(ERateThisAppResult result) {
        int dragonProgression = 0;
        IDragonData dragonData = DragonManager.currentDragon;
        if (dragonData != null)
        {
            dragonProgression = UsersManager.currentUser.GetDragonProgress(dragonData);
        }

        Track_RateThisAppShown(result, dragonProgression);
    }
    
    
    public override void Notify_SocialClick(string net, string zone) 
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_SocialClick net = " + net + " zone = " + zone);

        HDTrackingEvent e = new HDTrackingEvent("custom.social.click");
        {
            Track_AddParamString(e, TRACK_PARAM_NETWORK, net);
            Track_AddParamString(e, TRACK_PARAM_ZONE, zone);
            Track_AddParamPlayerProgress(e);
        }
        m_eventQueue.Enqueue(e);
    }
    
    public override void Notify_ShareScreen(string zone)
    {
      if (FeatureSettingsManager.IsDebugEnabled)
          Log("Track_ShareScreen zone = " + zone);

      HDTrackingEvent e = new HDTrackingEvent("custom.game.sharescreen");
      {
          Track_AddParamString(e, TRACK_PARAM_ZONE, zone);
          Track_AddParamPlayerProgress(e);
      }
      m_eventQueue.Enqueue(e);
    }
    

    public override void Notify_ExperimentApplied(string experimentName, string experimentGroup)
    {
        Track_ExperimentApplied(experimentName, experimentGroup);        
    }
    #endregion

    #region animoji
    private string dragon_name;
    private int recordings;
    private int duration;
    public override void Notify_AnimojiStart()
    {
        dragon_name = InstanceManager.menuSceneController.selectedDragon;
        recordings = 0;
        duration = (int)Time.realtimeSinceStartup;
    }
    public override void Notify_AnimojiRecord()
    {
        recordings++;
    }
    public override void Notify_AnimojiExit()
    {
        Track_AnimojiEvent(dragon_name, recordings, (int)(Time.realtimeSinceStartup - duration));
    }
    #endregion

    #region lab
    /// <summary>
    /// Called when the user clicks on the lab button
    /// </summary>
    public override void Notify_LabEnter()
    {        
        Track_LabEnter();
    }

    /// <summary>
    /// Called at the start of each game round (like <c>Notify_RoundStart()</c> for standard dragons)
    /// </summary>
    /// <param name="dragonName">Name of the current Lab Dragon</param>
    /// <param name="labHp">HP level of the current Lab Dragon </param>
    /// <param name="labSpeed">Speed level of the current Lab Dragon</param>
    /// <param name="labBoost">Boost level of the current Lab Dragon.</param> 
    /// <param name="labPower">Total number of Special Dragons unlock up to now</param>
    /// <param name="totalSpecialDragonsUnlocked"></param>
    /// <param name="currentLeague">Name of the league that user is participating</param>
    public override void Notify_LabGameStart(string dragonName, int labHp, int labSpeed, int labBoost, string labPower, int totalSpecialDragonsUnlocked, string currentLeague, List<string> pets)
    {
        // Resets the amount of runs in the current round because a new round has just started
        Session_RunsAmountInCurrentRound = 0;
        Session_HungryLettersCount = 0;

        // One more game round
        TrackingPersistenceSystem.GameRoundCount++;

        if (m_playingMode == EPlayingMode.NONE) {
            Track_StartPlayingMode(EPlayingMode.PVE);
        }
        
        Track_LabGameStart(dragonName, labHp, labSpeed, labBoost, labPower, totalSpecialDragonsUnlocked, currentLeague, pets);
        
        Session_NotifyRoundStart();
    }
    
    public override void Notify_LabGameEnd(string dragonName, int labHp, int labSpeed, int labBoost, string labPower, int timePlayed, int score,
    int eggFound,float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive, 
    int scGained, int hcGained, float powerTime, int mapUsage, string currentLeague ) 
    {
        if (m_playingMode == EPlayingMode.PVE) {
            Track_EndPlayingMode(true);
        }

        if (TrackingPersistenceSystem != null) {
            TrackingPersistenceSystem.EggsFound += eggFound;
        }
        
        Track_LabGameEnd(dragonName, labHp, labSpeed, labBoost, labPower, timePlayed, score, Session_LastDeathType, Session_LastDeathSource, Session_LastDeathCoordinates,
            eggFound, highestMultiplier, highestBaseMultiplier, furyRushNb, superFireRushNb, hcRevive, adRevive, scGained, hcGained, (int)(powerTime * 1000.0f), mapUsage, currentLeague);
            
        Session_NotifyRoundEnd();
    }

    /// <summary>
    /// Called whenever the user receives the results from the League (at the same time than eco-source is sent for rewards, weekly). 
    /// </summary>
    /// <param name="ranking">Rank achieved in current league</param>
    /// <param name="currentLeague">Name of the league that user have participated</param>
    /// <param name="upcomingLeague">Name of the league that user have been promoted/dropped in next week</param>
    public override void Notify_LabResult(int ranking, string currentLeague, string upcomingLeague)
    {
        Track_LabResult(ranking, currentLeague, upcomingLeague);
    }

	/// <summary>
	/// A daily reward has been collected.
	/// </summary>
	/// <param name="_rewardIdx">Reward index within the sequence [0..SEQUENCE_SIZE - 1].</param>
	/// <param name="_totalRewardIdx">Cumulated reward index [0..N].</param>
	/// <param name="_type">Reward type. For replaced pets, use pet-gf.</param>
	/// <param name="_amount">Final given amount (after scaling and doubling).</param>
	/// <param name="_sku">(Optional) Sku of the reward.</param>
	/// <param name="_doubled">Was the reward doubled by watching an ad?</param>
	public override void Notify_DailyReward(int _rewardIdx, int _totalRewardIdx, string _type, long _amount, string _sku, bool _doubled) {
		Track_DailyReward(_rewardIdx, _totalRewardIdx, _type, _amount, _sku, _doubled);
	}
    #endregion

    #region downloadables
    public override void Notify_DownloadablesStart(Downloadables.Tracker.EAction action, string downloadableId, long existingSizeAtStart, long totalSize)
    {
        if (action == Downloadables.Tracker.EAction.Download || action == Downloadables.Tracker.EAction.Update)
        {            
            Track_DownloadStarted(GetDownloadTypeFromDownloadableId(downloadableId), existingSizeAtStart, totalSize);
        }
    }

    public override void Notify_DownloadablesEnd(Downloadables.Tracker.EAction action, string downloadableId, long existingSizeAtStart, long existingSizeAtEnd, long totalSize, int timeSpent,
                                                string reachabilityAtStart, string reachabilityAtEnd, string result, bool maxAttemptsReached)
    {
        Track_DownloadablesEnd(action.ToString(), downloadableId, existingSizeAtStart, existingSizeAtEnd, totalSize, timeSpent, reachabilityAtStart, reachabilityAtEnd, result, maxAttemptsReached);

        if (action == Downloadables.Tracker.EAction.Download || action == Downloadables.Tracker.EAction.Update)
        {
			string status = (result == HDDownloadablesTracker.RESULT_SUCCESS) ? "completed" : "failed";            
            Track_DownloadComplete(status, GetDownloadTypeFromDownloadableId(downloadableId), existingSizeAtEnd, timeSpent);
        }
    }

    public override void Notify_PopupOTA(string _popupName, Downloadables.Popup.EAction _action) {
        string actionStr = "";

        switch (_action) {
			case Downloadables.Popup.EAction.Dismiss:               actionStr = "DISMISS"; break;
            case Downloadables.Popup.EAction.Wifi_Only:             actionStr = "WIFI ONLY"; break;
            case Downloadables.Popup.EAction.Wifi_Mobile:           actionStr = "WIFI AND MOBILE DATA"; break;
            case Downloadables.Popup.EAction.View_Storage_Options:  actionStr = "VIEW STORAGE OPTIONS"; break;
        }

        Track_PopupOTA(_popupName, actionStr);
    }
    #endregion

    /// <summary>
    /// Sent when the user unlocks the map.
    /// </summary>
    /// <param name="location">Where the map has been unlocked from.</param>
    /// <param name="unlockType">How the map has been unlocked.</param>
    public override void Notify_UnlockMap(ELocation location, EUnlockType unlockType)
    {
        Track_UnlockMap(ELocationToKey(location), EUnlockTypeToKey(unlockType));
    }

    #region track
    private const string TRACK_EVENT_TUTORIAL_COMPLETION = "tutorial_completion";
    private const string TRACK_EVENT_FIRST_10_RUNS_COMPLETED = "first_10_runs_completed";

    private bool Track_HasEventBeenSent(string e) {
        return TrackingPersistenceSystem != null && TrackingPersistenceSystem.HasEventAlreadyBeenSent(e);
    }

    private void Track_StartSessionEvent() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_StartSessionEvent");
        }

        Track_EtlStartEvent();
    }

    private void Track_GameStart() {
        HDTrackingEvent e = new HDTrackingEvent("game.start");

        Track_AddParamSubVersion(e);
        Track_AddParamProviderAuth(e);
        Track_AddParamPlayerID(e);
        Track_AddParamServerAccID(e);

        DeviceUtilsManager.SharedInstance.CheckAppOpenedBy();
        Calety.DeviceUtils.EAppOpenedBy openedBy = DeviceUtilsManager.SharedInstance.m_eAppOpenedBy;

        string typeNotif = "";
        if (openedBy == Calety.DeviceUtils.EAppOpenedBy.E_OPENED_BY_LOCAL_NOTIFICATION || openedBy == Calety.DeviceUtils.EAppOpenedBy.E_OPENED_BY_PUSH_NOTIFICATION)
        {
            string sku = DeviceUtilsManager.SharedInstance.strNotificationSku;           
            if (!string.IsNullOrEmpty(sku))
            {
                DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.NOTIFICATIONS, sku);
                if (def != null)
                {
                    typeNotif = def.Get("trackingSku");
                }
            }
        }

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            ControlPanel.Log("typeNotif = " + typeNotif + " openedBy = " + openedBy);
        }

        // "" is sent because Calety doesn't support this yet
        Track_AddParamString(e, TRACK_PARAM_TYPE_NOTIF, typeNotif);
        Track_AddParamLanguage(e);        
        Track_AddParamBoolAsInt(e, TRACK_PARAM_STORE_INSTALLED, DeviceUtilsManager.SharedInstance.CheckIsAppFromStore());

        Track_AddParamBoolAsInt(e, TRACK_PARAM_IS_HACKER, UsersManager.currentUser.isHacker);
        Track_AddParamString(e, TRACK_PARAM_DEVICE_PROFILE, FeatureSettingsManager.instance.Device_CurrentProfile);

#if UNITY_ANDROID

        float rating = FeatureSettingsManager.instance.Device_CalculateRating();
        int systemMemorySize = FeatureSettingsManager.instance.Device_GetSystemMemorySize();
        int gfxMemorySize = FeatureSettingsManager.instance.Device_GetGraphicsMemorySize();
        string profileName = FeatureSettingsManager.deviceQualityManager.Profiles_RatingToProfileName(rating, systemMemorySize, gfxMemorySize);
#else
        string profileName = "not available";
#endif
        Track_AddParamString(e, TRACK_PARAM_INITIALQUALITY, profileName);
        Track_AddParamTrackingID(e);

        Track_SendEvent(e);

        Session_GameStartSent = true;
    }

    private void Track_MarketingID(string marketingId) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_MarketingID id = " + marketingId);
        }
        HDTrackingEvent e = new HDTrackingEvent(TRACK_EVENT_CUSTOM_PLAYER_INFO);
        Track_AddParamString(e, TRACK_PARAM_ACQ_MARKETING_ID, marketingId);
        m_eventQueue.Enqueue(e);
    }

    private void Track_ApplicationEndEvent(string stopCause) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_ApplicationEndEvent " + stopCause);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.mobile.stop");
        {
            Track_AddParamBoolAsInt(e, TRACK_PARAM_IS_PAYING_SESSION, Session_IsPayingSession);
            Track_AddParamPlayerProgress(e);
            e.data.Add(TRACK_PARAM_SESSION_PLAY_TIME, (int)Session_PlayTime);
            Track_AddParamString(e, TRACK_PARAM_STOP_CAUSE, stopCause);
            Track_AddParamTotalPlaytime(e);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_MobileStartEvent() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_MobileStartEvent");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.mobile.start");
        {
            Track_AddParamAbTesting(e);
            Track_AddParamPlayerProgress(e);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_EtlStartEvent() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_EtlStartEvent");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.etl.session.start");
        //Track_SendEvent(e);
		m_eventQueue.Enqueue(e);
    }

    private void Track_EtlEndEvent() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_EtlEndEvent");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.etl.session.end");
        m_eventQueue.Enqueue(e);
    }

    private void Track_IAPCompleted(string storeTransactionID, string houstonTransactionID, string itemID, string promotionType, string moneyCurrencyCode, float moneyPrice, int moneyUSD, bool isOffer) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_IAPCompleted storeTransactionID = " + storeTransactionID + " houstonTransactionID = " + houstonTransactionID + " itemID = " + itemID +
                " promotionType = " + promotionType + " moneyCurrencyCode = " + moneyCurrencyCode + " moneyPrice = " + moneyPrice + " moneyUSD = " + moneyUSD +
                " isOffer = " + isOffer);
        }

        // iap event
        HDTrackingEvent e = new HDTrackingEvent("custom.player.iap");
        {
            Track_AddParamString(e, TRACK_PARAM_STORE_TRANSACTION_ID, storeTransactionID);
            Track_AddParamString(e, TRACK_PARAM_HOUSTON_TRANSACTION_ID, houstonTransactionID);
            Track_AddParamString(e, TRACK_PARAM_ITEM_ID, itemID);
            Track_AddParamString(e, TRACK_PARAM_PROMOTION_TYPE, promotionType);
            Track_AddParamString(e, TRACK_PARAM_MONEY_CURRENCY, moneyCurrencyCode);
            Track_AddParamFloat(e, TRACK_PARAM_MONEY_IAP, moneyPrice);

            // moneyPrice in cents of dollar
            e.data.Add(TRACK_PARAM_MONEY_USD, moneyUSD);

            Track_AddParamPlayerProgress(e);

            if (TrackingPersistenceSystem != null) {
                Track_AddParamTotalPurchases(e);
                e.data.Add(TRACK_PARAM_TOTAL_STORE_VISITS, TrackingPersistenceSystem.TotalStoreVisits);
            }

            e.data.Add(TRACK_PARAM_TRIGGERED, isOffer);
        }
        m_eventQueue.Enqueue(e);

        // af_purchase event
        e = new HDTrackingEvent("af_purchase");
        {
            Track_AddParamString(e, TRACK_PARAM_AF_DEF_CURRENCY, moneyCurrencyCode);
            Track_AddParamFloat(e, TRACK_PARAM_AF_DEF_LOGPURCHASE, moneyPrice);
            e.data.Add(TRACK_PARAM_AF_DEF_QUANTITY, 1);
        }
        m_eventQueue.Enqueue(e);

        // fb_purchase event
        e = new HDTrackingEvent("fb_purchase");
        {
            Track_AddParamString(e, TRACK_PARAM_FB_DEF_CURRENCY, moneyCurrencyCode);
            Track_AddParamFloat(e, TRACK_PARAM_FB_DEF_LOGPURCHASE, moneyPrice);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_PurchaseWithResourcesCompleted(string economyGroup, string itemID, int itemQuantity, string promotionType,
        UserProfile.Currency currency, float moneyPrice, int amountBalance) {
        string moneyCurrency = Track_UserCurrencyToString( currency );
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_PurchaseWithResourcesCompleted economyGroup = " + economyGroup + " itemID = " + itemID + " promotionType = " + promotionType +
                " moneyCurrency = " + moneyCurrency + " moneyPrice = " + moneyPrice + " amountBalance = " + amountBalance);
        }

        
        // HQ event
        HDTrackingEvent e = new HDTrackingEvent("custom.player.iap.secondaryStore");
        {
            Track_AddParamString(e, TRACK_PARAM_ECONOMY_GROUP, economyGroup);
            Track_AddParamString(e, TRACK_PARAM_ITEM_ID, itemID);
            e.data.Add(TRACK_PARAM_ITEM_QUANTITY, itemQuantity);
            Track_AddParamString(e, TRACK_PARAM_PROMOTION_TYPE, promotionType);
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, moneyCurrency);
            e.data.Add(TRACK_PARAM_MONEY_IAP, moneyPrice);

            Track_AddParamPlayerProgress(e);
            Track_AddParamTotalPurchases(e);
        }
        m_eventQueue.Enqueue(e);

        // Game event
        e = new HDTrackingEvent("custom.eco.sink");
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, moneyCurrency);
            e.data.Add(TRACK_PARAM_AMOUNT_DELTA, (int)moneyPrice);
            e.data.Add(TRACK_PARAM_AMOUNT_BALANCE, amountBalance);
            Track_AddParamString(e, TRACK_PARAM_ECO_GROUP, economyGroup);
            Track_AddParamString(e, TRACK_PARAM_ITEM, itemID);
        }
        m_eventQueue.Enqueue(e);

        if (SendRtTracking())
        {
            switch (currency)
            {
                case UserProfile.Currency.HARD:
                    {
                        // Send event
                        GameServerManager.SharedInstance.CurrencySpent("hc", amountBalance, (int)moneyPrice, economyGroup, CurrencyFluctuationResponse);
                    }
                    break;
                case UserProfile.Currency.GOLDEN_FRAGMENTS:
                    {
                        GameServerManager.SharedInstance.CurrencySpent("gf", amountBalance, (int)moneyPrice, economyGroup, CurrencyFluctuationResponse);
                    }
                    break;
                case UserProfile.Currency.SOFT:
                    {
                        GameServerManager.SharedInstance.CurrencySpent("sc", amountBalance, (int)moneyPrice, economyGroup, CurrencyFluctuationResponse);
                    }
                    break;
            }
        }
    }

    private void Track_EarnResources(string economyGroup, UserProfile.Currency currency, int amountDelta, int amountBalance, bool paid) {
        string moneyCurrency = Track_UserCurrencyToString( currency );
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_EarnResources economyGroup = " + economyGroup + " moneyCurrency = " + moneyCurrency + " moneyPrice = " + amountDelta + " amountBalance = " + amountBalance);
        }
        // Game event
        HDTrackingEvent e = new HDTrackingEvent("custom.eco.source");
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, moneyCurrency);
            e.data.Add(TRACK_PARAM_AMOUNT_DELTA, (int)amountDelta);
            e.data.Add(TRACK_PARAM_AMOUNT_BALANCE, amountBalance);
            Track_AddParamString(e, TRACK_PARAM_ECO_GROUP, economyGroup);
        }
        m_eventQueue.Enqueue(e);

        if (SendRtTracking())
        {
            switch( currency )
            {
                case UserProfile.Currency.HARD:
                {
                    // Send event
                    GameServerManager.SharedInstance.CurrencyEarned( "hc", amountBalance,  amountDelta, economyGroup, paid, CurrencyFluctuationResponse);    
                }break;
                case UserProfile.Currency.GOLDEN_FRAGMENTS:
                {
                    // Send event
                    GameServerManager.SharedInstance.CurrencyEarned( "gf", amountBalance,  amountDelta, economyGroup, paid, CurrencyFluctuationResponse);    
                }break;
                 case UserProfile.Currency.SOFT:
                {
                    GameServerManager.SharedInstance.CurrencyEarned( "sc", amountBalance,  amountDelta, economyGroup, paid, CurrencyFluctuationResponse);    
                }break;
            }
        }
    }
    
    private bool SendRtTracking()
    {
        bool ret = true;
        ret = !(GDPRManager.SharedInstance.IsAgeRestrictionEnabled() || GDPRManager.SharedInstance.IsConsentTrackingRestrictionEnabled());
        return ret;
    }

    private void CurrencyFluctuationResponse(FGOL.Server.Error error, GameServerManager.ServerResponse response)
    {
        if (error != null){
            Debug.LogError(error.ToString());
        }
        else if  (response != null)
        {
            if (response["response"] != null)
            {
                SimpleJSON.JSONNode ret = SimpleJSON.JSONNode.Parse(response["response"] as string);
                if ( ret != null )
                {
                    if (ret.ContainsKey("errorCode"))
                    {
                        int errorInt = ret["errorCode"];
                        switch (errorInt)
                        {
                            case 623:   // JSON_SYNTAX_ERROR
                            {
                            }break;
                            case 624:   // BUSSINES_ERROR
                            {
                            }break;
                            case 610:   // UNEXPECTED_ERROR
                            {
                            }break;
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("NO Response");
            }
        }
    }

    private void Track_CustomerSupportRequested() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_CustomerSupportRequested");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.cs");
        {
            Track_AddParamTotalPurchases(e);
            Track_AddParamSessionsCount(e);
            Track_AddParamPlayerProgress(e);
            // Always 0 since there's no pvp in the game
            e.data.Add(TRACK_PARAM_PVP_MATCHES_PLAYED, 0);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_AdStarted(string adType, string rewardType, bool adIsAvailable, string provider = null) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_AdStarted adType = " + adType + " rewardType = " + rewardType + " adIsAvailable = " + adIsAvailable + " provider = " + provider);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.ad.start");
        {
            if (TrackingPersistenceSystem != null) {
                // According to specification we need to include the current ad
                e.data.Add(TRACK_PARAM_NB_ADS_LTD, TrackingPersistenceSystem.AdsCount + 1);

                // According to specification we need to include the current session is it hasn't been condisered yet
                int adsSessions = TrackingPersistenceSystem.AdsSessions;
                if (!Session_IsAdSession) {
                    adsSessions++;
                }

                e.data.Add(TRACK_PARAM_NB_ADS_SESSION, adsSessions);
            }

            Track_AddParamBoolAsInt(e, TRACK_PARAM_AD_IS_AVAILABLE, adIsAvailable);
            Track_AddParamString(e, TRACK_PARAM_REWARD_TYPE, rewardType);
            Track_AddParamPlayerProgress(e);
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);
            Track_AddParamString(e, TRACK_PARAM_ADS_TYPE, adType);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_AdFinished(string adType, bool adIsLoaded, bool maxReached, int adViewingDuration, string provider) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_AdFinished adType = " + adType + " adIsLoaded = " + adIsLoaded + " maxReached = " + maxReached +
                " adViewingDuration = " + adViewingDuration + " provider = " + provider);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.ad.finished");
        {
            Track_AddParamBoolAsInt(e, TRACK_PARAM_IS_LOADED, adIsLoaded);
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);
            e.data.Add(TRACK_PARAM_AD_VIEWING_DURATION, adViewingDuration);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_MAX_REACHED, maxReached);
            Track_AddParamString(e, TRACK_PARAM_ADS_TYPE, adType);
        }
        m_eventQueue.Enqueue(e);

        // af_ad_shown
        e = new HDTrackingEvent("af_ad_shown");
        m_eventQueue.Enqueue(e);

        // fb_ad_shown
        e = new HDTrackingEvent("fb_ad_shown");
        m_eventQueue.Enqueue(e);
    }

    private void Track_RoundStart(int dragonXp, int dragonProgression, string dragonSkin, List<string> pets) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            string str = "Track_RoundStart dragonXp = " + dragonXp + " dragonProgression = " + dragonProgression + " dragonSkin = " + dragonSkin;
            if (pets != null) {
                int count = pets.Count;
                for (int i = 0; i < count; i++) {
                    str += " pet[" + i + "] = " + pets[i];
                }
            }

            Log(str);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.start");
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            e.data.Add(TRACK_PARAM_XP, dragonXp);
            e.data.Add(TRACK_PARAM_DRAGON_PROGRESSION, dragonProgression);
            string trackingSku = Translate_DragonDisguiseSkuToTrackingSku(dragonSkin);
            Track_AddParamString(e, TRACK_PARAM_DRAGON_SKIN, trackingSku);
            Track_AddParamPets(e, pets);
        }
        m_eventQueue.Enqueue(e);
    }

    public void Track_RoundEnd(int dragonXp, int deltaXp, int dragonProgression, int timePlayed, int score,
        string deathType, string deathSource, string deathCoordinates, int chestsFound, int eggFound,
        float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive,
        int scGained, int hcGained, int boostTimeMs, int mapUsage) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_RoundEnd dragonXp = " + dragonXp + " deltaXp = " + deltaXp + " dragonProgression = " + dragonProgression +
                " timePlayed = " + timePlayed + " score = " + score +
                " deathType = " + deathType + " deathSource = " + deathSource + " deathCoor = " + deathCoordinates +
                " chestsFound = " + chestsFound + " eggFound = " + eggFound +
                " highestMultiplier = " + highestMultiplier + " highestBaseMultiplier = " + highestBaseMultiplier +
                " furyRushNb = " + furyRushNb + " superFireRushNb = " + superFireRushNb + " hcRevive = " + hcRevive + " adRevive = " + adRevive +
                " scGained = " + scGained + " hcGained = " + hcGained +
                " boostTimeMs = " + boostTimeMs + " mapUsage = " + mapUsage
                );
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.end");
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamRunsAmount(e);
            Track_AddParamHighestDragonXp(e);
            Track_AddParamPlayerProgress(e);
            e.data.Add(TRACK_PARAM_XP, dragonXp);
            e.data.Add(TRACK_PARAM_DELTA_XP, deltaXp);
            e.data.Add(TRACK_PARAM_DRAGON_PROGRESSION, dragonProgression);
            e.data.Add(TRACK_PARAM_TIME_PLAYED, timePlayed);
            e.data.Add(TRACK_PARAM_SCORE, score);
            Track_AddParamString(e, TRACK_PARAM_DEATH_TYPE, deathType);
            Track_AddParamString(e, TRACK_PARAM_DEATH_CAUSE, deathSource);
            Track_AddParamString(e, TRACK_PARAM_DEATH_COORDINATES, deathCoordinates);
            e.data.Add(TRACK_PARAM_CHESTS_FOUND, chestsFound);
            e.data.Add(TRACK_PARAM_EGG_FOUND, eggFound);
            Track_AddParamFloat(e, TRACK_PARAM_HIGHEST_MULTIPLIER, highestMultiplier);
            Track_AddParamFloat(e, TRACK_PARAM_HIGHEST_BASE_MULTIPLIER, highestBaseMultiplier);
            e.data.Add(TRACK_PARAM_FIRE_RUSH_NB, furyRushNb);
            e.data.Add(TRACK_PARAM_SUPER_FIRE_RUSH_NB, superFireRushNb);
            e.data.Add(TRACK_PARAM_HC_REVIVE, hcRevive);
            e.data.Add(TRACK_PARAM_AD_REVIVE, adRevive);
            e.data.Add(TRACK_PARAM_SC_EARNED, scGained);
            e.data.Add(TRACK_PARAM_HC_EARNED, hcGained);
            e.data.Add(TRACK_PARAM_BOOST_TIME, boostTimeMs);
            e.data.Add(TRACK_PARAM_MAP_USAGE, mapUsage);
            e.data.Add(TRACK_PARAM_HUNGRY_LETTERS_NB, Session_HungryLettersCount);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_IS_HACKER, UsersManager.currentUser.isHacker);
            Track_AddParamEggsPurchasedWithHC(e);
            Track_AddParamEggsFound(e);
            Track_AddParamEggsOpened(e);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_RunEnd(int dragonXp, int timePlayed, int score, string deathType, string deathSource, string deathCoordinates) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_RunEnd dragonXp = " + dragonXp + " timePlayed = " + timePlayed + " score = " + score +
                " deathType = " + deathType + " deathSource = " + deathSource + " deathCoor = " + deathCoordinates);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.dead");
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            Track_AddParamRunsAmount(e);
            e.data.Add(TRACK_PARAM_XP, dragonXp);
            e.data.Add(TRACK_PARAM_TIME_PLAYED, timePlayed);
            e.data.Add(TRACK_PARAM_SCORE, score);
            Track_AddParamString(e, TRACK_PARAM_DEATH_TYPE, deathType);
            Track_AddParamString(e, TRACK_PARAM_DEATH_CAUSE, deathSource);
            Track_AddParamString(e, TRACK_PARAM_DEATH_COORDINATES, deathCoordinates);
        }
        m_eventQueue.Enqueue(e);
    }
    
    private void Track_OpenShop( string origin ){
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_OpenShop origin = " + origin );
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.shop.entry");
        {
            Track_AddParamString(e, TRACK_PARAM_ZONE , origin);
            Track_AddParamPlayerProgress(e);
            Track_AddParamPlayerSC(e);
            Track_AddParamPlayerPC(e);
        }
        m_eventQueue.Enqueue(e);
    }
    
    private void Track_ShopSection( string section ){
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_StoreSection section = " + section );
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.shop.view");
        {
            Track_AddParamString(e, TRACK_PARAM_SECTION , section);
            Track_AddParamPlayerProgress(e);
            Track_AddParamPlayerSC(e);
            Track_AddParamPlayerPC(e);
        }
        m_eventQueue.Enqueue(e);
    }
    
    private void Track_ShopItemView( string id ){
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_StoreItemView itemID = " + id );
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.shop.itemviewed");
        {
            Track_AddParamString(e, TRACK_PARAM_ITEM_ID, id);
            Track_AddParamPlayerProgress(e);
            Track_AddParamPlayerSC(e);
            Track_AddParamPlayerPC(e);
        }
        m_eventQueue.Enqueue(e);
    }
    
    

    private void Track_Funnel(string _event, string _step, int _stepDuration, int _totalDuration, bool _fistLoad) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_Funnel eventID = " + _event + " stepName = " + _step + " stepDuration = " + _stepDuration + " totalDuration = " + _totalDuration + " firstLoad = " + _fistLoad);
        }

        HDTrackingEvent e = new HDTrackingEvent(_event);
        {
            e.data.Add(TRACK_PARAM_STEP_NAME, _step);
            e.data.Add(TRACK_PARAM_STEP_DURATION, _stepDuration);
            e.data.Add(TRACK_PARAM_TOTAL_DURATION, _totalDuration);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_FIRST_LOAD, _fistLoad);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_SocialAuthentication(string provider, int yearOfBirth, string gender) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_SocialAuthentication provider = " + provider + " yearOfBirth = " + yearOfBirth + " gender = " + gender);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.player.authentication");
        {
            Track_AddParamString(e, TRACK_PARAM_PROVIDER, provider);
            e.data.Add(TRACK_PARAM_YEAR_OF_BIRTH, yearOfBirth);
            Track_AddParamString(e, TRACK_PARAM_GENDER, gender);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_ConsentPopupDisplay(string _source) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_ConsentPopupDisplay");
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.consentpopup_display");
        {
            e.data.Add(TRACK_PARAM_SOURCE, _source);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_ConsentPopupAccept(int _age, bool _enableAnalytics, bool _enableMarketing, string _modVersion, int _duration) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_ConsentPopupAccept age = " + _age + " analytics_optin = " + _enableAnalytics + " duration = " + _duration + " marketing_optin = " + _enableMarketing + " popup_modular_version = " + _modVersion);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.consentpopup");
        {
            // BI wants these two parameters to be false for minors
            if (GDPRManager.SharedInstance.IsAgeRestrictionEnabled())
            {
                _enableAnalytics = false;
                _enableMarketing = false;
            }
            
            e.data.Add(TRACK_PARAM_DURATION, _duration);            
            e.data.Add(TRACK_PARAM_POPUP_MODULAR_VERSION, _modVersion);

			LegalManager.ETermsPolicy termsPolicy = LegalManager.instance.GetTermsPolicy();

            // BI only wants these two parameters when terms policy is GDPR
			if (termsPolicy != LegalManager.ETermsPolicy.GDPR)
            {                
                e.data.Add(TRACK_PARAM_ANALYTICS_OPTION, null);
                e.data.Add(TRACK_PARAM_MARKETING_OPTION, null);
            }
            else
            {                
                e.data.Add(TRACK_PARAM_ANALYTICS_OPTION, (_enableAnalytics) ? 1 : 0);
                e.data.Add(TRACK_PARAM_MARKETING_OPTION, (_enableMarketing) ? 1 : 0);
            }

			// BI only wants age when terms policy is Coppa or GDPR
			if (termsPolicy == LegalManager.ETermsPolicy.Coppa || termsPolicy == LegalManager.ETermsPolicy.GDPR) 
			{
				e.data.Add (TRACK_PARAM_AGE, _age);
			} 
			else 
			{
				e.data.Add(TRACK_PARAM_AGE, null);
			}

        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_TutorialCompletion() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_TutorialCompletion");
        }

        // af_tutorial_completion
        HDTrackingEvent e = new HDTrackingEvent("af_tutorial_completion");
        m_eventQueue.Enqueue(e);

        // fb_tutorial_completion
        e = new HDTrackingEvent("fb_tutorial_completion");
        m_eventQueue.Enqueue(e);
    }

    private void Track_First10RunsCompleted() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_First10RunsCompleted");
        }

        // af_first_10_runs_completed
        HDTrackingEvent e = new HDTrackingEvent("af_first_10_runs_completed");
        m_eventQueue.Enqueue(e);

        // fb_first_10_runs_completed
        e = new HDTrackingEvent("fb_first_10_runs_completed");
        m_eventQueue.Enqueue(e);
    }

    private void Track_FirstPurchase() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_FirstPurchase");
        }

        // af_first_purchase
        HDTrackingEvent e = new HDTrackingEvent("af_first_purchase");
        m_eventQueue.Enqueue(e);

        // fb_first_purchase
        e = new HDTrackingEvent("fb_first_purchase");
        m_eventQueue.Enqueue(e);
    }

    private void Track_FirstAdShown() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_FirstAdShown");
        }

        // af_first_ad_shown
        HDTrackingEvent e = new HDTrackingEvent("af_first_ad_shown");
        m_eventQueue.Enqueue(e);

        // fb_first_ad_shown
        e = new HDTrackingEvent("fb_first_ad_shown");
        m_eventQueue.Enqueue(e);
    }

    private void Track_1EggBought() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_1EggBought");
        }

        // af_1_egg_bought
        HDTrackingEvent e = new HDTrackingEvent("af_1_egg_bought");
        m_eventQueue.Enqueue(e);

        // fb_1_egg_bought
        e = new HDTrackingEvent("fb_1_egg_bought");
        m_eventQueue.Enqueue(e);
    }

    private void Track_5EggBought() {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_5EggBought");
        }

        // af_5_egg_bought
        HDTrackingEvent e = new HDTrackingEvent("af_5_egg_bought");
        m_eventQueue.Enqueue(e);

        // fb_5_egg_bought
        e = new HDTrackingEvent("fb_5_egg_bought");
        m_eventQueue.Enqueue(e);
    }

    private void Track_DragonUnlocked(int order) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_DragonUnlocked order " + order);
        }

        string eventName = "_" + order + "_dragon_unlocked";

        // af_X_dragon_unlocked
        HDTrackingEvent e = new HDTrackingEvent("af" + eventName);
        m_eventQueue.Enqueue(e);

        // fb_X_dragon_unlocked
        e = new HDTrackingEvent("fb" + eventName);
        m_eventQueue.Enqueue(e);
    }

    void Track_StartPlayingMode(EPlayingMode _mode) {
        m_playingMode = _mode;
        m_playingModeStartTime = Time.time;

        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_StartPlayingMode playingMode = " + _mode);
        }
    }

    void Track_GameSettings( string zone )
    {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_GameSettings zone = " + zone);
        }
        
        HDTrackingEvent e = new HDTrackingEvent("custom.game.settings");
        {
            Track_AddParamString(e, TRACK_PARAM_ZONE, zone);
            Track_AddParamPlayerProgress(e);
        }
        m_eventQueue.Enqueue(e);
        
    }

    void Track_EndPlayingMode(bool _isSuccess) {
        if (m_playingMode != EPlayingMode.NONE) {
            string playingModeStr = "";
            string rank = "";
            switch (m_playingMode) {
                case EPlayingMode.TUTORIAL: {
                        playingModeStr = "Tutorial";
                        rank = (m_firstUXFunnel.currentStep + 1) + "-" + m_firstUXFunnel.stepCount; // 1-5, 2-5,... ,5-5
                    }
                    break;
                case EPlayingMode.PVE: {
                        playingModeStr = "PvE";
                        int value = (UsersManager.currentUser != null) ? UsersManager.currentUser.GetPlayerProgress() : 0;
                        rank = value.ToString();
                    }
                    break;
                case EPlayingMode.SETTINGS: {
                        playingModeStr = "Settings";
                    }
                    break;
            }
            int isSuccess = _isSuccess ? 1 : 0;
            int duration = (int)(Time.time - m_playingModeStartTime);

            // Track
            if (FeatureSettingsManager.IsDebugEnabled) {
                Log("Track_EndPlayingMode playingMode = " + m_playingMode + " rank = " + rank + " isSuccess = " + isSuccess + " duration = " + duration);
            }

            HDTrackingEvent e = new HDTrackingEvent("custom.player.mode");
            {
                e.data.Add(TRACK_PARAM_PLAYING_MODE, playingModeStr);
                if (!string.IsNullOrEmpty(rank))
                    e.data.Add(TRACK_PARAM_RANK, rank);
                e.data.Add(TRACK_PARAM_IS_SUCCESS, isSuccess);
                e.data.Add(TRACK_PARAM_DURATION, duration);
            }
            m_eventQueue.Enqueue(e);

            m_playingMode = EPlayingMode.NONE;
        }

    }

    private void Track_PerformanceTrack(int deltaXP, int avgFPS, Vector3 positionBL, Vector3 positionTR, bool fireRush) {
        string posblasstring = Track_CoordinatesToString(positionBL);
        string postrasstring = Track_CoordinatesToString(positionTR);
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("custom.gameplay.fps: deltaXP = " + deltaXP + " avgFPS = " + avgFPS + " coordinatesBL = " + posblasstring + " coordinatesTR = " + postrasstring + " fireRush = " + fireRush);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.gameplay.fps");
        {
            Track_AddParamSessionsCount(e);
            Track_AddParamGameRoundCount(e);
            e.data.Add(TRACK_PARAM_DELTA_XP, deltaXP);
            e.data.Add(TRACK_PARAM_AVERAGE_FPS, (int)FeatureSettingsManager.instance.AverageSystemFPS);
            Track_AddParamString(e, TRACK_PARAM_COORDINATESBL, posblasstring);
            Track_AddParamString(e, TRACK_PARAM_COORDINATESTR, postrasstring);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_FIRE_RUSH, fireRush);
            Track_AddParamString(e, TRACK_PARAM_DEVICE_PROFILE, FeatureSettingsManager.instance.Device_CurrentProfile);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_PopupSurveyShown(EPopupSurveyAction action) {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_PopupSurveyShown action = " + action);

        HDTrackingEvent e = new HDTrackingEvent("custom.survey.popup");
        {
            Track_AddParamString(e, TRACK_PARAM_POPUP_NAME, "HD_SURVEY_1");
            Track_AddParamString(e, TRACK_PARAM_POPUP_ACTION, action.ToString());
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_PopupUnsupportedDevice(EPopupUnsupportedDeviceAction action) {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_PopupUnsupportedDevice action = " + action);

        HDTrackingEvent e = new HDTrackingEvent("custom.leave.popup");
        {
            Track_AddParamString(e, TRACK_PARAM_POPUP_ACTION, action.ToString());
        }
        m_eventQueue.Enqueue(e);
    }


    private void Track_DeviceStats() {
#if UNITY_ANDROID
        float rating = FeatureSettingsManager.instance.Device_CalculateRating();

        int processorFrequency = FeatureSettingsManager.instance.Device_GetProcessorFrequency();
        int systemMemorySize = FeatureSettingsManager.instance.Device_GetSystemMemorySize();
        int gfxMemorySize = FeatureSettingsManager.instance.Device_GetGraphicsMemorySize();
        string profileName = FeatureSettingsManager.deviceQualityManager.Profiles_RatingToProfileName(rating, systemMemorySize, gfxMemorySize);
        string formulaVersion = FeatureSettingsManager.QualityFormulaVersion;

        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_DeviceStats rating = " + rating + " processorFrequency = " + processorFrequency + " system memory = " + systemMemorySize + " gfx memory = " + gfxMemorySize + " quality profile = " + profileName + " quality formula version = " + formulaVersion);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.device.stats");
        {
            e.data.Add(TRACK_PARAM_CPUFREQUENCY, processorFrequency);
            e.data.Add(TRACK_PARAM_CPURAM, systemMemorySize);
            e.data.Add(TRACK_PARAM_GPURAM, gfxMemorySize);
            Track_AddParamString(e, TRACK_PARAM_INITIALQUALITY, profileName);
            Track_AddParamString(e, TRACK_PARAM_VERSION_QUALITY_FORMULA, formulaVersion);
        }
        m_eventQueue.Enqueue(e);
#endif
    }

    private void Track_Crash(bool isFatal, string errorType, string errorMessage) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_Crash isFatal = " + isFatal + " errorType = " + errorType + " errorMessage = " + errorMessage);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.crash");
        {
            Track_AddParamPlayerProgress(e);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_IS_FATAL, isFatal);
            Track_AddParamString(e, TRACK_PARAM_ERROR_TYPE, errorType);
            Track_AddParamString(e, TRACK_PARAM_ERROR_MESSAGE, errorMessage);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_OfferShown(string action, string itemID, string offerName, string offerType) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_OfferShown action = " + action + " itemID = " + itemID + " offerName = " + offerName + " offerType = " + offerType);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.player.specialoffer");
        {
            Track_AddParamString(e, TRACK_PARAM_SPECIAL_OFFER_ACTION, action);
            Track_AddParamString(e, TRACK_PARAM_ITEM_ID, itemID);
            Track_AddParamString(e, TRACK_PARAM_OFFER_NAME, offerName);
            Track_AddParamString(e, TRACK_PARAM_OFFER_TYPE, offerType);
            Track_AddParamPlayerProgress(e);
            Track_AddParamPlayerSC(e);
            Track_AddParamPlayerPC(e);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_TournamentStep(string tournamentSku, string stepName, string currency) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_TournamentStep tournamentSku = " + tournamentSku + " stepName = " + stepName + " currency = " + currency);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.game.tournament");
        {
            Track_AddParamString(e, TRACK_PARAM_TOURNAMENT_SKU, tournamentSku);
            Track_AddParamString(e, TRACK_PARAM_STEP_NAME, stepName);
            e.data.Add(TRACK_PARAM_PAID, !string.IsNullOrEmpty(currency));
            Track_AddParamString(e, TRACK_PARAM_CURRENCY, currency);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_RateThisAppShown(ERateThisAppResult result, int dragonProgression) {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_RateThisAppShown result = " + result + " dragonProgression = " + dragonProgression);

        HDTrackingEvent e = new HDTrackingEvent("custom.game.ratethisapp");
        {
            Track_AddParamString(e, TRACK_PARAM_RATE_RESULT, result.ToString());
            e.data.Add(TRACK_PARAM_DRAGON_PROGRESSION, dragonProgression);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_ExperimentApplied(string experimentName, string experimentGroup)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_ExperimentApplied experimentName = " + experimentName + " experimentGroup = " + experimentGroup);

        HDTrackingEvent e = new HDTrackingEvent("custom.general.abtest.experiment");
        {
            Track_AddParamString(e, TRACK_PARAM_EXPERIMENT_NAME, experimentName);
            Track_AddParamString(e, TRACK_PARAM_EXPERIMENT_GROUP, experimentGroup);
        }
        m_eventQueue.Enqueue(e);
    }



	private void Track_AnimojiEvent(string dragonName, int recordings, int duration)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_Animoji_event = dragon name:" + dragonName + " recordings = " + recordings + " duration secs = " + duration);

        HDTrackingEvent e = new HDTrackingEvent("custom.game.animojis");
        {
            Track_AddParamString(e, TRACK_PARAM_DRAGON, dragonName);
            e.data.Add(TRACK_PARAM_RECORDINGS, recordings);
            e.data.Add(TRACK_PARAM_DURATION, duration);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_LabEnter()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_LabEnter");

        HDTrackingEvent e = new HDTrackingEvent("custom.lab.entry");
        {
            Track_AddParamPlayerProgress(e);
            Track_AddParamPlayerGoldenFragments(e);
            Track_AddParamPlayerPC(e);            
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_LabGameStart(string dragonName, int labHp, int labSpeed, int labBoost, string labPower, int totalSpecialDragonsUnlocked, string currentLeague, List<string> pets)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            string str = "Track_LabGameStart dragonName = " + dragonName + " labHp = " + labHp + " labSpeed = " + labSpeed + " labBoost = " + labBoost + " labPower = " + labPower + 
                " totalSpecialDragonsUnlocked = " + totalSpecialDragonsUnlocked + " currentLeague = " + currentLeague;
            if (pets != null) {
                int count = pets.Count;
                for (int i = 0; i < count; i++) {
                    str += " pet[" + i + "] = " + pets[i];
                }
            }

            Log(str);
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.lab.gamestart");
        {
            Track_AddParamString(e, TRACK_PARAM_DRAGON, dragonName);
            e.data.Add(TRACK_PARAM_LAB_HP, labHp);
            e.data.Add(TRACK_PARAM_LAB_SPEED, labSpeed);
            e.data.Add(TRACK_PARAM_LAB_BOOST, labBoost);
            Track_AddParamString(e, TRACK_PARAM_LAB_POWER, labPower);
            e.data.Add(TRACK_PARAM_TOTAL_SPECIAL_DRAGONS_UNLOCKED, totalSpecialDragonsUnlocked);
            Track_AddParamString(e, TRACK_PARAM_CURRENT_LEAGUE, currentLeague);    
            Track_AddParamPets(e, pets);        
        }
        m_eventQueue.Enqueue(e);
    }
    
    private void Track_LabGameEnd(string dragonName, int labHp, int labSpeed, int labBoost, string labPower, int timePlayed, int score,  
        string deathType, string deathSource, string deathCoordinates,
        int eggFound,float highestMultiplier, float highestBaseMultiplier, int furyRushNb, int superFireRushNb, int hcRevive, int adRevive, 
        int scGained, int hcGained, int powerTimeMs, int mapUsage, string currentLeague)
    {
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_LabGameEnd dragonName = " + dragonName + " labHp = " + labHp + " labSpeed = " + labSpeed + " labBoost = " + labBoost + " labPower = " + labPower +
                " timePlayed = " + timePlayed + " score = " + score +
                " deathType = " + deathType + " deathSource = " + deathSource + " deathCoor = " + deathCoordinates +
                " eggFound = " + eggFound + " highestMultiplier = " + highestMultiplier + " highestBaseMultiplier = " + highestBaseMultiplier +
                " furyRushNb = " + furyRushNb + " superFireRushNb = " + superFireRushNb + " hcRevive = " + hcRevive + " adRevive = " + adRevive +
                " scGained = " + scGained + " hcGained = " + hcGained +
                " powerTime = " + powerTimeMs + " mapUsage = " + mapUsage + " currentLeague = " + currentLeague
                );
        }

        HDTrackingEvent e = new HDTrackingEvent("custom.lab.gameend");
        {
            Track_AddParamString(e, TRACK_PARAM_DRAGON, dragonName);
            e.data.Add(TRACK_PARAM_LAB_HP, labHp);
            e.data.Add(TRACK_PARAM_LAB_SPEED, labSpeed);
            e.data.Add(TRACK_PARAM_LAB_BOOST, labBoost);
            Track_AddParamString(e, TRACK_PARAM_LAB_POWER, labPower);
            e.data.Add(TRACK_PARAM_TIME_PLAYED, timePlayed);
            // No Need? e.data.Add(TRACK_PARAM_SCORE, score);
            Track_AddParamString(e, TRACK_PARAM_DEATH_TYPE, deathType);
            Track_AddParamString(e, TRACK_PARAM_DEATH_CAUSE, deathSource);
            Track_AddParamString(e, TRACK_PARAM_DEATH_COORDINATES, deathCoordinates);
            e.data.Add(TRACK_PARAM_EGG_FOUND, eggFound);
            Track_AddParamFloat(e, TRACK_PARAM_HIGHEST_MULTIPLIER, highestMultiplier);
            Track_AddParamFloat(e, TRACK_PARAM_HIGHEST_BASE_MULTIPLIER, highestBaseMultiplier);
            e.data.Add(TRACK_PARAM_FIRE_RUSH_NB, furyRushNb);
            e.data.Add(TRACK_PARAM_SUPER_FIRE_RUSH_NB, superFireRushNb);
            e.data.Add(TRACK_PARAM_HC_REVIVE, hcRevive);
            e.data.Add(TRACK_PARAM_AD_REVIVE, adRevive);
            e.data.Add(TRACK_PARAM_SC_EARNED, scGained);
            e.data.Add(TRACK_PARAM_HC_EARNED, hcGained);
            e.data.Add(TRACK_PARAM_POWER_TIME, powerTimeMs);
            e.data.Add(TRACK_PARAM_MAP_USAGE, mapUsage);
            e.data.Add(TRACK_PARAM_HUNGRY_LETTERS_NB, Session_HungryLettersCount);
            Track_AddParamString(e, TRACK_PARAM_CURRENT_LEAGUE, currentLeague);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_LabResult(int ranking, string currentLeague, string upcomingLeague)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_LabResult ranking = " + ranking + " currentLeague = " + currentLeague + " upcomingLeague = " + upcomingLeague);

        HDTrackingEvent e = new HDTrackingEvent("custom.lab.result");
        {
            e.data.Add(TRACK_PARAM_RANKING, ranking);            
            Track_AddParamString(e, TRACK_PARAM_CURRENT_LEAGUE, currentLeague);
            Track_AddParamString(e, TRACK_PARAM_UPCOMING_LEAGUE, upcomingLeague);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_UnlockMap(string location, string unlockType)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Track_UnlockMap location = " + location + " unlockType = " + unlockType);

        HDTrackingEvent e = new HDTrackingEvent("custom.game.unlockmap");
        {            
            Track_AddParamString(e, TRACK_PARAM_LOCATION, location);
            Track_AddParamString(e, TRACK_PARAM_UNLOCK_TYPE, unlockType);
        }
        m_eventQueue.Enqueue(e);
    }

	/// <summary>
	/// A daily reward has been collected.
	/// </summary>
	/// <param name="_rewardIdx">Reward index within the sequence [0..SEQUENCE_SIZE - 1].</param>
	/// <param name="_totalRewardIdx">Cumulated reward index [0..N].</param>
	/// <param name="_type">Reward type. For replaced pets, use pet,gf.</param>
	/// <param name="_amount">Final given amount (after scaling and doubling).</param>
	/// <param name="_sku">(Optional) Sku of the reward. For replaced pets, use petSku,gf.</param>
	/// <param name="_doubled">Was the reward doubled by watching an ad?</param>
	private void Track_DailyReward(int _rewardIdx, int _totalRewardIdx, string _type, long _amount, string _sku, bool _doubled) {
		// Debug
		if(FeatureSettingsManager.IsDebugEnabled) {
			Log("Track_DailyReward _rewardIdx = " + (_rewardIdx + 1)
			    + ", _totalRewardIdx = " + (_totalRewardIdx + 1)
				+ ", _type = " + _type
			    + ", _amount = " + _amount
			    + ", _sku = " + _sku
			    + ", _doubled = " + _doubled
			   );
		}

		// Create event
		HDTrackingEvent e = new HDTrackingEvent("custom.game.sevendaylogin"); {
			Track_AddParamInt(e, TRACK_PARAM_DAY, _rewardIdx + 1);	// [0..N-1] -> [1..N]
			Track_AddParamInt(e, TRACK_PARAM_CUMULATIVE_DAYS, _totalRewardIdx + 1);	// [0..N-1] -> [1..N]
			Track_AddParamString(e, TRACK_PARAM_TYPE, _type);
			Track_AddParamInt(e, TRACK_PARAM_AMOUNT, (int)_amount);
			Track_AddParamString(e, TRACK_PARAM_SKU, _sku);
			Track_AddParamBool(e, TRACK_PARAM_AD_VIEWED, _doubled);
		}
		m_eventQueue.Enqueue(e);
	}    

    private void Track_DownloadablesEnd(string action, string downloadableId, long existingSizeAtStart, long existingSizeAtEnd, long totalSize, int timeSpent,
                                       string reachabilityAtStart, string reachabilityAtEnd, string result, bool maxAttemptsReached)
    {
        // Debug
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_DownloadablesEnd action = " + action
                + ", downloadableId = " + downloadableId
                + ", existingSizeMbAtStart = " + existingSizeAtStart
                + ", existingSizeMbAtEnd = " + existingSizeAtEnd
                + ", totalSizeMb = " + totalSize
                + ", timeSpent = " + timeSpent
                + ", reachabilityAtStart = " + reachabilityAtStart
                + ", reachabilityAtEnd = " + reachabilityAtEnd
                + ", result = " + result 
                + ", maxAttemptsReached = " + maxAttemptsReached
               );
        }

        // Create event
        HDTrackingEvent e = new HDTrackingEvent("custom.game.otaend");
        {
            Track_AddParamString(e, TRACK_PARAM_TYPE_BUILD_VERSION, Session_BuildVersion);
            Track_AddParamString(e, TRACK_PARAM_ACTION, action);
            Track_AddParamString(e, TRACK_PARAM_ASSET_BUNDLE, downloadableId);
            Track_AddParamFloat(e, TRACK_PARAM_MB_AVAILABLE_START, GetSizeInMb(existingSizeAtStart));
            Track_AddParamFloat(e, TRACK_PARAM_MB_AVAILABLE_END, GetSizeInMb(existingSizeAtEnd));
            Track_AddParamFloat(e, TRACK_PARAM_SIZE, GetSizeInMb(totalSize));
            Track_AddParamInt(e, TRACK_PARAM_TIME_SPENT, timeSpent);
            Track_AddParamString(e, TRACK_PARAM_NETWORK_TYPE_START, reachabilityAtStart);
            Track_AddParamString(e, TRACK_PARAM_NETWORK_TYPE_END, reachabilityAtEnd);
            Track_AddParamString(e, TRACK_PARAM_RESULT, result);
            Track_AddParamBoolAsInt(e, TRACK_PARAM_MAX_REACHED, maxAttemptsReached);
        }
        m_eventQueue.Enqueue(e);        
    }

    private void Track_DownloadStarted(string downloadType, long size, long totalSize)
    {
        // It needs to be an int because of specification, although it's sent as float below (it's sent as float because this event shares the size attribute with other event that needs the
        // parameter to be float
        int sizeInKb = (int)GetSizeInKb(totalSize);

        // Debug
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_DownloadablesStart "
                + ", downloadType = " + downloadType
                + ", size = " + size
                + ", totalSizeInKb = " + totalSize);
        }

        // Create event
        HDTrackingEvent e = new HDTrackingEvent("custom.player.contentDownload");
        {
            Track_AddParamString(e, TRACK_PARAM_STATUS, "started");
            Track_AddParamString(e, TRACK_PARAM_DOWNLOAD_TYPE, downloadType);
            e.data.Add(TRACK_PARAM_DURATION, 0);            
            Track_AddParamFloat(e, TRACK_PARAM_SIZE, sizeInKb);
        }
        m_eventQueue.Enqueue(e);
    }

    private void Track_DownloadComplete(string action, string downloadType, long size, int timeSpent)
    {
        // It needs to be an int because of specification, although it's sent as float below (it's sent as float because this event shares the size attribute with other event that needs the
        // parameter to be float
        int sizeInKb = (int)GetSizeInKb(size);

        // Debug
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Track_DownloadComplete action = " + action
                + ", downloadType = " + downloadType
                + ", sizeInKb = " + sizeInKb
                + ", timeSpent = " + timeSpent                
               );
        }

        // Create event
        HDTrackingEvent e = new HDTrackingEvent("custom.player.contentDownload");
        {
            Track_AddParamString(e, TRACK_PARAM_STATUS, action);
            Track_AddParamString(e, TRACK_PARAM_DOWNLOAD_TYPE, downloadType);
            e.data.Add(TRACK_PARAM_DURATION, timeSpent);            
            Track_AddParamFloat(e, TRACK_PARAM_SIZE, sizeInKb);
        }
        m_eventQueue.Enqueue(e);
    }

    private static string GetDownloadTypeFromDownloadableId(string downloadableId)
    {
        return "SecondaryDownload_" + downloadableId;
    }

    private void Track_PopupOTA(string _popupName, string _action) {
        // Debug
        if (FeatureSettingsManager.IsDebugEnabled) {
            Log("Track_PopupOTA popupName = " + _popupName
                + ", action = " + _action
               );
        }

        // Create event
        HDTrackingEvent e = new HDTrackingEvent("custom.ota.popups");
        {
            Track_AddParamString(e, TRACK_PARAM_POPUP_NAME, _popupName);
            Track_AddParamString(e, TRACK_PARAM_ACTION, _action);
            Track_AddParamPlayerProgress(e);
        }
        m_eventQueue.Enqueue(e);
    }


    // -------------------------------------------------------------
    // Events
    // -------------------------------------------------------------
    private const string TRACK_EVENT_CUSTOM_PLAYER_INFO = "custom.player.info";

    // -------------------------------------------------------------
    // Params
    // -------------------------------------------------------------

    // Please, respect the alphabetic order, string order
    private const string TRACK_PARAM_AB_TESTING = "abtesting";
    private const string TRACK_PARAM_ACCEPTED = "accepted";
    private const string TRACK_PARAM_ACQ_MARKETING_ID = "acq_marketing_id";
    private const string TRACK_PARAM_ACTION = "action";			// "automatic", "info_button" or "settings"
    private const string TRACK_PARAM_AD_IS_AVAILABLE = "adIsAvailable";
    private const string TRACK_PARAM_AD_REVIVE = "adRevive";
    private const string TRACK_PARAM_ADS_TYPE = "adsType";
    private const string TRACK_PARAM_AD_VIEWING_DURATION = "adViewingDuration";
	private const string TRACK_PARAM_AD_VIEWED = "ad_viewed";
    private const string TRACK_PARAM_AF_DEF_CURRENCY = "af_def_currency";
    private const string TRACK_PARAM_AF_DEF_LOGPURCHASE = "af_def_logPurchase";
    private const string TRACK_PARAM_AF_DEF_QUANTITY = "af_quantity";
    private const string TRACK_PARAM_AGE = "age";
	private const string TRACK_PARAM_AMOUNT = "amount";
    private const string TRACK_PARAM_AMOUNT_BALANCE = "amountBalance";
    private const string TRACK_PARAM_AMOUNT_DELTA = "amountDelta";
    private const string TRACK_PARAM_ANALYTICS_OPTION = "analytics_optin";
    private const string TRACK_PARAM_ASSET_BUNDLE = "assetBundle";
    private const string TRACK_PARAM_AVERAGE_FPS = "avgFPS";
    private const string TRACK_PARAM_BOOST_TIME = "boostTime";
    private const string TRACK_PARAM_CATEGORY = "category";
	private const string TRACK_PARAM_CHESTS_FOUND = "chestsFound";
	private const string TRACK_PARAM_COORDINATESBL = "coordinatesBL";
	private const string TRACK_PARAM_COORDINATESTR = "coordinatesTR";
	private const string TRACK_PARAM_CPUFREQUENCY = "cpuFrequency";
	private const string TRACK_PARAM_CPURAM = "cpuRam";
	private const string TRACK_PARAM_CUMULATIVE_DAYS = "cumulativeDays";
    private const string TRACK_PARAM_CURRENCY = "currency";
    private const string TRACK_PARAM_CURRENT_LEAGUE = "currentLeague";
	private const string TRACK_PARAM_DAY = "day";
    private const string TRACK_PARAM_DEATH_CAUSE = "deathCause";
    private const string TRACK_PARAM_DEATH_COORDINATES = "deathCoordinates";
    private const string TRACK_PARAM_DEATH_IN_CURRENT_RUN_NB = "deathInCurrentRunNb";
    private const string TRACK_PARAM_DEATH_TYPE = "deathType";
    private const string TRACK_PARAM_DELTA_XP = "deltaXp";
    private const string TRACK_PARAM_DEVICE_PROFILE = "deviceProfile";
    private const string TRACK_PARAM_DOWNLOAD_TYPE = "downloadType";
    private const string TRACK_PARAM_DRAGON = "dragon";
    private const string TRACK_PARAM_DRAGON_PROGRESSION = "dragonProgression";
    private const string TRACK_PARAM_DRAGON_SKIN = "dragonSkin";
    private const string TRACK_PARAM_DURATION = "duration";
    private const string TRACK_PARAM_ECO_GROUP = "ecoGroup";
    private const string TRACK_PARAM_ECONOMY_GROUP = "economyGroup";
    private const string TRACK_PARAM_EGG_FOUND = "eggFound";
    private const string TRACK_PARAM_ERROR_MESSAGE = "errorMessage";
    private const string TRACK_PARAM_ERROR_TYPE = "errorType";
    private const string TRACK_PARAM_EXPERIMENT_NAME = "experimentName";
    private const string TRACK_PARAM_EXPERIMENT_GROUP = "experimentGroup";
    private const string TRACK_PARAM_FB_DEF_LOGPURCHASE = "fb_def_logPurchase";
    private const string TRACK_PARAM_FB_DEF_CURRENCY = "fb_def_currency";
    private const string TRACK_PARAM_FIRE_RUSH = "fireRush";
    private const string TRACK_PARAM_FIRE_RUSH_NB = "fireRushNb";
    private const string TRACK_PARAM_FIRST_LOAD = "firstLoad";
    private const string TRACK_PARAM_GAME_RUN_NB = "gameRunNb";
    private const string TRACK_PARAM_GENDER = "gender";
    private const string TRACK_PARAM_GLOBAL_EVENT_ID = "glbEventID";
    private const string TRACK_PARAM_GLOBAL_EVENT_TYPE = "glbEventType";
    private const string TRACK_PARAM_GOLDEN_FRAGMENTS = "goldenFragments";
    private const string TRACK_PARAM_GPURAM = "gpuRam";
    private const string TRACK_PARAM_HARD_CURRENCY = "hardCurrency";
    private const string TRACK_PARAM_HC_EARNED = "hcEarned";
    private const string TRACK_PARAM_HC_REVIVE = "hcRevive";
    private const string TRACK_PARAM_HIGHEST_BASE_MULTIPLIER = "highestBaseMultiplier";
    private const string TRACK_PARAM_HIGHEST_MULTIPLIER = "highestMultiplier";
    private const string TRACK_PARAM_HOUSTON_TRANSACTION_ID = "houstonTransactionID";
    private const string TRACK_PARAM_HUNGRY_LETTERS_NB = "hungryLettersNb";
    private const string TRACK_PARAM_IN_GAME_ID = "InGameId";
    private const string TRACK_PARAM_INITIALQUALITY = "initialQuality";
    private const string TRACK_PARAM_IS_FATAL = "isFatal";
    private const string TRACK_PARAM_IS_HACKER = "isHacker";
    private const string TRACK_PARAM_IS_LOADED = "isLoaded";
    private const string TRACK_PARAM_IS_PAYING_SESSION = "isPayingSession";
    private const string TRACK_PARAM_IS_SUCCESS = "isSuccess";
    private const string TRACK_PARAM_ITEM = "item";
    private const string TRACK_PARAM_ITEM_ID = "itemID";
    private const string TRACK_PARAM_ITEM_QUANTITY = "itemQuantity";
    private const string TRACK_PARAM_LAB_BOOST = "labBoost";
    private const string TRACK_PARAM_LAB_HP = "labHp";
    private const string TRACK_PARAM_LAB_POWER = "labPower";
    private const string TRACK_PARAM_LAB_SPEED = "labSpeed";    
    private const string TRACK_PARAM_LANGUAGE = "language";
    private const string TRACK_PARAM_LOADING_TIME = "loadingTime";
    private const string TRACK_PARAM_LOCATION = "location";    
    private const string TRACK_PARAM_MAP_USAGE = "mapUsedNB";
    private const string TRACK_PARAM_MARKETING_OPTION = "marketing_optin";
    private const string TRACK_PARAM_MAX_REACHED = "maxReached";
    private const string TRACK_PARAM_MB_AVAILABLE_END = "mbAvailable_end";
    private const string TRACK_PARAM_MB_AVAILABLE_START = "mbAvailable_start";
    private const string TRACK_PARAM_MAX_XP = "maxXp";    
    private const string TRACK_PARAM_MISSION_DIFFICULTY = "missionDifficulty";
    private const string TRACK_PARAM_MISSION_TARGET = "missionTarget";
    private const string TRACK_PARAM_MISSION_TYPE = "missionType";
    private const string TRACK_PARAM_MISSION_VALUE = "missionValue";
    private const string TRACK_PARAM_MONEY_CURRENCY = "moneyCurrency";
    private const string TRACK_PARAM_MONEY_IAP = "moneyIAP";
    private const string TRACK_PARAM_MONEY_USD = "moneyUSD";
    private const string TRACK_PARAM_EVENT_MULTIPLIER = "multiplier";
    private const string TRACK_PARAM_NB_ADS_LTD = "nbAdsLtd";
    private const string TRACK_PARAM_NB_ADS_SESSION = "nbAdsSession";
    private const string TRACK_PARAM_NB_VIEWS = "nbViews";
    private const string TRACK_PARAM_NETWORK = "network";
    private const string TRACK_PARAM_NETWORK_TYPE_END = "network_type_end";
    private const string TRACK_PARAM_NETWORK_TYPE_START = "network_type_start";
    private const string TRACK_PARAM_NEW_AREA = "newArea";
    private const string TRACK_PARAM_OFFER_NAME = "offerName";
    private const string TRACK_PARAM_OFFER_TYPE = "offerType";
    private const string TRACK_PARAM_ORIGINAL_AREA = "originalArea";
    private const string TRACK_PARAM_PAID = "paid";
    private const string TRACK_PARAM_PET1 = "pet1";
    private const string TRACK_PARAM_PET2 = "pet2";
    private const string TRACK_PARAM_PET3 = "pet3";
    private const string TRACK_PARAM_PET4 = "pet4";
    private const string TRACK_PARAM_PETNAME = "petName";
    private const string TRACK_PARAM_PLAYER_ID = "playerID";
    private const string TRACK_PARAM_PLAYER_PROGRESS = "playerProgress";
    private const string TRACK_PARAM_PLAYING_MODE = "playingMode";
    private const string TRACK_PARAM_POPUP_ACTION = "popupAction";
    private const string TRACK_PARAM_POPUP_MODULAR_VERSION = "popup_modular_version";
    private const string TRACK_PARAM_POPUP_NAME = "popupName";
    private const string TRACK_PARAM_POWER_TIME = "powerTime";
    private const string TRACK_PARAM_PROMOTION_TYPE = "promotionType";
    private const string TRACK_PARAM_PROVIDER = "provider";
    private const string TRACK_PARAM_PROVIDER_AUTH = "providerAuth";
    private const string TRACK_PARAM_PVP_MATCHES_PLAYED = "pvpMatchesPlayed";
    private const string TRACK_PARAM_RADIUS = "radius";
    private const string TRACK_PARAM_RANK = "rank";
    private const string TRACK_PARAM_RANKING = "ranking";
    private const string TRACK_PARAM_RARITY = "rarity";
    private const string TRACK_PARAM_RATE_RESULT = "rateResult";
    private const string TRACK_PARAM_RECORDINGS = "recordings";
    private const string TRACK_PARAM_RESULT = "result";
    private const string TRACK_PARAM_REWARD_TIER = "rewardTier";
    private const string TRACK_PARAM_REWARD_TYPE = "rewardType";
    private const string TRACK_PARAM_SC_EARNED = "scEarned";
    private const string TRACK_PARAM_SCORE = "score";
    private const string TRACK_PARAM_SECTION = "section";
    private const string TRACK_PARAM_SIZE = "size";
    private const string TRACK_PARAM_SOFT_CURRENCY = "softCurrency";
    private const string TRACK_PARAM_EVENT_SCORE_RUN = "scoreRun";
    private const string TRACK_PARAM_EVENT_SCORE_TOTAL = "scoreTotal";
    private const string TRACK_PARAM_SESSION_PLAY_TIME = "sessionPlaytime";
    private const string TRACK_PARAM_SESSIONS_COUNT = "sessionsCount";
	private const string TRACK_PARAM_SKU = "sku";
    private const string TRACK_PARAM_SOCIAL_NETWORK_OPTION = "sns_optin";
    private const string TRACK_PARAM_SOURCE_OF_PET = "sourceOfPet";
    private const string TRACK_PARAM_SOURCE = "source";
    private const string TRACK_PARAM_SPECIAL_OFFER_ACTION = "specialOfferAction";
    private const string TRACK_PARAM_STATUS = "status";
    private const string TRACK_PARAM_STEP_DURATION = "stepDuration";
    private const string TRACK_PARAM_STEP_NAME = "stepName";
    private const string TRACK_PARAM_STOP_CAUSE = "stopCause";
    private const string TRACK_PARAM_STORE_INSTALLED = "storeInstalled";
    private const string TRACK_PARAM_STORE_TRANSACTION_ID = "storeTransactionID";
    private const string TRACK_PARAM_SUBVERSION = "SubVersion";
    private const string TRACK_PARAM_SUPER_FIRE_RUSH_NB = "superFireRushNb";
    private const string TRACK_PARAM_TIME_PLAYED = "timePlayed";
    private const string TRACK_PARAM_TIME_SPENT = "timeSpent";
    private const string TRACK_PARAM_TRACKING_ID = "trackingID";
    private const string TRACK_PARAM_GLOBAL_TOP_CONTRIBUTOR = "topContributor";
    private const string TRACK_PARAM_TOTAL_EGG_BOUGHT_HC = "totalEggBought";
    private const string TRACK_PARAM_TOTAL_EGG_FOUND = "totalEggFound";
    private const string TRACK_PARAM_TOTAL_EGG_OPENED = "totalEggOpened";
    private const string TRACK_PARAM_TOTAL_DURATION = "totalDuration";
    private const string TRACK_PARAM_TOTAL_PLAYTIME = "totalPlaytime";
    private const string TRACK_PARAM_TOTAL_PURCHASES = "totalPurchases";
    private const string TRACK_PARAM_TOTAL_SPECIAL_DRAGONS_UNLOCKED = "totalSpecialDragonsUnlocked";
    private const string TRACK_PARAM_TOTAL_STORE_VISITS = "totalStoreVisits";
    private const string TRACK_PARAM_TOURNAMENT_SKU = "tournamentSku";
    private const string TRACK_PARAM_TRIGGERED = "triggered";
	private const string TRACK_PARAM_TYPE = "type";
    private const string TRACK_PARAM_TYPE_BUILD_VERSION = "type_buildversion";
    private const string TRACK_PARAM_TYPE_NOTIF = "typeNotif";
    private const string TRACK_PARAM_UNLOCK_TYPE = "unlockType";    
    private const string TRACK_PARAM_UPCOMING_LEAGUE = "upcomingLeague";
    private const string TRACK_PARAM_VERSION_QUALITY_FORMULA = "versionQualityFormula";
    private const string TRACK_PARAM_VERSION_REVISION = "versionRevision";
    private const string TRACK_PARAM_XP = "xp";
    private const string TRACK_PARAM_YEAR_OF_BIRTH = "yearOfBirth";
    private const string TRACK_PARAM_ZONE = "zone";

    //------------------------------------------------------------------------//
    private void Track_SendEvent(HDTrackingEvent _e) {
        TrackingEvent tEvent = TrackingManager.SharedInstance.GetNewTrackingEvent(_e.name);
        if (tEvent != null) {
            foreach (KeyValuePair<string, object> pair in _e.data) {                
                tEvent.SetParameterValue(pair.Key, Track_ProcessParam(pair.Key, pair.Value));
            }
#if !EDITOR_MODE
            TrackingManager.SharedInstance.SendEvent(tEvent);
#endif
        }
    }

    private object Track_ProcessParam(string key, object value)
    {
        switch (key)
        {
            case TRACK_PARAM_ACQ_MARKETING_ID:
                // We need to recalculate the value of marketing id because it takes Calety a while to retrieve it so recalculating it
                // right before the event is sent maximizes our chances the value is ready                              
                string marketingId = PersistencePrefs.GetMarketingId();
                if (string.IsNullOrEmpty(marketingId))
                {
                    marketingId = GameSessionManager.SharedInstance.GetDeviceMarketingID();
                    if (string.IsNullOrEmpty(marketingId))
                    {
                        marketingId = MARKETING_ID_NOT_AVAILABLE;
                    }
                    else
                    {
                        // Marketing id is stored in prefs once retrieved successfully from device in order to be able to use it immediately next time it's required
                        // since retrieving it from device may take a while
                        PersistencePrefs.SetMarketingId(marketingId);
                    }

                    value = marketingId;                
                }

                if (marketingId != null)
                {
                    PersistencePrefs.SetLatestMarketingIdNotified(marketingId);
                }
                break;
        }

        return value;
    }      
    //------------------------------------------------------------------------//


    private void Track_AddParamSubVersion(HDTrackingEvent _e) {
        Track_AddParamString(_e, TRACK_PARAM_SUBVERSION, "WorldLaunch");
    }

    private void Track_AddParamProviderAuth(HDTrackingEvent _e) {
        string value = null;
        if (TrackingPersistenceSystem != null && !string.IsNullOrEmpty(TrackingPersistenceSystem.SocialPlatform)) {
            value = TrackingPersistenceSystem.SocialPlatform;
        }

        if (string.IsNullOrEmpty(value)) {
            value = "SilentLogin";
        }

        Track_AddParamString(_e, TRACK_PARAM_PROVIDER_AUTH, value);
    }

    private void Track_AddParamPlayerID(HDTrackingEvent _e) {
        string value = null;
        if (TrackingPersistenceSystem != null && !string.IsNullOrEmpty(TrackingPersistenceSystem.SocialID)) {
            value = TrackingPersistenceSystem.SocialID;
        }

        if (string.IsNullOrEmpty(value)) {
            value = "NotDefined";
        }

        Track_AddParamString(_e, TRACK_PARAM_PLAYER_ID, value);
    }

    private void Track_AddParamServerAccID(HDTrackingEvent _e) {
        int value = PersistencePrefs.ServerUserIdAsInt;
        _e.data.Add(TRACK_PARAM_IN_GAME_ID, value);
    }

    private void Track_AddParamAbTesting(HDTrackingEvent _e) {
        _e.data.Add(TRACK_PARAM_AB_TESTING, "");
    }

    private void Track_AddParamHighestDragonXp(HDTrackingEvent _e) {
        int value = 0;
        if (UsersManager.currentUser != null)
        {
			DragonDataClassic highestDragon = UsersManager.currentUser.GetHighestDragon();
            if (highestDragon != null && highestDragon.progression != null)
            {
                value = (int)highestDragon.progression.xp;
            }
        }

        _e.data.Add(TRACK_PARAM_MAX_XP, value);
    }

    private void Track_AddParamPlayerProgress(HDTrackingEvent _e) {
        int value = (UsersManager.currentUser != null) ? UsersManager.currentUser.GetPlayerProgress() : 0;
        _e.data.Add(TRACK_PARAM_PLAYER_PROGRESS, value);
    }

    private void Track_AddParamPlayerGoldenFragments(HDTrackingEvent _e)
    {
        int value = (UsersManager.currentUser != null) ? (int)UsersManager.currentUser.goldenEggFragments : 0;
        _e.data.Add(TRACK_PARAM_GOLDEN_FRAGMENTS, value);
    }

    private void Track_AddParamPlayerPC(HDTrackingEvent _e)
    {
        int value = (UsersManager.currentUser != null) ? (int)UsersManager.currentUser.pc : 0;
        _e.data.Add(TRACK_PARAM_HARD_CURRENCY, value);
    }

    private void Track_AddParamPlayerSC(HDTrackingEvent _e)
    {
        int value = (UsersManager.currentUser != null) ? (int)UsersManager.currentUser.coins : 0;
        _e.data.Add(TRACK_PARAM_SOFT_CURRENCY, value);
    }

    private void Track_AddParamSessionsCount(HDTrackingEvent _e) {
        int value = (TrackingPersistenceSystem != null) ? TrackingPersistenceSystem.SessionCount : 0;
        _e.data.Add(TRACK_PARAM_SESSIONS_COUNT, value);
    }

    private void Track_AddParamGameRoundCount(HDTrackingEvent _e) {
        int value = (TrackingPersistenceSystem != null) ? TrackingPersistenceSystem.GameRoundCount : 0;
        _e.data.Add(TRACK_PARAM_GAME_RUN_NB, value);
    }

    private void Track_AddParamTotalPlaytime(HDTrackingEvent _e) {
        int value = (TrackingPersistenceSystem != null) ? TrackingPersistenceSystem.TotalPlaytime : 0;
        _e.data.Add(TRACK_PARAM_TOTAL_PLAYTIME, value);
    }

    private void Track_AddParamPets(HDTrackingEvent _e, List<string> pets) {
        // 4 pets are currently supported
        string pet1 = null;
        string pet2 = null;
        string pet3 = null;
        string pet4 = null;
        if (pets != null) {
            int count = pets.Count;
            if (count > 0) {
                pet1 = pets[0];
            }

            if (count > 1) {
                pet2 = pets[1];
            }

            if (count > 2) {
                pet3 = pets[2];
            }

            if (count > 3) {
                pet4 = pets[3];
            }
        }
        if (!string.IsNullOrEmpty(pet1))
            pet1 = Translate_PetSkuToTrackingName(pet1);
        Track_AddParamString(_e, TRACK_PARAM_PET1, pet1);

        if (!string.IsNullOrEmpty(pet2))
            pet2 = Translate_PetSkuToTrackingName(pet2);
        Track_AddParamString(_e, TRACK_PARAM_PET2, pet2);

        if (!string.IsNullOrEmpty(pet3))
            pet3 = Translate_PetSkuToTrackingName(pet3);
        Track_AddParamString(_e, TRACK_PARAM_PET3, pet3);

        if (!string.IsNullOrEmpty(pet4))
            pet4 = Translate_PetSkuToTrackingName(pet4);
        Track_AddParamString(_e, TRACK_PARAM_PET4, pet4);
    }

    private void Track_AddParamTotalPurchases(HDTrackingEvent _e) {
        if (TrackingPersistenceSystem != null) {
            _e.data.Add(TRACK_PARAM_TOTAL_PURCHASES, TrackingPersistenceSystem.TotalPurchases);
        }
    }

    private void Track_AddParamRunsAmount(HDTrackingEvent _e) {
        if (TrackingPersistenceSystem != null) {
            _e.data.Add(TRACK_PARAM_DEATH_IN_CURRENT_RUN_NB, Session_RunsAmountInCurrentRound);
        }
    }

    private void Track_AddParamEggsPurchasedWithHC(HDTrackingEvent _e) {
        if (TrackingPersistenceSystem != null) {
            _e.data.Add(TRACK_PARAM_TOTAL_EGG_BOUGHT_HC, TrackingPersistenceSystem.EggSPurchasedWithHC);
        }
    }

    private void Track_AddParamEggsFound(HDTrackingEvent _e) {
        if (TrackingPersistenceSystem != null) {
            _e.data.Add(TRACK_PARAM_TOTAL_EGG_FOUND, TrackingPersistenceSystem.EggsFound);
        }
    }

    private void Track_AddParamEggsOpened(HDTrackingEvent _e) {
        if (TrackingPersistenceSystem != null) {
            _e.data.Add(TRACK_PARAM_TOTAL_EGG_OPENED, TrackingPersistenceSystem.EggsOpened);
        }
    }

    private void Track_AddParamString(HDTrackingEvent _e, string paramName, string value) {
        // null is not a valid value for Calety
        if (value == null) {
            value = "";
        }

        _e.data.Add(paramName, value);
    }

    private void Track_AddParamBoolAsInt(HDTrackingEvent _e, string paramName, bool value) {
        int valueToSend = (value) ? 1 : 0;
        _e.data.Add(paramName, valueToSend);
    }
    
    private void Track_AddParamBool(HDTrackingEvent _e, string paramName, bool value) {
        _e.data.Add(paramName, value);
    }

	private void Track_AddParamInt(HDTrackingEvent _e, string paramName, int value) {
		_e.data.Add(paramName, value);
	}

	private void Track_AddParamLong(HDTrackingEvent _e, string paramName, long value) {
		_e.data.Add(paramName, value);
	}

    private void Track_AddParamFloat(HDTrackingEvent _e, string paramName, float value) {
        // MAX value accepted by ETL
        const float MAX = 999999999.99f;
        if (value > MAX) {
            value = MAX;
        }
        _e.data.Add(paramName, value);
    }

    private string Track_UserCurrencyToString(UserProfile.Currency currency) {
        string returnValue = "";
        switch (currency) {
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

    private string Track_CoordinatesToString(Vector3 coord) {
        return "x=" + coord.x.ToString("0.0") + ", y=" + coord.y.ToString("0.0");
    }

    private void Track_AddParamLanguage(HDTrackingEvent _e) {
        string language = DeviceUtilsManager.SharedInstance.GetDeviceLanguage();
        if (string.IsNullOrEmpty(language)) {
            language = "ERROR";
        } else {
            // We need to separate language from country (en-GB)
            string[] tokens = language.Split('-');
            if (tokens != null && tokens.Length > 1) {
                language = tokens[0];
            }
        }

        Track_AddParamString(_e, TRACK_PARAM_LANGUAGE, language);
    }    

    private void Track_AddParamTrackingID(HDTrackingEvent _e) {
        Track_AddParamString(_e, TRACK_PARAM_TRACKING_ID, TrackingPersistenceSystem.UserID);
    }
    #endregion

    #region translate
    public string Translate_PetSkuToTrackingName(string _petSku) {
        string ret = _petSku;
        DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _petSku);
        if (petDef != null) {
            ret = petDef.GetAsString("trackingName", _petSku);
        }
        return ret;
    }

    public string Translate_DragonDisguiseSkuToTrackingSku(string _disguiseSku) {
        string ret = _disguiseSku;
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _disguiseSku);
        if (def != null) {
            ret = def.GetAsString("trackingSku", _disguiseSku);
        }
        return ret;
    }
    #endregion


    #region session
    // This region is responsible for storing data generated during the current session. These data don't need to be persisted.

    /// <summary>
    /// Enum with the reason why the session has ended. These names have to match the supported values defined for "stopCause" parameter of
    /// "custom.mobile.stop" event described at https://mdc-web-tomcat17.ubisoft.org/confluence/display/dna/Events+Guidelines+for+Mobile+Games#EventsGuidelinesforMobileGames-custom.mobile.start
    /// </summary>
    private enum ESeassionEndReason {
        app_closed,
        no_activity
    }

    public string Session_BuildVersion { get; set; }

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

    private bool Session_HasMenuEverLoaded { get; set; }

    private bool mSession_IsNotifyOnPauseEnabled;

    private bool Session_GameStartSent { get; set; }

    /// <summary>
    /// Whether or not the session is allowed to notify on pause/resume. This is used to avoid session paused/resumed events when the user
    /// goes to background because an ad or a purchase is being performed since those actions are considered part of the game
    /// </summary>
    private bool Session_IsNotifyOnPauseEnabled {
        get {
            return mSession_IsNotifyOnPauseEnabled;
        }

        set {
            if (FeatureSettingsManager.IsDebugEnabled) {
                Log("Session_IsNotifyOnPauseEnabled = " + mSession_IsNotifyOnPauseEnabled + " -> " + value);
            }

            mSession_IsNotifyOnPauseEnabled = value;
        }
    }

    private int Session_HungryLettersCount { get; set; }

    private Dictionary<UserProfile.Currency, int> Session_RewardsInRound;
    private Dictionary<UserProfile.Currency, int> Session_RewardsInRoundPaid;

    private bool Session_IsARoundRunning { get; set; }

    protected override void Session_Reset() {
        base.Session_Reset();

        Session_GameStartSent = false;
        Session_IsPayingSession = false;
        Session_IsAdSession = false;
        Session_PlayTime = 0f;
        Session_RunsAmountInCurrentRound = 0;
        Session_LastDeathType = null;
        Session_LastDeathSource = null;
        Session_LastDeathCoordinates = null;
        Session_IsFirstTime = false;
        Session_IsNotifyOnPauseEnabled = true;
        Session_HasMenuEverLoaded = false;
        Session_HungryLettersCount = 0;
        Session_IsARoundRunning = false;
        if (Session_RewardsInRound != null) {
            Session_RewardsInRound.Clear();
        }
        if (Session_RewardsInRoundPaid != null) {
            Session_RewardsInRoundPaid.Clear();
        }
    }

    private void Session_NotifyRoundStart() {
        Session_IsARoundRunning = true;
        if (Session_RewardsInRound != null) {
            Session_RewardsInRound.Clear();
        }
        if (Session_RewardsInRoundPaid != null) {
            Session_RewardsInRoundPaid.Clear();
        }
    }

    private void Session_NotifyRoundEnd() {
        Session_IsARoundRunning = false;

        string economyGroupString = EconomyGroupToString(EEconomyGroup.REWARD_RUN);
        UserProfile userProfile = UsersManager.currentUser;

        if (Session_RewardsInRound != null) {
            // TrackingManager is notified with all currencies earned during the run
            foreach (KeyValuePair<UserProfile.Currency, int> pair in Session_RewardsInRound) {
                if (pair.Value > 0) {
                    Track_EarnResources(economyGroupString, pair.Key, pair.Value, (int)userProfile.GetCurrency(pair.Key), false);
                }
            }
        }

        if (Session_RewardsInRoundPaid != null) {
            // TrackingManager is notified with all currencies earned during the run
            foreach (KeyValuePair<UserProfile.Currency, int> pair in Session_RewardsInRoundPaid) {
                if (pair.Value > 0) {
                    Track_EarnResources(economyGroupString, pair.Key, pair.Value, (int)userProfile.GetCurrency(pair.Key), true);
                }
            }
        }
    }

    private void Session_AccumRewardInRun(UserProfile.Currency currency, int amount, bool paid) {
        if ( paid )
        {
            if (Session_RewardsInRoundPaid == null) {
                Session_RewardsInRoundPaid = new Dictionary<UserProfile.Currency, int>();
            }

            if (Session_RewardsInRoundPaid.ContainsKey(currency)) {
                int currentAmount = Session_RewardsInRoundPaid[currency];
                Session_RewardsInRoundPaid[currency] = currentAmount + amount;
            } else {
                Session_RewardsInRoundPaid.Add(currency, amount);
            }
        }
        else
        {
            if (Session_RewardsInRound == null) {
                Session_RewardsInRound = new Dictionary<UserProfile.Currency, int>();
            }

            if (Session_RewardsInRound.ContainsKey(currency)) {
                int currentAmount = Session_RewardsInRound[currency];
                Session_RewardsInRound[currency] = currentAmount + amount;
            } else {
                Session_RewardsInRound.Add(currency, amount);
            }
        }
    }
    #endregion

    #region performance
    private bool Performance_IsTrackingEnabled { get; set; }
    private float Performance_TrackingDelay { get; set; }

    private float m_Performance_LastTrackTime = 0;

    private Bounds m_Performance_TrackArea = new Bounds();
    private int m_Performance_TickCounter;

    private bool m_Performance_FireRush;
    private float m_Performance_FireRushStartTime;

    private void Performance_Reset() {
        Performance_IsTrackingEnabled = false;
        Performance_TrackingDelay = 0f;
        m_Performance_LastTrackTime = 0f;
        m_Performance_TickCounter = 0;
        m_Performance_FireRush = false;
        m_Performance_FireRushStartTime = 0f;
    }

    private float Performance_Timer {
        get {
            return Time.unscaledTime;
        }
    }

    private void Reset_Performance_Tracker() {
        Performance_IsTrackingEnabled = FeatureSettingsManager.instance.IsPerformanceTrackingEnabled;
        if (Performance_IsTrackingEnabled) {
            Performance_TrackingDelay = FeatureSettingsManager.instance.PerformanceTrackingDelay;
            Vector3 currentPosition = InstanceManager.player.transform.position;
            m_Performance_TrackArea.SetMinMax(currentPosition, currentPosition);
            m_Performance_TickCounter = 0;
            m_Performance_FireRush = false;
            m_Performance_FireRushStartTime = m_Performance_LastTrackTime = Performance_Timer;
        }

    }

    private void Performance_Tracker() {
        float currentTime = Performance_Timer;
        float elapsedTime = currentTime - m_Performance_LastTrackTime;

        m_Performance_TrackArea.Encapsulate(InstanceManager.player.transform.position);
        m_Performance_TickCounter++;

        if (!m_Performance_FireRush) {
            if (InstanceManager.player.breathBehaviour.IsFuryOn()) {
                if (currentTime - m_Performance_FireRushStartTime > 1.0f) {
                    m_Performance_FireRush = true;
                }
            }
        } else {
            m_Performance_FireRushStartTime = currentTime;
        }


        if (elapsedTime > Performance_TrackingDelay) {
            Debug.Log("Performance tracking enabled: " + Performance_IsTrackingEnabled + " Delay: " + Performance_TrackingDelay);
            int fps = (int)((float)m_Performance_TickCounter / Performance_TrackingDelay);
            //int radius = (int)Mathf.Max(m_Performance_TrackArea.size.x, m_Performance_TrackArea.size.y);
            Track_PerformanceTrack((int)RewardManager.xp, fps, m_Performance_TrackArea.min, m_Performance_TrackArea.max, m_Performance_FireRush);
            //            Track_PerformanceTrack();

            Reset_Performance_Tracker();
            //            Debug.Log("Performance tracking event at: " + currentTime);
        }
    }
    #endregion


    #region debug
    private void Debug_Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            //Notify_RoundStart(0, 0, null, null);
            Debug.Log("gamRoundCount = " + TrackingPersistenceSystem.GameRoundCount + " Session_PlayTime = " + Session_PlayTime);
        }
    }
    #endregion
}
