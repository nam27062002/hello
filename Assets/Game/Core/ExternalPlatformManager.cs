using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;

public class ExternalPlatformManager :  SingletonMonoBehaviour<ExternalPlatformManager> 
{
	// CONSTANTE
	private const bool DEBUG_ENABLED = true;
	private const string DEBUG_TAG = "[EXTERNAL_LOGIN] ";


	IEnumerable <string> FACEBOOK_PERMISSIONS_READ = new List<string>(){ "public_profile", "user_friends", "email" };

	// CLASSES
	public class NewsFeed
	{
		public string toId = "";
		public string link = "";
		public string linkName = "";
		public string linkCaption = "";
		public string linkDescription = "";
		public string picture = "";
		public string mediaSource = "";
		public string actionName = "";
		public string actionLink = "";
	}

	// Delegates
	public delegate void VoidCallback();
	public VoidCallback OnLogin;
	public VoidCallback OnLoginError;

	public delegate void LoadPictureCallback (Texture texture);
	public LoadPictureCallback OnPicureCallback;

	// ATTRIBUTES
	bool m_initialized = false;
	public enum State
	{
		NOT_LOGGED,
		INITIALIZING,
		LOGGING,
		LOGGED_IN,
	};
	State m_loginState = State.NOT_LOGGED;
	public State loginState
	{
		get{ return m_loginState; }
	}

	public enum Platform
	{	
		OFFLINE,
		FACEBOOK,
		WEIBO,
	};
	Platform m_platform = Platform.FACEBOOK;

	bool m_logAfterInit;
	NewsFeed m_pendingFeed = null;

	string m_buttonPath = "";

	void Start()
	{
		OnLogin += OnLoginDone;
		SetPlatform( Platform.FACEBOOK);
		m_logAfterInit = false;
		Init();
	}

	void OnDestroy()
	{
		OnLogin -= OnLoginDone;
	}

	void SetPlatform( Platform platform )
	{
		m_platform = platform;
		switch( platform )
		{
			case Platform.FACEBOOK:
			{
				
			}break;
		}
	}

	public void Init()
	{
		switch( m_platform )
		{
			case Platform.OFFLINE:
			{
			}break;
			case Platform.FACEBOOK:
			{
        		// FB.Init(OnInitCallBack, null, null); 
				if (!FB.IsInitialized) 
				{
			        // Initialize the Facebook SDK
					FB.Init(OnFBInitCallback);
			    } else {
			        // Already initialized, signal an app activation App Event
			        FB.ActivateApp();
			        OnFBInitCallback();
			    }
			}break;
			case Platform.WEIBO:
			{

			}break;
		}
	}

	public void Login()
	{
		switch( m_platform )	
		{
			case Platform.FACEBOOK:
			{
				FBLogin();
			}break;
		}
	}

	void OnLoginDone()
	{
		if (m_pendingFeed != null)
		{
			NewsFeed newsFeed = m_pendingFeed;
			m_pendingFeed = null;
			PostFeed(newsFeed);
		}
	}

	public void Logout()
	{
		switch( m_platform )
		{
			case Platform.FACEBOOK:
			{
				FB.LogOut();
				m_loginState = State.NOT_LOGGED;
			}break;
		}
	}

	public bool IsLogged()
	{
		return false;
	}

	public string GetId()
	{
		return "";
	}


    public void PostFeed( NewsFeed feed )
    {
    	switch( m_platform )
    	{
    		case Platform.FACEBOOK:
    		{
    			FBPostFeed( feed );
    		}break;
    	}
    }

    public void ShowInviteFriends()
    {
    	switch( m_platform )
    	{
    		case Platform.FACEBOOK:
    		{
				// FB.Mobile.AppInvite(new System.Uri("https://fb.me/810530068992919"), new System.Uri("http://i.imgur.com/zkYlB.jpg"), OnFBInviteCallback);
				FB.Mobile.AppInvite(new System.Uri("https://fb.me/" + FacebookSettings.AppId), null, OnFBInviteCallback);
    		}break;
    	}
    }

	public void GetMyImage( int width, int heigth, LoadPictureCallback _callback)
	{
		GetImage( "me", width, heigth, _callback);
	}

    // Image url
	public void GetImage( string userId, int width, int heigth, LoadPictureCallback _callback)
    {
    	switch( m_platform )
    	{
    		case Platform.FACEBOOK:
    		{
    			
    		}break;
    	}
    }

    public string GetButtonImage()
    {
		return m_buttonPath;	
    }

	private void Log(string _msg)
    {
        if (DEBUG_ENABLED) 
        	Debug.Log(DEBUG_TAG + _msg);        
    }


    //////////////////////////
	// FACEBOOK
	/////////////////////////

	void OnFBInitCallback()
	{
		if (DEBUG_ENABLED) Log("OnInitCallBack()");

		m_initialized = true;

		if (FB.IsLoggedIn)
		{	
			m_loginState = State.LOGGING;	// ask for facebook user profile...
			FBRequestProfile();
		}
		else if (m_logAfterInit)
		{
			FBLogin();
		}
	}


	public void FBLogin()
	{
		switch( m_loginState )
		{
			case State.NOT_LOGGED:
			{	
				if ( m_initialized )
				{
					// Start Login
					m_loginState = State.LOGGING;
					FB.LogInWithReadPermissions(FACEBOOK_PERMISSIONS_READ, OnFBLoginCallback);
				}
				else
				{
					// Start initialization
					m_logAfterInit = true;
					m_loginState = State.INITIALIZING;
					Init();
				}
			}break;
			case State.INITIALIZING:
			{
				m_logAfterInit = true;
			}break;
			case State.LOGGING:
			{
				// Wait
			}break;
			case State.LOGGED_IN:
			{
				// Call login delegate
				if (OnLogin != null)
					OnLogin();
			}break;
		}
	}

