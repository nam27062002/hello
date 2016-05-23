using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

//using System.Net;
using System.Security.Cryptography;

using SimpleJSON;



/**
 * Class in charge of http/synchronous comunication
 * It needs to extend <c>RequestNetwork</c> to satisfy all the needs of the game in terms of persistence. This class implements the request/response model.
 */ 
public class RequestNetworkOnline : RequestNetwork
{
	// -------------------------------------------------------------------------- //

	private const bool debug = false;
	private const string DEBUG_TAG = "[RequestNetwork] ";

	// -------------------------------------------------------------------------- //

	private const string ENCRYPT_PASSWORD = "sirocco";

	// low-level communication with OUR servers
	private Server server = null;

	private AESEncryptionVault crypto;

	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	
	//--------------------------------------------//
	// MEMBERS          						  //
	//--------------------------------------------//	    		
	
	
	string selectedServer = "";
	Dictionary<string, string> serverRoots = new Dictionary<string, string>();
	
	
	// Use this for initialization
	public RequestNetworkOnline () 
	{
		m_uid = "";
		m_i_uid = 0;
		m_token = "";
		
		// Get servers and its names from file instead of this
		//serverRoots.Add( "Prod", "sandstorm.ubi.com");
		//serverRoots.Add( "Stage", "sandstorm-stage.ubi.com");
        //serverRoots.Add("Testing", "52.91.124.78:8080");
        //serverRoots.Add( "Old_Dev", "bcn-dev-sandstorm.ubi.com");
        //serverRoots.Add( "Dev", "bcn-mb-dev-sandstorm.ubisoft.org");
		//serverRoots.Add( "Integration", "bcn-integration-sandstorm.ubi.com");
		//serverRoots.Add( "Local", "sandstorm.local");
		serverRoots.Add( "Nacho", "10.44.4.34:8080");
		//serverRoots.Add( "Manel", "10.44.4.54:8080");
        //serverRoots.Add( "Alfonso", "10.44.4.63:8080");

        // string targetEnvironment = obtainServerToUseFromTargetEnvironment (BuildSettings.TARGET_ENVIRONMENT);
		string targetEnvironment = "Nacho";
		SetServer( targetEnvironment );
		
		crypto = new AESEncryptionVault(ENCRYPT_PASSWORD);
	}

	protected override void ExtendedOnDestroy() 
	{
		/*if (server != null)
		{
			server.FlushAllGamePlayActions();
			server.LogicUpdate();
			server.LogicUpdate();
		}*/
	}



	
	//--------------------------------------------//
	// METHODS          						  //
	//--------------------------------------------//	
	
	/*
	private string obtainServerToUseFromTargetEnvironment(BuildSettings.ETargetEnvironment targetEnvirontment)
	{
		// This ids are set in allowed servers
		switch (targetEnvirontment)
		{
			case BuildSettings.ETargetEnvironment.PROD:			return "Prod";
			case BuildSettings.ETargetEnvironment.STAGE:		return "Stage";
            case BuildSettings.ETargetEnvironment.TESTING:      return "Testing";
            //case BuildSettings.ETargetEnvironment.OLD_DEV:		return "Old_Dev";
            case BuildSettings.ETargetEnvironment.DEV:			return "Dev";
			case BuildSettings.ETargetEnvironment.INTEGRATION:	return "Integration";
			case BuildSettings.ETargetEnvironment.LOCAL:		return "Local";
			case BuildSettings.ETargetEnvironment.NACHO:		return "Nacho";
			case BuildSettings.ETargetEnvironment.MANEL:		return "Manel";
            case BuildSettings.ETargetEnvironment.ALFONSO:      return "Alfonso";
        }
		
		return "";
	}
	*/
	
	public void SetNextServer()
	{
		if (serverRoots.ContainsKey( selectedServer ))
		{
			bool getNext = false;
			bool getFirst = true;
			foreach( KeyValuePair<string,string> pair in serverRoots)
			{
				if ( getNext )
				{
					selectedServer = pair.Key;
					getFirst = false;
					break;
				}
				if ( pair.Key == selectedServer )
				{
					getNext = true;
				}
			}
			if (getFirst)
			{
				Dictionary<string, string>.Enumerator enumerator = serverRoots.GetEnumerator();
				enumerator.MoveNext();
				selectedServer = enumerator.Current.Key;
			}
			if ( serverRoots.ContainsKey( selectedServer ) )
			{
				SetServer( selectedServer  );
			}
		}
	}
	
