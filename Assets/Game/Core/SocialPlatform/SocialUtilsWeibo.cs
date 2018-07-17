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

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {
        if (onGetProfileInfo != null)
        {
            onGetProfileInfo(null);
        }
    }

    protected override void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256) { }
}
