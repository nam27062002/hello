using System;
public class SocialUtilsDummy : SocialUtils
{    
    private bool m_mockLoggedIn;

    public SocialUtilsDummy(bool isEnabled, bool mockLoggedIn) : base(EPlatform.None)
    {
        SetIsEnabled(isEnabled);
        m_mockLoggedIn = mockLoggedIn;
    }

    public override void Login(bool isAppInit)
    {
        bool isLoggedIn = IsLoggedIn();
        Messenger.Broadcast<bool>(MessengerEvents.SOCIAL_LOGGED, isLoggedIn);

        if (isLoggedIn)
        {
            Messenger.Broadcast(MessengerEvents.MERGE_SUCCEEDED);            
        }
    }

    public override string GetPlatformNameTID() { return null; }

    public override void Init(SocialPlatformManager manager) {}

    public override string GetSocialID() { return null; }

    public override string GetAccessToken() { return null;  }

    public override string GetUserName() { return null; }

    public override bool IsLoggedIn() { return GetIsEnabled() && m_mockLoggedIn; }

	public override bool IsLogInTimeoutEnabled() { return false; }

    public override void GetProfileInfoFromPlatform(Action<ProfileInfo> onGetProfileInfo)
    {
        if (onGetProfileInfo != null)
        {
            onGetProfileInfo(null);
        }
    }

    protected override void ExtendedGetProfilePicture(string socialID, string storagePath, Action<bool> onGetProfilePicture, int width = 256, int height = 256) {}
}
