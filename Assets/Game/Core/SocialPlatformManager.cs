using UnityEngine;
using System.Collections;

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
		}

		public override void onLogInFailed()
		{
			Debug.TaggedLog(TAG, "onLogInFailed");
		}
		
		public override void onLogOut()
		{
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

	// Delegates //////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////

	// Members //////////////////////////////////////////////////////

	GameFacebookListener m_delegate;

	public enum UsingPlatform
	{
		FACEBOOK,
		WEIBO
	};
	UsingPlatform m_platform;

	bool m_useFacebook = true;

	public void Init()
	{
		switch(m_platform)
		{
			case UsingPlatform.FACEBOOK:
			{
				m_delegate = new GameFacebookListener( this );
				FacebookManager.SharedInstance.SetFacebookListener( m_delegate );
				FacebookManager.SharedInstance.Initialise();
			}break;
		}
	}

	public void Login()
	{
		switch(m_platform)
		{
			case UsingPlatform.FACEBOOK:
			{
				FacebookManager.SharedInstance.LogIn();
			}break;
		}
	}

	public bool IsLoggedIn()
	{
		switch(m_platform)
		{
			case UsingPlatform.FACEBOOK:
			{
				return FacebookManager.SharedInstance.IsLoggedIn();
			}break;
		}
		return false;
	}

	public string GetSocialIconPath()
	{
		switch( m_platform )
		{
			case UsingPlatform.FACEBOOK:
			{
				
			}break;
			case UsingPlatform.WEIBO:
			{
				
			}break;
		}
		return "";
	}

	//////////////////////////////////////////////////////////////////////////
}
