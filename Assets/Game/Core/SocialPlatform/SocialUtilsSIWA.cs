using Facebook.Unity;
using System;
using System.Collections.Generic;
using XTech.SIWA;
using XTech.SIWA.Interface;

public class SocialUtilsSIWA : SocialUtils
{
    // Social Listener //////////////////////////////////////////////////////
    private  class GameSocialListener : SIWAFacade.Listener
	{
        const string TAG = "GameSocialListener";
        private SocialPlatformManager m_manager;

        public GameSocialListener(SocialPlatformManager manager)
        {
            m_manager = manager;
        }
		
		public override void OnSignedInSuccess(SignInType type, SignInCredential credential)
		{
			Debug.TaggedLog(TAG, "onLogInCompleted");
			m_manager.OnSocialPlatformLogin();
		}

		public override void OnSignedInError(SignInType type, SignInError error)
		{
			Debug.TaggedLog(TAG, "onLogInFailed");
			m_manager.OnSocialPlatformLoginFailed();
		}

		public override void OnSignedOut()
		{
			m_manager.OnSocialPlatformLogOut();
			Debug.TaggedLog(TAG, "onLogOut");
		}		
    }
    //////////////////////////////////////////////////////////////////////////
    
    public SocialUtilsSIWA() : base(EPlatform.SIWA)
    {
    }

    public override string GetPlatformNameTID()
    {
        return "TID_SOCIAL_APPLE";
    }

    public override void Init(SocialPlatformManager manager)
    {        
        GameSocialListener listener = new GameSocialListener(manager);
        GameSessionManager.SharedInstance.SIWA_AddListener(listener);        
    }

    public override string GetSocialID()
    {
        return GameSessionManager.SharedInstance.SIWA_GetUserId();
    }    

    public override string GetUserName()
    {
        // Not supported (Safest approach to be GDPR compliant)
        return null;
    }

    public override bool IsLoggedIn()
    {
        return GameSessionManager.SharedInstance.SIWA_IsSignedIn();
    }

	public override bool IsLogInTimeoutEnabled()
	{
		#if UNITY_ANDROID
		return true;
		#else
		return false;
		#endif
	}

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {
        // There's no support for profile info in SIWA
        if (onGetProfileInfo != null)
		{
			onGetProfileInfo(null);
		}        
    }

    protected override void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256)
    {
		// There's no support for profile info in SIWA
		if (onGetProfilePicture != null)
		{
			onGetProfilePicture(false);
		}        
    }   
}