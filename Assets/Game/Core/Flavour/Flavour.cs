/// <summary>
/// Documentation: https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Flavours
/// 
/// This class is responsible for storing a collection of settings for a particular flavour.
///
/// ollowing Android build variant or flavour approach, a flavour is a variant of the application.
/// It's useful when you need to maintain several versions of the application to adapt it to different audiences,
/// typically for different countries or ages or device platforms. 
///
/// A setting is anything that may look or behave differently for different flavours.
/// </summary>
public class Flavour 
{
#region setting
    private static SocialUtils.EPlatform Setting_ESocialPlatformToSocialUtilsEPlatform(Setting_ESocialPlatform value)
    {
        switch (value)
        {
            case Setting_ESocialPlatform.Facebook:
                return SocialUtils.EPlatform.Facebook;

            case Setting_ESocialPlatform.Weibo:
                return SocialUtils.EPlatform.Weibo;
        }

        return SocialUtils.EPlatform.None;
    }

    public static string Setting_EAddressablesVariantToString(Setting_EAddressablesVariant value)
    {
        return value.ToString();
    }

    public enum Setting_ESocialPlatform
    {
        Facebook,
        Weibo
    };

    public enum Setting_EAddressablesVariant
    {
        WW,
        CN
    };

    public enum Setting_EDevicePlatform
    {
        iOS,
        Android
    };

    public static string SETTING_EDEVICEPLATFORM_IOS = Setting_EDevicePlatform.iOS.ToString();
    public static string SETTING_EDEVICEPLATFORM_ANDROID = Setting_EDevicePlatform.Android.ToString();
    #endregion

    public static Setting_EAddressablesVariant ADDRESSABLES_VARIANT_DEFAULT = Setting_EAddressablesVariant.WW;
    public static string ADDRESSABLES_VARIANT_DEFAULT_SKU = Setting_EAddressablesVariantToString(ADDRESSABLES_VARIANT_DEFAULT);

    public string Sku
    {
        get;
        private set;
    }

    public SocialUtils.EPlatform SocialPlatform
    {
        get;
        private set;
    }

    public string AddressablesVariant
    {
        get;
        private set;
    }

    public bool IsSIWAEnabled
    {
        get;
        private set;
    }   
    
    public void Setup(string sku, Setting_ESocialPlatform socialPlatform, Setting_EAddressablesVariant addressablesVariant, bool isSIWAEnabled)
    {
        Sku = sku;
        SocialPlatform = Setting_ESocialPlatformToSocialUtilsEPlatform(socialPlatform);
        AddressablesVariant = Setting_EAddressablesVariantToString(addressablesVariant);
        IsSIWAEnabled = isSIWAEnabled;
    }    
}