	/**
	 * 
	 */
	private void OnFBLoginCallback(ILoginResult result)
	{
		if (DEBUG_ENABLED) 
			Log("OnLoginCallBack()");

		if (result.Error != null)
		{
			if (DEBUG_ENABLED)  
				Log ("ERROR: " + result.Error.ToString ());
		}

		if (DEBUG_ENABLED) 
			Log("loginResponse: " + result.AccessToken );

		if (result.Error != null || !FB.IsLoggedIn)
		{
			m_loginState = State.NOT_LOGGED;
			if ( OnLoginError != null )
				OnLoginError();
		}
		else if ( FB.IsLoggedIn )
		{
			AccessToken aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
			if (aToken != null)
			{
				string userId = aToken.UserId;
				string token = aToken.TokenString;
				FBRequestProfile();
			}
		}
	}

	/**
	 * 
	 */
	public void FBRequestProfile()
	{

		if (DEBUG_ENABLED) 
			Log("RequestProfile()");

		// Reqest player info and profile picture                                                                           
		FB.API("/me?fields=id,name,first_name,last_name,gender,locale,age_range,friends.limit(100).fields(first_name,id)", HttpMethod.GET, OnFBProfileCallback);
	}


	/**
	 * 
	 */
	private void OnFBProfileCallback(IGraphResult result)                                                                                              
	{                                                                                                                              
		if (DEBUG_ENABLED) Log("OnRequestProfileCallBack()");

		if (result.Error != null)
		{
			// We make sure that loginState is NOT_LOGGED
			m_loginState = State.NOT_LOGGED;

			if (DEBUG_ENABLED) 
				Log("ERROR: " + result.Error );

			// TODO LOGIN ERROR
			if ( OnLoginError != null )
				OnLoginError();
			return;
		}

		if (DEBUG_ENABLED) 
			Log("Profile returned: " + result.RawResult);

		// userProfile = JSONClass.Parse(result.RawResult);
		// _userName = userProfile["name"];

		/*
		JSONArray friendsProfiles = new JSONArray();
		try
		{
			friendsProfiles = userProfile["friends"]["data"] as JSONArray;
		}
		catch
		{
			
		}
		*/
		
		if ( m_loginState == State.LOGGING )
		{
			m_loginState = State.LOGGED_IN;
			if ( OnLogin != null )
				OnLogin();
		}
	}                 


	private void FBPrintAccessTokenInfo()
	{
		if (DEBUG_ENABLED)
		{
			string str = "";
			if (FB.IsLoggedIn) 
			{
				AccessToken accessToken = Facebook.Unity.AccessToken.CurrentAccessToken;
				if (accessToken != null) 
				{
					str += "userId " + accessToken.UserId + " token = " + accessToken.TokenString + " permissions granted = ";

					// Print current access token's granted permissions
					foreach (string perm in accessToken.Permissions) 
					{
						str += perm + ",";
					}
				}
			}

		    Log(str);
		}
	}

	void FBPostFeed( NewsFeed feed )
	{
		if (!IsLogged ()) 
		{
			// pendingNewsFeed = newsFeed;
			Login ();
			return;
		} 
		else 
		{
			FBDoPostToFeed( feed );
		}
	}

	private void FBDoPostToFeed(NewsFeed newsFeed)
	{
		if (DEBUG_ENABLED) 
			Log("FB.OnPostToFeed()");
		
		if (newsFeed.toId == null || newsFeed.toId.Length == 0)
		{
			AccessToken aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
			if (aToken != null)
			{
				newsFeed.toId = aToken.UserId;
			}
		}

        if (DEBUG_ENABLED)
        {
            string paramsString = "id = " + newsFeed.toId + " link = " + newsFeed.link + " picture = " + newsFeed.picture + " linkName = " + newsFeed.linkName + " caption = " + newsFeed.linkCaption +
                " description = " + newsFeed.linkDescription + " mediasource = " + newsFeed.mediaSource;
            Log(paramsString);
        }        

		//FB.Feed(newsFeed.toId, newsFeed.link, newsFeed.linkName, newsFeed.linkCaption, newsFeed.linkDescription, newsFeed.picture, newsFeed.mediaSource, newsFeed.actionName, newsFeed.actionLink, "reference", new System.Collections.Generic.Dictionary<string,string[]>(), OnPostToFeedCallback);
		System.Uri uriLink = new System.Uri(newsFeed.link);
		System.Uri uriPicture = new System.Uri(newsFeed.picture);

		if (DEBUG_ENABLED) 
			Log ("uriLink = " + uriLink + " uriPicture = " + uriPicture);

		//FB.FeedShare(newsFeed.toId, uriLink, newsFeed.linkName, newsFeed.linkCaption, newsFeed.linkDescription, uriPicture, newsFeed.mediaSource, OnPostToFeedCallback);
		FB.ShareLink(uriLink, newsFeed.linkName, newsFeed.linkDescription, uriPicture, OnFBPostToFeedCallback);
	}

	private void OnFBPostToFeedCallback(IShareResult result)                                                                                              
	{   
		if (DEBUG_ENABLED) 
			Log("OnPostToFeedCallback()");

		if (result.Error == null)
		{
			// Delegate?
		}
		else
		{
			// Delegate?
			if (DEBUG_ENABLED) 
				Log ("ERROR: " + result.Error.ToString ());
		}

		if (DEBUG_ENABLED) 
			Log("PostToFeed response: " + result.RawResult );
	}  

	private void OnFBInviteCallback( IAppInviteResult result)
	{
		
	}

	// WEIBO TODO
}
