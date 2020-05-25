using System.Collections.Generic;

/// <summary>
/// Documentation: https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+Flavours
/// 
/// This class is responsible for storing a collection of settings for a particular flavour.
///
/// </summary>
public class Flavour 
{


    //------------------------------------------------------------------------//
    // ENUMS		    													  //
    //------------------------------------------------------------------------//

    // This settings will be used by components in the editor. DO NOT REMOVE/CHANGE ORDER
    public enum SettingKey
    {

        SHOW_LANGUAGE_SELECTOR,
        BLOOD_ALLOWED,
        INSTAGRAM_ALLOWED,
        TWITTER_ALLOWED,
        WECHAT_ALLOWED,
        SIWA_ALLOWED,
        SHOW_SPLASH_LEGAL_TEXT,
        CORPSES_ALLOWED,
        MACABRE_ALLOWED, // Restriction for bones, tombs, ghosts, skulls...
        WEAPONS_ALLOWED,
        SHOW_CONSUME_PROPERLY_REMINDER

    }

    //
    // Social Platform
    //
    public enum ESocialPlatform
    {
        Facebook,
        Weibo
    };

    //
    // Addressables variant
    //
    public enum EAddressablesVariant
    {
        WW,
        CN,
        KR
    };

    //
    // Device Platform
    public enum EDevicePlatform
    {
        iOS,
        Android
    };

    //------------------------------------------------------------------------//
    // PARAMETERS & PROPERTIES                                                //
    //------------------------------------------------------------------------//

    private Dictionary<SettingKey, bool> boolSettings;
    private HashSet<string> blackListSFX;
    private Dictionary<string, List<string>> blackListAccessories;
    private HashSet<string> blackListEntities;

