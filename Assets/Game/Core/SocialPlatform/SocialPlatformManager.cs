#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using UnityEngine;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
public class SocialPlatformManager : MonoBehaviour
{
	// Singleton ///////////////////////////////////////////////////////////
	
	private static SocialPlatformManager s_pInstance = null;
	
	public static SocialPlatformManager SharedInstance
	{
		get
		{
			if (s_pInstance == null)
			{
				s_pInstance = GameContext.AddMainComponent<SocialPlatformManager> ();
			}
			
			return s_pInstance;
		}
	}

	//////////////////////////////////////////////////////////////////////////	

	// Social Platform Response //////////////////////////////////////////////

	public void OnSocialPlatformLogin(SocialUtils.EPlatform platformId)
	{
        bool isLoggedIn = IsLoggedIn(platformId);
        Log("OnSocialPlatformLogin platform: " + platformId + " isLoggedIn: " + isLoggedIn);
        Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, true);        
    }

    public void OnSocialPlatformLoginFailed(SocialUtils.EPlatform platformId)
	{
        bool isLoggedIn = IsLoggedIn(platformId);
        Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, isLoggedIn);        
    }

    public void OnSocialPlatformLogOut(SocialUtils.EPlatform platformId)
	{
		SocialPlatformManager.SharedInstance.OnLogout();

        bool isLoggedIn = IsLoggedIn(platformId);
        Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, isLoggedIn);
	}
    //////////////////////////////////////////////////////////////////////////    

    private bool IsInited { get; set; }    

	private Dictionary<SocialUtils.EPlatform, SocialUtils> m_platforms = new Dictionary<SocialUtils.EPlatform, SocialUtils>();
	private List<SocialUtils.EPlatform> m_supportedPlatformIds = new List<SocialUtils.EPlatform> ();

	private Action m_onSocialPlatformLogout;

	public void SetOnSocialPlatformLogout(Action value)
	{
		m_onSocialPlatformLogout = value;
	}

    /// <summary>
    /// Returns <c>true</c> if the social platform passed as an argument is supported by current device platform (iOS, Android) 
    /// </summary>
    /// <param name="platform">Social platform: (Facebook, SIWA)</param>    
    public static bool IsSocialPlatformIdSupportedByDevicePlatform(SocialUtils.EPlatform platform)
    {
        bool returnValue = true;
        if (platform == SocialUtils.EPlatform.SIWA)
        {
            returnValue = FlavourManager.Instance.GetCurrentFlavour().GetSetting<bool>(Flavour.SettingKey.SIWA_ALLOWED);
        }

        return returnValue;
    }

	public void Init(bool useAgeProtection)
	{
        if (!IsInited)
        {            
            IsInited = true;
			m_platforms.Clear();
			m_supportedPlatformIds.Clear();

            if (useAgeProtection)
            {
				m_currentPlatformId = SocialUtils.EPlatform.None;
				AddSocialPlatform(SocialUtils.EPlatform.None);
            }
            else
            {
                // Social platform depends on the app's flavour
                SocialUtils.EPlatform platformId = FlavourManager.Instance.GetCurrentFlavour().SocialPlatformASSocialUtilsEPlatform;
				if (platformId == SocialUtils.EPlatform.Facebook && !FacebookManager.SharedInstance.CanUseFBFeatures ()) 
				{
					platformId = SocialUtils.EPlatform.None;
				}

				AddSocialPlatform(platformId);


                //
                // SIWA
                // 
                if (IsSocialPlatformIdSupportedByDevicePlatform(SocialUtils.EPlatform.SIWA) && GameSessionManager.SharedInstance.SIWA_IsPlatformSupported())
                {
                    AddSocialPlatform(SocialUtils.EPlatform.SIWA);
                }

                SocialUtils dnaSocialUtils = null;
                if (FeatureSettingsManager.instance.IsImplicitCloudSaveEnabled())
                {
                    //
                    // DNA
                    //                 
                    dnaSocialUtils = AddSocialPlatform(SocialUtils.EPlatform.DNA);
                }

                // Retrieves the current social platform: Checks that the user was logged in when she quit and if so then the
                // latest social platform key is retrieved
                string socialPlatformKey = PersistenceFacade.instance.LocalDriver.Prefs_SocialPlatformKey;                
                if (PersistenceFacade.instance.LocalDriver.Prefs_SocialWasLoggedInWhenQuit && IsPlatformKeySupported(socialPlatformKey)) 
				{
					m_currentPlatformId = SocialUtils.KeyToEPlatform(socialPlatformKey);
				} 
				else 
				{
                    // DNA is the default social platform, as long as it is enabled, since it provides the user with cloud save automatically
                    m_currentPlatformId = (dnaSocialUtils != null && dnaSocialUtils.GetIsEnabled()) ? SocialUtils.EPlatform.DNA : SocialUtils.EPlatform.None;
				}
            }          
        }        
    }   

	private SocialUtils AddSocialPlatform(SocialUtils.EPlatform platformId)
	{
		SocialUtils socialPlatform;
		switch (platformId) 
		{
			case SocialUtils.EPlatform.Facebook:
				socialPlatform = new SocialUtilsFb();
				break;

			case SocialUtils.EPlatform.Weibo:
				socialPlatform = new SocialUtilsWeibo();
				break;

            case SocialUtils.EPlatform.SIWA:
                socialPlatform = new SocialUtilsSIWA();
                break;

            case SocialUtils.EPlatform.DNA:
                socialPlatform = new SocialUtilsDNA();
                break;

            default:
				socialPlatform = new SocialUtilsDummy (false, false);
				break;
		}

		socialPlatform.Init(this);  
		if (m_platforms.ContainsKey(platformId))
		{
			m_platforms[platformId] = socialPlatform;
		}
		else if (socialPlatform.GetIsEnabled())
		{
			m_platforms.Add(platformId, socialPlatform);
			m_supportedPlatformIds.Add(platformId);
		}

        return socialPlatform;
	}
    
    public void Reset()
    {
        IsInited = false;
    }    
		
	public List<SocialUtils.EPlatform> GetSupportedPlatformIds()
	{
		return m_supportedPlatformIds;
	}

	public bool IsPlatformIdSupported(SocialUtils.EPlatform platformId)
	{
		SocialUtils platform = GetPlatform(platformId);
		return platform != null && platform.GetIsEnabled();
	}

	public bool IsPlatformKeySupported(string platformKey)
	{
		return IsPlatformIdSupported(SocialUtils.KeyToEPlatform(platformKey));
	}

	public SocialUtils GetPlatform(SocialUtils.EPlatform platformId)
	{
		return (m_platforms.ContainsKey(platformId)) ? m_platforms[platformId] : null;
	}

    public bool GetIsEnabled()
    {
		// If there's at least one platform enabled then the manager is enabled
		foreach (KeyValuePair<SocialUtils.EPlatform, SocialUtils> pair in m_platforms) 
		{
			if (pair.Value.GetIsEnabled())
				return true;
		}

		return false;
    }

	public string GetPlatformName(SocialUtils.EPlatform platformId)
	{
		string returnValue = null;
		SocialUtils platform = GetPlatform(platformId);
		if (platform != null) 
		{
			string tid = platform.GetPlatformNameTID();
			returnValue = LocalizationManager.SharedInstance.Localize(tid);  
		}

		return returnValue;
	}	

	public string GetUserID(SocialUtils.EPlatform platformId)
	{
		SocialUtils platform = GetPlatform(platformId);
		return (platform != null) ? platform.GetSocialID() : null;
	}	

	public string GetUserName(SocialUtils.EPlatform platformId)
	{
		SocialUtils platform = GetPlatform(platformId);
		return (platform != null) ? platform.GetUserName() : null;
	}

	/// <summary>
	/// Returns user's profile information.
	/// </summary>
	/// <param name=""></param>
	public void GetProfileInfo(SocialUtils.EPlatform platformId, Action<SocialUtils.ProfileInfo> onDone)
	{
		SocialUtils platform = GetPlatform(platformId);
		if (platform == null) 
		{
			if (onDone != null) 
			{
				onDone (null);
			}
		}
		else
		{
			platform.GetProfileInfoFromPlatform (onDone);
		}
	}

	/// <summary>
	/// Returns the user's first name and her picture.
	/// </summary>
	/// <param name="onDone"></param>
	public void GetSimpleProfileInfo(SocialUtils.EPlatform platformId, Action<string, Texture2D> onDone)
	{
		SocialUtils platform = GetPlatform(platformId);
		if (platform == null) 
		{
			if (onDone != null) 
			{
				onDone(null, null);
			}
		} 
		else 
		{
			platform.Profile_GetSimpleInfo(onDone);
		}
	}
		
	public bool NeedsProfileInfoToBeUpdated(SocialUtils.EPlatform platformId)
	{
		SocialUtils platform = GetPlatform(platformId);
		return (platform == null) ? false : platform.Profile_NeedsInfoToBeUpdated();
	}

	public bool NeedsSocialIdToBeUpdated(SocialUtils.EPlatform platformId)
    {
		SocialUtils platform = GetPlatform (platformId);
		return (platform == null) ? false : platform.Profile_NeedsSocialIdToBeUpdated();
    }

	public void InvalidateCachedSocialInfo(SocialUtils.EPlatform platformId)
    {
		SocialUtils platform = GetPlatform(platformId);
		if (platform != null && platform.Cache != null)
        {
            platform.Cache.Invalidate();
        }
    }

    public bool IsImplicit(SocialUtils.EPlatform platformId)
    {
        SocialUtils platform = GetPlatform(platformId);
        return platform != null && platform.IsImplicit;        
    }

    /// <summary>
    /// Platform that this class is logging in to
    /// </summary>
    public SocialUtils.EPlatform LoggingInTo_PlatformId { get; set; }

	#region current_platform
	private SocialUtils.EPlatform m_currentPlatformId;

	public SocialUtils.EPlatform CurrentPlatform_GetId()
	{
		return m_currentPlatformId;
	}

	public string CurrentPlatform_GetKey()
	{
		return SocialUtils.EPlatformToKey(CurrentPlatform_GetId());
	}

	public string CurrentPlatform_GetName()
	{
		return GetPlatformName(m_currentPlatformId);
	}
   
	public string CurrentPlatform_GetUserID()
	{
		return GetUserID(m_currentPlatformId);
	}	

	public string CurrentPlatform_GetUserName()
	{
		return GetUserName(m_currentPlatformId);
	}

	/// <summary>
	/// Returns user's profile information.
	/// </summary>
	/// <param name=""></param>
	public void CurrentPlatform_GetProfileInfo(Action<SocialUtils.ProfileInfo> onDone)
	{
		GetProfileInfo(m_currentPlatformId, onDone);
	}

	/// <summary>
	/// Returns the user's first name and her picture.
	/// </summary>
	/// <param name="onDone"></param>
	public void CurrentPlatform_GetSimpleProfileInfo(Action<string, Texture2D> onDone)
	{
		GetSimpleProfileInfo(m_currentPlatformId, onDone);      
	}

	public bool CurrentPlatform_NeedsProfileInfoToBeUpdated()
	{
		return NeedsProfileInfoToBeUpdated(m_currentPlatformId);
	} 

	public bool CurrentPlatform_NeedsSocialIdToBeUpdated()
	{
		return NeedsSocialIdToBeUpdated(m_currentPlatformId);
	}

	public void CurrentPlatform_InvalidateCachedSocialInfo()
	{
		InvalidateCachedSocialInfo(m_currentPlatformId);
	}

    public bool CurrentPlatform_IsImplicit()
    {
        return IsImplicit(m_currentPlatformId);
    }

	public bool CurrentPlatform_IsLoggedIn()
	{
		return IsLoggedIn(m_currentPlatformId);
	}

	public bool CurrentPlatform_IsLogInTimeoutEnabled()
	{
		return IsLogInTimeoutEnabled(m_currentPlatformId);
	}

	public void CurrentPlatform_OnLogInTimeout()
	{
		OnLogInTimeout(m_currentPlatformId);
	}
	#endregion

    //////////////////////////////////////////////////////////////////////////

    #region login    
    public enum ELoginResult
    {
        Ok,
        Error,
        MergeLocalOrOnlineAccount,
        MergeDifferentAccountWithProgress,
        MergeDifferentAccountWithoutProgress
    }

    private enum ELoginMergeState
    {
        Waiting,
        Succeeded,
        Failed,
        MergeLocalOrOnlineAccount,
        MergeDifferentAccountWithProgress,
        MergeDifferentAccountWithoutProgress        
    }

    private ELoginMergeState m_loginMergeState;
    private ELoginMergeState Login_MergeState
    {
        get
        {
            return m_loginMergeState;
        }

        set
        {
            m_loginMergeState = value;
        }
    }

    private bool Login_IsLogInReady { get; set; }

    private Action<ELoginResult, string> Login_OnDone { get; set; }

    private bool Login_IsLogging { get; set; }            

    private string Login_MergePersistence { get; set;  }
    
	public bool IsLoggedIn(SocialUtils.EPlatform platformId)
    {
		SocialUtils platform = GetPlatform(platformId);
		return platform != null && platform.IsLoggedIn();
    }

    public bool IsLoggedInByKey(string platformKey)
    {
        SocialUtils.EPlatform platformId = SocialUtils.KeyToEPlatform(platformKey);
        SocialUtils platform = GetPlatform(platformId);
        return platform != null && platform.IsLoggedIn();
    }

    public bool IsLogInTimeoutEnabled(SocialUtils.EPlatform platformId)
	{
		SocialUtils platform = GetPlatform(platformId);
		return platform != null && platform.IsLogInTimeoutEnabled();
	}

	public void OnLogInTimeout(SocialUtils.EPlatform platformId)
	{
		SocialUtils platform = GetPlatform(platformId);
		if (platform != null)
			platform.OnLogInTimeout();
	}

    public void Logout()
    {		
        SocialUtils.EPlatform platformId = CurrentPlatform_GetId();
        Log("Logout from " + platformId + " isLoggedIn = " + IsLoggedIn(platformId));
        if (IsLoggedIn(platformId))
        {
            SocialUtils platform = GetPlatform(platformId);
            if (platform != null)
                platform.Logout();            
        }
    }

	private void OnLogout()
	{
		if (m_onSocialPlatformLogout != null)
			m_onSocialPlatformLogout();
	}

	public void Login(SocialUtils.EPlatform platformId, bool isSilent, bool isAppInit, Action<ELoginResult, string> onDone, bool forceMerge = false)
    {
		if (!IsPlatformIdSupported(platformId))
		{
			LogError("Social login requested to an unsupported platform: " + platformId);

			if (onDone != null)
			{
				onDone(ELoginResult.Error, null);
			}

			return;
		}

        LoggingInTo_PlatformId = platformId;

        // An implicit platform has less priority than an explicit platform
        bool setCurrentPlaform = !(!IsImplicit(m_currentPlatformId) && IsImplicit(platformId));
        if (setCurrentPlaform)
        {
            m_currentPlatformId = platformId;
        }

        // Forced to false because when Calety is called with true some flow can be performed that doesn't trigger any callback which would lead
        // this login flow to stay waiting forever
        isAppInit = false;

        string socialId = PersistenceFacade.instance.LocalDriver.Prefs_SocialId;        
        Log("LOGGING IN... currentPlatform = " + platformId + " isSilent = " + isSilent + " isAppInit = " + isAppInit +
            " alreadyLoggedIn = " + IsLoggedIn(platformId) + " SocialId = " + socialId + " forceMerge = " + forceMerge);

        Login_Discard();

        Login_IsLogging = true;
        Login_OnDone = onDone;
        Login_AddMergeListeners();

        Login_IsLogInReady = false;
        Login_MergeState = ELoginMergeState.Waiting;

        if (isSilent)
        {
            bool neverLoggedIn = string.IsNullOrEmpty(socialId);
            if (platformId == SocialUtils.EPlatform.DNA)
            {
                neverLoggedIn = false;
            }

#if UNITY_EDITOR
            if (platformId == SocialUtils.EPlatform.Facebook)
            {
                // We want to prevent developers from seeing social login popup every time the game is started since editor doesn't cache the social token
                neverLoggedIn = true;
            }
#endif

            // If the user has never logged in then we should just mark it as not loggedIn
            if (neverLoggedIn)
            {
                Login_OnLoggedIn(false);
            }
        }
        else
        {
            // We need to make sure that a previous incomplete merge is reseted. When a user decides to keep her local account when prompted to merge with 
            // a different account that has also used the same social account then the user is logged out automatically and we don't want to bother the user
            // with the same merge popup every time she loads the game. The remove account id that was declined is stored in order to avoid that popup from being shown 
            // again. We need to reset that variable because the user is expressing explicitly her intention to log in again
            GameSessionManager.SharedInstance.ResetSocialPlatformCancelState();
        }

        if (!Login_IsLogInReady)
        {
            Messenger.AddListener<bool>(MessengerEvents.SOCIAL_LOGGED, Login_OnLoggedInHelper);
			GetPlatform(platformId).Login(isAppInit, forceMerge);
        }

        /*
        if (IsLoggedIn())
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("LOGGING IN ALREADY LOGGED IN ProcessMergeAccounts manual call Logged in server " + GameSessionManager.SharedInstance.IsLogged());

            string strUserID = GetUserID();
            string strUserName = GetUserName();
            GameSessionManager.SharedInstance.ProcessMergeAccounts(strUserID, strUserName);
        }
        else
        {
            if (isSilent)
            {
                Login_OnLoggedIn(false);
            }
            else
            {
                // We need to make sure that a previous incomplete merge is reseted. When a user decides to keep her local account when prompted to merge with 
                // a different account that has also used the same social account then the user is logges out automatically and we don't want to bother the user
                // with the same merge popup every time she loads the game. The remove account id that was declined is stored in order to avoid that popup from being shown 
                // again. We need to reset that variable because the user is expressing explicitly her intention to log in again
                GameSessionManager.SharedInstance.ResetSocialPlatformCancelState();

                Login_IsLogInReady = false;
                Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, Login_OnLoggedInHelper);
                GameSessionManager.SharedInstance.LogInToSocialPlatform(isAppInit);
            }
        }  
        */      
    }    

    private void Login_OnLoggedInHelper(bool logged)
    {        
        Log("(LOGGING) onLogged " + logged);

        Messenger.RemoveListener<bool>(MessengerEvents.SOCIAL_LOGGED, Login_OnLoggedInHelper);
        Login_OnLoggedIn(logged);
    }

    protected void Login_OnLoggedIn(bool logged)
    {       
        Login_IsLogInReady = true;
        if (!logged)
        {
            Login_MergeState = ELoginMergeState.Failed;
        }
    }    

    private void Login_AddMergeListeners()
    {
        Messenger.AddListener(MessengerEvents.MERGE_SUCCEEDED, Login_OnMergeSucceeded);
        Messenger.AddListener(MessengerEvents.MERGE_FAILED, Login_OnMergeFailed);
        Messenger.AddListener<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(MessengerEvents.MERGE_SHOW_POPUP_NEEDED, Login_OnMergeShowPopupNeeded);
    }

    private void Login_RemoveMergeListeners()
    {
        Messenger.RemoveListener(MessengerEvents.MERGE_SUCCEEDED, Login_OnMergeSucceeded);
        Messenger.RemoveListener(MessengerEvents.MERGE_FAILED, Login_OnMergeFailed);
        Messenger.RemoveListener<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(MessengerEvents.MERGE_SHOW_POPUP_NEEDED, Login_OnMergeShowPopupNeeded);
    }

    private void Login_OnMergeSucceeded()
    {        
        Log("(LOGGING) MERGE SUCCEEDED!");
        Login_MergeState = ELoginMergeState.Succeeded;
    }

    private void Login_OnMergeFailed()
    {        
        Log("(LOGGING) MERGE FAILED!");
        Login_MergeState = ELoginMergeState.Failed;
    }

    private void Login_OnMergeShowPopupNeeded(CaletyConstants.PopupMergeType eType, JSONNode kLocalAccount, JSONNode kCloudAccount)
    {        
        Log("(LOGGING) MERGE POPUP NEEDED! eType = " + eType + " kCloudAccount = " + kCloudAccount);

        switch (eType)
        {
            case CaletyConstants.PopupMergeType.POPUP_MERGE_LOCAL_OR_ONLINE_ACCOUNTS:
                Login_MergeState = ELoginMergeState.MergeLocalOrOnlineAccount;
                break;

            case CaletyConstants.PopupMergeType.POPUP_MERGE_DIFFERENT_GAMECENTER_ACOUNT_WITH_PROGRESS:
                Login_MergeState = ELoginMergeState.MergeDifferentAccountWithProgress;
                break;

            case CaletyConstants.PopupMergeType.POPUP_MERGE_DIFFERENT_GAMECENTER_ACOUNT_WITHOUT_PROGRESS:
                Login_MergeState = ELoginMergeState.MergeDifferentAccountWithoutProgress;
                break;
        }        

        JSONNode persistenceAsJson = null;
        
        const string key = "profile";        
        if (kCloudAccount != null && kCloudAccount.ContainsKey(key))
        {            
            persistenceAsJson = kCloudAccount[key];                        
        }

        string persistenceAsString = null;
        if (persistenceAsJson != null)
        {
            persistenceAsString = persistenceAsJson.ToString();
        }

        // If it's an empty persistence then the default one is used instead
        // Sometimes server sends "{\"sc\":0,\"pc\":0}" as a persistence
        bool persistenceBrokenFromServer = persistenceAsString == "{\"sc\":0,\"pc\":0}";
        if (persistenceAsJson == null || persistenceAsString == "{}" || persistenceBrokenFromServer)
        {            
            persistenceAsJson = PersistenceUtils.GetDefaultDataFromProfile();
        }

        if (persistenceBrokenFromServer)
            LogError("Persistence Broken from server");

        Login_MergePersistence = persistenceAsJson.ToString();
    }

    private void Login_Update()
    {
        bool isLoggedIn = IsLoggedIn(LoggingInTo_PlatformId);
        if (Login_IsLogInReady && (Login_MergeState != ELoginMergeState.Waiting || !isLoggedIn))
        {
            if (isLoggedIn)
            {
                ELoginResult result = ELoginResult.Error;

                switch (Login_MergeState)
                {
                    case ELoginMergeState.Succeeded:
                        result = ELoginResult.Ok;
                        break;

                    case ELoginMergeState.MergeLocalOrOnlineAccount:
                        result = ELoginResult.MergeLocalOrOnlineAccount;
                        break;

                    case ELoginMergeState.MergeDifferentAccountWithProgress:
                        result = ELoginResult.MergeDifferentAccountWithProgress;
                        break;

                    case ELoginMergeState.MergeDifferentAccountWithoutProgress:
                        result = ELoginResult.MergeDifferentAccountWithoutProgress;
                        break;
                }

                SocialUtils platform = GetPlatform(LoggingInTo_PlatformId);
                  
                if (platform != null && platform.IsImplicit && false)
                {
                    switch (Login_MergeState)
                    {
                        case ELoginMergeState.MergeDifferentAccountWithoutProgress:
                            // This situation is not allowed for implicit log in because it'd mean that the social userId has changed and
                            // that's not possible because social userId in an implicit social platform is linked to the device
                            LogWarning("(LOGGING):: MergeDifferentAccountWithoutProgress not allowed when logging in implicitly");
                            result = ELoginResult.Error;
                            break;

                        case ELoginMergeState.MergeLocalOrOnlineAccount:
                        case ELoginMergeState.MergeDifferentAccountWithProgress:
                            // If the user has ever logged in explicitly then it means that the user might not be using her original server
                            // user id, which is the only oe that is linked to her device id. Merging via an implicit platform is not allowed
                            // after an user has logged in explicitly. If the user was using her original user server id then no merge conflict
                            // had arisen an we don't want to give the user the chance to mess her progress up
                            if (!string.IsNullOrEmpty(PersistenceFacade.instance.LocalDriver.Prefs_SocialPlatformKey))
                            {
                                Log("(LOGGING):: Merge conflict when loging in to an automatic social platform is ignored because the user has ever logged in explicitly");
                                result = ELoginResult.Error;
                            }                           
                            break;
                    }
                }

                Login_PerformDone(result);                
            }
            else
            {
                Login_PerformDone(ELoginResult.Error);
            }
        }
    }

    private void Login_PerformDone(ELoginResult result)
    {        
        Log("(LOGGING) DONE! " + result + " isLoggedInReady = " + Login_IsLogInReady + " Login_MergeState = " + Login_MergeState + " isLoggedIn = " + IsLoggedIn(LoggingInTo_PlatformId) +
            " currentPlatform = " + LoggingInTo_PlatformId);

        LoggingInTo_PlatformId = SocialUtils.EPlatform.None;

        if (Login_OnDone != null)
        {
            Login_OnDone(result, Login_MergePersistence);
        }

        Login_Discard();        
    }

    public void Login_Discard()
    {
        if (Login_IsLogging)
        {
            Login_RemoveMergeListeners();
            Login_OnDone = null;
            Login_IsLogging = false;
            Login_MergePersistence = null;
        }
    }
    #endregion
   
    public void Update()
    {
        if (Login_IsLogging)
        {
            Login_Update();
        }

        SocialUtils.EPlatform currentPlatformId = CurrentPlatform_GetId();
		SocialUtils platform = GetPlatform(currentPlatformId);
		if (platform != null) 
		{
			platform.Update();
		}

        SocialUtils.EPlatform loggingInToPlatformId = LoggingInTo_PlatformId;
        if (loggingInToPlatformId != currentPlatformId)
        {
            platform = GetPlatform(loggingInToPlatformId);
            if (platform != null)
            {
                platform.Update();
            }
        }
    }

    private const string LOG_CHANNEL = "[Social] ";

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }

    #if ENABLE_LOGS
    [Conditional("DEBUG")]
    #else
    [Conditional("FALSE")]
    #endif
    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_CHANNEL + msg);
    }
}
