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

    public void SetupFlavourBasedOnCriteria(Flavour flavour, string countryCode, Flavour.EDevicePlatform devicePlatform)
    {
        string flavourSku;

        // Android only supports WW flavour
        if (devicePlatform == Flavour.EDevicePlatform.Android)
        {
            flavourSku = SETTINGS_SKU_WW;
        }
        else
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                countryCode = SETTINGS_SKU_WW;
            }
            else
            {
                countryCode = countryCode.ToUpper();
            }

            flavourSku = countryCode;
        }

        Settings_SetFlavour(flavour, flavourSku, devicePlatform);                  
    }       

    private bool IsSIWAEnabled(Flavour.EDevicePlatform devicePlatform)
    {       
#if USE_SIWA        
        return devicePlatform == Flavour.EDevicePlatform.iOS;
#else
        return false;
#endif        
    }

#region settings
    // Flavour skus: So far "WW" is used for worldwide version and the country code for every country that requires a different flavour    
    public const string SETTINGS_SKU_DEFAULT = SETTINGS_SKU_WW;
    public const string SETTINGS_SKU_WW = "WW";
    public const string SETTINGS_SKU_CHINA = PlatformUtils.COUNTRY_CODE_CHINA;             

    private void Settings_SetFlavour(Flavour flavour, string flavourSku, Flavour.EDevicePlatform devicePlatform)
    {
        if (flavourSku == SETTINGS_SKU_WW)
        {
            Settings_SetFlavourWW(flavour, devicePlatform);
        }
        else if (flavourSku == SETTINGS_SKU_CHINA)
        {
            Settings_SetFlavourChina(flavour, devicePlatform);
        }
        else
        {
            // If there's no Flavour defined for flavourSku then the default one is returned
            Settings_SetFlavour(flavour, SETTINGS_SKU_DEFAULT, devicePlatform);
        }                
    }

    private void Settings_SetFlavourWW(Flavour flavour, Flavour.EDevicePlatform devicePlatform)
    {
        flavour.Setup(
           sku: SETTINGS_SKU_WW, 
           socialPlatform: Flavour.ESocialPlatform.Facebook,
           addressablesVariant: Flavour.EAddressablesVariant.WW,
           isSIWAEnabled: IsSIWAEnabled(devicePlatform),
           showLanguageSelector: true);
    }

    private void Settings_SetFlavourChina(Flavour flavour, Flavour.EDevicePlatform devicePlatform)
    {
        flavour.Setup(
           sku: SETTINGS_SKU_CHINA,
           socialPlatform: Flavour.ESocialPlatform.Weibo,
           addressablesVariant: Flavour.EAddressablesVariant.CN,
           isSIWAEnabled: IsSIWAEnabled(devicePlatform),
           showLanguageSelector: false);
    }    
#endregion
}
