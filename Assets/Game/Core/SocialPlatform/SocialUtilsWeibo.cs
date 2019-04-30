using System;
public class SocialUtilsWeibo : SocialUtils
{
    // Social Listener //////////////////////////////////////////////////////
    private class GameSocialListener : WeiboManager.WeiboListenerBase
    {
        const string TAG = "GameSocialListener";
        private SocialPlatformManager m_manager;

        public GameSocialListener(SocialPlatformManager manager)
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

        public override void onLogInFailed(string strError)
        {
            Debug.TaggedLog(TAG, "onLogInFailed with error: " + strError);
            m_manager.OnSocialPlatformLoginFailed();
        }

        public override void onLogOut()
        {
            m_manager.OnSocialPlatformLogOut();
            Debug.TaggedLog(TAG, "onLogOut");
        }       
    }
    //////////////////////////////////////////////////////////////////////////

    public SocialUtilsWeibo() : base(EPlatform.Weibo)
    {
    }

    public override void Init(SocialPlatformManager manager)
    {
        GameSocialListener listener = new GameSocialListener(manager);
        WeiboManager.SharedInstance.AddWeiboListener(listener);
        WeiboManager.SharedInstance.Initialise("all");
    }

    public override string GetPlatformNameTID()
    {
        return "TID_SOCIAL_NETWORK_WEIBO_NAME";
    }

    public override string GetSocialID()
    {
        return WeiboManager.SharedInstance.GetAuthUserID();
    }

    public override string GetAccessToken()
    {
        return WeiboManager.SharedInstance.GetAuthToken();
    }

    public override string GetUserName()
    {
        return WeiboManager.SharedInstance.GetAuthUserName();
    }

    public override bool IsLoggedIn()
    {
        return WeiboManager.SharedInstance.IsLoggedIn();
    }

	public override bool IsLogInTimeoutEnabled()
	{		
		// Timeout is enabled to address HDK-2590
		return true;	
	}

	public override void OnLogInTimeout() 
	{		
		WeiboManager.SharedInstance.LogOut();
	}

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {		
		ProfileInfo profileInfo = new ProfileInfo();
		profileInfo.SetValueAsString(ProfileInfo.KEY_ID, WeiboManager.SharedInstance.GetAuthUserID());
		profileInfo.SetValueAsString(ProfileInfo.KEY_FIRST_NAME, WeiboManager.SharedInstance.GetAuthUserName()); 

		// Translates the gender returned by Weibo into the values defined by the specification of this event
		string gender = WeiboManager.SharedInstance.GetAuthUserGender();
		switch(gender) 
		{
			case "m":
				gender = "male";
				break;

			case "f":
				gender = "female";
				break;
		}			
		
		profileInfo.SetValueAsString(ProfileInfo.KEY_GENDER, gender);

		if (onGetProfileInfo != null)
		{
			onGetProfileInfo(profileInfo);
		}
    }

    protected override void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256) 
	{
		string url = WeiboManager.SharedInstance.GetAuthUserProfileImageUrl();

		if(string.IsNullOrEmpty(url)) 
		{
			onGetProfilePicture(false);
		} 
		else 
		{
			UnityEngine.Events.UnityAction<bool, string, long> onThisDone = delegate(bool success, string key, long size) 
			{
				onGetProfilePicture(success);
			};

			NetworkManager.SharedInstance.DownloadFile("profilePic_" + socialID, url, FileUtils.GetDeviceStoragePath(storagePath, CaletyConstants.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED), onThisDone);
		}
	}
}
