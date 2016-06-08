/// <summary>
/// 16/Jan/2014
/// David Germade
/// 
/// Class declaring all methods in the game requiring persistence. The model used to communicate with server will be request/response.
/// This class should be extended by different classes to implement the offline and online persistence. 
/// This class has the common code for all implementations.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class RequestNetwork : SingletonMonoBehaviour<RequestNetwork> 
{
    public const string REQUEST_ACTION_LOGIN = "login";
    public const string REQUEST_ACTION_UNIVERSE = "universe";
	public const string REQUEST_ACTION_SYNC = "sync";

	public const int NET_ERROR_UPDATE_REQUIRED = 426;
	public const int NET_ERROR_NO_RESPONSE = -1;

	/// LEAGUE REQUESTS
	public const string REQUEST_LEAGUE_GLOBAL = "/api/leaderboard/global";
	public const string REQUEST_LEAGUE_REGION = "/api/leaderboard/local";
	public const string REQUEST_LEAGUE_FRIENDS = "/api/leaderboard/friends";
	
	public const string REQUEST_LEAGUE = "/api/leaderboard/league";

	public const string ADD_SOCIAL_MAPPING = "/api/social/add";
	public const string REQUEST_SOCIAL_MAPPINGS = "/api/social/mappings";
	public const string ONLINE_STATUS = "/api/social/onlineStatus";

	public const string BUY_PC_APPLE = "/api/iosPayment/apply";
	public const string BUY_PC_GOOGLE = "/api/googlePayment/apply";
	public const string BUY_PC_AMAZON = "/api/amazonPayment/apply";

    public string m_serverUrl;

    // Platform
    public string m_platform = "iOS";        

    // Server version
    // Client build
	protected string m_clientVersion = "";	// Now this data will come from a file called "version" inside Resources

	#if UNITY_ANDROID        
	protected string m_clientBuild = "Android";
	#elif UNITY_IPHONE
	protected string m_clientBuild = "IOS";
	#else
	protected string m_clientBuild = "unity";
	#endif
    
    // platform user Id
    public string m_platformUserId;
    
    // retrieved info from the server
    // User id
    public string m_uid;
    // User id as Int
    public int m_i_uid;
    // Connection token
    public string m_token;
    
    protected long m_playerSince = 0;

	public bool InSyncWithServerAfterAppPaused = false;

	// newUser
	protected bool m_isNewUser = false;

    public enum ELoginState
    {
        NOT_LOGGED,
        LOGGING,
        LOGGED,
        FAILED
    }
    private  ELoginState m_loginState = ELoginState.NOT_LOGGED;

    // Delegates to inform other clasees that we have a new command response
    	// AUTH
    // OnAuthResponse delegate
    public delegate void OnAuthResponse( JSONNode result);
	// OnAuthResponse attribute
    public OnAuthResponse onAuthResponse;
	// OnMergeResponse delegate
	public delegate void OnMergeResponse(JSONNode result, int statusCode);
	// OnMergeResponse attribute
	public OnMergeResponse onMergeResponse;
	// OnMergeResponse error delegate
	public delegate void OnMergeResponseError( int errorCode, string errorDesc, string response );
	// On Merge responde error delegate attribute
	public OnMergeResponseError onMergeResponseError;
	// RequestAuthKey delegate
    public delegate void OnRequestAuthKey(string key);
	public OnRequestAuthKey onAuthKey;
	
		// DELEGATES USED FOR GAME/LEADERBOARD/ETC
	// On Action Delegaate
	public delegate void OnActionResponse( string action, JSONNode result);
	// On Action Delegate Error
	public delegate void OnActionResponseError( string action, JSONNode result);
	
		// GAME
    // On Game messages response delegate attribute
    public OnActionResponse onGameResponse;
    // On Game Response error delegate attribute
    public OnActionResponseError onGameResponseError;
    
    	// LEADERBOARD API
	// On Game messages response delegate attribute
	public OnActionResponse onLeaderboardResponse;
	// On Game Response error delegate attribute
	public OnActionResponseError onLeaderboardResponseError;
	
		// SOCIAL API
	public OnActionResponse onSocialResponse;
	public OnActionResponseError onSocialResponseError;

		// INTERNAL
    // Delegates called after wait for response
    public delegate void OnResponse(string response, string cmd, string responseStatus);
    public delegate void OnResponseError(int errorCode, string errorDesc, string response, string cmd);


    // Server time in seconds. This is the value returned by the server. It's stored in seconds as it will be used to check some logic transitions (counters to change the state
    // of data) and all logic works in seconds. Please use GetServerTime() when you need this timer updated.
    protected double m_currentServerTime = 0;
    
    // Time.time when currentServerTime was set
    private double currentServerTimeAt = 0;        
    
    // Time.time when application is paused
    private double m_pauseTime = 0;

	private string m_country = "";

    protected RequestNetwork()
	{
		// System.DateTime origin = new System.DateTime( 1970,1,1,0,0,0,0);
		// currentServerTime = (System.DateTime.Now - origin).TotalSeconds;
		m_currentServerTime = 0;
		//TextAsset textAsset  = (TextAsset) Resources.Load( "version", typeof(TextAsset));
		//m_clientVersion = textAsset.text.Trim();
	}
	
	void OnDestroy()
	{
		ExtendedOnDestroy();
	}

	protected virtual void ExtendedOnDestroy() {}
	
	public virtual void Update() {}
		
	bool reallyPausedBefore = false;

	void OnApplicationPause(bool _paused)
	{
		Debug.Log("OnApplicationPause " + _paused);

		if (_paused)
		{
			reallyPausedBefore = true;

			m_pauseTime = Time.realtimeSinceStartup;

			FlushAllPendingRequestsNow();

			if (IsLogged())
			{
				Dictionary<string, string> _data = new Dictionary<string, string>();
				_data["serverTime"] = GetServerTime() + "";
				// DebugAction("App-Minimized", _data);
			}
		}
		else
		{
			double deltaTime = Time.realtimeSinceStartup - m_pauseTime;			
			if (deltaTime >= 30.0f)	// Seems that if device is not connected after 2-3 min realtimeSinceStartup stops, we cannot trust it so we ask the server
			{
				// Force reload
				// InstanceManager.FlowManager.GoToLoading();
				
				// Ask server for time!!!
				RequestSync();
			}

			if (IsLogged() && reallyPausedBefore)
			{
				Dictionary<string, string> _data = new Dictionary<string, string>();
				_data["serverTime"] = GetServerTime() + "";
				// DebugAction("App-Maximized", _data);
			}
		}
	}

	protected virtual void FlushAllPendingRequestsNow() {}


	protected void Log(string _str)
	{
		Debug.Log("REQUEST_NETWORK: " + _str);
	}
	
	
	public string Country
	{
		get
		{
			return m_country;
		}
		set
		{
			m_country = value;
            Debug.Log("[Country]" + m_country);
		}
	}
	
	public string ClientVersion
	{
		get
		{
			return m_clientVersion;
		}
	}
	
	public string ClientBuild
	{
		get
		{
			return m_clientBuild;
		}
	}

	public virtual string GetServerName()
	{
		return "Offline";
	}

	public double GetServerTime()
	{
		return m_currentServerTime + (Time.realtimeSinceStartup - currentServerTimeAt);		
	}
		
	public void SetServerTime(double _value)
	{		
		if (_value > 0)
		{
			m_currentServerTime = _value;		
			currentServerTimeAt = Time.realtimeSinceStartup;
			Log("Server time = " + m_currentServerTime);
		}
	}
	
	// Returns the time elapsed since the player started playing the game on the first login
	public long GetElapsedSinceFirstAuth()
	{
		 return ((long)GetServerTime()) - m_playerSince;
	}
		
    protected void ProcessGameCommandResponse(string _action, JSONNode _response)
    {
        switch (_action)
        {
            case REQUEST_ACTION_LOGIN:
                SetLoginState(ELoginState.LOGGED);

				// InstanceManager.MetricsManager.LoadingFunnel(MetricsManager.ELoadingFunnelStep.LOGIN_OK);
				break;
			case REQUEST_ACTION_SYNC:
			{
				/// SetServerTime( currentServerTime );
				double _time = _response["time"].AsDouble;
				if (_time > 0)
				{
					_time /= 1000;
					SetServerTime(_time);

					Dictionary<string, string> _data = new Dictionary<string, string>();
					_data["serverTime"] = _time + "";
				}

				InSyncWithServerAfterAppPaused = false;
			}break;
			case REQUEST_ACTION_UNIVERSE:
				{
				}
				break;

			
		}
	
		
		if (onGameResponse != null)
        {			
			onGameResponse(_action, _response);
        }
    }

	private void AddAttToJSONNode(JSONNode _node, string _attName, string _value)
	{
		if (_node != null)
		{
			string _oldValue = _node[_attName];
			if (_value == null)
			{
				if (_oldValue != null)
				{
					_node.Remove(_attName);
				}
			}
			else
			{
				if (_oldValue == null)
				{
					_node.Add(_attName, _value);
				}
				else
				{
					_node[_attName] = _value;
				}
			}
		}
	}


    protected void ProcessOnResponseError(string _action, JSONNode _response)
    {
        switch (_action)
        {
            case REQUEST_ACTION_LOGIN:
                SetLoginState(ELoginState.FAILED);
                break;
        }
        
        if (onGameResponseError != null)
        {
            onGameResponseError(_action, _response);
        }
    }

    public virtual bool IsOnline()  { return false; }

	public virtual void Authenticate(string _serverUrl,string sufix, string _platformUserId, string _platformUserName = "") {}

	public virtual void Merge(string _serverUrl, string _sufix, string _targetPlatform, string _targetPlatformId, string _targetPlatformUserName = "", bool _force = false, Dictionary<string,string> platformTokens = null ) {}

	public virtual void DeleteMappings(string targetPlatformId, string socialPlatform){}

	public virtual void SetServerUrl( string _serverUrl )
	{
		m_serverUrl = _serverUrl;
	}

    public void Login() 
    {
        SetLoginState(ELoginState.LOGGING);
		ExtendedLogin();
    }
    
    public virtual void RequestSync()
	{
		InSyncWithServerAfterAppPaused = true;
	}

    protected virtual void ExtendedLogin() {}     

    public bool IsLogging()
    {
        return m_loginState == ELoginState.LOGGING;
    }

    public bool IsLogged() 
    {
        return m_loginState == ELoginState.LOGGED;
    }

    private void SetLoginState(ELoginState _state)
    {
        m_loginState = _state;
    }    	

	public bool IsNewUser()
	{
		return m_isNewUser;
	}

	//
	// Register device token on the server for push notifications.
	//
	public virtual void SendDeviceTokenForRemoteNotifications (string deviceToken) {}

    public virtual void RequestUniverse() {}

	public virtual void NotifyCustomizerPopupAction_view(long code) {}
	public virtual void NotifyCustomizerPopupAction_accept(long code) {}
	public virtual void NotifyCustomizerPopupAction_cancel(long code) {}

    #region pvp
    /// <summary>
    /// Enum for the reasons why the PvP match has finished
    /// </summary>
    public enum EPvPMatchFinishReason
    {
        None,
        // The PvP battle has finished without any errors
        Battle,
        // The user decides to stop waiting for an opponent
        Cancel,
        // The battles are disabled from serger
        BattlesDisabled,
        // The user quitted a PvP battle, she didn't wait for the battle to finish
        UserQuitted,
        // There was an error when the real time connection was tried to be established
        RtError
    }

    private static List<string> pvpMatchFinishReasonSkus = new List<string>
	{
		"none",
		"battle",
		"cancel",
		"battlesDisabled",
        "userQuitted",
        "rtError"
	};

    protected static EPvPMatchFinishReason SkuToEPvPMatchFinishReason(string _value)
    {
        int _index = pvpMatchFinishReasonSkus.IndexOf(_value);
        return (_index == -1) ? EPvPMatchFinishReason.None : (EPvPMatchFinishReason)_index;
    }

    protected static string EPvPMatchFinishReasonToSku(EPvPMatchFinishReason _value)
    {
        int _index = (int)_value;
        return (_index < 0 || _index >= pvpMatchFinishReasonSkus.Count) ? EPvPMatchFinishReasonToSku(EPvPMatchFinishReason.None) : pvpMatchFinishReasonSkus[_index];
    }
	
	/// <summary>
	/// Requests the pvP match.
	/// </summary>
	/// <param name="_currentShipHp">HP of the ship used by the user to go to the PvP battle. It should be maxHP. It's sent so the server can make sure that the hp is right</param>
	/// <param name="_currentShipId">_current ship identifier.</param>
	/// <param name="_seasonSku">Sku of the season which conditions should be applied according to client.</param>
    public virtual void RequestPvPMatch(int _currentShipHp, int _currentShipId, string _seasonSku) {}
    
	/// <summary>
	/// Notifies the pvP match is over.
	/// </summary>
	/// <param name="_currentShipHp">HP of the ship after the PvP battle. It's send in order to let the server check if the hp set to the ship after the battle PvP 
	///                              is right (take into consideration that the hp ship has to be full when a PvP battle starts, but once it's over the former
	///                              hp plus the percentage repaired during the time that the battle lasted has to be set.	
	/// <param name="_reason">Reason why the PvP match is over.</param>
    public virtual void FinishPvPMatch(int _currentShipHp, int _currentShipId, EPvPMatchFinishReason _reason) { }
    #endregion

	/// <summary>
	/// Requests the auth key for the user.
	/// </summary>
	public virtual void RequestAuthKey(){}

	/// <summary>
	/// Requests the device settings to optimize the performance.
	/// </summary>
	public virtual void RequestDeviceSettings(){}
	
	
	/// <summary>
	/// Sends the cheat.
	/// </summary>
	/// <param name="cheatTask">Cheat task.</param>
	public virtual void SendCheat(string cheatTask) {}
	
    /// <summary>
    /// Sends a cheat to add currencies to the server.
    /// </summary>    
    /// <param name="_currencty">Code of the currency affected by the action.</param>
    /// <param name="_amount">Amount to apply to the currency.</param>
    /// <param name="_itemSku">Sku of the item, when required, otherwise it should be <c>null</c>.</param>
    public virtual void SendCheatAddCurrency(string _currency, long _amount, string _itemSku) {}        	
		
	public virtual void SendCheatSetWorldMapCurrentNodeSku(string _currentNodeSku) {}	

	public virtual void SendCheatSetLastMainMissionCompletedSku(string _sku) {}
	
	/// <summary>
	/// Cheat to test server push notifications
	/// </summary>
	public virtual void SendCheatPushNotification() {}
	
    public virtual void SendAuth() {}

    /// <summary>
    /// A chain is translated on a command with its parameters. This method must be used only for cheating. For an actual communication with server
    /// please, declare a method with as many parameters as the command requires.
    /// </summary>
    /// <param name="_chain">A string containing the command and the parameters to send to the server.</param>
    public virtual void SendCommand(string _chain) {}    

  
    

    //
    // Assets
    //
    public virtual void UpdateAssets(OnResponse _onSuccess) {}

    #region leaderboards
    // LEADERBOARD API

    /// <summary>
    /// Requesets the globa ranking.
    /// </summary>
    public virtual void RequestGlobalLeaderboard(){}
	
	/// <summary>
	/// Requests the region leaderboard.
	/// </summary>
	public virtual void RequestRegionLeaderboard(string region = ""){}
	
	public virtual void RequestFriendsLeadeboard( List<string> friend_ids){}
	
	public virtual void CleanLeaderboardCache(){}
	#endregion
	
	#region social
	// SOCIAL API
	
	/// <summary>
	/// Adds the social API. Associates a social network id to this account
	/// </summary>
	/// <param name="platformName">Platform name.</param>
	/// <param name="platformId">Platform identifier.</param>
	/// <param name="platformToken">Platform token.</param>
	public virtual void AddSocialApi( string platformName, string platformId, string platformToken ){}
	
	/// <summary>
	/// Given social platforms ids returns game ids. platform_ids is of type
	///
	//	{
	//		gamecenter: [platform_id, platform_id...]
	//		facebook: [platform_id, platform_id...]
	//	}
	/// </summary>
	/// <param name="platform_ids">Platform_ids.</param>
	public virtual void GetSocialMapping( JSONNode platform_ids ){}
	
	/// <summary>
	/// Gets the online status. Asks the server for friends uids and the server returns a json of type
	//  { uid: <status>, uid: <status>... }
	// 	where <status> = ACTIVE, UNAVAILABLE, DISCONNECTED
	/// </summary>
	/// <param name="uids">Uids.</param>
	public virtual void RequestOnlineStatus( List<long> uids){}
	
	#endregion

	#region buy premium currency
	public virtual void VerifyPurchaseTransaction(string jsonData, string signature, OnResponse onResponse, OnResponseError onResponseError){}
	
	public virtual void PendingTransactions(bool delayedRequest = false) {}
	#endregion




	#region customizer

	public delegate void OnCustomizerResponse(string response);

	public virtual void Customizer_Request(OnCustomizerResponse onResponse) {}

	// It is only done, to inform server that next call will be a get customizer request
	// needed by omniata limitations system
	public virtual void Customizer_PreRequest(OnCustomizerResponse onResponse) {}

    #endregion


    public virtual bool HasConnection()
    {		
        return false;		
	}

	/// <summary>
	/// Sends the user's current language.
	/// </summary>
	/// <param name="_isoCode">Iso code of the user's current language.</param>
	public virtual void SetLanguage(string _isoCode)
	{
	}	


	// ----------------------------------------------------------------------- //

	public virtual float Latency_GetAverageDuration() {return 0;}
	public virtual float Latency_GetLastEntryDuration() {return 0;}
	public virtual void Latency_SendSomePingsToServer() {}

    // ----------------------------------------------------------------------- //

    protected void onSocialCommand(string resopnse, string cmd, string responseStatus)
    {
        if (onSocialResponse != null)
            onSocialResponse(cmd, JSON.Parse(resopnse));
    }

    protected void onSocialcCommandError(int errorCode, string errorDesc, string response, string cmd)
    {
        if (onSocialResponseError != null)
            onSocialResponseError(cmd, JSON.Parse(response));
    }

    // ----------------------------------------------------------------------- //
}

