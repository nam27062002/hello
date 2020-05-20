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

    // The following entities cannot be shown on China flavour, due to new China law
    readonly string[] blackListEntitiesChina = new string[]
    {
        "Water/PF_TubeMan",
        "Junk/PF_BadJunkBone",
        "Junk/PF_BadJunkEye",
        "Junk/PF_BadJunkFrog",
        "Junk/PF_BadJunkMagicBottle"
    };

    // The following audio clips cannot be played on China flavour, due to new China law
    readonly string[] blackListSFXChina = new string[]
    {
        "hd_villager_alert_01",
        "hd_villager_alert_02",
        "hd_villager_alert_03",
        "hd_villager_alert_04",
        "hd_villager_death_01",
        "hd_villager_death_02",
        "hd_villager_death_03",
        "hd_villager_death_04",
        "hd_villager_death_05",
        "hd_bakerwoman_alert_01",
        "hd_bakerwoman_alert_02",
        "hd_bakerwoman_dead_01",
        "hd_bakerwoman_dead_02",
        "hd_witch_dead_03",
        "hd_goblin_dead_03",
        "hd_soldier_death_02",
        "hd_soldier_death_04",
        "hd_soldier_death_05",
        "hd_soldier_death_06"
    };

    // The following accessories per-skin cannot be shown on China flavour, due to new China law
    readonly Dictionary<string, List<string>> blackListAccessoriesChina = new Dictionary<string, List<string>>
    {
        { "dragon_bug_1", new List<string> { "PF_Pirate_Hook", "PF_Pirate_Pegleg" } }, // Jekyll (Swashbuckler): Hook, PegLeg
        { "dragon_bug_2", new List<string> { "PF_Executioner_L_Hat" } }, // Jekyll (Executioner): Axe
        { "dragon_crocodile_2", new List<string> { "PF_Pirate_Hook" } }, // Mad Snax (Captain Crunch): Hook
        { "dragon_tony_2", new List<string> { "PF_tony_mafia_gun" } }, // Tony Dragone (Vito Dragone): Gun
        { "dragon_devil_3", new List<string> { "PF_Knight_Waist" } }, // Dante (Sir Burnalot): Sword
        { "dragon_jawfrey_3", new List<string> { "PF_Orc_Axe" } }, // Jawfrey (Jawrc the Hungry): Axe
        { "dragon_jawfrey_4", new List<string> { "PF_Samurai_Sword" } }, // Jawfrey (Ryujawn): Sword
        { "dragon_skeleton_1", new List<string> { "PF_skeleton_motu_axe" } }, // Skully (Skulletor): Axe
        { "dragon_skeleton_4", new List<string> { "PF_skeleton_viking_axe" } } // Skully (Dragnarok): Axe
    };

    #region settings
    // Flavour skus: So far "WW" is used for worldwide version and the country code for every country that requires a different flavour    
    private const string SETTINGS_SKU_DEFAULT = SETTINGS_SKU_WW;
    private const string SETTINGS_SKU_WW = "WW";
    private const string SETTINGS_SKU_CHINA = PlatformUtils.COUNTRY_CODE_CHINA;
    private const string SETTINGS_SKU_KOREA = PlatformUtils.COUNTRY_CODE_KOREA;

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
        else if (flavourSku == SETTINGS_SKU_KOREA)
        {
            Settings_SetFlavourKorea(flavour, devicePlatform);
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
           showLanguageSelector: true,
           showBloodSelector: true,
           isTwitterEnabled: true,
           isInstagramEnabled: true,
           isWeChatEnabled: false,
           showSplashLegalText: false,
           blackListedSFX: null,
           corpsesAllowed: true,
           macabreAllowed: true,
           weaponsAllowed: true,
           consumeProperlyReminderEnabled: false,
           shareLocationDef: "url",
           monoLanguageSku: null,
           blackListedAccessories: null,
           blackListedEntities: null,
           gameWebsite: GameSettings.instance.GameWebsiteUrl);
    }

    private void Settings_SetFlavourChina(Flavour flavour, Flavour.EDevicePlatform devicePlatform)
    {
        flavour.Setup(
           sku: SETTINGS_SKU_CHINA,
           socialPlatform: Flavour.ESocialPlatform.Weibo,
           addressablesVariant: Flavour.EAddressablesVariant.CN,
           isSIWAEnabled: IsSIWAEnabled(devicePlatform),
           showLanguageSelector: false,
           showBloodSelector: false,
           isTwitterEnabled: false,
           isInstagramEnabled: false,
           isWeChatEnabled: true,
           showSplashLegalText: true,
           blackListedSFX: blackListSFXChina,
           corpsesAllowed: false,
           macabreAllowed: false,
           weaponsAllowed: false,
           consumeProperlyReminderEnabled: true,
           shareLocationDef: "urlChina",
           monoLanguageSku: "lang_chinese",
           blackListedAccessories: blackListAccessoriesChina,
           blackListedEntities: blackListEntitiesChina,
           gameWebsite: GameSettings.instance.GameWebsiteUrlChina);
    }

    private void Settings_SetFlavourKorea(Flavour flavour, Flavour.EDevicePlatform devicePlatform)
    {
        flavour.Setup(
           sku: SETTINGS_SKU_KOREA,
           socialPlatform: Flavour.ESocialPlatform.Facebook,
           addressablesVariant: Flavour.EAddressablesVariant.KR,
           isSIWAEnabled: IsSIWAEnabled(devicePlatform),
           showLanguageSelector: true,
           showBloodSelector: false,
           isTwitterEnabled: true,
           isInstagramEnabled: true,
           isWeChatEnabled: false,
           showSplashLegalText: false,
           blackListedSFX: null,
           corpsesAllowed: false,
           macabreAllowed: true,
           weaponsAllowed: true,
           consumeProperlyReminderEnabled: false,
           shareLocationDef: "url",
           monoLanguageSku: null,
           blackListedAccessories: null,
           blackListedEntities: null,
           gameWebsite: GameSettings.instance.GameWebsiteUrl);
    }
    #endregion
}
