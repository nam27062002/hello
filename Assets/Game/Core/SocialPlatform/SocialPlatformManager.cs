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
    
    public static SocialUtils.EPlatform GetSocialPlatform()
    {
        SocialUtils.EPlatform returnValue = SocialUtils.EPlatform.Facebook;
        
        // In iOS we need to check the user's country to decide the social platform: either Facebook or Weibo (only in China)
#if UNITY_IOS
            // Checks if the user has already logged in a social platform, if so then that's the platform that the user will keep seeing
            string socialPlatformKey = PersistencePrefs.Social_PlatformKey;
            returnValue = SocialUtils.KeyToEPlatform(socialPlatformKey);

            // If no social platform has ever been used then we decide which one to show based on the country
            if (returnValue == SocialUtils.EPlatform.None)
            {
                string countryCode = PlatformUtils.Instance.GetCountryCode();
                if (countryCode != null)
                {
                    countryCode.ToUpper();
                }                    

                // Weibo is shown only in China
                if (countryCode == "CN")
                {
                    returnValue = SocialUtils.EPlatform.Weibo;
                }
                else
                {
                    returnValue = SocialUtils.EPlatform.Facebook;
                }
            }
#endif

        return returnValue;
    }

	//////////////////////////////////////////////////////////////////////////	

	// Social Platform Response //////////////////////////////////////////////

	public void OnSocialPlatformLogin()
	{
		Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, IsLoggedIn());        
    }

    public void OnSocialPlatformLoginFailed()
	{
		Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, IsLoggedIn());        
    }

    public void OnSocialPlatformLogOut()
	{
		Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, IsLoggedIn());
	}
    //////////////////////////////////////////////////////////////////////////    
	    
    private bool IsInited { get; set; }    

    private SocialUtils m_socialUtils;

    public void Init(bool useAgeProtection)
	{
        if (!IsInited)
        {            
            IsInited = true;
            
            if (useAgeProtection)
            {
                m_socialUtils = new SocialUtilsDummy(false, false);
            }
            else
            {
                SocialUtils.EPlatform socialPlatform = SocialUtils.EPlatform.Facebook;

                // In iOS we need to check the user's country to decide the social platform: either Facebook or Weibo (only in China)
#if UNITY_IOS
                // Checks if the user has already logged in a social platform, if so then that's the platform that the user will keep seeing
                string socialPlatformKey = PersistenceFacade.instance.LocalDriver.Prefs_SocialPlatformKey;
                socialPlatform = SocialUtils.KeyToEPlatform(socialPlatformKey);

                // If no social platform has ever been used then we decide which one to show based on the country
                if (socialPlatform == SocialUtils.EPlatform.None)
                {                
                    // Weibo is shown only in China
                    if (PlatformUtils.Instance.IsChina())
                    {
                        socialPlatform = SocialUtils.EPlatform.Weibo;
                    }
                    else
                    {
                        socialPlatform = SocialUtils.EPlatform.Facebook;
                    }
                }
#endif                
                switch (socialPlatform)
                {
                    case SocialUtils.EPlatform.Facebook:
                        if (FacebookManager.SharedInstance.CanUseFBFeatures())
                        {
                            m_socialUtils = new SocialUtilsFb();
                        }
                        else
                        {
                            m_socialUtils = new SocialUtilsDummy(false, false);
                        }
                        break;

                    case SocialUtils.EPlatform.Weibo:
                        m_socialUtils = new SocialUtilsWeibo();
                        break;
                }                
            }

            m_socialUtils.Init(this);            
        }        
    }    
    
    public void Reset()
    {
        IsInited = false;
    }    

    public SocialUtils.EPlatform GetPlatform()
    {
        return (m_socialUtils == null) ? SocialUtils.EPlatform.None : m_socialUtils.GetPlatform();
    }

    public string GetPlatformKey()
    {
        return SocialUtils.EPlatformToKey(GetPlatform());
    }

    public bool GetIsEnabled()
    {
        return (IsInited && m_socialUtils.GetIsEnabled());
    }

    public string GetPlatformName()
	{
        string tid = m_socialUtils.GetPlatformNameTID();
        return LocalizationManager.SharedInstance.Localize(tid);     
	}

	public string GetToken()
	{
        return m_socialUtils.GetAccessToken();
    }

	public string GetUserID()
	{
        return m_socialUtils.GetSocialID();        
	}	

    public string GetUserName()
    {
        return m_socialUtils.GetUserName();
    }

    /// <summary>
    /// Returns user's profile information.
    /// </summary>
    /// <param name=""></param>
    public void GetProfileInfo(Action<SocialUtils.ProfileInfo> onDone)
    {
        m_socialUtils.GetProfileInfoFromPlatform(onDone);
    }

    /// <summary>
    /// Returns the user's first name and her picture.
    /// </summary>
    /// <param name="onDone"></param>
    public void GetSimpleProfileInfo(Action<string, Texture2D> onDone)
    {
        m_socialUtils.Profile_GetSimpleInfo(onDone);        
    }

    public bool NeedsProfileInfoToBeUpdated()
    {
        return m_socialUtils.Profile_NeedsInfoToBeUpdated();
    }    

    public bool NeedsSocialIdToBeUpdated()
    {
        return m_socialUtils.Profile_NeedsSocialIdToBeUpdated();
    }

    public void InvalidateCachedSocialInfo()
    {
        if (m_socialUtils != null && m_socialUtils.Cache != null)
        {
            m_socialUtils.Cache.Invalidate();
        }
    }
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

    private ELoginMergeState Login_MergeState { get; set; }
    private bool Login_IsLogInReady { get; set; }

    private Action<ELoginResult, string> Login_OnDone { get; set; }

    private bool Login_IsLogging { get; set; }            

    private string Login_MergePersistence { get; set;  }
    
    public bool IsLoggedIn()
    {
        if (m_socialUtils != null)
            return m_socialUtils.IsLoggedIn();

        return false;
    }

	public bool IsLogInTimeoutEnabled()
	{
        if (m_socialUtils != null)
		    return m_socialUtils.IsLogInTimeoutEnabled();

        return false;
	}

	public void OnLogInTimeout()
	{
        if (m_socialUtils != null)
		    m_socialUtils.OnLogInTimeout();
	}

    public void Logout()
    {
		GameSessionManager.SharedInstance.LogOutFromSocialPlatform();
    }

    public void Login(bool isSilent, bool isAppInit, Action<ELoginResult, string> onDone)
    {
        // Forced to false because when Calety is called with true some flow can be performed that doesn't trigger any callback which would lead
        // this login flow to stay waiting forever
        isAppInit = false;

        string socialId = PersistenceFacade.instance.LocalDriver.Prefs_SocialId;        
        Log("LOGGING IN... isSilent = " + isSilent + " isAppInit = " + isAppInit + " alreadyLoggedIn = " + IsLoggedIn() + " SocialId = " + socialId);

        Login_Discard();

        Login_IsLogging = true;
        Login_OnDone = onDone;
        Login_AddMergeListeners();

        Login_IsLogInReady = false;
        Login_MergeState = ELoginMergeState.Waiting;

        if (isSilent)
        {
            bool neverLoggedIn = string.IsNullOrEmpty(socialId);

#if UNITY_EDITOR
            // We want to prevent developers from seeing social login popup every time the game is started since editor doesn't cache the social token
            neverLoggedIn = true;
#endif

            // If the user has never logged in then we should just marked as not loggedIn
            if (neverLoggedIn)
            {
                Login_OnLoggedIn(false);
            }
        }
        else
        {
            // We need to make sure that a previous incomplete merge is reseted. When a user decides to keep her local account when prompted to merge with 
            // a different account that has also used the same social account then the user is logges out automatically and we don't want to bother the user
            // with the same merge popup every time she loads the game. The remove account id that was declined is stored in order to avoid that popup from being shown 
            // again. We need to reset that variable because the user is expressing explicitly her intention to log in again
            GameSessionManager.SharedInstance.ResetSocialPlatformCancelState();
        }

        if (!Login_IsLogInReady)
        {
            Messenger.AddListener<bool>(MessengerEvents.SOCIAL_LOGGED, Login_OnLoggedInHelper);
            m_socialUtils.Login(isAppInit);
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
        bool isLoggedIn = IsLoggedIn();
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
        Log("(LOGGING) DONE! " + result);

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

        if (m_socialUtils != null)
        {
            m_socialUtils.Update();
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
}
