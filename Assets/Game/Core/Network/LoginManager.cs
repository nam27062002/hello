//#define USE_TOKENS 0

using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID
// TODO (miguel) Import Google Play Games
// using GooglePlayGames;
#endif

// https://developer.apple.com/library/ios/documentation/GameKit/Reference/GKLocalPlayer_Ref/index.html#//apple_ref/occ/instm/GKLocalPlayer/generateIdentityVerificationSignatureWithCompletionHandler
// publicKeyUrl, signature, salt
// http://developer.android.com/reference/android/accounts/AccountManager.html
// oatuh token



public sealed class LoginManager
{
	static readonly LoginManager instance = new LoginManager();

	private const string KEY_ANONYMOUS_PLATFORM_USER_ID = "Login.AnonymousPlatformUserID";
	// private const string KEY_SOCIAL_PLATFORM_USER_ID = "Login.SocialPlatformUserID";
	
	// DIFFERENT PLATFORMS
		// Editor
	private const string PLATFORM_EDITOR = "game";
	private const string PLATFORM_LDAP = "gamecenter";
    // IOS
	public const string PLATFORM_IOS = "iOS";
	public const string PLATFORM_GAMECENTER = "gamecenter";
	
    // Android Google
	public const string PLATFORM_ANDROID = "android";
    public const string PLATFORM_GOOGLEPLAY = "google";

    // Android Amazon
    public const string PLATFORM_AMAZON = "amazon";
    public const string PLATFORM_GAMECIRCLE = "gamecircle";
    // END DIFFERENT PLATFORMS

    private string anonymousPlatform = "";
	private string anonymousPlatformUserId = "";
	private string multipleChoiceAnonymousUserId = "";
	
	private string socialPlatform = "";

	private bool authenticated = false;
	private bool externalAuthenticated = false;
	private bool mergedWithExternal = false;
	
	private string accessToken;
	
	
	// Game Center secure login	data
	string gameCenterPublicKeyURL = "";
	string gameCenterSignature = "";
	string gameCenterSalt = "";
	string gameCenterTimestamp = "";
	
	private bool mergeAllowed = true;
		
	// ------------------------------------------------------------------ //

	public static LoginManager Instance
	{
		get
		{
			return instance;
		}
	}

	//static LoginManager() {}
	private LoginManager()
	{
		Initialize();
	}

	/// <summary>
	/// This function gets saved userid and social platform user id. Also it starts
	/// the resource loader to get the assets and starts the music of the game.
	/// Finally it assign the callbacks to the network
	/// </summary>
	private void Initialize()
	{
		InitializeExternalPlatform();

		// Getting user accounts
		LoadKeys();

		//Registering the callbacks
		RegisterRequestNetwork();
	}

	public void InitializeExternalPlatform()
	{
#if UNITY_EDITOR
		
		anonymousPlatform = PLATFORM_EDITOR;
		socialPlatform = PLATFORM_LDAP;
		Social.Active = new EditorSocialPlatform();
		
#elif UNITY_IPHONE
		
		anonymousPlatform = PLATFORM_IOS;
		socialPlatform = PLATFORM_GAMECENTER;
		
#elif UNITY_ANDROID
		
		anonymousPlatform = PLATFORM_ANDROID;
		socialPlatform = PLATFORM_GOOGLEPLAY;
		
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
		
#endif
	}

	public static string GetSocialPlatformKeyFromPlatform(string _platform)
	{
		switch (_platform)
		{
		case PLATFORM_IOS:		return PLATFORM_GAMECENTER;
		case PLATFORM_ANDROID:	return PLATFORM_GOOGLEPLAY;
		case PLATFORM_AMAZON:	return PLATFORM_GAMECIRCLE;
		}
		return null;
	}

	public void DeinitializeExternalPlatform()
	{
#if UNITY_ANDROID 
	// TODO (miguel) Import Google Play Games
/*
		((GooglePlayGames.PlayGamesPlatform)Social.Active).SignOut();
		Log("Signout performed successfully. Sending SocialStatusUpdated event");
		externalAuthenticated = false;

		EventManager.Instance.DispatchEvent(EventManager.EventName.SocialStatusUpdated);
		*/
#endif
	}

