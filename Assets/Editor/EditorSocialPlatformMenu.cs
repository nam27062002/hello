using UnityEditor;
using UnityEngine;

/// <summary>
/// This class is responsible for handling "Tech/Social Platform" menu which offers some Social Platform (Facebook, Weibo, SIWA)
/// related utilities 
/// </summary>
public class EditorSocialPlatformMenu : MonoBehaviour
{
    private const string SOCIAL_PLATFORM_MENU = "Tech/Social Platform";
    private const string SOCIAL_PLATFORM_NONE = SOCIAL_PLATFORM_MENU + "/None";
    private const string SOCIAL_PLATFORM_FACEBOOK = SOCIAL_PLATFORM_MENU + "/Facebook";
    private const string SOCIAL_PLATFORM_WEIBO = SOCIAL_PLATFORM_MENU + "/Weibo";
    private const string SOCIAL_PLATFORM_SIWA = SOCIAL_PLATFORM_MENU + "/SIWA";    

    private static PersistenceLocalDriver s_persistenceLocalDriver = new PersistenceLocalDriver();    

    private static SocialUtils.EPlatform SocialPlatform
    {
        get
        {            
            SocialUtils.EPlatform platformId = SocialUtils.KeyToEPlatform(s_persistenceLocalDriver.Prefs_SocialPlatformKey);
            bool isPlatformSupported = SocialPlatformManager.IsSocialPlatformIdSupportedByDevicePlatform(platformId);
            if (!isPlatformSupported)
            {
                platformId = SocialUtils.EPlatform.None;
            }

            return platformId;
        }

        set
        {
            string platformKey = (value == SocialUtils.EPlatform.None) ? "" : SocialUtils.EPlatformToKey(value);
            s_persistenceLocalDriver.Prefs_SocialPlatformKey = platformKey;

            // Call Update() to make it save prefs
            s_persistenceLocalDriver.Update(); 
        }
    }    

    [MenuItem(SOCIAL_PLATFORM_NONE)]
    public static void SocialPlatform_SetNone()
    {
        SocialPlatform = SocialUtils.EPlatform.None;
    }

    [MenuItem(SOCIAL_PLATFORM_NONE, true)]
    public static bool SocialPlatform_SetNoneValidate()
    {
        Menu.SetChecked(SOCIAL_PLATFORM_NONE, SocialPlatform == SocialUtils.EPlatform.None);
        return true;
    }

    [MenuItem(SOCIAL_PLATFORM_FACEBOOK)]
    public static void SocialPlatform_SetFacebook()
    {
        SocialPlatform = SocialUtils.EPlatform.Facebook;
    }

    [MenuItem(SOCIAL_PLATFORM_FACEBOOK, true)]
    public static bool SocialPlatform_SetFacebookValidate()
    {
        Menu.SetChecked(SOCIAL_PLATFORM_FACEBOOK, SocialPlatform == SocialUtils.EPlatform.Facebook);
        return true;
    }

    [MenuItem(SOCIAL_PLATFORM_WEIBO)]
    public static void SocialPlatform_SetWeibo()
    {
        SocialPlatform = SocialUtils.EPlatform.Weibo;
    }

    [MenuItem(SOCIAL_PLATFORM_WEIBO, true)]
    public static bool SocialPlatform_SetWeiboValidate()
    {
        Menu.SetChecked(SOCIAL_PLATFORM_WEIBO, SocialPlatform == SocialUtils.EPlatform.Weibo);
        return true;
    }

    [MenuItem(SOCIAL_PLATFORM_SIWA)]
    public static void SocialPlatform_SetSIWA()
    {
        SocialPlatform = SocialUtils.EPlatform.SIWA;
    }

    [MenuItem(SOCIAL_PLATFORM_SIWA, true)]
    public static bool SocialPlatform_SetSIWAValidate()
    {
        Menu.SetChecked(SOCIAL_PLATFORM_SIWA, SocialPlatform == SocialUtils.EPlatform.SIWA);
        return true;
    }   
}