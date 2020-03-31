using System.Collections.Generic;

/// <summary>
/// Documentation: https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Flavours
/// 
/// This class is responsible for storing a collection of settings for a particular flavour.
///
/// </summary>
public class Flavour 
{    
    public string Sku
    {
        get;
        private set;
    }

    //
    // Social Platform
    //
    public enum ESocialPlatform
    {
        Facebook,
        Weibo
    };

    private static SocialUtils.EPlatform ESocialPlatformToSocialUtilsEPlatform(ESocialPlatform value)
    {
        switch (value)
        {
            case ESocialPlatform.Facebook:
                return SocialUtils.EPlatform.Facebook;

            case ESocialPlatform.Weibo:
                return SocialUtils.EPlatform.Weibo;
        }

        return SocialUtils.EPlatform.None;
    }

    public SocialUtils.EPlatform SocialPlatformASSocialUtilsEPlatform
    {
        get
        {
            return ESocialPlatformToSocialUtilsEPlatform(SocialPlatform);
        }
    }

    public ESocialPlatform SocialPlatform
    {
        get;
        private set;
    }

    //
    // Addressables variant
    //
    public enum EAddressablesVariant
    {
        WW,
        CN
    };

    public static List<string> ADDRESSABLE_VARIANT_KEYS = new List<string>() 
    {
        "WW",
        "CN"
    };

    public static EAddressablesVariant ADDRESSABLES_VARIANT_DEFAULT = EAddressablesVariant.WW;
    public static string ADDRESSABLES_VARIANT_DEFAULT_SKU = EAddressablesVariantToString(ADDRESSABLES_VARIANT_DEFAULT);

    public static string EAddressablesVariantToString(EAddressablesVariant value)
    {
        string returnValue;

        int index = (int)value;
        if (index >= ADDRESSABLE_VARIANT_KEYS.Count)
        {
            returnValue = ADDRESSABLES_VARIANT_DEFAULT_SKU;
        }
        else
        {
            returnValue = ADDRESSABLE_VARIANT_KEYS[index];
        }

        return returnValue;
    }

    public EAddressablesVariant AddressablesVariant
    {
        get;
        private set;
    }

    public string AddressablesVariantAsString
    {
        get
        {
            return EAddressablesVariantToString(AddressablesVariant);
        }
    }

    //
    // Device Platform
    public enum EDevicePlatform
    {
        iOS,
        Android
    };

    public bool ShowLanguageSelector
    {
        get;
        private set;
    }

    public bool ShowBloodSelector
    {
        get;
        private set;
    }

    public bool IsTwitterEnabled
    {
        get;
        private set;
    }

    public bool IsInstagramEnabled
    {
        get;
        private set;
    }

    public bool IsWeChatEnabled
    {
        get;
        private set;
    }

    public static string DEVICEPLATFORM_IOS = EDevicePlatform.iOS.ToString();
    public static string DEVICEPLATFORM_ANDROID = EDevicePlatform.Android.ToString();       

    public bool IsSIWAEnabled
    {
        get;
        private set;
    }   
    
    public void Setup(string sku, ESocialPlatform socialPlatform, EAddressablesVariant addressablesVariant,
        bool isSIWAEnabled, bool showLanguageSelector, bool showBloodSelector, bool isTwitterEnabled, bool isInstagramEnabled,
        bool isWeChatEnabled)
    {
        Sku = sku;
        SocialPlatform = socialPlatform;
        AddressablesVariant = addressablesVariant;
        IsSIWAEnabled = isSIWAEnabled;
        ShowLanguageSelector = showLanguageSelector;
        ShowBloodSelector = showBloodSelector;
        IsTwitterEnabled = isTwitterEnabled;
        IsInstagramEnabled = isInstagramEnabled;
        IsWeChatEnabled = isWeChatEnabled;
    }
}