	public bool isExternalAuthenticated() {return externalAuthenticated;}

	public bool hasMergedWithExternal() {return mergedWithExternal;}

	public string GetExternalPlatformId() {return isExternalAuthenticated()? Social.Active.localUser.id : "";}

	public void Reset()
	{
		authenticated = false;
		externalAuthenticated = false;
		Log ("Reset externalAuth = " + externalAuthenticated);

		mergedWithExternal = false;
		
		// Fixes problem when network is reset
		UnRegisterRequestNetwork();
		RegisterRequestNetwork();
	}

	public bool isAuthenticated()
	{
		return authenticated;
	}

	public bool MergeAllowed
	{
		set
		{
			mergeAllowed = value;
		}
	}

	/// <summary>
	/// This function unregisters the callbacks of the networking and the callback
	/// to the resource loader.
	/// </summary>
	public void Destroy()
	{
		UnRegisterRequestNetwork();
	}

	/// <summary>
	/// This function is to login the game checking the state of the accounts.
	/// </summary>
	public void Login()
	{
		if (!string.IsNullOrEmpty(anonymousPlatformUserId))
		{
			Log("authentication by anonymous platform: " + anonymousPlatformUserId);
			Authenticate(anonymousPlatform, anonymousPlatformUserId);
		}
		// Ask to our servers to create a new Anonymous UserId, after that, we logins with that UserId...
		else
		{
			Log ("asking server for an user ID for authentication anonymous");
			SetNetworkPlatform( anonymousPlatform );
			RequestNetwork.instance.RequestAuthKey();
		}

	}

	private void LoadKeys()
	{
		anonymousPlatformUserId = PlayerPrefs.GetString(KEY_ANONYMOUS_PLATFORM_USER_ID);
	}

	private void SaveKeys()
	{
		PlayerPrefs.SetString(KEY_ANONYMOUS_PLATFORM_USER_ID, anonymousPlatformUserId);
	}
	
	static public void RemoveKeys()
	{
		PlayerPrefs.DeleteKey(KEY_ANONYMOUS_PLATFORM_USER_ID);
	}


	private void ProcessAuthenticateToGameSocialPlatform(bool success)
	{
		Log("auth Success: " + success);

		if (success)
		{
			accessTokenRetries = 5;
			ProcessAuthenticateToGameSocialPlatform_Loop();
		} else {
			ProcessAuthenticateToGameSocialPlatform_EndPart(success);
		}
	}

	int accessTokenRetries;

	private void ProcessAuthenticateToGameSocialPlatform_Loop()
	{
		accessToken = getToken ();
		Log("accessToken: " + accessToken);

		if (!string.IsNullOrEmpty (accessToken) || accessTokenRetries <= 0)
		{
			ProcessAuthenticateToGameSocialPlatform_EndPart(true);
		} else {
			accessTokenRetries--;
			// InstanceManager.Instance.DelayedMethodCall(ProcessAuthenticateToGameSocialPlatform_Loop, 1f);
		}
	}


