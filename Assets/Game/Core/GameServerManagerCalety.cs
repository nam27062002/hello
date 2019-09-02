/// <summary>
/// This class is responsible for implementing the <c>GameServerManager</c>interface by using Calety.
/// </summary>

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using Calety.Server;
using FGOL.Server;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameServerManagerCalety : GameServerManager {
	//------------------------------------------------------------------------//
	// LISTENER IMPLEMENTATION												  //
	//------------------------------------------------------------------------//
	#region listener
	private class GameSessionDelegate : GameSessionManager.GameSessionListener {
		const string tag = "GameSessionDelegate";

		public bool m_logged = false;

		// WAITING FLAGS
		public bool m_waitingLoginResponse = false;
		public bool m_waitingGetUniverse = false;
		public bool m_waitingMergeResponse = false;

		public SimpleJSON.JSONClass m_lastRecievedUniverse = null;
		public int m_saveDataCounter = 0;

		public delegate bool OnResponse(string response, int responseCode);

		private OnResponse m_onResponse;

		public bool IsNewAppVersionNeeded { get; set; }

		public GameSessionDelegate(OnResponse onResponse) {
			Debug.TaggedLog(tag, "GameSessionDelegate instantiated");
			m_onResponse = onResponse;
		}

		// Triggers when user was succesfully logged into our server
		public override void onLogInToServer() {
			Debug.TaggedLog(tag, "onLogInToServer");
			m_waitingLoginResponse = false;            
			if(!m_logged) {
				m_logged = true;
				Messenger.Broadcast<bool>(MessengerEvents.LOGGED, m_logged);
			}

			if(m_onResponse != null) {
				JSONNode response = ServerManager.SharedInstance.GetServerAuthBConfig();
				string responseAsString = null;
				if(response != null) {
					responseAsString = response.ToString();
				}

				m_onResponse(responseAsString, (IsNewAppVersionNeeded) ? 426 : 200);
			}
		}

		public override void onLogInFailed() {
			Debug.TaggedLog(tag, "onLogInToServer");
			m_waitingLoginResponse = false;

			if(m_onResponse != null) {
				m_onResponse(null, 401);
			}
		}

		// Triggers when logout from our server is called
		public override void onLogOutFromServer() {
			Debug.TaggedLog(tag, "onLogOutFromServer");
			m_waitingLoginResponse = false;
			if(m_logged) {
				m_logged = false;
				Messenger.Broadcast<bool>(MessengerEvents.LOGGED, m_logged);
			}

			// An error is sent, just in case the client is waiting for a response for any command            
			if(m_onResponse != null) {
				//m_onResponse(null, 500);
			}

            GameServerManager.SharedInstance.OnLogOut();

            // no problem, continue playing
        }

		// The GC login is finished after receiving GC token
		public override void onGameCenterAuthenticationFinished() {
			Debug.TaggedLog(tag, "onGameCenterAuthenticationFinished");
		}

		// Trying to use GC functionality without being authenticated previously
		public override void onGameCenterNotAuthenticatedException() {
			Debug.TaggedLog(tag, "onGameCenterNotAuthenticatedException");
		}

		// The user cancelled the GC login or iOS is returning a cancelled GC state
		public override void onGameCenterAuthenticationCancelled() {
			Debug.TaggedLog(tag, "onGameCenterAuthenticationCancelled");
		}

		// There are merge conflicts and asks to show the merging popup
		public override void onMergeShowPopupNeeded(CaletyConstants.PopupMergeType eType, JSONNode kLocalAccount, JSONNode kCloudAccount) {
			m_waitingMergeResponse = false;            
			Debug.TaggedLog(tag, "onMergeShowPopupNeeded");
            Messenger.Broadcast<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(MessengerEvents.MERGE_SHOW_POPUP_NEEDED, eType, kLocalAccount, kCloudAccount);
        }

        public override void onSessionExpired() {
            Debug.TaggedLog(tag, "onSessionExpired");
            GameServerManager.SharedInstance.OnLogOut();
        }

        public override void onShowMaintenanceMode() {         
			Debug.TaggedLog(tag, "onShowMaintenanceMode");
            GameServerManager.SharedInstance.OnLogOut();
		}

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeSucceeded() {
			m_waitingMergeResponse = false;
			Debug.TaggedLog(tag, "onMergeSucceeded");
            Messenger.Broadcast(MessengerEvents.MERGE_SUCCEEDED);                
        }

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeFailed(bool bNeedToUnAuthenticateFromSocialPlatform = false) {
			m_waitingMergeResponse = false;
			Debug.TaggedLog(tag, "onMergeFailed");
            Messenger.Broadcast(MessengerEvents.MERGE_FAILED);                        
        }

		// The user has requested a password to do a cross platform merge
		public override void onMergeXPlatformPass(string strParole, long iSecondsToExpire) {
			Debug.TaggedLog(tag, "onMergeXPlatformPass " + strParole);
		}

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeXPlatformSucceeded(string platform, string platformUserId) {
			Debug.TaggedLog(tag, "onMergeXPlatformSucceeded");
		}

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeXPlatformFailed(GameSessionManager.EMergeXPlatformErrorResult eMergeError = GameSessionManager.EMergeXPlatformErrorResult.E_MERGE_ERROR_UNKNOWN) {
			Debug.TaggedLog(tag, "onMergeXPlatformFailed");
		}

		// Notify the game that a new version of the app is released. Show a popup that redirects to the store.
		public override void onNewAppVersionNeeded() {
			Debug.TaggedLog(tag, "onNewAppVersionNeeded");
			CacheServerManager.SharedInstance.SaveCurrentVersionAsObsolete();
			IsNewAppVersionNeeded = true;

            GameServerManager.SharedInstance.OnLogOut();
        }

        public override void onCountryBlacklisted() {
            CacheServerManager.SharedInstance.SetCountryBlacklisted(true);
            GameServerManager.SharedInstance.OnLogOut();
        }


		// Notify the game that a new version of the app is released. Show a popup that redirects to the store.
		public override void onUserBlackListed() {
			Debug.TaggedLog(tag, "onUserBlackListed");
		}

		// Returns current achievements state
		public override void onGetAchievementsInfo(Dictionary<string, GameCenterManager.GameCenterAchievement> kAchievementsInfo) {
			Debug.TaggedLog(tag, "onGetAchievementsInfo");
		}

		// Returns current leaderboard score
		public override void onGetLeaderboardScore(string strLeaderboardSKU, int iScore, int iRank) {
			Debug.TaggedLog(tag, "onGetLeaderboardScore");
		}

		// Some API needs a restart of the game
		public override void onRequestGameReset() {
			Debug.TaggedLog(tag, "onRequestGameReset");
		}

        public override void onShowAccountsConflict() { // When the same GC account is used in different devices this will make the game to show a popup for exit 
            Debug.TaggedLog(tag, "onShowAccountsConflict");
            GameServerManager.SharedInstance.OnLogOut();
        }

        public override void onUserBanned(long iMilliseconds) {  // Called when user is banned
            Debug.TaggedLog(tag, "onUserBanned");
            GameServerManager.SharedInstance.OnLogOut();
        }

        public override void onShowLostConnection () {
			Debug.TaggedLog(tag, "onShowLostConnection");
			GameServerManager.SharedInstance.OnConnectionLost();
		} 

		public override void onNetworkFailedInAPacket() {
			Debug.TaggedLog(tag, "onNetworkFailedInAPacket");
			GameServerManager.SharedInstance.OnConnectionLost();
		}

        // When a gameplay action was successful it passes the result to gameplay
        public override void onGamePlayActionProcessed(string strAction, JSONNode kResponseData)
        {            
            GameServerManager.SharedInstance.OnGameActionProcessed(strAction, kResponseData);
        }

        // When a gameplay action fails it returns the error code
        public override void onGamePlayActionFailed(string strAction, int iErrorStatus)
        {
            GameServerManager.SharedInstance.OnGameActionFailed(strAction, iErrorStatus);            
        } 

        void ResetWaitingFlags() {
			m_waitingLoginResponse = false;
			m_waitingGetUniverse = false;
			m_waitingMergeResponse = false;
		}
	}
	#endregion

	//------------------------------------------------------------------------//
	// HUNGRY DRAGON COMMANDS												  //
	//------------------------------------------------------------------------//
	#region hungry_dragon_commands
	private GameSessionDelegate m_delegate;

    public override void Destroy()
    {
        base.Destroy();
        Login_Destroy();
    }

    public override void Update()
    {
        Connection_Update();
        Commands_Update();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void ExtendedConfigure() {        
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");

		// Init server game details
		ServerConfig kServerConfig = new ServerConfig();

		if(settingsInstance != null) {
			kServerConfig.m_strServerURL = settingsInstance.m_strLocalServerURL[settingsInstance.m_iBuildEnvironmentSelected];
			kServerConfig.m_strServerPort = settingsInstance.m_strLocalServerPort[settingsInstance.m_iBuildEnvironmentSelected];
			kServerConfig.m_strServerDomain = settingsInstance.m_strLocalServerDomain[settingsInstance.m_iBuildEnvironmentSelected];
			kServerConfig.m_eBuildEnvironment = (CaletyConstants.eBuildEnvironments)settingsInstance.m_iBuildEnvironmentSelected;

			kServerConfig.m_strClientVersion = settingsInstance.GetClientBuildVersion();
		}

		kServerConfig.m_bIsNewVersionRestrictive = false;
		kServerConfig.m_bIsUsingHTTPS = false;

#if UNITY_EDITOR
        kServerConfig.m_strClientPlatformBuild = "editor";
#elif UNITY_ANDROID
		kServerConfig.m_strClientPlatformBuild = "android";
        #elif UNITY_IOS
		kServerConfig.m_strClientPlatformBuild = "ios";
#endif

        kServerConfig.m_strApplicationParole = "avefusilmagnifica";
        // Avoid Messages. We never Flush so it was stacking
        kServerConfig.m_bUseMessagingTracking = false;

        kServerConfig.m_iConnectTimeOut = 6000;
        kServerConfig.m_iReadTimeOut = 6000;

        // Social platform in Calety depends on our social platform (either Fb or Weibo), which depends on the user's country
        SocialUtils.EPlatform socialPlatform = SocialPlatformManager.GetSocialPlatform();
        switch (socialPlatform)
        {
            case SocialUtils.EPlatform.Facebook:
                settingsInstance.m_iSocialPlatformSelected = (int)CaletyConstants.eSocialPlatforms.FACEBOOK;
                break;

            case SocialUtils.EPlatform.Weibo:
                settingsInstance.m_iSocialPlatformSelected = (int)CaletyConstants.eSocialPlatforms.WEIBO;
                break;
        }
        ServerManager.SharedInstance.Initialise(ref kServerConfig);

		m_delegate = new GameSessionDelegate(Commands_OnResponse);
		GameSessionManager.SharedInstance.SetListener(m_delegate);

        //[DGR] Extra api calls which are needed by Dragon but are not defined in Calety. Maybe they could be added in Calety when it supports offline mode
        CaletyExtensions_Init();

        Reset();
    }

    public override void Reset() {
        Login_Init();
        Commands_Init();
        Connection_Init();
        m_isProcessingConnectionLost = false;
    }

    public override void OnGameActionProcessed(string cmd, SimpleJSON.JSONNode response)
    {
        string responseAsString = (response == null) ? null : response.ToString();
        CaletyExtensions_OnCommandDefaultResponse(responseAsString, cmd, 200);
    }

    public override void OnGameActionFailed(string cmd, int errorCode)
    {
        Error error = GetLogicServerInternalError(errorCode);
        Commands_OnExecuteCommandDone(error, null);
    }    

    protected override void InternalPing(ServerCallback callback, bool highPriority=false) {
		Commands_EnqueueCommand(ECommand.Ping, null, callback, highPriority);
	}

    private bool m_isProcessingConnectionLost;

	protected override void InternalOnConnectionLost() {		
	    Log("SERVER DOWN REPORTED..... " + Commands_ToString());		

        // This stuff is done only if it's not already being processed
        if (!m_isProcessingConnectionLost)
        {
            // We need to use this flag because this method could be called several times when processing NetworkManager.SharedInstance.CancelRequest()
            m_isProcessingConnectionLost = true;

            Commands_OnServerDown();

            NetworkManager.SharedInstance.CancelRequests();
            ServerManager.SharedInstance.CancelPendingCommands();
            NetworkManager.SharedInstance.ReportServerDownShouldBeSolved();

            // We need to log out from server to overcome a Calety's limitation. Calety was made for an online game considering that the game would reload every time a request fails, which guarantees that 
            // action packet id will get reseted. We need to simulate that behaviour to make sure the action packet ids is going to be in sync with the server after network recovery
            GameSessionManager.SharedInstance.LogOutFromServer();

            Connection_OnServerDown();

            m_isProcessingConnectionLost = false;
        }
    }
    #endregion

    #region login
    private const string PREFS_LATEST_UID = "latestUID";

    private enum ELoginState
    {
        LoggingIn,
        LoggedIn,
        NotLoggedIn
    };

    private ELoginState m_loginState;
    private ELoginState Login_State
    {
        get { return m_loginState; }
        set
        {
            if (m_loginState != value)
            {
                m_loginState = value;

                switch (m_loginState)
                {
                    case ELoginState.LoggedIn:
                        // Updates the user id cached if it's different
                        if (GameSessionManager.SharedInstance.IsLogged())
                        {
                            string uID = GameSessionManager.SharedInstance.GetUID();
                            if (uID != GetLatestUIDFromCache())
                            {
                                SetLatestUIDFromCache(uID);
                            }
                        }
                        break;
                }
            }
        }
    }               

    private Queue<ServerCallback> Login_Callbacks { get; set; }

    private void Login_Init()
    {
        Messenger.AddListener<bool>(MessengerEvents.LOGGED, Login_OnLogged);

        Login_State = ELoginState.NotLoggedIn;      
        if (Login_Callbacks != null)
        {
            Login_Callbacks.Clear();
        }  
    }    

    private void Login_Destroy()
    {
        Messenger.RemoveListener<bool>(MessengerEvents.LOGGED, Login_OnLogged);
    }

    protected override void InternalAuth(ServerCallback callback, bool highPriority=false)
    {
        if (callback != null)
        {
            if (Login_Callbacks == null)
            {
                Login_Callbacks = new Queue<ServerCallback>();
            }

            Login_Callbacks.Enqueue(callback);
        }

        if (Login_State == ELoginState.LoggedIn)
        {
            Login_OnAuthResponse(null, null);            
        }
        else
        {            
            if (Login_State == ELoginState.NotLoggedIn)
            {
                Login_State = ELoginState.LoggingIn;
                Commands_EnqueueCommand(ECommand.Auth, null, Login_OnAuthResponse, highPriority);
            }
        }                
    }

    private void Login_OnAuthResponse(Error error, ServerResponse response)
    {
        Login_State = (error == null) ? ELoginState.LoggedIn : ELoginState.NotLoggedIn;        
        
        if (Login_Callbacks != null)
        {
            ServerCallback loginCallback;     
            while (Login_Callbacks.Count > 0)
            {
                loginCallback = Login_Callbacks.Dequeue();
                if (loginCallback != null)
                {
                    loginCallback(error, response);
                }
            }
        }
    }
	
	/// <summary>
	/// 
	/// </summary>
	public override void LogOut()
    {
		// The response is immediate. We don't want to treat it as a command because it could be trigger at any moment and we don't want it to mess with a command that is being processed
		GameSessionManager.SharedInstance.LogOutFromServer(false);        
    }

    public override void OnLogOut()
    {
        Login_State = ELoginState.NotLoggedIn;

        // Something went wrong on server side so we should cancel commands
        OnConnectionLost();
    }

	/// <summary>
	/// 
	/// </summary>
	public override bool IsLoggedIn()
    {
        //return m_delegate.m_logged;
        return Login_State == ELoginState.LoggedIn;
	}   

    private void Login_OnLogged(bool logged)
    {
        if (logged && Login_State != ELoginState.LoggedIn)
        {
            Login_State = ELoginState.LoggedIn;
        }
        else if (!logged && Login_State != ELoginState.NotLoggedIn)
        {
            Login_State = ELoginState.NotLoggedIn;
        }
    }    

    private string GetLatestUIDFromCache()
    {
        return Prefs.GetStringPlayer(PREFS_LATEST_UID);
    }

    private void SetLatestUIDFromCache(string value)
    {
        Prefs.SetStringPlayer(PREFS_LATEST_UID, value);
    }

    /// <summary>
    /// Returns the most recent user ID in our server known by the client. Every time the client logs in our server the user ID is cached so that the client can have
    /// this information in offline mode or right after the game is launched.
    /// </summary>
    /// <returns>Returns the user ID in our server if the user is logged, otherwise it returns the user ID when the user last logged in our server.</returns>
    public override string GetLatestUID()
    {
        if (GameSessionManager.SharedInstance.IsLogged())
        {
            return GameSessionManager.SharedInstance.GetUID();
        }
        else
        {
            return GetLatestUIDFromCache();
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public override void GetServerTime(ServerCallback callback) {
		Commands_EnqueueCommand(ECommand.GetTime, null, callback);            
	}

	/// <summary>
	/// 
	/// </summary>
	public override void GetPersistence(ServerCallback callback) {
		Commands_EnqueueCommand(ECommand.GetPersistence, null, callback);        
	}

	/// <summary>
	/// 
	/// </summary>
	public override void SetPersistence(string persistence, ServerCallback callback) {        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("persistence", persistence);
		Commands_EnqueueCommand(ECommand.SetPersistence, parameters, callback);        
	}

	/// <summary>
	/// 
	/// </summary>
	public override void UpdateSaveVersion(bool prelimUpdate, ServerCallback callback) {
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("fgolID", GameSessionManager.SharedInstance.GetUID());
		parameters.Add("prelimUpdate", prelimUpdate.ToString());
		Commands_EnqueueCommand(ECommand.UpdateSaveVersion, parameters, callback);
	}

	/// <summary>
	/// 
	/// </summary>
	public override void GetQualitySettings(ServerCallback callback) {
		Commands_EnqueueCommand(ECommand.GetQualitySettings, null, callback);
	}

	/// <summary>
	/// Uploads the quality settings information calculated by client to the server
	/// </summary>
	/// <param name="qualitySettings">Json in string format of the quality settings to upload</param>    
	public override void SetQualitySettings(string qualitySettings, ServerCallback callback) {
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("qualitySettings", qualitySettings);
		Commands_EnqueueCommand(ECommand.SetQualitySettings, parameters, callback);
	}

    public override void GetGameSettings(ServerCallback callback)
    {
        Commands_EnqueueCommand(ECommand.GetGameSettings, null, callback);
    }    

    /// <summary>
    /// 
    /// </summary>
    public override void SendPlayTest(bool silent, string playTestUserId, string trackingData, ServerCallback callback) {
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("silent", silent.ToString());
		parameters.Add("playTestUserId", playTestUserId);
		parameters.Add("trackingData", trackingData);
		Commands_EnqueueCommand(ECommand.PlayTest, parameters, callback);                
	}

    public override void SendTrackLoading(string step, int deltaTime, bool isFirstTime, int sessionsCount, ServerCallback callback) {
        deltaTime = Mathf.Max(1, deltaTime);

        JSONNode json = new JSONClass();
        json["step"] = step;
        json["delta"] = deltaTime.ToString();
        json["appLaunches"] = sessionsCount.ToString();
        json["freshInstall"] = isFirstTime.ToString();

        Dictionary<string, string> parameters = new Dictionary<string, string>();                
        parameters.Add("body", json.ToString());        
        Commands_EnqueueCommand(ECommand.TrackLoading, parameters, callback);                       
    }

    public override void GetPendingTransactions(ServerCallback callback) {          
        Commands_EnqueueCommand(ECommand.PendingTransactions_Get, null, callback);
    }

    protected override void DoConfirmPendingTransactions(List<Transaction> transactions, ServerCallback callback) {
        JSONNode json = GetPendingTransactionsJSON(transactions);        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

        Commands_EnqueueCommand(ECommand.PendingTransactions_Confirm, parameters, callback);
    }

    public override void SetLanguage(string serverCode, ServerCallback onDone)
    {
        JSONNode json = new JSONClass();
        json["language"] = serverCode;
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

        Commands_EnqueueCommand(ECommand.Language_Set, parameters, onDone);
    }
    
    public override void CurrencySpent( string currency, int balance, int amount, string group, ServerCallback onDone)
    {
        SendCurencyFluctuation( currency, balance, -amount, false, group, onDone );
    }

    public override void CurrencyEarned(string currency, int balance, int amount, string group, bool paid, ServerCallback onDone)
    {
        SendCurencyFluctuation( currency, balance, amount, paid , group, onDone );
    }
    
    private void SendCurencyFluctuation(string currency, int balance, int amount, bool paid, string action, ServerCallback onDone)
    {
        JSONNode json = new JSONClass();
        json["currency"] = currency;
        json["amount"] = amount;
        json["type"] = paid ? "paid" : "free";
        json["action"] = action;
        json["balance"] = balance;

        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "body", json.ToString() }
        };

        Commands_EnqueueCommand(ECommand.CurrencyFluctuation, parameters, onDone);
    }
    

    override public void GlobalEvent_TMPCustomizer(ServerCallback _callback) {
		Commands_EnqueueCommand(ECommand.GlobalEvents_TMPCustomizer, null, _callback);
	}

		/// <summary>
	/// Get an event for this user from the server.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action.</param>
	public override void GlobalEvent_GetEvent(int _eventID, ServerCallback _callback) 
	{
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.GlobalEvents_GetEvent, parameters, _callback);
	}

	/// <summary>
	/// Get the current value and (optionally) the leaderboard for a specific event.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose state we want.</param>
	/// <param name="_getLeaderboard">Whether to retrieve the leaderboard as well or not (top 100 + player).</param>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_GetState(int _eventID, ServerCallback _callback) {
		// Compose parameters and enqeue command
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.GlobalEvents_GetState, parameters, _callback);
	}

	/// <summary>
	/// Register a score to a target event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_score">The score to be registered.</param>
	/// <param name="_callback">Callback action.</param>
	override public void GlobalEvent_RegisterScore(int _eventID, int _score, ServerCallback _callback) {
		// Compose parameters and enqeue command
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		parameters.Add("progress", _score.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.GlobalEvents_RegisterScore, parameters, _callback);
	}

	/// <summary>
	/// Reward the player for his contribution to an event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action. Given rewards?</param>
	override public void GlobalEvent_GetRewards(int _eventID, ServerCallback _callback) {
		// Compose parameters and enqeue command
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.GlobalEvents_GetRewards, parameters, _callback);
	}

	override public void GlobalEvent_GetLeaderboard(int _eventID, ServerCallback _callback) 
	{
		// Compose parameters and enqeue command
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.GlobalEvents_GetLeadeboard, parameters, _callback);
	}