	void SetServer( string serverId )
	{
		selectedServer = serverId;
		
		string host = "";
		if (serverRoots.ContainsKey(serverId))
			host = serverRoots[serverId];
		m_serverUrl = "http://" + host + "/dragon";				

		server = new Server (m_serverUrl, m_clientVersion, m_clientBuild);
	}
	
	public override string GetServerName()
	{
		return selectedServer;
	}


	protected override void FlushAllPendingRequestsNow()
	{
		if (server != null)
		{
			server.FlushAllGamePlayActions();
		}
	}

	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //

	public override void RequestAuthKey()
	{
		if (debug) Debug.Log (DEBUG_TAG + "RequestAuthKey (/api/auth/a)");

		server.SendCommand ("/api/auth/a", onGetKey);
	}

	private void onGetKey(string response, string cmd, string responseStatus)
	{
		if ( onAuthKey != null )
		{
			onAuthKey(crypto.Decrypt(response));
		}
	}
	
	// -------------------------------------------------------------------------- //

	//TODO: ALBERT: Get device settings from server

	public override void RequestDeviceSettings()
	{
		server.SendCommand("/api/quality/settings", onGetDeviceSetting, onGetDeviceSettingsError);
	}


	private void onGetDeviceSetting(string response, string command, string responseStatus)
	{
		JSONNode json = JSON.Parse(response);

		// InstanceManager.CustomSettings.LoadSettings(json);

		// save the response somewhere, could be needed if the service fails in the future
		PlayerPrefs.SetString("qualitySettings", response);
	}

	private void onGetDeviceSettingsError(int errorCode, string errorDesc, string response, string command)
	{
		if (debug) Debug.Log(DEBUG_TAG + "Error requesting settings: " + response);

		// try to use last downloaded settings, if any
		if(PlayerPrefs.HasKey("qualitySettings")){
			string settings = PlayerPrefs.GetString("qualitySettings");
			JSONNode json = JSON.Parse(response);
			/*
			if(json != null){
				InstanceManager.CustomSettings.LoadSettings(json);
			}
			*/
		}

	}


	// -------------------------------------------------------------------------- //


	/**
	 * 
	 */
	public override void Authenticate(string _serverUrl, string _sufix, string _platformUserId, string _platformUserName = "")
	{
		m_serverUrl = _serverUrl;
		m_platformUserId = _platformUserId;
		SendAuthCommand(_sufix, _platformUserId, _platformUserName);
	}

	/**
	 * Sends auth Commands and the response comes in onAuth/onAuthError function
	 */
	private void SendAuthCommand(string _sufix, string _platformUserId, string _platformUserName = "")
	{
		byte[] datas = null;
		if (_sufix == "/api/auth/b")
		{
			JSONNode data = new JSONClass();
			data["platformUserId"] = _platformUserId;
			data["timestamp"] = System.DateTime.Now.ToString();
			if (!string.IsNullOrEmpty( _platformUserName ))
			{
				data["platformData"]["name"] = _platformUserName;
			}
			string trackingId = PlatformUtils.Instance.GetTrackingId();
			if (trackingId != null)
			{
				data["trackingId"] = trackingId;
			}
			Debug.Log ("[Tracking id]" + PlatformUtils.Instance.GetTrackingId());

			string encrypted = crypto.Encrypt(data.ToString());
			datas = Encoding.UTF8.GetBytes(encrypted);
		}
		else
		{
			WWWForm form = new WWWForm();
			form.AddField("platformUserId", _platformUserId);
			datas = form.data;
		}

		server.SendCommand (_sufix, datas, onAuth);
	}

	/**
      * DELEGATE to manage onAuth response
      * @param response. string response
      */ 
	private void onAuth(string response, string command, string responseStatus)
	{
		JSONNode json;		
		if (command.Contains("auth/b"))
		{		   
			json = JSON.Parse( crypto.Decrypt(response));
		}
		else
		{
			json = JSON.Parse(response);
		}
		
		m_uid = json["uid"].Value;
		m_i_uid = int.Parse(m_uid);
		m_token = json["token"].Value;
		m_playerSince = json["playerSince"].AsLong / 1000;
		
		m_isNewUser = json["new"] != null && bool.Parse(json["new"]);
		
		if (debug) Debug.Log(DEBUG_TAG + "AuthResponse: " + json.ToString());
		
		if ( onAuthResponse!= null )
			onAuthResponse( json );		
	}
	