	/// <summary>
	/// This is the callback called by the authentication to game social platform
	/// </summary>
	/// <param name="success"><c>true</c> is authentication successed.</param>
	private void ProcessAuthenticateToGameSocialPlatform_EndPart(bool success)
	{
		if (success)
		{
#if USE_TOKENS		
			// Ask for token
			PlatformUtils.Instance.GetTokens();
#else
			accessToken = getToken ();

			if (authenticated)
			{
				// Checks whether or not the merge is allowed. The merge is not allowed if the user has already seen the merge popup and decided to
				// keep her local progress, so a new login has been forced. This forced login is not allowed to show the merge popup again. We followed
				// this approach in order to prevent potential Out of syncs, such as for example when the user sees this merge popup right after installing
				// a room and before confirming the installation
				if (mergeAllowed)
				{
					Dictionary<string, string> tokens = new Dictionary<string, string>();
					tokens[socialPlatform] = accessToken;
					Log("socialPlatform = " + socialPlatform);
					Log("tokens[" + socialPlatform + "] = " + tokens[socialPlatform]);
					Merge(RequestNetwork.instance.m_platform, socialPlatform, Social.localUser.id, Social.localUser.userName, tokens);
				}
			}
#endif
			// InstanceManager.ScoreManager.InitPlatformAchievements();
		}
#if UNITY_ANDROID
		// fix for SS-2194
		else
		{
			PlayerPrefs.SetInt ("cancelledExternalAuth", 1);
		}
#endif

		// Popup merge is not allowed to be shown only once (after the user chose to keep her local progress, in order to prevent the user from
		// falling in a merge popup - loading - merge popup loop). 
		if (!mergeAllowed)
		{
			mergeAllowed = true;
		}

		externalAuthenticated = success;
		Log ("ProcessAuthenticateToGameSocialPlatform_EndPart externalAuth = " + externalAuthenticated);
		/*
		CustomEvent ev = new CustomEvent();
		ev.type = EventManager.EventName.SocialStatusUpdated;
		EventManager.Instance.DispatchEvent(ev);
		*/
	}
	
	private string getToken()
	{
#if UNITY_ANDROID
		// TODO (miguel) : Import PlayGamesPlatform
		// return PlayGamesPlatform.Instance.GetToken();
		return "";
#else
		return "";
#endif
	}
	
	private void onToken( bool success, Dictionary<string,string> tokens)
	{
#if USE_TOKENS
		if ( success )
		{
			Merge(InstanceManager.RequestNetwork.platform, socialPlatform, Social.localUser.id, Social.localUser.userName, tokens);
		}
		else
		{
			if ( Social.localUser.authenticated )
			{
				// Renew token?
			}
			else
			{
				// Authenticate again?
			}
		}
#endif
	}

	/// <summary>
	/// This function is the callback of the RequesteAuthKey. Here we get the response of the server
	/// and we extract the key from it.
	/// </summary>
	/// <param name="response">Is the response of the server in JSON format</param>
	private void onGetKey(string response)
	{
		//JSONNode json = JSON.Parse(response);
		anonymousPlatformUserId = response; //json["uid"].Value;
		
		SaveKeys ();

		Authenticate(anonymousPlatform, anonymousPlatformUserId);
	}

	/// <summary>
	/// Authenticate the specified userId.
	/// </summary>
	/// <param name="userId">User identifier.</param>
	private void Authenticate(string platform, string userId, string userName = "")
	{
		authenticated = false;
		Log("Authenticate platform=" + platform + " userId=" + userId + " userName=" + userName);
		RequestNetwork network = RequestNetwork.instance;
		network.m_platform = platform;				
		network.Authenticate(network.m_serverUrl,"/api/auth/b", userId, userName);
	}

	private void Merge(string platform, string targetPlatform, string targetUserId, string targetUserName = "", Dictionary<string,string> platformTokens = null )
	{
		Log("Merge platform=" + platform + " targetPlatform=" + targetPlatform + " targetUserId=" + targetUserId + " targetUserName=" + targetUserName);
		RequestNetwork network = RequestNetwork.instance;
		// network.platform = platform;
		RequestNetwork.instance.Merge(network.m_serverUrl,"/api/merge/c", targetPlatform, targetUserId, targetUserName, false, platformTokens);
	}

	public void DeleteExternalMappings()
	{
		if(isExternalAuthenticated())
		{
			RequestNetwork.instance.DeleteMappings(GetExternalPlatformId(), socialPlatform);
		}
	}

	private void SetNetworkPlatform(string _platform)
	{
		RequestNetwork.instance.m_platform = _platform;
	}

