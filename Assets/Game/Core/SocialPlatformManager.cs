using UnityEngine;
using SimpleJSON;
using System;
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

    public void GetProfileInfo(Action<string> onGetName, Action<Texture2D> onGetImage)
    {
        m_socialUtils.GetProfileInfo(onGetName, onGetImage);                  
    }
    //////////////////////////////////////////////////////////////////////////

    #region login    
    public enum ELoginResult
    {
        Ok,
        Error,
        NeedsToMerge
    }

    private enum ELoginMergeState
    {
        Waiting,
        Succeeded,
        Failed,
        ShowPopupNeeeded
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
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("LOGGING IN... isSilent = " + isSilent + " isAppInit = " + isAppInit );

        Login_Discard();

        Login_IsLogging = true;
        Login_OnDone = onDone;
        Login_AddMergeListeners();

        Login_IsLogInReady = true;
        Login_MergeState = ELoginMergeState.Waiting;

        if (IsLoggedIn())
        {
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

        Login_MergeState = ELoginMergeState.ShowPopupNeeeded;

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
                if (Login_MergeState == ELoginMergeState.Failed)
                {
                    // If merge fails then no persistence can be retrieved
                    Login_PerformDone(ELoginResult.Error);
                }
                else if (Login_MergeState == ELoginMergeState.ShowPopupNeeeded)
                {
                    Login_PerformDone(ELoginResult.NeedsToMerge);
                }
                else
                {
                    Login_PerformDone(ELoginResult.Ok);
                }
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
