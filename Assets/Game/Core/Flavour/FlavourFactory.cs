using System;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for creating a Flavour object depending on a country code and the device platform
/// </summary>
public class FlavourFactory
{   
    public Flavour CreateFlavour()
    {
        return new Flavour();
    }

    public void SetupFlavourBasedOnCriteria(Flavour flavour, string countryCode, Setting_EDevicePlatform devicePlatform)
    {        
        if (!Catalog_IsInitialized())
        {
            Catalog_Initialize();
        }
      
        string flavourSku = Catalog_GetCountryCodeToFlavourSku(countryCode, devicePlatform);
        SetupFlavourDelegate setupFlavourDelegate = Catalog_GetSetupFlavourDelegate(flavourSku);
        setupFlavourDelegate(flavour, flavourSku, devicePlatform);        
    }

    private void SetupFlavour(Flavour flavour, string sku, Setting_ESocialPlatform socialPlatform, Setting_EAddressablesVariant addressablesVariant,
                              bool isSIWAEnabled)
    {
        flavour.Setup(
            sku: sku,
            socialPlatform:Setting_ESocialPlatformToSocialUtilsEPlatform(socialPlatform),
            addressablesVariant:Setting_EAddressablesVariantToString(addressablesVariant),
            isSIWAEnabled:isSIWAEnabled);
    }

    private void Worldwide_SetupFlavour(Flavour flavour, string sku, Setting_EDevicePlatform devicePlatform)
    {
         SetupFlavour(
            flavour:flavour,
            sku:sku,
            socialPlatform:Setting_ESocialPlatform.Facebook,
            addressablesVariant:Setting_EAddressablesVariant.WW,
            isSIWAEnabled:IsSIWAEnabled(devicePlatform));
    }

    private void China_SetupFlavour(Flavour flavour, string sku, Setting_EDevicePlatform devicePlatform)
    {
        SetupFlavour(
            flavour:flavour,        
            sku:sku,            
            socialPlatform:Setting_ESocialPlatform.Weibo,
            addressablesVariant: Setting_EAddressablesVariant.CN,
            isSIWAEnabled:IsSIWAEnabled(devicePlatform));
    }

    private bool IsSIWAEnabled(Setting_EDevicePlatform devicePlatform)
    {       
#if USE_SIWA        
        return devicePlatform == FlavourFactory.Setting_EDevicePlatform.iOS;
#else
        return false;
#endif        
    }

#region catalog
    // Flavour skus: So far "ww" is used for worldwide version and the country code for every country that requires a different flavour
    public const string CATALOG_SKU_DEFAULT = CATALOG_SKU_WW;
    public const string CATALOG_SKU_WW = "WW";
    public const string CATALOG_SKU_CHINA = PlatformUtils.COUNTRY_CODE_CHINA;   
    
    delegate void SetupFlavourDelegate(Flavour flavour, string sku, Setting_EDevicePlatform devicePlatform);

    private Dictionary<string, SetupFlavourDelegate> m_catalog;
    private List<string> m_catalogSkus;

    private void Catalog_Initialize()
    {
        if (!Catalog_IsInitialized())
        {
            m_catalog = new Dictionary<string, SetupFlavourDelegate>();
            m_catalog.Add(CATALOG_SKU_WW, Worldwide_SetupFlavour);
            m_catalog.Add(CATALOG_SKU_CHINA, China_SetupFlavour);
        }
    }

    private bool Catalog_IsInitialized()
    {
        return m_catalog != null;
    }

    private string Catalog_GetCountryCodeToFlavourSku(string countryCode, Setting_EDevicePlatform devicePlatform)
    {
        // Android only supports WW flavour
        if (devicePlatform == Setting_EDevicePlatform.Android)
        {
            countryCode = CATALOG_SKU_WW;
        }
        else
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                countryCode = CATALOG_SKU_WW;
            }
            else
            {
                countryCode = countryCode.ToUpper();
            }
        }

        return (m_catalog.ContainsKey(countryCode)) ? countryCode : CATALOG_SKU_WW;
    }

    private SetupFlavourDelegate Catalog_GetSetupFlavourDelegate(string flavourSku)
    {        
        string sku = (m_catalog.ContainsKey(flavourSku)) ? flavourSku : CATALOG_SKU_WW;
        return m_catalog[sku];        
    }

    public bool Catalog_ContainsSku(string sku)
    {
        return !string.IsNullOrEmpty(sku) && m_catalog.ContainsKey(sku);
    }

    public List<string> Catalog_GetSkus()
    {
        if (m_catalogSkus == null)
        {
            m_catalogSkus = new List<string>(m_catalog.Keys);
        }

        return m_catalogSkus;
    }
#endregion

#region setting
    // This region is responsible for defining Setting types
    
    public enum Setting_ESocialPlatform
    {
        Facebook,
        Weibo
    };

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

    public enum Setting_EAddressablesVariant
    {
        WW,
        CN
    };

    public static Setting_EAddressablesVariant SETTING_ADDRESSABLES_VARIANT_DEFAULT = Setting_EAddressablesVariant.WW;
    public static string SETTING_ADDRESSABLES_VARIANT_DEFAULT_SKU = Setting_EAddressablesVariantToString(SETTING_ADDRESSABLES_VARIANT_DEFAULT);

    public static string Setting_EAddressablesVariantToString(Setting_EAddressablesVariant value)
    {
        return value.ToString();
    }

    public enum Setting_EDevicePlatform
    {
        iOS,
        Android
    };

    public static string SETTING_EDEVICEPLATFORM_IOS = Setting_EDevicePlatform.iOS.ToString();
    public static string SETTING_EDEVICEPLATFORM_ANDROID = Setting_EDevicePlatform.Android.ToString();    
#endregion   
}
