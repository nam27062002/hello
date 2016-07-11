using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class GameServerManager :  MonoBehaviour
{

#region singleton
	// Singleton ///////////////////////////////////////////////////////////
	public static GameServerManager s_pInstance = null;
	
	public static GameServerManager SharedInstance
	{
		get
		{
			if (s_pInstance == null)
			{
				s_pInstance = GameContext.AddMainComponent<GameServerManager> ();
			}
			
			return s_pInstance;
		}
	}
	// End Singleton ////////////////////////////////////////////////////////
#endregion

#region listener
	/// Listener /////////////////////////////////////////////////////////////
	private class GameSessionDelegate : GameSessionManager.GameSessionListener
    {
		const string tag = "GameSessionDelegate";

		public bool m_logged = false;

		// WAITING FLAGS
		public bool m_waitingLoginResponse = false;
		public bool m_waitingGetUniverse = false;
		public bool m_waitingMergeResponse = false;

		public SimpleJSON.JSONClass m_lastRecievedUniverse = null;
		public int m_saveDataCounter = 0;

		// Triggers when user was succesfully logged into our server
		public override void onLogInToServer()
		{
			Debug.TaggedLog(tag, "onLogInToServer");
			m_waitingLoginResponse = false;
			// GameServerManager.SharedInstance.
			if (!m_logged)
			{
				m_logged = true;
				Messenger.Broadcast<bool>(GameEvents.LOGGED, m_logged);
			}
		} 
		// Triggers when logout from our server is called
		public override void onLogOutFromServer()
		{
			Debug.TaggedLog(tag, "onLogOutFromServer");
			m_waitingLoginResponse = false;
			if ( m_logged )
			{
				m_logged = false;
				Messenger.Broadcast<bool>(GameEvents.LOGGED, m_logged);
			}

			// no problem, continue playing
		} 

		// The GC login is finished after receiving GC token
		public override void onGameCenterAuthenticationFinished()
		{
			Debug.TaggedLog(tag, "onGameCenterAuthenticationFinished");
		} 

		// Trying to use GC functionality without being authenticated previously
		public override void onGameCenterNotAuthenticatedException()
		{
			Debug.TaggedLog(tag, "onGameCenterNotAuthenticatedException");
		} 

		// The user cancelled the GC login or iOS is returning a cancelled GC state
		public override void onGameCenterAuthenticationCancelled()
		{
			Debug.TaggedLog(tag, "onGameCenterAuthenticationCancelled");
		} 

		// There are merge conflicts and asks to show the merging popup
		public override void onMergeShowPopupNeeded()
		{
			m_waitingMergeResponse = false;
			Debug.TaggedLog(tag, "onMergeShowPopupNeeded");
		} 

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeSucceeded()
		{
			m_waitingMergeResponse = false;
			Debug.TaggedLog(tag, "onMergeSucceeded");
		} 

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeFailed()
		{
			m_waitingMergeResponse = false;
			Debug.TaggedLog(tag, "onMergeFailed");
		} 

		// The user has requested a password to do a cross platform merge
		public override void onMergeXPlatformPass(string pass)
		{
			Debug.TaggedLog(tag, "onMergeXPlatformPass " + pass);
		} 

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeXPlatformSucceeded(string platform, string platformUserId)
		{
			Debug.TaggedLog(tag, "onMergeXPlatformSucceeded");
		} 

		// Probably not needed anywhere, but useful for test cases (actually implemented in unit tests)
		public override void onMergeXPlatformFailed()
		{
			Debug.TaggedLog(tag, "onMergeXPlatformFailed");
		} 

		// Notify the game that a new version of the app is released. Show a popup that redirects to the store.
		public override void onNewAppVersionNeeded()
		{
			Debug.TaggedLog(tag, "onNewAppVersionNeeded");
		} 

		// Notify the game that a new version of the app is released. Show a popup that redirects to the store.
		public override void onUserBlackListed()
		{
			Debug.TaggedLog(tag, "onUserBlackListed");
		} 

		// Returns current achievements state
		public override void onGetAchievementsInfo(Dictionary<string, GameCenterManager.GameCenterAchievement> kAchievementsInfo)
		{
			Debug.TaggedLog(tag, "onGetAchievementsInfo");
		} 

		// Returns current leaderboard score
		public override void onGetLeaderboardScore(string strLeaderboardSKU, int iScore, int iRank)
		{
			Debug.TaggedLog(tag, "onGetLeaderboardScore");
		} 

		// Some API needs a restart of the game
		public override void onRequestGameReset ()
		{
			Debug.TaggedLog(tag, "onRequestGameReset");
		} 

		// When a gameplay action was successful it passes the result to gameplay
		public override void onGamePlayActionProcessed (string strAction, string strResponseData)
		{
			Debug.TaggedLog(tag, "onGamePlayActionProcessed : " + strAction);
			switch( strAction )	
			{
				case GameServerManager.GET_UNIVERSE:
				{
					Debug.TaggedLog(tag, strResponseData);
					onGetUniverse( strResponseData );
				}break;
				case GameServerManager.SET_UNIVERSE:
				{
					m_saveDataCounter--;
					UsersManager.currentUser.saveCounter++; 
				}break;
			}
		} 

		// When a gameplay action fails it returns the error code
		public override void onGamePlayActionFailed (string strAction, int iErrorStatus)
		{
			Debug.TaggedLog(tag, "onGamePlayActionFailed");
			switch( strAction )
			{
				case GameServerManager.GET_UNIVERSE:
				{
					onGetUniverseError( iErrorStatus );
				}break;
				case GameServerManager.SET_UNIVERSE:
				{
					// Reason?
					// If cannot connect?
						// -> m_saveDataCounter = 0;
					// If server has new version -> I shouldn't be setting Universe!!
				}break;
			}
		} 


		void ResetWaitingFlags()
		{
			m_waitingLoginResponse = false;
			m_waitingGetUniverse = false;
			m_waitingMergeResponse = false;
		}

		// Treat Game Play Actions Response
		void onGetUniverse( string serverUniverse )
		{
			m_waitingGetUniverse = false;
			// Load universe and compare with current to check solutions
			SimpleJSON.JSONClass serverSave = SimpleJSON.JSONNode.Parse( serverUniverse )  as SimpleJSON.JSONClass;
			if ( serverSave != null )
			{
				m_lastRecievedUniverse = serverSave;
				Messenger.Broadcast(GameEvents.NEW_SAVE_DATA_FROM_SERVER);
			}
		}

		void onGetUniverseError( int errorStatus )
		{
			m_waitingGetUniverse = false;

		}
    }

	/// End Listener //////////////////////////////////////////////////////////
#endregion

#region action_names
	private const string GET_UNIVERSE = "universe/get";
	private const string SET_UNIVERSE = "universe/set";
#endregion


	private bool m_configured = false;

	private bool m_saveDataRecovered = false;	// Tells if we recovered the save data from the server and merged with the local savedata
	public bool saveDataRecovered
	{
		get{ return m_saveDataRecovered; }
		set { m_saveDataRecovered = value; }
	}
	private bool m_mergedWithSocial = false;	// Tells if social already merged
	public bool mergedWithSocial
	{
		get{ return m_mergedWithSocial; }
		set { m_mergedWithSocial = value; }
	}

	// Game Login and Save Data Flow ////////////////////////////////
	// Try To Login -> Get Universe -> Resolve Current Universe / Server Universe -> Set m_saveDataRecovered = true -> *
	// Try To Login On Social Platform -> *
	// * At this point on both flows we can try to merge account -> once merged Set m_mergedWithSocial = true

	// Notes: 
	// 		- if we ever Logout from Social Platform and Login Again -> *
	//		- 
	////////////////////////////////////////////////////////////////

	private GameSessionDelegate m_delegate;

	void Awake()
	{
		PrepareServerConfig();
	}

	void OnDestroy()
	{
		
	}


	public void PrepareServerConfig()
    {
		if (!m_configured)
		{
	        CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");

	        // Init server game details
	        ServerManager.ServerConfig kServerConfig = new ServerManager.ServerConfig();


	        // Dev
	        // {
			kServerConfig.m_strServerURL = "bcn-dev-dragon01";
	        kServerConfig.m_strServerPort = "8080";
	        kServerConfig.m_strServerDomain = "dragon";
			kServerConfig.m_strClientVersion = "1.0";
	        kServerConfig.m_eBuildEnvironment = CaletyConstants.eBuildEnvironments.BUILD_DEV;
	        // }

	        if (settingsInstance != null) 
	        {
				switch( settingsInstance.m_iBuildEnvironmentSelected )
				{
					case (int)CaletyConstants.eBuildEnvironments.BUILD_LOCAL:
					{
						kServerConfig.m_strServerURL = settingsInstance.m_strLocalServerURL;
		                kServerConfig.m_strServerPort = settingsInstance.m_strLocalServerPort;
		                kServerConfig.m_strServerDomain = settingsInstance.m_strLocalServerDomain;
		                kServerConfig.m_eBuildEnvironment = CaletyConstants.eBuildEnvironments.BUILD_LOCAL;	
					}break;
					case (int)CaletyConstants.eBuildEnvironments.BUILD_INTEGRATION:
					{
						kServerConfig.m_strServerURL = "bcn-int-dragon01";
		                kServerConfig.m_strServerPort = "8080";
		                kServerConfig.m_strServerDomain = "dragon";
		                kServerConfig.m_eBuildEnvironment = CaletyConstants.eBuildEnvironments.BUILD_INTEGRATION;
					}break;
					case (int)CaletyConstants.eBuildEnvironments.BUILD_STAGE:
					{
						kServerConfig.m_strServerURL = "bcn-stage-dragon01";
		                kServerConfig.m_strServerPort = "8080";
		                kServerConfig.m_strServerDomain = "dragon";
		                kServerConfig.m_eBuildEnvironment = CaletyConstants.eBuildEnvironments.BUILD_STAGE;
					}break;
					case (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION:
					{
						kServerConfig.m_strServerURL = "bcn-production-dragon01";
		                kServerConfig.m_strServerPort = "8080";
		                kServerConfig.m_strServerDomain = "dragon";
		                kServerConfig.m_eBuildEnvironment = CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION;
					}break;
					case (int)CaletyConstants.eBuildEnvironments.BUILD_DEV:
					{
						// already set up
					}break;
				}
	            kServerConfig.m_strClientVersion = settingsInstance.GetClientBuildVersion ();
	        }

	#if UNITY_EDITOR
	        kServerConfig.m_strClientPlatformBuild = "editor";
	#elif UNITY_ANDROID
	        kServerConfig.m_strClientPlatformBuild = "android";
	#elif UNITY_IOS
	        kServerConfig.m_strClientPlatformBuild = "ios";
	#endif
			kServerConfig.m_strServerApplicationSecretKey = "avefusilmagnifica";

	        ServerManager.SharedInstance.Initialise (ref kServerConfig);

			m_delegate = new GameServerManager.GameSessionDelegate();
	        GameSessionManager.SharedInstance.SetListener( m_delegate );

			m_configured = true;
        }
    }

	public void LoginToServer ()
    {
		PrepareServerConfig();
		if ( !m_delegate.m_logged )
    	{
			if (!m_delegate.m_waitingLoginResponse)
			{
				m_delegate.m_waitingLoginResponse = true;
	        	GameSessionManager.SharedInstance.LogInToServer ();
	        }
        }
    }

    public bool IsLogged()
    {
    	return m_delegate.m_logged;
    }

    public SimpleJSON.JSONClass GetLastRecievedUniverse()
    {
    	return m_delegate.m_lastRecievedUniverse;
    }

    public void CleanLastRecievedUniverse()
    {
    	m_delegate.m_lastRecievedUniverse = null;
    }

    public void GetUniverse()
    {
    	if (!m_delegate.m_waitingGetUniverse)
    	{
    		m_delegate.m_waitingGetUniverse = true;
			GameSessionManager.SharedInstance.SendGamePlayActionInJSON (GameServerManager.GET_UNIVERSE);
		}
    }

    public void SetUniverse( SimpleJSON.JSONClass universe )
    {
		// if ( m_saveDataRecovered )
		{
			universe["userProfile"]["saveCounter"] = (universe["userProfile"]["saveCounter"].AsInt + m_delegate.m_saveDataCounter).ToString();
			GameSessionManager.SharedInstance.SendGamePlayActionInJSON (GameServerManager.SET_UNIVERSE, universe);
			m_delegate.m_saveDataCounter++;
		}
    }


    public void MergeSocialAccount()
    {
    	if ( SocialPlatformManager.SharedInstance.IsLoggedIn() )
    	{
    		if (!m_delegate.m_waitingMergeResponse)
    		{
				string platform = SocialPlatformManager.SharedInstance.GetPlatformName();
				string userId = SocialPlatformManager.SharedInstance.GetUserId();
				string userName = SocialPlatformManager.SharedInstance.GetUserName();
				string token = SocialPlatformManager.SharedInstance.GetToken();
				m_delegate.m_waitingMergeResponse = true;
				GameSessionManager.SharedInstance.MergeGameAccounts	( platform, userId, userName, token);
			}
    	}
    	else
    	{
    		Debug.LogError("Cannot Merge With Social if not logged in");
    	}

	}


	/// <summary>
	/// Tries the merge save datas. If true
	/// </summary>
	/// <returns><c>true</c>, if merge save datas was tryed, <c>false</c> otherwise.</returns>
	/// <param name="saveData1">Save data1.</param>
	/// <param name="saveData2">Save data2.</param>
	/*
	public bool TryMergeSaveDatas( SimpleJSON.JSONClass saveData1, SimpleJSON.JSONClass saveData2)
	{
		

	}
	*/
}