	// -------------------------------------------------------------------------- //


	public override void Merge(string _serverUrl, string _sufix, string _targetPlatform, string _targetPlatformId, string _targetPlatformUserName = "", bool _force = false, Dictionary<string,string> platformTokens = null)
	{
		m_serverUrl = _serverUrl;
		SendMergeCommand(_sufix, _targetPlatform, _targetPlatformId, _targetPlatformUserName, _force, platformTokens);
	}

	public override void DeleteMappings(string targetPlatformId, string socialPlatform){
		Dictionary<string, string> parameters = new Dictionary<string, string>();
		parameters["platformId"] = targetPlatformId;
		parameters["platform"] = socialPlatform;
		server.SendCommand("/mapping/delete", parameters, null);
	}

	/**
	 * Sends merge commands and the response comes in onMerge/onMergeError function
	 */
	private void SendMergeCommand(string _sufix, string _targetPlatform, string _targetPlatformId, string _targetPlatformUserName = "", bool _force = false, Dictionary<string,string> platformTokens = null)
	{
		byte[] datas = null;
		if (_sufix == "/api/merge/c")
		{
			JSONNode data = new JSONClass();
			data["targetPlatform"] = _targetPlatform;
			data["targetPlatformId"] = _targetPlatformId;
			data["timestamp"] = GetServerTime().ToString();
			if (!string.IsNullOrEmpty(_targetPlatformUserName))
			{
				data["targetPlatformData"]["name"] = _targetPlatformUserName;
			}
			data["force"] = _force? "true":"false";
			
			AddTokenData( data, _targetPlatform, platformTokens);

			string encrypted = crypto.Encrypt(data.ToString());
			datas = Encoding.UTF8.GetBytes(encrypted);
		}
		else
		{
			WWWForm form = new WWWForm();
			form.AddField("targetPlatform", _targetPlatform);
			form.AddField("targetPlatformId", _targetPlatformId);
			form.AddField("force", _force?"true":"false");

			datas = form.data;
		}
		
		Dictionary<string,string> urlParams = new Dictionary<string, string>();
		urlParams.Add("uid", m_uid );
		urlParams.Add("token", m_token );
		
		server.SendCommand (_sufix, urlParams ,datas, onMerge, onMergeError);
	}


	private void AddTokenData( JSONNode data, string platform, Dictionary<string,string> platformTokens)
	{
		Debug.Log("PlatformTokens != null? = " + (platformTokens != null) + "\nPlatform: " + platform + " =? " + Application.platform + "\nPrevious data: " + data.ToString());
		if ( platformTokens != null)
		{
			switch( Application.platform )
			{
			case RuntimePlatform.IPhonePlayer:
				// data["targetPlatformToken"] = "";
				break;
			case RuntimePlatform.Android:
				foreach(string key in platformTokens.Keys)
				{
					Debug.Log("PlatformTokens[" + key + "] = " + platformTokens[key]);
				}
				Debug.Log("PlatformTokens[platform] = " + platformTokens[platform]);
				data["targetPlatformToken"] = platformTokens[platform];
				break;
			}
		}
		Debug.Log("Resulting data: " + data.ToString());
	}

	/**
      * DELEGATE to manage onMerge response
      * @param response. string response
      */ 
	private void onMerge(string response, string command, string responseStatus)
	{
		JSONNode json;
		if (command.Contains("merge/c")) {		   
			json = JSON.Parse( crypto.Decrypt(response));
		} else {
			json = JSON.Parse(response);
		}

		// get status code
		int statusCode = 200; // Merge successful by default (mapping already existed).
		string trimmed = responseStatus.TrimStart(' ');
		if ( !string.IsNullOrEmpty(trimmed) )
		{
			int spaceIndex = trimmed.IndexOf(' ');
			if ( spaceIndex > 0 )
			{
				trimmed = trimmed.Substring(0, spaceIndex);
			}
			
			try
			{
				int newStatusCode = Convert.ToInt32( trimmed );
				statusCode = newStatusCode;
			}
			catch( System.Exception e )
			{
				if (debug)
					Debug.Log( "To Int 32 error " + e.ToString() );
			}
			
		}
		
		if (debug) Debug.Log(DEBUG_TAG + "MergeResponse: '" + json.ToString() + "' command: " + command + " responseStatus: '" + responseStatus + "'");
		
		if (onMergeResponse!= null)
			onMergeResponse(json, statusCode);		
	}
	
