using System.Collections.Generic;
using UnityEngine;

public abstract class PlatformUtils 
{
	// public delegate void OnTokens(bool success, Dictionary<string,string> tokens);
	// public static OnTokens onTokens;

	private static PlatformUtils instance;

	public static PlatformUtils Instance
	{
		get{
			if(instance == null) {
#if UNITY_IPHONE
				instance = new PlatformUtilsIOSImpl();
#elif UNITY_ANDROID
				instance = new PlatformUtilsAndroidImpl();
#else
				instance = new PlatformUtilsDummyImpl();
#endif
			}
			return instance;
		}
	}
	
	public abstract void GetTokens();
	public abstract void ShareImage(string filename, string caption);
	public abstract void MakeToast(string text, bool longDuration);
	public abstract string GetTrackingId();
	public abstract string FormatPrice( float price, string currencyLocale );
	
	
	public virtual void askPermissions(){}
	public virtual bool arePermissionsGranted(){ return true; }
	
	public virtual long getFreeDataSpaceAvailable(){ return 0; }
	
	// Replaces Social.ReportProgress in iOS because it doesn't work 
	public virtual void ReportProgress( string achievementId, double progress) {}

	public virtual string[] GetCommandLineArgs(){ return System.Environment.GetCommandLineArgs(); }

	public virtual bool InputPressureSupprted(){ return Input.touchPressureSupported; }

    public virtual bool ApplicationExists(string applicationURI){ return false; }

    // -----------------------------------------------------------------------------------------------------------------------

    // -----------------------------------------------------------------------------------------------------------------------
    #region country
    public const string COUNTRY_CODE_WW_DEFAULT = "US"; // United States is used as the default country code for WW flavor
    public const string COUNTRY_CODE_CHINA = "CN";
    public const string COUNTRY_CODE_KOREA = "KR";

    public static List<string> COUNTRY_CODES = new List<string>()
    {
        COUNTRY_CODE_WW_DEFAULT,
        COUNTRY_CODE_CHINA,
        COUNTRY_CODE_KOREA
    };

    public bool IsChina()
    {
        string countryCode = Country_GetCurrentCode();
        if (countryCode != null)
        {
            countryCode.ToUpper();
        }

        return countryCode == COUNTRY_CODE_CHINA;
    }

	public bool IsKorea() {
		string countryCode = Country_GetCurrentCode();
		if(countryCode != null) {
			countryCode.ToUpper();
		}

		return countryCode == COUNTRY_CODE_KOREA;
	}

    /// <summary>
    /// Returns the code of the country currently set
    /// </summary>    
    public abstract string Country_GetCurrentCode();

    /// <summary>
    /// Returns the country code when the app was installed. App flavor takes into consideration this value instead of
    /// current country code in order to garantee that the flavor stays the same even though the user changes regions
    /// </summary>    
    public string Country_GetCodeOnInstall()
    {
        string returnValue = PersistencePrefs.CountryCodeOnInstall;

        // If it hasn't been stored yet then it needs to be calculated
        if (string.IsNullOrEmpty(returnValue))
        {            
            // Assume that the current country code is the same as the one that the user had when the game was installed
            returnValue = Country_GetCurrentCode();

            returnValue = returnValue.ToUpper();

            // Check social platform to make sure flavor won't change.
            // Before countryCodeOnInstall was implemented we only needed to identify installs made in China. This used to be done
            // by storing "Weibo" in ersistencePrefs.Social_PlatformKey. We need to check this stuff to make the new solution backwards compatible
            string socialPlatformKey = PersistencePrefs.Social_PlatformKey;
            if (socialPlatformKey == SocialUtils.EPlatformToKey(SocialUtils.EPlatform.Weibo))
            {
                // If "Weibo" is stored then it means that the user installed the game in China so we need to use
                // "CN" as country code on install
                returnValue = COUNTRY_CODE_CHINA;
            }
            else if (!string.IsNullOrEmpty(socialPlatformKey) && returnValue == COUNTRY_CODE_CHINA)
            {
                // As the social platform is not "Weibo" then we know that the user wasn't in China when the game was installed
                // so we make sure that the user keeps the WW flavor even though the current country code is China
                returnValue = COUNTRY_CODE_WW_DEFAULT;
            }

            // Store country code on install
            PersistencePrefs.CountryCodeOnInstall = returnValue;
        }

        return returnValue;
    }
    #endregion
    // -----------------------------------------------------------------------------------------------------------------------
}
