#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

#define LOG_USE_COLOR

using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for managing app flavours. Following Android Studio approach (https://developer.android.com/studio/build/build-variants)
/// for handling build variants our game can have different flavours. Only one flavour can be applied at a time.
/// </summary>
public class FlavourManager
{
    private static FlavourManager s_instance = null;

    public static FlavourManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new FlavourManager();
            }

            return s_instance;
        }
    }

    private Flavour m_currentFlavour;

    private FlavourFactory m_factory;

    private FlavourManager()
    {
        m_factory = new FlavourFactory();
    }

    /// <summary>
    /// Returns the flavour currently applied
    /// </summary>
    /// <returns></returns>
    public Flavour GetCurrentFlavour()
    {
        if (m_currentFlavour == null)
        {
            m_currentFlavour = m_factory.CreateFlavour();
            SetupCurrentFlavour();            
        }

        return m_currentFlavour;
    }

    private FlavourFactory.Setting_EDevicePlatform GetDevicePlatform()
    {
        FlavourFactory.Setting_EDevicePlatform devicePlatform;

#if UNITY_ANDROID
        devicePlatform = FlavourFactory.Setting_EDevicePlatform.Android;
#else
        devicePlatform = FlavourFactory.Setting_EDevicePlatform.iOS;
#endif

#if UNITY_EDITOR
        // Cheat to override devicePlatform. Used to be able to test flavours that depend on device platform without switching platforms
        string value = Prefs_GetDevicePlatform();
        if (value == FlavourFactory.SETTING_EDEVICEPLATFORM_IOS)
        {
            devicePlatform = FlavourFactory.Setting_EDevicePlatform.iOS;
        }
        else if (value == FlavourFactory.SETTING_EDEVICEPLATFORM_ANDROID)
        {
            devicePlatform = FlavourFactory.Setting_EDevicePlatform.Android;
        }
#endif

        return devicePlatform;
    }

    private void SetupCurrentFlavour()
    {
        string countryCode = PlatformUtils.Instance.Country_GetCodeOnInstall();
       
        // The flavour to apply depends on the country code that the device had when the user installed the game.
        // We want the user to stick to the same flavour from installation on
        m_factory.SetupFlavourBasedOnCriteria(m_currentFlavour, countryCode, GetDevicePlatform());
    }

    /// <summary>
    /// Sets <c>flavourSku</c> as the current flavour. This method should be used only as a cheat
    /// </summary>
    /// <param name="flavourSku"></param>
    public void SetSkuAsCurrentFlavour(string flavourSku)
    {
        if (FeatureSettingsManager.AreCheatsEnabled)
        {
            if (m_currentFlavour != null && m_currentFlavour.Sku != flavourSku)
            {
                string countryCode = FlavourSkuToCountryCode(flavourSku);
                PersistencePrefs.CountryCodeOnInstall = countryCode;
                SetupCurrentFlavour();
            }
        }
    }

    private string FlavourSkuToCountryCode(string flavourSku)
    {
        // flavourSku is the same as the country code except for WW
        return (string.IsNullOrEmpty(flavourSku) || flavourSku == FlavourFactory.CATALOG_SKU_WW) ? PlatformUtils.COUNTRY_CODE_WW_DEFAULT : flavourSku;
    }    

    public List<string> GetFlavourSkus()
    {
        return m_factory.Catalog_GetSkus();
    }

#region prefs
#if UNITY_EDITOR
    private const string PREFS_CHEAT_DEVICE_PLATFORM = "cheat.devicePlatform";

    public static void Prefs_SetDevicePlatform(string value)
    {
        UnityEngine.PlayerPrefs.SetString(PREFS_CHEAT_DEVICE_PLATFORM, value);
    }

    public static string Prefs_GetDevicePlatform()
    {
        return UnityEngine.PlayerPrefs.GetString(PREFS_CHEAT_DEVICE_PLATFORM);
    }
#endif
#endregion

#region log
    private const string LOG_CHANNEL = "[Flavours] ";
    private const string LOG_CHANNEL_COLOR = "<color=teal>" + LOG_CHANNEL;

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string msg)
    {
#if LOG_USE_COLOR
        // Multiline texts don't work well with color tags in the console, so do some tricks :P
        int idx = msg.IndexOf('\n');
        if (idx >= 0)
        {
            Debug.Log(
                LOG_CHANNEL_COLOR +
                msg.Substring(0, idx) +
                "</color>" +
                msg.Substring(idx, msg.Length - idx)
            );
        }
        else
        {
            Debug.Log(LOG_CHANNEL_COLOR + msg + " </color>");
        }
#else
        Debug.Log(LOG_CHANNEL + msg);        
#endif
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }
#endregion
}