	/**
      * DELEGATE to manage onMerge error response
      * @param response. string response
      */ 
	private void onMergeError(int errorCode, string errorDesc, string response, string command )
	{
		if (command.Contains("merge/c") && !string.IsNullOrEmpty(response) ) 
		{
			response = crypto.Decrypt(response);
		}		
		
		if ( onMergeResponseError != null )
			onMergeResponseError( errorCode, errorDesc, response );
	}


	// -------------------------------------------------------------------------- //


	public override void SendDeviceTokenForRemoteNotifications(string deviceToken)
	{
		if (debug) Debug.Log(DEBUG_TAG + "SendDeviceTokenForRemoteNotifications: uid='" + m_uid + "' token='" + m_token + "' deviceToken='" + deviceToken + "'");

		WWWForm form = new WWWForm();
		form.AddField("uid", m_uid);
		form.AddField("token", m_token);
		form.AddField("deviceToken", deviceToken);

		server.SendCommand ("/api/device/register", form.data);
	}


	// -------------------------------------------------------------------------- //
	
	
	public override void UpdateAssets(OnResponse _onSuccess)
	{        
		server.SendCommand ("/assetsLUT?combined", _onSuccess);
	}


	public override void SendCheatPushNotification() 
	{
		WWWForm form = new WWWForm();
		form.AddField("accountId", m_uid);
		form.AddField("channel", "test");
		form.AddField("textId", "test_textId");
		
		server.SendCommand ("/api/notification/send", form.data);
	}


	// -------------------------------------------------------------------------- //


	#region social
	public override void AddSocialApi( string _platformName, string _platformId, string _platformToken )
	{
		Dictionary<string,string> _data = GetDictionaryForParams();
		_data[ "platform" ] = _platformName;
		_data[ "platformId" ] = _platformId;
		_data[ "platformToken" ] = _platformToken;
		SendApiCommand( ADD_SOCIAL_MAPPING, _data, onSocialCommand, onSocialcCommandError);
	}
	
	public override void GetSocialMapping( JSONNode platform_ids )
	{
		Dictionary<string,string> _data = GetDictionaryForParams();
		_data[ "data" ] = platform_ids.ToString();
		SendApiCommand( REQUEST_SOCIAL_MAPPINGS, _data, onSocialCommand, onSocialcCommandError);
	}

	public override void RequestOnlineStatus( List<long> uids)
	{
		Dictionary<string,string> _data = GetDictionaryForParams();
		SimpleJSON.JSONArray array = new SimpleJSON.JSONArray();
		for( int i = 0; i<uids.Count; i++ )
			array.Add( uids[i].ToString() );
		
		_data[ "data" ] = crypto.Encrypt( array.ToString() );
		SendApiCommand( ONLINE_STATUS, _data, onSocialCommand, onSocialcCommandError);
	}


	void SendApiCommand( string command, Dictionary<string, string> data, OnResponse onResponse, OnResponseError onResponseError )
	{
		WWWForm form = new WWWForm();
		form.AddField("uid", m_uid);
		form.AddField("token", m_token);
		foreach(KeyValuePair<string,string> pair in data)
		{
			form.AddField( pair.Key, pair.Value );
		}
		
		server.SendCommand(command, form.data, onResponse, onResponseError);
		
	}
	#endregion

	// -------------------------------------------------------------------------- //

	#region buy premium currency
	public override void VerifyPurchaseTransaction(string jsonData, string signature, OnResponse onResponse, OnResponseError onResponseError)
	{
		string sufix = "";
		
		if (debug) Debug.Log(DEBUG_TAG + "Start verify transaction");
		
		WWWForm form = new WWWForm();
		
		form.AddField("uid", m_uid);
		form.AddField("token", m_token);
		
		JSONClass jsonInfo = JSONNode.Parse(jsonData) as JSONClass;
		
		if (debug) Debug.Log(DEBUG_TAG + "dataString: " + jsonData);
		
		#if UNITY_ANDROID
		sufix =  BUY_PC_GOOGLE;
		if(jsonInfo != null){
			form.AddField("purchaseData", jsonData);
			form.AddField("signature", signature);
			form.AddField("orderId", jsonInfo["orderId"]);
		}
		
		#elif UNITY_IOS || UNITY_IPHONE
		if (jsonInfo != null)
		{
			form.AddField("receiptData", jsonInfo["receiptData"]);
			form.AddField("orderId", jsonInfo["orderId"]);
		}
		sufix = BUY_PC_APPLE;
		#elif UNITY_AMAZON
		sufix = BUY_PC_AMAZON;
		#endif
		
		server.SendCommand (sufix, form.data, onResponse, onResponseError);
	}
	