	/// <summary>
	/// This function is the callback of the server when it may me to login in the game.
	/// It launch the final login of the game.
	/// </summary>
	/// <param name="response">Response.</param>
	void onAuth( SimpleJSON.JSONNode response )
	{
		RequestNetwork network = RequestNetwork.instance;
		Log ("onAuth network.platform=" + network.m_platform + " Social.localUser.authenticated=" + Social.localUser.authenticated
				+ " Social.localUser.id=" + Social.localUser.id + " Social.localUser.userName=" + Social.localUser.userName
				+ " anonymousPlatformUserId=" + anonymousPlatformUserId);
		authenticated = true;

		// FIRST of all, populate all configs obtained from our server
		JSONNode configsByEnvironmentJObj = response ["config"];		
		// InstanceManager.Config.PopulateRewritableParamsFromServer (configsByEnvironmentJObj);		

		RequestNetwork.instance.Country = response ["country"];

		// if (!InstanceManager.Config.UseCustomizerRequestAfterAuth())
		{
			JSONNode customizerJObj = response ["customizer"];
			if (customizerJObj != null)
			{
				// InstanceManager.CustomizerManager.processCustomizer (customizerJObj);				
				// InstanceManager.Instance.tempCustomizerJObj = (JSONClass)customizerJObj;
			}
		}

		/*
		if (InstanceManager.Config.UseCustomizerRequestAfterAuth())
		{
			InstanceManager.Instance.DelayedMethodCall(RequestCustomizer, 0.75f);
		} else {
			OnCustomizerResponse(null);
		}
		*/
		OnCustomizerResponse(null);
	}

	private void RequestCustomizer()
	{
	/*
		if (InstanceManager.Config.useServerCallToPreventCustomizerRequest ())
		{
			RequestNetwork.instance.Customizer_PreRequest (OnPreRequestCustomizerResponse);
		} else {
			RequestNetwork.instance.Customizer_Request (OnCustomizerResponse);
		}
		*/
	}
	
	// ONLY USED if (Config.useServerCallToPreventCustomizerRequest() == true)
	private void OnPreRequestCustomizerResponse(string response)
	{
		RequestNetwork.instance.Customizer_Request (OnCustomizerResponse);
	}

	public void OnCustomizerResponse(string response)
	{
		Log("OnCustomizerResponse");

		if (!string.IsNullOrEmpty(response))
		{
			JSONNode customizerJObj = null;

			try
			{
				customizerJObj = JSONClass.Parse(response);
			}
			catch(System.Exception e)
			{
				Debug.Log("[CUSTOMIZER] Customizer parsing failed: " + e.ToString() + " :: " + response);
				
				// InstanceManager.Instance.tempCustomizerJObj = (JSONClass)JSON.Parse("{\"error\": \"json parse failed\", \"desc\": \"" + e.ToString() + "\", \"json\": \"" + response + "\"}");
			}
			/*
			if (customizerJObj != null)
			{
				InstanceManager.CustomizerManager.processCustomizer (customizerJObj);				
				InstanceManager.Instance.tempCustomizerJObj = (JSONClass)customizerJObj;
			}
			*/
		}
		else
		{
			// InstanceManager.Instance.tempCustomizerJObj = (JSONClass)JSON.Parse("{\"error\": \"server response failed or timeout\"}");
		}

		RequestNetwork.instance.Login();


		// if the player has played before anonymously and never with a social platform id
#if UNITY_ANDROID
		// fix for SS-2194
		if(PlayerPrefs.GetInt("cancelledExternalAuth", 0) == 0)
		{
#endif
		AuthenticateExternal();
#if UNITY_ANDROID
		}
#endif
	}

