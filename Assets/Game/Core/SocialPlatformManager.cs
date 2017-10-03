﻿using UnityEngine;
using SimpleJSON;
using System;
using System.Collections.Generic;
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

	// Social Listener //////////////////////////////////////////////////////
	public class GameSocialListener : FacebookManager.FacebookListenerBase
	{
		const string TAG = "GameSocialListener";
		private SocialPlatformManager m_manager;

		public GameSocialListener( SocialPlatformManager manager )
		{
			m_manager = manager;
		}

		public override void onLogInCompleted()
		{
			Debug.TaggedLog(TAG, "onLogInCompleted");
			m_manager.OnSocialPlatformLogin();
		}

        public override void onLogInCancelled()
        {
            Debug.TaggedLog(TAG, "onLogInCancelled");
            m_manager.OnSocialPlatformLoginFailed();
        }

        public override void onLogInFailed()
		{
			Debug.TaggedLog(TAG, "onLogInFailed");
			m_manager.OnSocialPlatformLoginFailed();
		}
		
		public override void onLogOut()
		{
			m_manager.OnSocialPlatformLogOut();
			Debug.TaggedLog(TAG, "onLogOut");
		}
		
		public override void onPublishCompleted()
		{
			Debug.TaggedLog(TAG, "onPublishCompleted");
		}

		public override void onPublishFailed()
		{
			Debug.TaggedLog(TAG, "onPublishFailed");
		}
		
		public override void onFriendsReceived()
		{
			Debug.TaggedLog(TAG, "onFriendsReceived");
		}
		public override void onLikesReceived(bool bIsLiked)
		{
			Debug.TaggedLog(TAG, "onLikesReceived");
		}

		public override void onPostsReceived()
		{
			Debug.TaggedLog(TAG, "onPostsReceived");
		}
	}
	//////////////////////////////////////////////////////////////////////////

	// Social Platform Response //////////////////////////////////////////////

	void OnSocialPlatformLogin()
	{
		Messenger.Broadcast<bool>(GameEvents.SOCIAL_LOGGED, IsLoggedIn());        
    }

	void OnSocialPlatformLoginFailed()
	{
		Messenger.Broadcast<bool>(GameEvents.SOCIAL_LOGGED, IsLoggedIn());        
    }

	void OnSocialPlatformLogOut()
	{
		Messenger.Broadcast<bool>(GameEvents.SOCIAL_LOGGED, IsLoggedIn());
	}
    //////////////////////////////////////////////////////////////////////////

    private GameSocialListener m_socialListener;
	
    private bool IsInited { get; set; }    

    private SocialUtils m_socialUtils;

    public void Init()
	{
        if (!IsInited)
        {
            m_socialListener = new GameSocialListener(this);

            IsInited = true;

            // TODO
            // m_platform = Get Platform from calety settings            
            m_socialUtils = new SocialUtilsFb();
            m_socialUtils.Init(m_socialListener);            
        }        
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
        return m_socialUtils.IsLoggedIn();
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

        if (FeatureSettingsManager.IsDebugEnabled)
            Log("LOGGING IN... isSilent = " + isSilent + " isAppInit = " + isAppInit + " alreadyLoggedIn = " + IsLoggedIn() + " SocialId = " + PersistencePrefs.Social_Id);

        Login_Discard();

        Login_IsLogging = true;
        Login_OnDone = onDone;
        Login_AddMergeListeners();

        Login_IsLogInReady = false;
        Login_MergeState = ELoginMergeState.Waiting;

        if (isSilent)
        {
            bool neverLoggedIn = string.IsNullOrEmpty(PersistencePrefs.Social_Id);

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
            Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, Login_OnLoggedInHelper);
            GameSessionManager.SharedInstance.LogInToSocialPlatform(isAppInit);
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
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("(LOGGING) onLogged " + logged);

        Messenger.RemoveListener<bool>(GameEvents.SOCIAL_LOGGED, Login_OnLoggedInHelper);
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
        Messenger.AddListener(GameEvents.MERGE_SUCCEEDED, Login_OnMergeSucceeded);
        Messenger.AddListener(GameEvents.MERGE_FAILED, Login_OnMergeFailed);
        Messenger.AddListener<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(GameEvents.MERGE_SHOW_POPUP_NEEDED, Login_OnMergeShowPopupNeeded);
    }

    private void Login_RemoveMergeListeners()
    {
        Messenger.RemoveListener(GameEvents.MERGE_SUCCEEDED, Login_OnMergeSucceeded);
        Messenger.RemoveListener(GameEvents.MERGE_FAILED, Login_OnMergeFailed);
        Messenger.RemoveListener<CaletyConstants.PopupMergeType, JSONNode, JSONNode>(GameEvents.MERGE_SHOW_POPUP_NEEDED, Login_OnMergeShowPopupNeeded);
    }

    private void Login_OnMergeSucceeded()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("(LOGGING) MERGE SUCCEEDED!");

        Login_MergeState = ELoginMergeState.Succeeded;
    }

    private void Login_OnMergeFailed()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("(LOGGING) MERGE FAILED!");

        Login_MergeState = ELoginMergeState.Failed;
    }

    private void Login_OnMergeShowPopupNeeded(CaletyConstants.PopupMergeType eType, JSONNode kLocalAccount, JSONNode kCloudAccount)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
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

        // Makes sure it's a valid persistence
        if (persistenceAsJson == null || persistenceAsJson.ToString() == "{}")
        {
            persistenceAsJson = PersistenceUtils.GetDefaultDataFromProfile();
        }

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
        if (FeatureSettingsManager.IsDebugEnabled)
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
    }

    private const string LOG_CHANNEL = "[Social] ";
    public static void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }
}
