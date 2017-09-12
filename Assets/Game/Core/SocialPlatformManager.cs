using UnityEngine;
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

    /*public enum ELoginResult
    {
        Ok,
        Error,
        MergeNeed
    };
    */

    public void Login()
    {

    }

	public void Login(bool isAppInit)
	{
        GameSessionManager.SharedInstance.LogInToSocialPlatform(isAppInit);        
    }

	public bool IsLoggedIn()
	{
        return m_socialUtils.IsLoggedIn();
	}

    public void Logout()
    {
        GameSessionManager.SharedInstance.LogOutFromSocialPlatform();        
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

	public string GetUserId()
	{
        return m_socialUtils.GetSocialID();        
	}	

    public void GetProfileInfo(Action<string> onGetName, Action<Texture2D> onGetImage)
    {
        m_socialUtils.GetProfileInfo(onGetName, onGetImage);                  
    }    
    //////////////////////////////////////////////////////////////////////////
}
