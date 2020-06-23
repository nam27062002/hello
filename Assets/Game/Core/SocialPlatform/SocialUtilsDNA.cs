using System;

public class SocialUtilsDNA : SocialUtils
{
    // Social Listener //////////////////////////////////////////////////////
    private  class GameSocialListener : DNASocialPlatformManager.Listener
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

        public override void onLogInFailed()
        {
            Debug.TaggedLog(TAG, "onLogInFailed");
            m_manager.OnSocialPlatformLoginFailed();
        }
    }
    //////////////////////////////////////////////////////////////////////////    

    // It needs to enable autoFirstLogin since the user is not aware of this system
    public SocialUtilsDNA() : base(EPlatform.DNA, true)
    {
    }

    public override string GetPlatformNameTID()
    {
        // It won't be used as DNA is used silently
        return "";
    }

    public override void Init(SocialPlatformManager manager)
    {        
        GameSocialListener listener = new GameSocialListener(manager);
        GameSessionManager.SharedInstance.DNAA_AddListener(listener);
    }

    public override string GetSocialID()
    {
        return GameSessionManager.SharedInstance.DNA_GetUserId();
    }    

    public override string GetUserName()
    {
        // Not supported (Safest approach to be GDPR compliant)
        return null;
    }

    public override bool IsLoggedIn()
    {
        return GameSessionManager.SharedInstance.DNA_IsLoggedIn();
    }

	public override bool IsLogInTimeoutEnabled()
	{
        // Timout is implemented by Calety DNASocialPlatform
        return false;
	}

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {
        // There's no support for profile info in DNA
        if (onGetProfileInfo != null)
		{
			onGetProfileInfo(null);
		}        
    }

    protected override void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256)
    {
		// There's no support for profile info in DNA
		if (onGetProfilePicture != null)
		{
			onGetProfilePicture(false);
		}        
    }   
}