    public string Sku
    {
        get;
        private set;
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

    public string ShareLocationDef
    {
        get;
        private set;
    }

    public GameSettings.ShareData ShareData
    {
        get;
        private set;
    }

    public string GetMonoLanguageSku
    {
        get;
        private set;
    }

    public string GameWebsiteUrl
    {
        get;
        private set;
    }

    //------------------------------------------------------------------------//
    // STATIC   		    												  //
    //------------------------------------------------------------------------//

    public static string DEVICEPLATFORM_IOS = EDevicePlatform.iOS.ToString();
    public static string DEVICEPLATFORM_ANDROID = EDevicePlatform.Android.ToString();

    private static List<string> s_addressablesVariantKeys;
    public static EAddressablesVariant ADDRESSABLES_VARIANT_DEFAULT = EAddressablesVariant.WW;
    public static string ADDRESSABLES_VARIANT_DEFAULT_SKU = EAddressablesVariantToString(ADDRESSABLES_VARIANT_DEFAULT);

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

    private static List<string> GetAddressablesVariantKeys()
    {
        if (s_addressablesVariantKeys == null)
        {
            s_addressablesVariantKeys = new List<string>();
            foreach (EAddressablesVariant val in System.Enum.GetValues(typeof(EAddressablesVariant)))
            {
                s_addressablesVariantKeys.Add(val.ToString().ToLower());
            }
        }

        return s_addressablesVariantKeys;
    }

    public static string EAddressablesVariantToString(EAddressablesVariant value)
    {
        string returnValue;

        int index = (int)value;
        List<string> variantKeys = GetAddressablesVariantKeys();
        if (index >= variantKeys.Count)
        {
            returnValue = ADDRESSABLES_VARIANT_DEFAULT_SKU;
        }
        else
        {
            returnValue = variantKeys[index];
        }

        return returnValue;
    }


    //------------------------------------------------------------------------//
    // METHODS  		    												  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Returns the boolean value of a setting
    /// </summary>
    /// <typeparam name="Bool"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetSetting<Bool> (SettingKey key)
    {
        if (boolSettings.ContainsKey(key) )
        {
            return boolSettings[key];
        }

        // Default
        return false;
    }

    /// <summary>
    /// Validate if an audio clip it's on a forbidden list
    /// </summary>
    /// <param name="audioSubItemId"></param>
    /// <returns>TRUE if the audio clip can be played, FALSE otherwise</returns>
    public bool CanPlaySFX(string audioSubItemId)
    {
        if (blackListSFX == null)
        {
            return true;
        }

        return !blackListSFX.Contains(audioSubItemId);
    }

    /// <summary>
    /// Validate if an entity it's on a blacklist
    /// </summary>
    /// <param name="entityName"></param>
    /// <returns>TRUE if the entity can be shown, FALSE otherwise</returns>
    public bool CanSpawnEntity(string entityName)
    {
        if (blackListEntities == null)
        {
            return true;
        }

        return !blackListEntities.Contains(entityName);
    }

    /// <summary>
    /// Check if the flavour has black listed entities
    /// </summary>
    /// <returns>TRUE if the flavour has blacklisted entities, FALSE otherwise</returns>
    public bool HasBlacklistedEntities()
    {
        if (blackListEntities == null)
        {
            return false;
        }

        return blackListEntities.Count > 0;
    }

    /// <summary>
    /// Validate if an accessory can be shown
    /// </summary>
    /// <param name="skin">skin SKU</param>
    /// <param name="accessory">accessory SKU</param>
    /// <returns>TRUE if the accessory is blacklisted for the given skin and cannot be shown, FALSE otherwise</returns>
    public bool IsAccessoryBlacklisted(string skin, string accessory)
    {
        if (blackListAccessories == null)
            return false;

        if (blackListAccessories.TryGetValue(skin, out List<string> accessoriesSKU))
        {
            for (int i = 0; i < accessoriesSKU.Count; i++)
            {
                if (accessoriesSKU[i] == accessory)
                    return true;
            }
        }

        return false;
    }

    public void Setup(string sku, ESocialPlatform socialPlatform, EAddressablesVariant addressablesVariant,
        bool isSIWAEnabled, bool showLanguageSelector, bool showBloodSelector, bool isTwitterEnabled, bool isInstagramEnabled,
        bool isWeChatEnabled, bool showSplashLegalText, string[] blackListedSFX, bool corpsesAllowed, bool macabreAllowed,
        bool weaponsAllowed, bool consumeProperlyReminderEnabled,
        string shareLocationDef, string monoLanguageSku, Dictionary<string, List<string>> blackListedAccessories,
        string[] blackListedEntities, string gameWebsite)
    {
        Sku = sku;
        SocialPlatform = socialPlatform;
        AddressablesVariant = addressablesVariant;

        // Boolean settings
        boolSettings = new Dictionary<SettingKey, bool>();

        boolSettings.Add(SettingKey.SIWA_ALLOWED, isSIWAEnabled);
        boolSettings.Add(SettingKey.SHOW_LANGUAGE_SELECTOR, showLanguageSelector);
        boolSettings.Add(SettingKey.BLOOD_ALLOWED, showBloodSelector);
        boolSettings.Add(SettingKey.TWITTER_ALLOWED, isTwitterEnabled);
        boolSettings.Add(SettingKey.INSTAGRAM_ALLOWED, isInstagramEnabled);
        boolSettings.Add(SettingKey.WECHAT_ALLOWED, isWeChatEnabled);
        boolSettings.Add(SettingKey.SHOW_SPLASH_LEGAL_TEXT, showSplashLegalText);
        boolSettings.Add(SettingKey.CORPSES_ALLOWED, corpsesAllowed);
        boolSettings.Add(SettingKey.MACABRE_ALLOWED, macabreAllowed);
        boolSettings.Add(SettingKey.WEAPONS_ALLOWED, weaponsAllowed);
        boolSettings.Add(SettingKey.SHOW_CONSUME_PROPERLY_REMINDER, consumeProperlyReminderEnabled);

        // Push the forbbiden audio clips
        SetupBlackListSFX(blackListedSFX);

        // Share data
        ShareLocationDef = shareLocationDef;
        SetupShareData();

        // Unique language SKU allowed (for China)
        GetMonoLanguageSku = monoLanguageSku;

        // Black list for accessories (body_parts not allowed in China)
        blackListAccessories = blackListedAccessories;

        // Black list entities (entity prefabs not allowed in China)
        SetupBlackListEntities(blackListedEntities);

        GameWebsiteUrl = gameWebsite;
    }

    private void SetupBlackListEntities(string[] blackListedEntities)
    {
        if (blackListedEntities != null)
        {
            blackListEntities = new HashSet<string>();
            for (int i = 0; i < blackListedEntities.Length; i++)
            {
                blackListEntities.Add(blackListedEntities[i]);
            }
        }
    }

    private void SetupBlackListSFX(string[] blackListedSFX)
    {
        if (blackListedSFX != null)
        {
            blackListSFX = new HashSet<string>();
            for (int i = 0; i < blackListedSFX.Length; i++)
            {
                blackListSFX.Add(blackListedSFX[i]);
            }
        }
    }

    private void SetupShareData()
    {
        if (Sku == PlatformUtils.COUNTRY_CODE_CHINA) {
            ShareData = GameSettings.instance.ShareDataChina;
        }
        else {
#if UNITY_IOS
			ShareData = GameSettings.instance.ShareDataIOS;
#else
            ShareData = GameSettings.instance.ShareDataAndroid;
#endif
        }
    }
}