	public override void PendingTransactions(bool delayedRequest = false)
	{
		if (delayedRequest)
		{
			StartCoroutine (WaitForAskForNewPendingTransactions());
		}
		else
		{
			server.SendGamePlayAction ("transaction");
		}
	}

	private IEnumerator WaitForAskForNewPendingTransactions()
	{
		yield return new WaitForSeconds(1.5f);
		PendingTransactions(false);
	}

	#endregion
	
	
	// -------------------------------------------------------------------------- //


	#region leaderboard
	public override void RequestGlobalLeaderboard()
	{
		SendApiCommand(REQUEST_LEAGUE_GLOBAL, GetDictionaryForParams(), onLeagueCommand, onLeagueCommandError);
	}
	
	public override void RequestRegionLeaderboard(string region = "")
	{
		if ( region != "" )
		{
			SendApiCommand(REQUEST_LEAGUE_REGION, GetDictionaryForParams(), onLeagueCommand, onLeagueCommandError);
		}
		else
		{
			SendApiCommand(REQUEST_LEAGUE_REGION, GetDictionaryForParams(), onLeagueCommand, onLeagueCommandError);
		}
	}
	
	public override void RequestFriendsLeadeboard( List<string> friend_ids)
	{
		Dictionary<string,string> _data = GetDictionaryForParams();
		JSONArray array = new JSONArray();
		foreach( string s in friend_ids)
		{
			array.Add( s );
		}
		_data["data"] = crypto.Encrypt( array.ToString() );
		SendApiCommand(REQUEST_LEAGUE_FRIENDS, _data, onLeagueCommand, onLeagueCommandError);
		
	}
	
	

	
	
	
	
	private void onLeagueCommand(string response, string cmd, string responseStatus)
	{
		onLeagueCommand(JSON.Parse(response), cmd);
	}
	
	private void onLeagueCommand( JSONNode response, string cmd)
	{
		if ( onLeaderboardResponse != null )
			onLeaderboardResponse( cmd, response);
	}
	
	private void onLeagueCommandError( int errorCode, string errorDesc, string response, string cmd)
	{
		if ( onLeaderboardResponseError != null )
			onLeaderboardResponseError( cmd, JSON.Parse(response));
	}		
	#endregion
	
	
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //


    #region RequestNetwork
    public override bool IsOnline()  { return true; }


    protected override void ExtendedLogin()
    {
        Dictionary<string, string> data = GetDictionaryForParams();
        server.SendLogin (m_uid, data, m_token, OnGamePlayActionsResponse);
    }
    
    public override void RequestUniverse()                
    {
        server.SendGamePlayAction(REQUEST_ACTION_UNIVERSE, null, true);
    }

	public override void RequestSync()
	{
		base.RequestSync();
		server.SendGamePlayAction(REQUEST_ACTION_SYNC, null, true);
	}

	public override void NotifyCustomizerPopupAction_view(long code)
	{
		WWWForm form = new WWWForm();
		form.AddField("code", ""+code);

		server.SendCommand ("/api/popup/view", form.data);
	}

    public override void NotifyCustomizerPopupAction_accept(long code)
	{
		WWWForm form = new WWWForm();
		form.AddField("code", ""+code);
		
		server.SendCommand ("/api/popup/accept", form.data);
	}

	public override void NotifyCustomizerPopupAction_cancel(long code)
	{
		WWWForm form = new WWWForm();
		form.AddField("code", ""+code);
		
		server.SendCommand ("/api/popup/cancel", form.data);
	}


	public override void SendCheat(string cheatTask) 
	{
		Dictionary<string,string> _data = GetDictionaryForParams();
		_data ["task"] = cheatTask;		
		
		// Send server command
		server.SendGamePlayAction( "cheat/cheat", _data, true);
	}

	public override void SendCheatAddCurrency(string _currency, long _amount, string _itemSku)
    {
        Dictionary<string,string> _data = GetDictionaryForParams();
        _data ["task"] = "add";                
	    _data ["currency"] = _currency;
	    _data ["amount"] = _amount.ToString();
	    if (_itemSku != null)
	    {
	    	_data["itemSku"] = _itemSku;	        
	    }	    

        // Send server command
        server.SendGamePlayAction( "cheat/cheat", _data, true);
    }        	   		
	