#region HD_LiveData
		
	public override void HDEvents_GetMyLiveData(ServerCallback _callback) {
		Commands_EnqueueCommand(ECommand.HDLiveEvents_GetMyEvents, null, _callback);
 	}

    public override void HDEvents_GetMyEventOfType(int _typeToUpdate, ServerCallback _callback) { 
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("typeToUpdate", _typeToUpdate.ToString());
        Commands_EnqueueCommand(ECommand.HDLiveEvents_GetMyEvents, parameters, _callback);
	}

    public override void HDLiveData_GetMyLeagues(ServerCallback _callback) {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("updateLeagues", true.ToString());
        Commands_EnqueueCommand(ECommand.HDLiveEvents_GetMyEvents, parameters, _callback);
    }

    //--------------------------------------------------------------------------

    public override void HDEvents_GetDefinition(int _eventID, ServerCallback _callback) {
		JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

		Commands_EnqueueCommand(ECommand.HDLiveEvents_GetEventDefinition, parameters, _callback);
	}

	public override void HDEvents_GetMyProgess(int _eventID, ServerCallback _callback) {
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.HDLiveEvents_GetMyProgress, parameters, _callback);
	}

    public override void HDEvents_GetLeaderboard(int _eventID, ServerCallback _callback)
    {
        // Compose parameters and enqeue command
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
        Commands_EnqueueCommand(ECommand.HDLiveEvents_GetLeaderboard, parameters, _callback);
    }

    public override void HDEvents_Tournament_SetScore(int _eventID, int _score, SimpleJSON.JSONNode _build, ServerCallback _callback) {
        JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        json.Add("score", _score.ToString(JSON_FORMAT));
        json.Add("build", _build.ToString());
        json.Add("fetchLeaderboard", "true");
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

        Commands_EnqueueCommand(ECommand.HDLiveEvents_Tournament_SetScore, parameters, _callback);
    }

	public override void HDEvents_Tournament_EnterEvent(int _eventID, string _type, long _amount, int _matchmakingValue, ServerCallback _callback) {
		JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        json.Add("amount", _amount.ToString(JSON_FORMAT));
        json.Add("elo", _matchmakingValue.ToString(JSON_FORMAT));
        json.Add("type", _type);
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());
        
		Commands_EnqueueCommand(ECommand.HDLiveEvents_Tournament_Enter, parameters, _callback);
	}    

    public override void HDEvents_Tournament_GetRefund(int _eventID, ServerCallback _callback) {
		JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

		Commands_EnqueueCommand(ECommand.HDLiveEvents_Tournament_GetRefund, parameters, _callback);
	}

	public override void HDEvents_Tournament_GetMyReward(int _eventID, ServerCallback _callback) {
        JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());
		
        Commands_EnqueueCommand(ECommand.HDLiveEvents_Tournament_GetMyReward, parameters, _callback);
	}

	public override void HDEvents_Quest_AddProgress(int _eventID, int _score, ServerCallback _callback) {
		JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        json.Add("progress", _score.ToString(JSON_FORMAT));
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());
        
		Commands_EnqueueCommand(ECommand.HDLiveEvents_Quest_AddProgress, parameters, _callback);
	}

    public override void HDEvents_Quest_GetMyReward(int _eventID, ServerCallback _callback) {
        JSONNode json = new JSONClass();
        json.Add("eventId", _eventID.ToString(JSON_FORMAT));
        
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());
		
        Commands_EnqueueCommand(ECommand.HDLiveEvents_Quest_GetMyReward, parameters, _callback);
	}

	public override void HDEvents_FinishMyEvent(int _eventID, ServerCallback _callback) {
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters.Add("eventId", _eventID.ToString(JSON_FORMAT));
		Commands_EnqueueCommand(ECommand.HDLiveEvents_FinishMyEvent, parameters, _callback);
	}


    //--------------------------------------------------------------------------

    public override void HDLeagues_GetSeason(bool _fetchLeaderboard, ServerCallback _callback) {
        JSONNode json = new JSONClass();
        json.Add("fetchLeaderboard", _fetchLeaderboard.ToString(JSON_FORMAT));

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

        Commands_EnqueueCommand(ECommand.HDLeagues_GetSeason, parameters, _callback);
    }

    public override void HDLeagues_GetLeague(string _sku, ServerCallback _callback) {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("sku", _sku);

        Commands_EnqueueCommand(ECommand.HDLeagues_GetLeague, parameters, _callback);
    }

    public override void HDLeagues_GetAllLeagues(ServerCallback _callback) {
        Commands_EnqueueCommand(ECommand.HDLeagues_GetAllLeagues, null, _callback);
    }

    public override void HDLeagues_GetLeaderboard(ServerCallback _callback) {
        Commands_EnqueueCommand(ECommand.HDLeagues_GetLeaderboard, null, _callback);
    }

    public override void HDLeagues_SetScore(long _score, SimpleJSON.JSONClass _build, bool _fetchLeaderboard, ServerCallback _callback) {
        JSONNode json = new JSONClass();
        json.Add("score", _score.ToString(JSON_FORMAT));
        json.Add("build", _build);
        json.Add("fetchLeaderboard", _fetchLeaderboard.ToString(JSON_FORMAT));

        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("body", json.ToString());

        Commands_EnqueueCommand(ECommand.HDLeagues_SetScore, parameters, _callback);
    }

    public override void HDLeagues_GetMyRewards(ServerCallback _callback) {
        Commands_EnqueueCommand(ECommand.HDLeagues_GetMyRewards, null, _callback);
    }

    public override void HDLeagues_FinishMySeason(ServerCallback _callback) {
        Commands_EnqueueCommand(ECommand.HDLeagues_FinishMySeason, null, _callback);
    }
    #endregion

    //------------------------------------------------------------------------//
    // INTERNAL COMMANDS MANAGEMENT											  //
    //------------------------------------------------------------------------//
    #region commands
    /// <summary>
    /// Max amount of retries when an authorization error is got after sending a command. If this happens then Auth is tried to sent before resending the command that caused the error.
    /// It's set to 0 because Calety already retries to send commands.
    /// </summary>
    private const int COMMANDS_MAX_AUTH_RETRIES = 0;

	private enum ECommand {
		Unknown = -1,
		None,
        Auth,
		Ping,
		Login,
		GetTime,
		GetPersistence,
		SetPersistence,
		UpdateSaveVersion,
		GetQualitySettings,
		SetQualitySettings,
        GetGameSettings,
        PlayTest,
        TrackLoading,
        PendingTransactions_Get,
        PendingTransactions_Confirm,
        Language_Set,
        CurrencyFluctuation,

        GlobalEvents_TMPCustomizer,
		GlobalEvents_GetEvent,		// params: int _eventID. Returns an event description
		GlobalEvents_GetState,		// params: int _eventID. Returns the current total value of an event
		GlobalEvents_RegisterScore,	// params: int _eventID, float _score
		GlobalEvents_GetRewards,	// params: int _eventID
		GlobalEvents_GetLeadeboard,	// params: int _eventID


		HDLiveEvents_GetMyEvents,
		HDLiveEvents_GetEventDefinition,// params: int _eventID. Returns an event description
        HDLiveEvents_GetMyProgress,		// params: int _eventID. Returns the event progres for this player        
        HDLiveEvents_GetLeaderboard,   // params: int _eventID        
        HDLiveEvents_Tournament_Enter,       // params: int _eventID. entrance type, amount, matchmaking value
        HDLiveEvents_Tournament_SetScore,     // params: int _eventID. _score on tournaments
        HDLiveEvents_Tournament_GetRefund,			// params: int _eventID
		HDLiveEvents_Tournament_GetMyReward,		// params: int _eventID
        HDLiveEvents_Quest_AddProgress,	// params: int _eventID, _contribution on quests
        HDLiveEvents_Quest_GetMyReward,		// params: int _eventID
		HDLiveEvents_FinishMyEvent,		// params: int _eventID
        

        HDLeagues_GetSeason,            // params: string _sku
        HDLeagues_GetLeague,
        HDLeagues_GetAllLeagues,
        HDLeagues_GetLeaderboard,
        HDLeagues_SetScore,
        HDLeagues_GetMyRewards,
        HDLeagues_FinishMySeason
    }    

	/// <summary>
	/// 
	/// </summary>
	private class Command {        
		public ECommand Cmd { get; private set; }
		public Dictionary<string, string> Parameters { get; set; }
		public ServerCallback Callback { get; private set; }
        public float LastLogin { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public void Reset() {
			Cmd = ECommand.None;
			Parameters = null;
			Callback = null;            
        }

		/// <summary>
		/// 
		/// </summary>
		public void Setup(ECommand cmd, Dictionary<string, string> parameters, ServerCallback callback) {
			Cmd = cmd;
			Parameters = parameters;
			Callback = callback;
            LastLogin = 0f;
        }        
	}

	/// <summary>
	/// Pool of commands to reduce the impact in memory of sending commands to the server. Every time a <c>Command</c> object is needed we should check this pool first to get the object
	/// and once we're done we have to return the object to this pool
	/// </summary>
	private Queue<Command> Commands_Pool { get; set; }	

    private const int COMMANDS_PRIORITIES_COUNT = 2;

    private List<Command>[] Commands_List { get; set; }

    private Command m_commandsCurrentCommand;
	private Command Commands_CurrentCommand
    {
        get { return m_commandsCurrentCommand; }
        set
        {
            m_commandsCurrentCommand = value;
        }
    }		
	
	/// <summary>
	/// 
	/// </summary>
	private void Commands_Init() {
		Commands_Pool = new Queue<Command>();	
        
        Commands_List = new List<Command>[COMMANDS_PRIORITIES_COUNT];
        for (int i = 0; i < COMMANDS_PRIORITIES_COUNT; i++) {
            Commands_List[i] = new List<Command>();
        }		
	}

	/// <summary>
	/// 
	/// </summary>
	private Command Commands_GetCommand() {
		Command returnValue = null;
		if(Commands_Pool.Count == 0) {
			returnValue = new Command();
		} else {
			returnValue = Commands_Pool.Dequeue();
		}

		return returnValue;
	}

	/// <summary>
	/// 
	/// </summary>
	private void Commands_ReturnCommand(Command command) {
		command.Reset();

		if(Commands_Pool.Contains(command)) {            
            LogError("This command is already in the pool");
		} else {            
			Commands_Pool.Enqueue(command);
		}
	}

    private bool Commands_IsEmpty() {
        bool returnValue = true;
        for (int i = 0; i < COMMANDS_PRIORITIES_COUNT && returnValue; i++) {
            returnValue = Commands_List[i].Count == 0;
        }

        return returnValue;
    }

    private void Commands_Update() {
        if (Commands_CurrentCommand == null && !Commands_IsEmpty()) {            
            // If the connection is down and there are commands to send then we try to recover the connection before trying to send any commands
            if (m_connectionState == EConnectionState.Down) {
                Connection_Recover(Commands_OnRecovered);
            } else {
                int count;
                int i;
                Command command = null;
                ELoginState loginState = Login_State;

                // Picks a command to be processed
                for (i = 0; i < COMMANDS_PRIORITIES_COUNT; i++) {
                    count = Commands_List[i].Count;

                    for (int j = 0; j < count && command == null; j++) {
                        command = Commands_List[i][j];

                        // The command needs the user to be logged in before being processed
                        if (Commands_NeedsToBeLoggedIn(command.Cmd)) {                            
                            switch (loginState) {
                                case ELoginState.LoggingIn:
                                    // This command can't be processed yet because log in process is still being processed
                                    command = null;
                                    break;                                
                            }
                        }                        
                    }

                    if (command != null) {
                        break;
                    }
                }

                // Processes the command
                if (command != null) {
                    if (Commands_NeedsToBeLoggedIn(command.Cmd)) {
                        if (loginState == ELoginState.NotLoggedIn) {
                            // If this command hasn't triggered a log in process yet then it has to do it
                            if (command.LastLogin == 0f) {
                                command.LastLogin = Time.realtimeSinceStartup;

                                // It shouldn't be processed yet. It will have another chance once the recovery process is completed
                                command = null;
                                Connection_Recover(Commands_OnRecovered);
                            }
                        }
                    } 
                    
                    if (command != null) {                        
                        // The command has to be deleted from the list                    
                        Commands_List[i].Remove(command);
                        Commands_ProcessCommand(command);
                    }                            
                }
            }            
        }
    }

    private void Commands_ProcessCommand(Command command) {
        Commands_CurrentCommand = command;
        
        //Make sure we have a valid parameters object as before or after command callbacks may modify it
        if (command.Parameters == null)
        {
            command.Parameters = new Dictionary<string, string>();
        }

        command.Parameters["version"] = Globals.GetApplicationVersion();
        command.Parameters["platform"] = Globals.GetPlatform().ToString();

        Commands_RunCommand(command);        
    }

    private void Commands_OnRecovered() {                    
        // If the connection couldn't be recovered then all commands have to be discarded
        if (m_connectionState == EConnectionState.Down) {
            Commands_OnServerDown();
        }        
    }

	/// <summary>
	/// 
	/// </summary>
	private void Commands_EnqueueCommand(ECommand command, Dictionary<string, string> parameters, ServerCallback callback, bool highPriority=false) {
		Command cmd = Commands_GetCommand();
		cmd.Setup(command, parameters, callback);
        
        int index = (highPriority) ? 0 : 1;
        Commands_List[index].Add(cmd);
        
        Log("Command requested " + command.ToString());
	}		    

    private bool Commands_NeedsToBeLoggedIn(ECommand command)
    {
        bool returnValue = false;

        switch (command) {
            case ECommand.GetPersistence:
            case ECommand.SetPersistence:            
            case ECommand.SetQualitySettings: // The user is required to be logged to set its quality settings to prevent anonymous users from messing with the quality settings of other users who have the same device model
            case ECommand.GlobalEvents_GetState:
            case ECommand.GlobalEvents_GetEvent:
            case ECommand.GlobalEvents_GetRewards:
            case ECommand.GlobalEvents_GetLeadeboard:
            case ECommand.GlobalEvents_RegisterScore:
            case ECommand.PendingTransactions_Get:
            case ECommand.PendingTransactions_Confirm:

            case ECommand.HDLiveEvents_GetMyEvents:
			case ECommand.HDLiveEvents_GetEventDefinition:
			case ECommand.HDLiveEvents_GetMyProgress:
			case ECommand.HDLiveEvents_Quest_AddProgress:
            case ECommand.HDLiveEvents_Quest_GetMyReward:
            case ECommand.HDLiveEvents_GetLeaderboard:
            case ECommand.HDLiveEvents_Tournament_SetScore:
            case ECommand.HDLiveEvents_Tournament_Enter:
			case ECommand.HDLiveEvents_Tournament_GetMyReward:
			case ECommand.HDLiveEvents_FinishMyEvent:
            case ECommand.HDLiveEvents_Tournament_GetRefund:

            case ECommand.HDLeagues_GetSeason:
            case ECommand.HDLeagues_GetLeague:
            case ECommand.HDLeagues_GetAllLeagues:
            case ECommand.HDLeagues_GetLeaderboard:
            case ECommand.HDLeagues_SetScore:
            case ECommand.HDLeagues_GetMyRewards:
            case ECommand.HDLeagues_FinishMySeason:
                returnValue = true;
                break;
        }

        return returnValue;
    }

	/// <summary>
	/// 
	/// </summary>
	private void Commands_RunCommand(Command command) {        
        Log("RunCommand " + command.Cmd + " CurrentCommand = " + Commands_CurrentCommand.Cmd);
        // Commands have to be executed one by one since we're not using actions on server side       

        //
        // [DGR] 
        // NOTE: Please use Command_SendCommand() to send the command to the server. This method will add "uid" and "token" parameters automatically if the 
        // command is not anonymous
        //        
		if(Commands_CurrentCommand == command) {

            // If there's no connection then request timeout error is simulated
            if (m_connectionState == EConnectionState.Down) {
                // Request timeout
                Commands_OnResponse(null, 408);
                return;
            } else if (Commands_NeedsToBeLoggedIn(command.Cmd) && !IsLoggedIn()) {
                // If the command needs to be logged in but the game is not currently logged in then an error is returned                                        
                LogError("Command " + command.Cmd + " requires the user to be logged in but she's not");

                // Unauthorized error is simulated
                Commands_OnResponse(null, 401);
                return;
            }

            Dictionary<string, string> parameters = command.Parameters;

            switch (command.Cmd) {
                case ECommand.Ping: {
                        Command_SendCommand(COMMAND_PING);
                    } break;

                case ECommand.GetTime: {
                        Command_SendCommand(COMMAND_TIME);
                    } break;

                case ECommand.Auth: {                        
                        Log("Command Auth");

                        //GameSessionManager.SharedInstance.ResetAnonymousPlatformUserID();
                        GameSessionManager.SharedInstance.LogInToServer();
                    }
                    break;

                case ECommand.Login: {                        
                        Log("Command Login");

                        ServerManager.SharedInstance.Server_SendAuth(parameters["platformId"], parameters["platformToken"]);
                    } break;

                case ECommand.GetPersistence: {
                        Command_SendCommand(COMMAND_GET_PERSISTENCE);
                    } break;

                case ECommand.SetPersistence: {
                        Command_SendCommand(COMMAND_SET_PERSISTENCE, null, null, parameters["persistence"]);
                    } break;

                case ECommand.UpdateSaveVersion: {
                        // [DGR] SERVER: To change for an actual request to the server
                        Commands_OnResponse(null, 200);
                    } break;

                case ECommand.GetQualitySettings: {
                        // The user is not required to be logged to request the quality settings for her device     
                        Command_SendCommand(COMMAND_GET_QUALITY_SETTINGS);
                    } break;

                case ECommand.SetQualitySettings: {
                        Command_SendCommand(COMMAND_SET_QUALITY_SETTINGS, null, null, parameters["qualitySettings"]);
                    } break;

                case ECommand.GetGameSettings: {
                        Command_SendCommand(COMMAND_GET_GAME_SETTINGS);
                    }
                    break;

                case ECommand.PlayTest: {
                        bool silent = (parameters["silent"].ToLower() == "true");
                        string cmd = (silent) ? COMMAND_PLAYTEST_A : COMMAND_PLAYTEST_B;

                        // This endpoint is anonymous but we need to send the playtest user id for tracking purposes
                        Dictionary<string, string> kParams = new Dictionary<string, string>();
                        kParams["uid"] = parameters["playTestUserId"];
                        Command_SendCommand(cmd, kParams, null, parameters["trackingData"]);
                    } break;

                case ECommand.TrackLoading: {
                        Command_SendCommand(COMMAND_TRACK_LOADING, null, null, parameters["body"]);
                    } break;

                case ECommand.PendingTransactions_Get: {
                        Command_SendCommand(COMMAND_PENDING_TRANSACTIONS_GET);
                    } break;

                case ECommand.PendingTransactions_Confirm: {
                        JSONClass data = null;
                        string paramsAsString = parameters["body"];
                        if (!string.IsNullOrEmpty(paramsAsString)) {
                            data = JSON.Parse(paramsAsString) as JSONClass;
                        }

                        Command_SendCommandAsGameAction(COMMAND_PENDING_TRANSACTIONS_CONFIRM, data, false);
                    } break;

                case ECommand.GlobalEvents_TMPCustomizer: {
                        Command_SendCommand(COMMAND_GLOBAL_EVENTS_TMP_CUSTOMIZER, null, null, "");
                    } break;

                case ECommand.GlobalEvents_GetState:
                case ECommand.GlobalEvents_GetEvent:
                case ECommand.GlobalEvents_GetRewards:
                case ECommand.GlobalEvents_GetLeadeboard: {
                        Dictionary<string, string> kParams = new Dictionary<string, string>();
                        kParams["eventId"] = parameters["eventId"];
                        string global_event_command = "";
                        switch (command.Cmd) {
                            case ECommand.GlobalEvents_GetState: global_event_command = COMMAND_GLOBAL_EVENTS_GET_STATE; break;
                            case ECommand.GlobalEvents_GetEvent: global_event_command = COMMAND_GLOBAL_EVENTS_GET_EVENT; break;
                            case ECommand.GlobalEvents_GetRewards: global_event_command = COMMAND_GLOBAL_EVENTS_GET_REWARDS; break;
                            case ECommand.GlobalEvents_GetLeadeboard: global_event_command = COMMAND_GLOBAL_EVENTS_GET_LEADERBOARD; break;
                        }

                        Command_SendCommand(global_event_command, kParams);
                    } break;

                case ECommand.GlobalEvents_RegisterScore: {
                        Dictionary<string, string> kParams = new Dictionary<string, string>();
                        kParams["eventId"] = parameters["eventId"];
                        kParams["progress"] = parameters["progress"];
                        Command_SendCommand(COMMAND_GLOBAL_EVENTS_REGISTER_SCORE, kParams, parameters, "");
                        // progress					
                    } break;

                case ECommand.HDLiveEvents_GetMyEvents: {
                        Dictionary<string, string> kParams = new Dictionary<string, string>();
                        kParams.Add("isChildren", GDPRManager.SharedInstance.IsAgeRestrictionEnabled().ToString().ToLower());
                        Command_SendCommand(COMMAND_HD_LIVE_EVENTS_GET_MY_EVENTS, kParams);
                    } break;
                
                case ECommand.HDLiveEvents_GetMyProgress:
                case ECommand.HDLiveEvents_GetLeaderboard:                
                case ECommand.HDLiveEvents_FinishMyEvent:                 {
                        Dictionary<string, string> kParams = new Dictionary<string, string>();
                        kParams["eventId"] = parameters["eventId"];
                        string global_event_command = "";
                        switch (command.Cmd) {                            
                            case ECommand.HDLiveEvents_GetMyProgress: global_event_command = COMMAND_HD_LIVE_EVENTS_GET_MY_PROGRESS; break;
                            case ECommand.HDLiveEvents_GetLeaderboard: global_event_command = COMMAND_HD_LIVE_EVENTS_GET_LEADERBOARD; break;                            
                            case ECommand.HDLiveEvents_FinishMyEvent: global_event_command = COMMAND_HD_LIVE_EVENTS_FINISH_MY_EVENT; break;                            
                        }

                        Command_SendCommand(global_event_command, kParams);
                    } break;

                case ECommand.HDLiveEvents_GetEventDefinition: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_GET_EVENT_DEF, data, true);
                } break;

                case ECommand.HDLiveEvents_Tournament_Enter: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_TOURNAMENT_ENTER, data, true);
                    } break;

                case ECommand.HDLiveEvents_Tournament_SetScore: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_TOURNAMENT_SET_SCORE, data, true);
                    }
                    break;

                case ECommand.HDLiveEvents_Tournament_GetRefund: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_TOURNAMENT_GET_REFUND, data, true);
                    }
                    break;

                case ECommand.HDLiveEvents_Tournament_GetMyReward: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_TOURNAMENT_GET_MY_REWARD, data, true);
                    } break;

                case ECommand.HDLiveEvents_Quest_AddProgress: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_QUEST_REGISTER_PROGRESS, data, true);
                    } break;

                case ECommand.HDLiveEvents_Quest_GetMyReward: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LIVE_EVENTS_QUEST_GET_MY_REWARD, data, true);
                    } break;


                //--------------------------------------------------------------
                case ECommand.HDLeagues_GetSeason: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LEAGUES_GET_SEASON, data, true);
                    }
                    break;
                
                case ECommand.HDLeagues_GetLeague: {
                        Command_SendCommand(COMMAND_HD_LEAGUES_GET_LEAGUE, parameters);
                    }
                    break;

                case ECommand.HDLeagues_GetAllLeagues: {
                        Command_SendCommand(COMMAND_HD_LEAGUES_GET_ALL_LEAGUES);
                    }
                    break;

                case ECommand.HDLeagues_GetLeaderboard:
                    Command_SendCommand(COMMAND_HD_LEAGUES_GET_LEADERBOARD);
                break;

                case ECommand.HDLeagues_SetScore: {
                        JSONClass data = JSON.Parse(parameters["body"]) as JSONClass;
                        Command_SendCommandAsGameAction(ACTION_HD_LEAGUES_SET_SCORE, data, true);
                }
                break;
                
                case ECommand.HDLeagues_GetMyRewards:
                    Command_SendCommandAsGameAction(ACTION_HD_LEAGUES_GET_MY_REWARDS, null, true);
                break;

                case ECommand.HDLeagues_FinishMySeason:
                    Command_SendCommandAsGameAction(ACTION_HD_LEAGUES_FINISH_MY_SEASON, null, true);
                break;
                //--------------------------------------------------------------


                case ECommand.Language_Set: {
                    JSONClass data = null;
                    string paramsAsString = parameters["body"];
                    if (!string.IsNullOrEmpty(paramsAsString))
                    {
                        data = JSON.Parse(paramsAsString) as JSONClass;
                    }
                   
                    Command_SendCommandAsGameAction(COMMAND_LANGUAGE_SET, data, false);
                }
                break;
                case ECommand.CurrencyFluctuation:{
                    JSONClass data = null;
                    string paramsAsString = parameters["body"];
                    if (!string.IsNullOrEmpty(paramsAsString))
                    {
                        data = JSON.Parse(paramsAsString) as JSONClass;
                    }
                    Command_SendCommandAsGameAction(COMMAND_CURRENCY_FLUCTUATION, data, true);
                }
                break;
                default: {                    
                    LogWarning("Missing call to the server in GameServerManagerCalety.Commands_RunCommand() form command " + command.Cmd);

                    // An error is simulated because no information is available
                    Commands_OnResponse(null, 401);
                } break;
            }
		} else {            
            LogError("GameServerManagerCalety error: command " + command.Cmd + " can't be executed because command " + Commands_CurrentCommand.Cmd + " is still being processed.");
		}
	}

    /// <summary>
    /// It sends the command request to the server.
    /// </summary>    
    private void Command_SendCommand(string commandName, Dictionary<string, string> urlParams=null, Dictionary<string, string> headerParams=null, string body=null) {        
        if (!Command_IsAnonymous(commandName)) {
            if (urlParams == null) {
                urlParams = new Dictionary<string, string>();
            }

            string key = "uid";
            if (!urlParams.ContainsKey(key)) {
                urlParams[key] = GameSessionManager.SharedInstance.GetUID();                                
            }

            key = "token";
            if (!urlParams.ContainsKey(key)) {
                urlParams[key] = GameSessionManager.SharedInstance.GetUserToken();
            }            
        }
        
        Log("Command " + commandName + " sent");

        ServerManager.SharedInstance.SendCommand(commandName, urlParams, headerParams, body);

        // Connection checker timer is reseted because a request is already being sent
        Connection_ResetTimer();
    }

    private void Command_SendCommandAsGameAction(string commandName, JSONClass parameters, bool sendNow) {
        GameSessionManager.SharedInstance.SendGamePlayActionInJSON(commandName, parameters, sendNow);
    }

    /// <summary>
    /// Returns whether or not the command passed as a parameter is anonymous. An anonymous command is a command that doesn't require to contain the uid and the session token
    /// for the server to respond.
    /// </summary>        
    private bool Command_IsAnonymous(string commandName) {
        return commandName == COMMAND_PING || commandName == COMMAND_TIME || 
               commandName == COMMAND_GET_QUALITY_SETTINGS || commandName == COMMAND_GET_GAME_SETTINGS;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Commands_OnExecuteCommandDone(Error error, ServerResponse result) {
        if (Commands_CurrentCommand != null) {
            ServerCallback callback = Commands_CurrentCommand.Callback;
            Commands_ReturnCommand(Commands_CurrentCommand);
            Commands_CurrentCommand = null;
            if (callback != null) {
                callback(error, result);
            }
        }		
	}

	/// <summary>
	/// 
	/// </summary>
	private bool Commands_OnResponse(string responseData, int statusCode) {
		Error error = null;
		ServerResponse response = null;

        // Makes sure there's a command waiting for the response
        if (Commands_CurrentCommand == null) {
            return false;
        }
        
        Log("Command " + Commands_CurrentCommand.Cmd.ToString() + " response received");

		// 426 code means that there's a new version of the application available. We simulate that the response was 200 (SUCCESS) because we don't want to force the
		// user to upgrade        
		bool upgradeAvailable = false;
		if(statusCode == 426) {
			statusCode = 200;
			upgradeAvailable = true;
		}

		int status = (int)Math.Floor(statusCode / 100.0);

		switch(status) {
			case 2: {// Respose is ok
				// [DGR] SERVER: Receive response
				//error = new InvalidServerResponseError("(WRONG RESPONSE FORMAT) " + responseData);
				//error = new UserAuthError("test");

				/*if (response.DataAsText != null && response.DataAsText != "")
				{
				    ServerResponse result = Json.Deserialize(response.DataAsText) as ServerResponse;
				    if (result != null)
				    {
				        if (result.ContainsKey("response"))
				        {
				            ServerResponse expectedResult = result["response"] as ServerResponse;

				            if (expectedResult != null)
				            {
				                callback(null, expectedResult);
				            }
				            else
				            {
				                error = new InvalidServerResponseError("(WRONG RESPONSE FORMAT) " + response.DataAsText);
				                LogError(url, error);
				                callback(error, null);
				            }
				        }
				        else if (result.ContainsKey("error"))
				        {
				            ServerResponse errorJson = result["error"] as ServerResponse;

				            string errorMessage = errorJson["message"] as string;
				            string errorName = errorJson["name"] as string;

				            //TODO do we still need status?
				            //string errorStatus = errorJson.ContainsKey("status") ? errorJson["status"] as string : string.Empty;

				            ErrorCodes errorCode = ErrorCodes.UnknownError;

				            try
				            {
				                int errorCodeRaw = errorJson.ContainsKey("code") ? Convert.ToInt32(errorJson["code"]) : -1;

				                object parsedCode = Enum.ToObject(typeof(ErrorCodes), errorCodeRaw);

				                if (Enum.IsDefined(typeof(ErrorCodes), parsedCode))
				                {
				                    errorCode = (ErrorCodes)parsedCode;
				                }
				            }
				            catch (Exception) { }

				            ServerResponse errorData = null;

				            if (errorJson.ContainsKey("data"))
				            {
				                errorData = errorJson["data"] as ServerResponse;
				            }

				            error = new ServerInternalError(errorMessage, errorName, errorCode);

				            bool logError = true;

				            if (errorName != null)
				            {
				                switch (errorName)
				                {
				                    case "AuthError":
				                        error = new AuthenticationError(errorMessage, errorCode);
				                        break;
				                    case "CompatibilityError":
				                        error = new CompatibilityError(errorMessage, errorCode, errorData);
				                        break;
				                    case "UploadDisallowedError":
				                        error = new UploadDisallowedError(errorMessage, errorCode);
				                        logError = false;
				                        break;
				                    case "UserError":
				                        error = new UserAuthError(errorMessage, errorCode);
				                        break;
				                }
				            }

				            if (logError)
				            {
				                LogError(url, error);
				            }

				            callback(error, null);
				        }
				        else if (result.ContainsKey("maintenance"))
				        {
				            error = new MaintenanceError();

				            LogError(url, error);
				            callback(error, null);
				        }
				        else
				        {
				            error = new InvalidServerResponseError("(WRONG FORMAT) " + response.DataAsText);
				            LogError(url, error);
				            callback(error, null);
				        }
				    }
				    else
				    {
				        error = new InvalidServerResponseError("(NOT JSON) " + response.DataAsText);
				        LogError(url, error);
				        callback(error, null);
				    }
				}
				else
				{
				    error = new InvalidServerResponseError("(EMPTY RESPONSE)");

				    LogError(url, error);
				    callback(error, null);
				}*/
    } break;

			case 4: {
				error = new ClientConnectionError("Status code: " + statusCode);                
			} break;
			
			case 5: {
				error = new ServerConnectionError("Status code: " + statusCode);                
			} break;
			
			default: {
				error = new UnknownError("Status code: " + statusCode);                
			} break;
		}

		if(error == null) {
			// No error! Pre-process the response based on command
			response = new ServerResponse();

			// Parse response into a json object
			JSONNode responseJSON = null;
			if(responseData != null) {
				responseJSON = SimpleJSON.JSON.Parse(responseData);                
			}

			switch(Commands_CurrentCommand.Cmd) {
				case ECommand.Login: {
					// [DGR] SERVER: Receive these parameters from server
					response["fgolID"] = GameSessionManager.SharedInstance.GetUID();
					response["sessionToken"] = GameSessionManager.SharedInstance.GetUserToken();					
					if(responseJSON != null) {
						string key = "upgradeAvailable";
						response[key] = upgradeAvailable;

						key = "cloudSaveAvailable";
						response[key] = responseJSON.ContainsKey(key) && Convert.ToBoolean((object)responseJSON[key]);                        
					}
				} break;

				case ECommand.GetTime:
				case ECommand.UpdateSaveVersion: {
					long time = Globals.GetUnixTimestamp();

					// Checks if the response from server can be interpreted
					string key = "t";                
					if(responseJSON != null && responseJSON.ContainsKey(key)) {
						long timeAsLong = responseJSON[key].AsLong;
						time = timeAsLong / 1000;	// Server time comes in millis, convert to seconds
					}

					// [DGR] SERVER: Receive these parameters from server
					response["dateTime"] = time;
					response["unixTimestamp"] = time;

					// Update latest stored server time
					ServerManager.SharedInstance.SetServerTime((double)time);
				} break;

				case ECommand.GetQualitySettings: {
					response["response"] = responseData;

					// statusCode 204 means that the client has to upload its settings to the server
					response["upLoadRequired"] = (statusCode == 204);                    
				} break;

				case ECommand.GlobalEvents_TMPCustomizer:
				case ECommand.GlobalEvents_GetEvent:
				case ECommand.GlobalEvents_GetState:
				case ECommand.GlobalEvents_RegisterScore:
				case ECommand.GlobalEvents_GetRewards: 
				case ECommand.GlobalEvents_GetLeadeboard:{
					// Propagate server response directly as a JSON object
					// [DGR] SERVER: Receive these parameters from server
					response["response"] = responseData;
				} break;

				default: {
					// Propagate server response directly as a string
					// [DGR] SERVER: Receive these parameters from server
					response["response"] = responseData;
				} break;
			}
		} else {
            // Server returned an error            
            if (Commands_CurrentCommand != null) {
                LogWarning(Commands_CurrentCommand.Cmd, error);
            } else {
                LogWarning(ECommand.Unknown, error);
            }            
		}

		Commands_OnExecuteCommandDone(error, response);
		return error == null;       
	}

	private void Commands_OnServerDown() {		
		Error error = new ServerConnectionError("Server down");
		ServerCallback callback;

		// Clears latest command sent to server manager
		if (Commands_CurrentCommand != null) {
			callback = Commands_CurrentCommand.Callback;
			Commands_ReturnCommand(Commands_CurrentCommand);
			Commands_CurrentCommand = null;
			if (callback != null) {
				callback(error, null);
			}
		}

		// Clears commands pending to be sent to server manager
		Command command;

        for (int i = 0; i < COMMANDS_PRIORITIES_COUNT; i++) {
            for (int j = 0; j < Commands_List[i].Count; j++) {
                command = Commands_List[i][j];
                callback = command.Callback;
                Commands_ReturnCommand(command);
                if (callback != null)
                {
                    callback(error, null);
                }
			}

            Commands_List[i].Clear();
        }       
	}

	private string Commands_ToString() {
		string str = "COMMANDS LIST: ";
		if (Commands_List != null) {
            for (int i = 0; i < COMMANDS_PRIORITIES_COUNT; i++) {
                str += "--------------PRIORITY " + i + "-----------------------" + System.Environment.NewLine;
                int count = Commands_List[i].Count;
                for (int j = 0; j < count; j++) {
                    str += Commands_List[i][j].Cmd.ToString() + System.Environment.NewLine;
                }
            }
		}

		str += System.Environment.NewLine + " CURRENT COMMAND: ";

		if (Commands_CurrentCommand != null) {
			str += Commands_CurrentCommand.Cmd.ToString();
		}

		return str;
	}
	#endregion

	//------------------------------------------------------------------------//
	// CALETY EXTENSIONS IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	#region calety_extensions
	private const string COMMAND_PING = "/api/server/ping";
	private const string COMMAND_TIME = "/api/server/time";
	private const string COMMAND_GET_PERSISTENCE = "/api/persistence/get";
	private const string COMMAND_SET_PERSISTENCE = "/api/persistence/set";
	private const string COMMAND_GET_QUALITY_SETTINGS = "/api/aquality/settings";
	private const string COMMAND_SET_QUALITY_SETTINGS = "/api/quality/upload";
    private const string COMMAND_GET_GAME_SETTINGS = "/api/settings/get";
    private const string COMMAND_PLAYTEST_A = "/api/playtest/a";
	private const string COMMAND_PLAYTEST_B = "/api/playtest/b";
    private const string COMMAND_TRACK_LOADING = "/api/loading/step";

    //[TODO] CLEAN THIS SHIT----------------------------------------------------------------------
    private const string COMMAND_GLOBAL_EVENTS_TMP_CUSTOMIZER = "/api/gevent/customizer";
	private const string COMMAND_GLOBAL_EVENTS_GET_EVENT = "/api/gevent/get";
	private const string COMMAND_GLOBAL_EVENTS_GET_STATE = "/api/gevent/progress";
	private const string COMMAND_GLOBAL_EVENTS_REGISTER_SCORE = "/api/gevent/addProgress";
	private const string COMMAND_GLOBAL_EVENTS_GET_REWARDS = "/api/gevent/reward";
	private const string COMMAND_GLOBAL_EVENTS_GET_LEADERBOARD = "/api/gevent/leaderboard";
    //--------------------------------------------------------------------------------

    private const string ACTION_HD_LIVE_EVENTS_GET_EVENT_DEF = "events/get";
    private const string ACTION_HD_LIVE_EVENTS_TOURNAMENT_ENTER = "tournaments/register";
    private const string ACTION_HD_LIVE_EVENTS_TOURNAMENT_SET_SCORE = "tournaments/score/set";
    private const string ACTION_HD_LIVE_EVENTS_TOURNAMENT_GET_REFUND = "tournaments/refund";
    private const string ACTION_HD_LIVE_EVENTS_TOURNAMENT_GET_MY_REWARD = "tournaments/rewards/get";
    private const string ACTION_HD_LIVE_EVENTS_QUEST_REGISTER_PROGRESS = "quests/progress/add";    
    private const string ACTION_HD_LIVE_EVENTS_QUEST_GET_MY_REWARD = "quests/rewards/get";
    private const string COMMAND_HD_LIVE_EVENTS_GET_MY_EVENTS = "/api/levent/getMyEvents";
    
    private const string COMMAND_HD_LIVE_EVENTS_GET_MY_PROGRESS = "/api/levent/getProgress";
    private const string COMMAND_HD_LIVE_EVENTS_GET_LEADERBOARD = "/api/levent/getLeaderboard";    
    private const string COMMAND_HD_LIVE_EVENTS_FINISH_MY_EVENT = "/api/levent/finish";    
    

    private const string ACTION_HD_LEAGUES_GET_SEASON           = "leagues/season/get";
    private const string ACTION_HD_LEAGUES_SET_SCORE            = "leagues/score/set";
    private const string ACTION_HD_LEAGUES_GET_MY_REWARDS       = "leagues/rewards/get";
    private const string ACTION_HD_LEAGUES_FINISH_MY_SEASON     = "leagues/finish";
    private const string COMMAND_HD_LEAGUES_GET_LEAGUE          = "/api/leagues/get";
    private const string COMMAND_HD_LEAGUES_GET_ALL_LEAGUES     = "/api/leagues/getAll";
    private const string COMMAND_HD_LEAGUES_GET_LEADERBOARD     = "/api/leagues/getLeaderboard";

    private const string COMMAND_PENDING_TRANSACTIONS_GET = "/api/ptransaction/getAll";
    private const string COMMAND_PENDING_TRANSACTIONS_CONFIRM = "transaction";
    private const string COMMAND_LANGUAGE_SET = "language";
    private const string COMMAND_CURRENCY_FLUCTUATION = "currencyfluctuation";

    /// <summary>
    /// Initialize Calety's NetworkManager.
    /// </summary>
    private void CaletyExtensions_Init() {
		// All codes need to be handled in order to be sure that the game will continue regardless the network error
		int[] codes = new int[] {
			200, 204, 301, 302, 303, 304, 305, 306, 307, 400, 401, 402, 403, 404,
			405, 406, 407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417, 426,
			500, 501, 502, 503, 504, 505
		};

		// Register all endpoints to the network manager
        // CONTROLLERS
		NetworkManager nm = NetworkManager.SharedInstance;	// Shorter notation
		nm.RegistryEndPoint(COMMAND_PING, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_TIME, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GET_PERSISTENCE, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnGetPersistenceResponse);
		nm.RegistryEndPoint(COMMAND_SET_PERSISTENCE, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GET_QUALITY_SETTINGS, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_SET_QUALITY_SETTINGS, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_GET_GAME_SETTINGS, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_PLAYTEST_A, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_PLAYTEST_B, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_TRACK_LOADING, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_PENDING_TRANSACTIONS_GET, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_LANGUAGE_SET, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
        // nm.RegistryEndPoint(COMMAND_CURRENCY_FLUCTUATION, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);

        nm.RegistryEndPoint(COMMAND_GLOBAL_EVENTS_TMP_CUSTOMIZER, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GLOBAL_EVENTS_GET_EVENT, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GLOBAL_EVENTS_GET_STATE, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GLOBAL_EVENTS_REGISTER_SCORE, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GLOBAL_EVENTS_GET_REWARDS, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_GLOBAL_EVENTS_GET_LEADERBOARD, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);        

		nm.RegistryEndPoint(COMMAND_HD_LIVE_EVENTS_GET_MY_EVENTS, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_HD_LIVE_EVENTS_GET_MY_PROGRESS, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_HD_LIVE_EVENTS_GET_LEADERBOARD, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
		nm.RegistryEndPoint(COMMAND_HD_LIVE_EVENTS_FINISH_MY_EVENT, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);     

        nm.RegistryEndPoint(COMMAND_HD_LEAGUES_GET_ALL_LEAGUES, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
        nm.RegistryEndPoint(COMMAND_HD_LEAGUES_GET_LEADERBOARD, NetworkManager.EPacketEncryption.E_ENCRYPTION_NONE, codes, CaletyExtensions_OnCommandDefaultResponse);
    }    

    /// <summary>
    /// Default callback from Calety's NetworkManager, will propagate the response to the command system.
    /// </summary>
    private bool CaletyExtensions_OnCommandDefaultResponse(string strResponse, string strCmd, int iResponseCode) {        
        Log("Received response for command " + strCmd + ",  statusCode=" + iResponseCode);

		return Commands_OnResponse(strResponse, iResponseCode);
	}

	/// <summary>
	/// Special treatment for the GetPersistence command.
	/// </summary>
	private bool CaletyExtensions_OnGetPersistenceResponse(string strResponse, string strCmd, int iResponseCode) {
		// [DGR] Server: Default universe
		if(string.IsNullOrEmpty(strResponse) || strResponse == "{}")
        {
			SimpleJSON.JSONNode defaultJson = PersistenceUtils.GetDefaultDataFromProfile(PersistenceProfile.DEFAULT_PROFILE);			            
			strResponse = defaultJson.ToString();
		}

		return Commands_OnResponse(strResponse, iResponseCode);                
	}
    #endregion

    #region connection
    // This region is responsible for 
    // 1)Trying to reconnect after a network failure
    // 2)Keeping the session in server alive by sending a ping command periodically   

    private enum EConnectionState
    {        
        Up,
        Down,
        Recovering
    };

    private EConnectionState m_connectionState;

    private Error m_connectionServerDownError;

    // In Seconds    
    private float m_connectionTimeLeftToPing;

    private bool m_connectionIsCheckEnabled;
    private bool m_connectionIsPerformingCheck;

    private bool m_connectionIsReady;    

    private void Connection_Init() {        
        m_connectionIsReady = false;

        // If has to be enabled explicitly
        m_connectionIsCheckEnabled = false;
        m_connectionIsPerformingCheck = false;
        Connection_ResetTimer();

        Connection_SetState(EConnectionState.Up);
    }    

    private void Connection_SetState(EConnectionState value) {
        m_connectionState = value;
    }
    
    private Error Connection_GetServerDownError() {
        if (m_connectionServerDownError == null)
        {
            m_connectionServerDownError = new TimeoutError();
        }

        return m_connectionServerDownError;
    }

    private void Connection_OnServerDown() {
        // If it's already recovering then we let the flow finish
        if (m_connectionState != EConnectionState.Recovering) {
            m_connectionState = EConnectionState.Down;
        }
    }

    private void Connection_Recover(Action onDone) {        
        Log("Trying to recover connection....");

        Action<bool> onRecoverDone = delegate (bool success) {            
            EConnectionState state = (success) ? EConnectionState.Up : EConnectionState.Down;
            
            // Threre's connection again
            if (success) {
                // Notifies that network is up again
                Messenger.Broadcast(MessengerEvents.CONNECTION_RECOVERED);
            }
            
            Log("Recovery connection " + ((success) ? "succeeded" : "failed"));

            Connection_SetState(state);

            if (onDone != null) {
                onDone();
            }
        };

        Connection_SetState(EConnectionState.Recovering);

        Connection_ResetTimer();

        InternalCheckConnection((Error checkError) => {
            if (checkError == null) {
                // Logs in server
                InternalAuth((Error error, ServerResponse response) => {
                    bool isLoggedInServer = IsLoggedIn();

                    // If it's logged in server then tries to sync
                    /*if (isLoggedInServer) {
                        PersistenceFacade.instance.Sync_FromReconnecting((PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail) => {
                            onRecoverDone(isLoggedInServer);
                        }
                        );
                    } else {
                        onRecoverDone(isLoggedInServer);
                    }*/
                    onRecoverDone(isLoggedInServer);
                }, true);
            } else {
                onRecoverDone(false);
            }
        },
        true
        );
    }     
   
    private void Connection_ResetTimer() {
        m_connectionTimeLeftToPing = FeatureSettingsManager.instance.GetAutomaticReloginPeriod();
    }                   
    
    private bool Connection_IsCheckEnabled() {
        return m_connectionIsCheckEnabled;
    }

    public override void Connection_SetIsCheckEnabled(bool value) {
        m_connectionIsCheckEnabled = value;
    }

    private void Connection_Update() {
        if (FeatureSettingsManager.instance.IsAutomaticReloginEnabled()) {
            if (m_connectionIsReady) {
                // System ready
                // Check that a connection check is not already being performed 
                if (Connection_IsCheckEnabled() && !m_connectionIsPerformingCheck && m_connectionState != EConnectionState.Recovering) {
                    m_connectionTimeLeftToPing -= Time.unscaledDeltaTime;

                    // Time's up
                    if (m_connectionTimeLeftToPing < 0f) {
                        Action onDone = delegate () {
                            m_connectionIsPerformingCheck = false;
                            Connection_ResetTimer();
                        };

                        m_connectionIsPerformingCheck = true;

                        // Checks if we needs to relogin to cloud, if so then we force a sync (which also checks connection and login)
                        if (SocialPlatformManager.SharedInstance.IsLoggedIn() && !PersistenceFacade.instance.CloudDriver.IsLoggedIn && !PersistenceFacade.instance.Sync_IsSyncing) {                            
                            Log("Automatic relogin performing cloud sync...");

                            PersistenceFacade.instance.Sync_FromReconnecting((PersistenceStates.ESyncResult result, PersistenceStates.ESyncResultDetail resultDetail) => { onDone(); });
                        } else {                            
                            Log("Automatic relogin performing a ping...");

                            // We just need to check the connection because just sending a command will force network and login check
                            CheckConnection((Error error) => { onDone(); });
                        }
                    }
                }
            } else {
                // System is not ready: Waiting for content to be ready
                if (ContentManager.m_ready) {
                    m_connectionIsReady = true;
                }
            }
        }
    }
    #endregion

    //------------------------------------------------------------------------//
    // LOGGING METHODS														  //
    //------------------------------------------------------------------------//
    #region log
    private const string LOG_CHANNEL = "[GameServerManagerCalety]";

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void LogWarning(ECommand command, Error error, Exception e = null) {        
		LogWarning(String.Format("{0} Error when sending command {1}: {2}: {3} ({4})", LOG_CHANNEL, command, error.GetType().Name, error.message, error.code));        
		if(e != null) {
			LogWarning(e.ToString());
		}

	}

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void Log(string message) {		
		ControlPanel.Log(LOG_CHANNEL + message, ControlPanel.ELogChannel.Server);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void LogWarning(string message) {		
		ControlPanel.LogWarning(LOG_CHANNEL + message, ControlPanel.ELogChannel.Server);
	}

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private void LogError(string message) {		
		ControlPanel.LogError(LOG_CHANNEL + message, ControlPanel.ELogChannel.Server);
	}
	#endregion
}