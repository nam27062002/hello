﻿using System.Collections.Generic;

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
        SHOW_SPLASH_LEGAL_TEXT

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
        CN
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


   

    //------------------------------------------------------------------------//
    // STATIC   		    												  //
    //------------------------------------------------------------------------//

    public static string DEVICEPLATFORM_IOS = EDevicePlatform.iOS.ToString();
    public static string DEVICEPLATFORM_ANDROID = EDevicePlatform.Android.ToString();

    public static List<string> ADDRESSABLE_VARIANT_KEYS = new List<string>()
    {
        "WW",
        "CN"
    };


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


    public void Setup(string sku, ESocialPlatform socialPlatform, EAddressablesVariant addressablesVariant,
        bool isSIWAEnabled, bool showLanguageSelector, bool showBloodSelector, bool isTwitterEnabled, bool isInstagramEnabled,
        bool isWeChatEnabled, bool showSplashLegalText)
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

    }

}
