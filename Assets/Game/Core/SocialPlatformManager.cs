using UnityEngine;
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

	// FB Listener //////////////////////////////////////////////////////
	private class GameFacebookListener : FacebookManager.FacebookListenerBase
	{
		const string TAG = "GameFacebookListener";
		private SocialPlatformManager m_manager;

		public GameFacebookListener( SocialPlatformManager manager )
		{
			m_manager = manager;
		}

		public override void onLogInCompleted()
		{
			Debug.TaggedLog(TAG, "onLogInCompleted");
			m_manager.OnSocialPlatformLogin();
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

	// Delegate /////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////

	// Members //////////////////////////////////////////////////////

	GameFacebookListener m_facebookDelegate;

	public enum UsingPlatform
	{
		FACEBOOK,
		WEIBO
	};
	UsingPlatform m_platform = UsingPlatform.FACEBOOK;

    private static string[] PLATFORM_TIDS = new string[]
    {
        "TID_SOCIAL_FACEBOOK",
        "TID_SOCIAL_NETWORK_WEIBO_NAME"        
    };    

    private bool IsInited { get; set; }

    public void Init()
	{
        if (!IsInited)
        {
            IsInited = true;

            switch (m_platform)
            {
                case UsingPlatform.FACEBOOK:
                    {
                        m_facebookDelegate = new GameFacebookListener(this);
                        FacebookManager.SharedInstance.SetFacebookListener(m_facebookDelegate);
                        FacebookManager.SharedInstance.Initialise();
                    }
                    break;
            }
        }
	}

	public void Login(bool isAppInit)
	{                    
        switch (m_platform)
		{
			case UsingPlatform.FACEBOOK:
			{
                GameSessionManager.SharedInstance.LogInToFacebook(isAppInit);
				//FacebookManager.SharedInstance.LogIn();
			}break;
		}
	}

	public bool IsLoggedIn()
	{
		bool ret = false;
		switch(m_platform)
		{
			case UsingPlatform.FACEBOOK:
			{
				ret = FacebookManager.SharedInstance.IsLoggedIn();
			}break;
		}
		return ret;
	}

	public string GetSocialIconPath()
	{
		string ret = "";
		switch( m_platform )
		{
			case UsingPlatform.FACEBOOK:
			{
				
			}break;
			case UsingPlatform.WEIBO:
			{
				
			}break;
		}
		return ret;
	}

	public string GetPlatformName()
	{
        return LocalizationManager.SharedInstance.Localize(PLATFORM_TIDS[(int)m_platform]);     
	}

	public string GetToken()
	{
		string ret = "";
		switch( m_platform )
		{
			case UsingPlatform.FACEBOOK:
			{
				Facebook.Unity.AccessToken aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				ret = aToken.TokenString;
			}break;
			case UsingPlatform.WEIBO:
			{
				
			}break;
		}
		return ret;
	}

	public string GetUserId()
	{
		string ret = "";
		switch( m_platform )
		{
			case UsingPlatform.FACEBOOK:
			{
				Facebook.Unity.AccessToken aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				ret = aToken.UserId;
			}break;
			case UsingPlatform.WEIBO:
			{
				
			}break;
		}
		return ret;
	}

	public string GetUserName()
	{
		string ret = "";
		switch( m_platform )
		{
			case UsingPlatform.FACEBOOK:
			{
				ret = FacebookManager.SharedInstance.UserName;
			}break;
			case UsingPlatform.WEIBO:
			{
				
			}break;
		}
		return ret;
	}

	//////////////////////////////////////////////////////////////////////////
}