	public override void SendCheatSetWorldMapCurrentNodeSku(string _currentNodeSku)
	{								
		if (!string.IsNullOrEmpty(_currentNodeSku))
		{			
			Dictionary<string,string> _data = GetDictionaryForParams();
			_data ["task"] = "set_current_node";                
			_data["sku"] = _currentNodeSku;		
			// Send server command. This command will change latestHangarNodeSku and currentSectorSku variables accordingly
			server.SendGamePlayAction( "cheat/cheat", _data, true);
		}
	}
	
	public override void SendCheatSetLastMainMissionCompletedSku(string _sku)
	{
		// We make sure that there's no flags for the new mission
		
		if (_sku == null)
		{
			_sku = "";
		}
		
		Dictionary<string,string> _data = GetDictionaryForParams();
		_data ["task"] = "set_main_mission_completed";                
		_data["sku"] = _sku;		
		// Send server command
		server.SendGamePlayAction( "cheat/cheat", _data, true);
	}
		
    public override void SendCommand(string _val) 
    {
        string[] _values = _val.Split(' ');
        if (_values.Length >= 1)
        {
            // Params
            Dictionary<string, string> _data = GetDictionaryForParams();
            for( int i = 1; i<_values.Length; i++ )
            {
                string[] _keyValue = _values[i].Trim().Split(':');
                if (_keyValue.Length > 1)
                {
                    _data[ _keyValue[0] ] = _keyValue[1];
                }
            }
            
			server.SendGamePlayAction( _values[0].Trim(), _data);
        }
    }    


    #endregion
	
	// -------------------------------------------------------------------------- //

	/**
	 * DELEGATE RESPONSE from Send Stacked Commands
	 */
	private void OnGamePlayActionsResponse(string response, string cmd2, string responseStatus)
	{
		#if ENCRYPT
		JSONNode json = JSON.Parse(crypto.Decript(response));
		#else
		JSONNode json = JSON.Parse(response);
		#endif
		
		if (debug) Debug.Log(DEBUG_TAG + response);
		
		if ( json != null )
		{
			JSONNode data = json["data"];
			string cmd = data["cmd"];
			switch( cmd )
			{
			case "login":

				m_token = data["data"]["res"]["token"];

				ProcessGameCommandResponse(cmd, data["data"]);

				// The language is sent to the server so the remote push notifications can be localized properly
				// InstanceManager.SettingsManager.NotifyCurrentLanguageToServer();
				break;
				
			case "actionList":
				
				JSONArray args = data["data"]["res"].AsArray;
				int num = args.Count;
				for( int i = 0; i<num;i++ )
				{
					JSONNode action = args[i];
					// Process action response
					string action_name = action["action"];
					ProcessGameCommandResponse(action_name, action["ares"]);
				}
				break;
			}
		}
	}

    // -------------------------------------------------------------------------- //


    public override bool HasConnection()
    {
        bool _returnValue = false;
        string _server = null;
        if (serverRoots.ContainsKey(selectedServer))
        {
            _server = serverRoots[selectedServer];
            if (!string.IsNullOrEmpty(_server))
            {
                try
                {
                    System.Net.IPHostEntry i = System.Net.Dns.GetHostEntry(_server); //"sandstorm-stage.ubi.com");                                                                   
                    _returnValue = true;
                }
                catch
                {
                    return false;
                }
            }
        }

        return _returnValue;
    }

    // -------------------------------------------------------------------------- //

    private OnCustomizerResponse onCustomizerResponse;

	public override void Customizer_Request(OnCustomizerResponse onResponse)
	{
		onCustomizerResponse = onResponse;

		WWWForm form = new WWWForm();
		form.AddField("uid", m_uid);
		form.AddField("token", m_token);

		server.SendCommand ("/api/omniata/customizer", form.data, Customizer_OnResponse, Customizer_OnResponseError);
	}

	private void Customizer_OnResponse(string response, string cmd, string responseStatus)
	{
		onCustomizerResponse (response);
	}

