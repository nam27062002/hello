using System;
using UnityEngine;

public static class Globals
{
#if !PRODUCTION
    private static string s_debugOverrideVersion = null;

    public static string debugOverrideVersion
    {
        get { return s_debugOverrideVersion; }
        set { s_debugOverrideVersion = value; }
    }
#endif
    
    public enum Platform
    {
        iOS,
        Android,
        Amazon,
        Unknown
    }   

    public static Platform GetPlatform()
    {
        Platform platform = Platform.Unknown;

#if UNITY_IPHONE
        platform = Platform.iOS;
#elif UNITY_ANDROID
#if AMAZON
        platform = Platform.Amazon;
#else
        platform = Platform.Android;
#endif
#else
        Debug.LogWarning("HSXServer :: Unknown platform being played: " + Application.platform.ToString());
#endif

        return platform;
    }

    public static string GetApplicationVersion()
    {        
        string version = FGOL.Plugins.Native.NativeBinding.Instance.GetBundleVersion();
#if !PRODUCTION
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            if (!string.IsNullOrEmpty(s_debugOverrideVersion))
            {
                version = s_debugOverrideVersion;
            }
        }
#endif
        return version;
    }

    public static long GetUnixTimestamp()
    {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
    }

	public static DateTime GetDateFromUnixTimestamp(long timestamp)
	{
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
	}
}