	public void AuthenticateExternal()
	{
		Social.localUser.Authenticate(ProcessAuthenticateToGameSocialPlatform);
		
		externalAuthenticated = Social.Active.localUser.authenticated;
		Log("AuthenticateExternal externalAuth = " + externalAuthenticated);
	}
/*
	void OnOmniataCustomizerResponse(string response)
	{
		if (response != null && response.Length > 0)
		{
			SimpleJSON.JSONNode customizerJObj = null;
			
			try
			{
				SimpleJSON.JSONNode fullJObj = SimpleJSON.JSONClass.Parse(response);
				
				SimpleJSON.JSONArray contentJAry = fullJObj["content"].AsArray;
				
				if (contentJAry != null && contentJAry.Count > 0)
				{
					customizerJObj = contentJAry[0].AsObject;
				}
			}
			catch(System.Exception)
			{
				customizerJObj = null;
			}
			
			if (customizerJObj != null)
			{
				InstanceManager.CustomizerManager.processCustomizer(customizerJObj);
			}
		}
		
		InstanceManager.RequestNetwork.Login();
	}
*/


	
	/// <summary>
	/// This function is the callback of the server when it may me to merge accounts.
	/// </summary>
	/// <param name="response">Response.</param>
	void onMerge(SimpleJSON.JSONNode response, int statusCode)
	{
		Log("onMerge statusCode=" + statusCode);

		switch (statusCode)
		{
			case 200: // Merge successful. Mapping already existed.
			case 201: // Merge successful. A new mapping for targetPlatform + targetPlatformId was created as a result
			case 205: // Merge successful. Account merging has been forced.
				// socialPlatformUserId = Social.localUser.id;
				// SaveKeys();
				// InstanceManager.RequestNetwork.Login();
				
				// Check if we need to change name!
				/*
				if (Social.localUser.authenticated)
				{
					if (InstanceManager.MyUserData != null && InstanceManager.MyUserData.userName != Social.localUser.userName)
					{
						if (log)
							Log("Name Change: " + InstanceManager.MyUserData.userName + " => " + Social.localUser.userName );
						InstanceManager.MyUserData.userName = Social.localUser.userName;
						EventManager.Instance.DispatchEvent(  EventManager.EventName.UserNameChanged );
					}
				}
				mergedWithExternal = true;
				EventManager.Instance.DispatchEvent( EventManager.EventName.OnMergeSuccesful );
				*/
				break;
				
			case 300: // Multiple Choices
			{
				onMerge300( response );
			}break;
			case 302:	// Conflict between accounts, so we restart with the last social account
			{
				onMerge302( response );
			}break;

			case 403:	// SocialPlatform don't respond
			{
				Log("onMerge social platform doesn't respond!");
			}break;
        }
	}

	public void ShowMergePopup(Action _onMergeConfirmCloud, Action _onMergeConfirmLocal)
	{
		string localUID = RequestNetwork.instance.m_uid;
		string cloudUID = "";

		Log("ShowMergePopup" + mergeMultipleChoicesObject.ToString());
		foreach(string key in mergeMultipleChoicesObject.m_Dict.Keys)
		{
			int aux = 0;

			if(key != localUID && int.TryParse(key, out aux))
			{
				cloudUID = key;
			}
		}

		// popup.onKeepLocalButton = _onMergeConfirmLocal;
		// popup.onKeepCloudButton = _onMergeConfirmCloud;
	}

	// continue with the local game account
	public void onMergeConfirmNo()
	{
		// Do nothing, continue playing as normal
		// EventManager.Instance.DispatchEvent( EventManager.EventName.OnMergeSuccesful );
		DeinitializeExternalPlatform();
	}

	// load game center account
	public void onMergeConfirmYes()
	{
		// socialPlatformUserId = Social.localUser.id;
		anonymousPlatformUserId = multipleChoiceAnonymousUserId;
		SaveKeys();
		// InstanceManager.FlowManager.GoToLoading();
	}