	private void Customizer_OnResponseError(int errorCode, string errorDesc, string response, string cmd)
	{
	/*
		if (BuildSettings.TARGET_ENVIRONMENT == BuildSettings.ETargetEnvironment.PROD)
		{
			onCustomizerResponse (null);
		} else {
			InstanceManager.FlowManager.NotifyCriticalError(false, -1001, response);
		}
		*/
	}


	// It is only done, to inform server that next call will be a get customizer request
	// needed by omniata limitations system
	public override void Customizer_PreRequest(OnCustomizerResponse onResponse)
	{
		onCustomizerResponse = onResponse;
		
		WWWForm form = new WWWForm();
		form.AddField("uid", m_uid);
		form.AddField("token", m_token);

		// form.AddField("sdkVersion", OmniataSDK.Omniata.SDK_VERSION);

		// string pushNotificationsToken = NotificationsManager.Instance.getRemoteNotificationsToken();
		string pushNotificationsToken = "";
		form.AddField("deviceId", pushNotificationsToken.Replace("-", string.Empty));

		form.AddField("locale", UnityEngine.Application.systemLanguage.ToString());

		server.SendCommand ("/api/omniata/trackomload", form.data, Customizer_PreRequest_OnResponse, Customizer_PreRequest_OnResponseError);
	}

	private void Customizer_PreRequest_OnResponse(string response, string cmd, string responseStatus)
	{
		onCustomizerResponse (response);
	}
	
	private void Customizer_PreRequest_OnResponseError(int errorCode, string errorDesc, string response, string cmd)
	{
		onCustomizerResponse (response);
	}

	// -------------------------------------------------------------------------- //



	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //


	public override void Update()
	{
		base.Update();

		if (server != null)
		{
			server.LogicUpdate();

			Latency_LogicUpdate();
		}
	}
	



	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //
	// -------------------------------------------------------------------------- //



	


	
	/**
	 * COMMON FUNCTIONS
	 */

	private Dictionary<string,string> GetDictionaryForParams()
	{
		return new Dictionary<string,string>();
	}
	
	/// <summary>
	/// Sets a parameter of type string in the dictionary under <c>_key</c>. This method makes sure that the value is stored in a JSON friendly way
	/// </summary>	
	private void SetStringInDictionary(Dictionary<string, string> _dictionary, string _key, string _value)
	{	
		if (_dictionary != null)
		{
			// In unity editor a null value is not changed to "" in order to let the developer see the error and decide what to do
#if !UNITY_EDITOR
			// JSON doesn't accept null values
			if (_value == null)
			{
				_value = "";
			}
#endif
			
			_dictionary[_key] = _value;
		}
	}
	
	private void SetIntInDictionary(Dictionary<string, string> _dictionary, string _key, int _value)
	{	
		if (_dictionary != null)
		{											
			_dictionary[_key] = _value + "";
		}
	}
	
	private string boolToString(bool _value)
	{
		return _value.ToString();
	}

	public override void SetLanguage(string _isoCode)
	{
		Dictionary<string,string> data = GetDictionaryForParams();				
		SetStringInDictionary(data, "language", _isoCode);		
		server.SendGamePlayAction ("language/set", data);
	}

	// ----------------------------------------------------------------------- //

	private const float LATENCY_PING_TIMER = 7;	// one PING every 'LATENCY_PING_TIMER' seconds
	private const int LATENCY_PING_STEPS = 10;		// repeat PING 'LATENCY_PING_STEPS' times

	private float latencyPingTimer = 0;
	private int latencyPingCount = 0;

	public override float Latency_GetAverageDuration()
	{
		return server.Latency_GetAverageDuration();
	}

	public override float Latency_GetLastEntryDuration()
	{
		return server.Latency_GetLastEntryDuration();
	}

	public override void Latency_SendSomePingsToServer()
	{
		latencyPingTimer = 0;
		latencyPingCount = LATENCY_PING_STEPS;
	}

	private void Latency_LogicUpdate()
	{
		if (latencyPingCount > 0)
		{
			if (latencyPingTimer > 0)
			{
				latencyPingTimer -= Time.deltaTime;
			}

			if (latencyPingTimer <= 0)
			{
				server.SendGamePlayAction(REQUEST_ACTION_SYNC, null, true);		// 'REQUEST_ACTION_SYNC' is only used like a 'PING'

				latencyPingTimer = LATENCY_PING_TIMER;
				latencyPingCount--;

				Debug.Log("[PIPO] eppps: " + latencyPingCount);
			}
		}



		
	
	}

    // ----------------------------------------------------------------------- //   
}
