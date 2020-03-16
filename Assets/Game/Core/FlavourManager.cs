/// <summary>
/// This class is responsible for managing app flavours. Following Android Studio approach (https://developer.android.com/studio/build/build-variants)
/// for handling build variants our game can have different flavours. Some game features are configurable (Example: Splash screen, social platform, ...).
/// Every single configuration is called "flavour". The two main flavours are:
/// 1)Worldwide (WW): This is the master
/// 2)China: Build variant for China
/// </summary>
public class FlavourManager
{
    /// <summary>
    /// Returns the social platform configured for current flavour. Possible values:
    ///        Weibo for China
    ///        Facebook otherwise
    /// </summary>    
    public static SocialUtils.EPlatform GetSocialPlatform()
    {
        SocialUtils.EPlatform returnValue = SocialUtils.EPlatform.Facebook;

        // On iOS we need to check the user's country to decide the social platform: either Facebook or Weibo (only in China)
#if UNITY_IOS
        string countryCode = PlatformUtils.Instance.Country_GetCodeOnInstall();
        returnValue = (countryCode == PlatformUtils.COUNTRY_CODE_CHINA) ? SocialUtils.EPlatform.Weibo : SocialUtils.EPlatform.Facebook;
#endif

        return returnValue;
    }
}