	/// <summary>
	/// Merge has produced an error.
	/// </summary>
	/// <param name="response">Is the error message that we got from the server</param>
	void onMergeError( int errorCode, string errorDesc, string response )
	{
		// Decript response!
		
		Log("onMergeError error (" + errorCode + ") " + errorDesc + " response=" + response);

		switch (errorCode)
		{
		case 200:
		case 201:
		case 205:
			onMerge(new JSONClass(), errorCode);
				return;


			case 300: // Multiple Choices
			{
				JSONNode json = JSON.Parse(response);	
				onMerge300( json );			
			}
			break;
			case 302:	// Conflict between accounts, restart with the last social account
			{
				JSONNode json = JSON.Parse(response);
				onMerge302( json );
			}break;
			
			// new social platform user id without anonymous and previous anonymous was already linked to another social
			case 409:
			{
				// We start again as new
				anonymousPlatformUserId = "";	
				SaveKeys();
				// InstanceManager.FlowManager.GoToLoading();
				
				// Authenticate(anonymousPlatform, anonymousPlatformUserId);
	            // InstanceManager.RequestNetwork.Login();
            }
			break;
	
			default:
			{
				/*
				NetError netErrorEvent = new NetError();
				// Send out of sync event
				if( string.IsNullOrEmpty(response) ) {
					netErrorEvent.error_msg = errorCode + " " + errorDesc;
				} else {
					JSONNode json = JSON.Parse(response);
					
					if ( json != null )
						netErrorEvent.error_msg = json["error_msg"];
					else
						netErrorEvent.error_msg = response;
				}
				netErrorEvent.response_code = -1;
				EventManager.Instance.DispatchEvent(netErrorEvent);
				*/
			}
			break;
		}
	}


	private JSONClass mergeMultipleChoicesObject;

	private void onMerge300( JSONNode json )
	{

		multipleChoiceAnonymousUserId = "";
		mergeMultipleChoicesObject = json.AsObject;
		Log("onMerge300 response : " + json.ToString());
		foreach( System.Collections.Generic.KeyValuePair<string, JSONNode> pair in mergeMultipleChoicesObject.m_Dict)
		{
			if ( pair.Key != RequestNetwork.instance.m_uid && pair.Value["mappings"] != null )
			{
				JSONClass c = pair.Value["mappings"].AsObject;
				multipleChoiceAnonymousUserId = c[ anonymousPlatform.ToLower() ];
				if (multipleChoiceAnonymousUserId == null)
					multipleChoiceAnonymousUserId = "";
			}
		}
		
		// InstanceManager.FlowManager.NotifyMergeEvent();
		
		/*
		// multipleChoiceAnonymousUserId = json["mappings"][anonymousPlatform];;
		// give the player an option to load local or social account
		InstanceManager.PopupFactory.ShowConfirmationPopup(
			Localization.Get("TID_UI_MERGE_LOAD_GC_ACCOUNT_TITLE"),
			Localization.Get("TID_UI_MERGE_LOAD_GC_ACCOUNT_TEXT"),		// Your local progress is different than the progress associated with this Game Center account.\nDo you want to load the game associated with this Game Center account?
			Localization.Get("TID_GEN_YES"), Localization.Get("TID_GEN_NO"),
			onMergeConfirmYes, onMergeConfirmNo);
			*/
	}
	
	private void onMerge302( JSONNode json )
	{
		anonymousPlatformUserId = json["mappings"][ anonymousPlatform.ToLower() ];
		if ( anonymousPlatformUserId == null )
			anonymousPlatformUserId = "";
		SaveKeys();
		// InstanceManager.FlowManager.GoToLoading();
	}

	/// <summary>
	/// This is the callback launched after do the login. In this function we get the last game saved and enters to the game.
	/// </summary>
	/// <param name="action">Is the action the function has to launch.</param>
	/// <param name="response">Not userd for the moment</param>
	private void onGameResponse( string action, SimpleJSON.JSONNode response)
	{		
		switch( action )
		{
			case RequestNetwork.REQUEST_ACTION_LOGIN:
				// InstanceManager.RequestNetwork.token = response["res"]["token"];
				// InstanceManager.RequestNetwork.RequestUniverse();
			break;
	
			case RequestNetwork.REQUEST_ACTION_UNIVERSE:
				// InstanceManager.FlowManager.OnRequestUniverseResponse(response, false);	
			break;
		}
	}

