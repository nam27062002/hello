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

    public void SetupFlavourBasedOnCriteria(Flavour flavour, string countryCode, FlavourSettings.EDevicePlatform devicePlatform)
    {               
        FlavourSettings flavourSettings = Settings_GetCountryCodeToFlavourSettings(countryCode, devicePlatform);
        flavourSettings.SetupFlavour(flavour, countryCode);               
    }       

    private bool IsSIWAEnabled(FlavourSettings.EDevicePlatform devicePlatform)
    {       
#if USE_SIWA        
        return devicePlatform == FlavourSettings.EDevicePlatform.iOS;
#else
        return false;
#endif        
    }

#region settings
    // Flavour skus: So far "WW" is used for worldwide version and the country code for every country that requires a different flavour    
    public const string SETTINGS_SKU_DEFAULT = SETTINGS_SKU_WW;
    public const string SETTINGS_SKU_WW = "WW";
    public const string SETTINGS_SKU_CHINA = PlatformUtils.COUNTRY_CODE_CHINA;             

    private FlavourSettings Settings_GetCountryCodeToFlavourSettings(string countryCode, FlavourSettings.EDevicePlatform devicePlatform)
    {        
        // Android only supports WW flavour
        if (devicePlatform == FlavourSettings.EDevicePlatform.Android)
        {
            countryCode = SETTINGS_SKU_WW;
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
        }

        // countryCode is used as flavourSettingSku
        return Settings_GetFlavourSettings(countryCode, devicePlatform);
    }

    private FlavourSettings Settings_GetFlavourSettings(string flavourSettingsSku, FlavourSettings.EDevicePlatform devicePlatform)
    {
        FlavourSettings returnValue = null;               
        if (flavourSettingsSku == SETTINGS_SKU_WW)
        {
            returnValue = Settings_GetFlavourSettingsWW(devicePlatform);
        }
        else if (flavourSettingsSku == SETTINGS_SKU_CHINA)
        {
            returnValue = Settings_GetFlavourSettingsChina(devicePlatform);
        }        

        // If there's no FlavourSettings defined for flavourSku then the default one is returned
        if (returnValue == null)
        {
            returnValue = Settings_GetFlavourSettings(SETTINGS_SKU_DEFAULT, devicePlatform);
        }

        return returnValue;
    }

    private FlavourSettings Settings_GetFlavourSettingsWW(FlavourSettings.EDevicePlatform devicePlatform)
    {
        return new FlavourSettings(
           socialPlatform: FlavourSettings.ESocialPlatform.Facebook,
           addressablesVariant: FlavourSettings.EAddressablesVariant.WW,
           isSIWAEnabled: IsSIWAEnabled(devicePlatform));
    }

    private FlavourSettings Settings_GetFlavourSettingsChina(FlavourSettings.EDevicePlatform devicePlatform)
    {
        return new FlavourSettings(        
           socialPlatform: FlavourSettings.ESocialPlatform.Weibo,
           addressablesVariant: FlavourSettings.EAddressablesVariant.CN,
           isSIWAEnabled: IsSIWAEnabled(devicePlatform));
    }    
#endregion
}
