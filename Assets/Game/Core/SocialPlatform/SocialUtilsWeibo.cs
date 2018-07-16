using System;
public class SocialUtilsWeibo : SocialUtils
{    
    public override void Init(SocialPlatformManager manager) { }

    public override string GetPlatformNameTID()
    {
        return "TID_SOCIAL_NETWORK_WEIBO_NAME";
    }

    public override string GetSocialID() { return null; }

    public override string GetAccessToken() { return null; }

    public override string GetUserName() { return null; }

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