	/// <summary>
	/// Error produced when we tries to load the game
	/// </summary>
	/// <param name="action">In which case the error has launched</param>
	/// <param name="response">Not used for the moment</param>
	private void onGameResponseError( string action, SimpleJSON.JSONNode response)
	{
		switch( action )
		{
		case RequestNetwork.REQUEST_ACTION_LOGIN:
			Log("Error on Login\n" + response.ToString());
		break;
		
		case RequestNetwork.REQUEST_ACTION_UNIVERSE:
			Log("Error on getUniverse\n" + response.ToString());
		break;
		}				
	}

	/// <summary>
	/// Registers the request network.
	/// </summary>
	private void RegisterRequestNetwork()
	{
		RequestNetwork _requestNetwork = RequestNetwork.instance;
		if (_requestNetwork)
		{
			_requestNetwork.onAuthResponse += onAuth;

			_requestNetwork.onMergeResponse += onMerge;
			_requestNetwork.onMergeResponseError += onMergeError;
			
			_requestNetwork.onGameResponse += onGameResponse;
			_requestNetwork.onGameResponseError += onGameResponseError;

			_requestNetwork.onAuthKey += onGetKey;
		}
	}
	
	/// <summary>
	/// Unregister request network.
	/// </summary>
	private void UnRegisterRequestNetwork()
	{
		RequestNetwork _requestNetwork = RequestNetwork.instance;
		if ( _requestNetwork != null )
		{
			_requestNetwork.onAuthResponse -= onAuth;

			_requestNetwork.onMergeResponse -= onMerge;
			_requestNetwork.onMergeResponseError -= onMergeError;
			
			_requestNetwork.onGameResponse -= onGameResponse;
			_requestNetwork.onGameResponseError -= onGameResponseError;

			_requestNetwork.onAuthKey -= onGetKey;
		}
	}
	
#region GC_secure_signature_accesors	
	public string GCPublicKeyURL
	{
		get
		{
			return gameCenterPublicKeyURL;
		}
		set
		{
			gameCenterPublicKeyURL = value;
		}
	}
	public string GCSignature
	{
		get
		{
			return gameCenterSignature;
		}
		set
		{
			gameCenterSignature = value;
		}
	}
	public string GCSalt
	{
		get
		{
			return gameCenterSalt;
		}
		set
		{
			gameCenterSalt = value;
		}
	}
	public string GCTimestamp
	{
		get
		{
			return gameCenterTimestamp;
		}
		set
		{
			gameCenterTimestamp = value;
		}
	}
	
	public void OnGCTokens(bool success)
	{
		// Prepare dictionary
		Dictionary<string,string> tokens = new Dictionary<string, string>();
		tokens.Add("publicKeyUrl", gameCenterPublicKeyURL);
		tokens.Add("signature", gameCenterSignature);
		tokens.Add("salt", gameCenterSalt);
		tokens.Add("timestamp", gameCenterTimestamp);
		onToken( success, tokens);
	}
#endregion

	public void test300Popup()
	{
		mergeMultipleChoicesObject = JSON.Parse("{\"165\":{\"mappings\":{\"android\":\"95d6f4f5-84a7-485b-b3ba-93818170f0da\", \"google\":\"115511087868969616594\"}, \"totalTime\":\"1158447\", \"maxTierSku\":\"sh_usr_dps_01\"}, \"serverTime\":\"1434721549938\", \"" + RequestNetwork.instance.m_uid + "\":{\"totalTime\":\"0\", \"maxTierSku\":\"sh_usr_dps_01\"}}") as JSONClass;
		ShowMergePopup(() => {}, () => {});
	}


	public string AnonymousPlatform
	{
		get
		{
			return anonymousPlatform;
		}
	}
	
	public string SocialPlatform
	{
		get
		{
			return socialPlatform;
		}
	}

	// ------------------------------------------------------------------ //

#region log
	private bool log = true;
	private const string CHANNEL_LOG = "[LoginManager] ";

	private void Log(string msg)
	{
		Debug.Log(CHANNEL_LOG + msg);
	}

	private void LogError(string msg)
	{
		Debug.LogError(CHANNEL_LOG + msg);
	}
#endregion

	// ------------------------------------------------------------------ //
}
