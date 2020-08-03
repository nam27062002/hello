#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using UnityEngine;
using System.Diagnostics;

/// <summary>
/// This class is responsible for hiding the ad provider implementation
/// </summary>
public abstract class AdProvider
{
    public enum AdType
    {
        None,
        V4VC,
        Interstitial
    }

    public delegate void VoidDelegate();
    public VoidDelegate onVideoAdOpen;
    public VoidDelegate onVideoAdClosed;

    public delegate void OnPlayVideoCallback(bool giveReward, int duration, string msg);        

    public class Ad
    {
        public AdType Type { get; set; }
        public OnPlayVideoCallback Callback { get; set; }
        public int Timeout { get; set; }
        private float CurrentAdStartTimestamp { get; set; }        

        public Ad()
        {
            Clear();
        }

        public void Clear()
        {
            Type = AdType.None;
            Callback = null;            
            CurrentAdStartTimestamp = 0f;
            Timeout = 0;
        }

        public void Setup(AdType type, OnPlayVideoCallback callback, int timeout = 1)
        {
            Clear();
            Type = type;
            Callback = callback;
            Timeout = timeout;
        }
        
        public void Play()
        {
            CurrentAdStartTimestamp = Time.unscaledTime;
        }

        public void OnPlayed(bool videoPlayed, string msg)
        {
            if (Callback != null)
            {
                Callback(videoPlayed, GetDuration(), msg);
            }            
        }

        private int GetDuration()
        {
            return (int)(Time.unscaledTime - CurrentAdStartTimestamp);
        }
    }

    protected Ad m_ad;

    public string GetInfo()
    {
        return GetId() + " " + ExtendedGetInfo();
    }

    protected virtual string ExtendedGetInfo()
    {
        return "";
    }

    public void Init(bool useAgeProtection, bool consentRestriction)
    {        
        m_ad = new Ad();

        ExtendedInit(useAgeProtection, consentRestriction);
    }

    protected virtual void ExtendedInit(bool useAgeProtection, bool consentRestriction) {}

    public void ShowInterstitial(OnPlayVideoCallback callback)
    {
        if (IsProcessingAnAd())
        {           
            callback(false, 0, "An ad is already in process, skipping this new petition");
            return;            
        }

        m_ad.Setup(AdType.Interstitial, callback, FeatureSettingsManager.instance.GetAdTimeout()); 
		m_ad.Play();
        ExtendedShowInterstitial();
    }

    protected virtual void ExtendedShowInterstitial() { }

    protected void OnShowInterstitial(bool videoPlayed, string msg=null)
    {        
	    Log("Interstitial played = " + videoPlayed + " msg = " + msg + " m_ad.Type = " + m_ad.Type);        

        if (m_ad.Type == AdType.Interstitial)
        {
            OnAdPlayed(m_ad, videoPlayed, msg);
        }
    }

    public void ShowRewarded(OnPlayVideoCallback callback)
    {
        if (IsProcessingAnAd())
        {
            callback(false, 0, "An ad is already in process, skipping this new petition");
            return;
        }

        m_ad.Setup(AdType.V4VC, callback, FeatureSettingsManager.instance.GetAdTimeout());
		m_ad.Play();
        ExtendedShowRewarded();
    }   

    protected virtual void ExtendedShowRewarded() { }

    protected void OnShowRewarded(bool videoPlayed, string msg=null)
    {        
		Log("Rewarded played = " + videoPlayed + " msg = " + msg + " m_ad.Type = " + m_ad.Type);        

        if (m_ad.Type == AdType.V4VC)
        {
            OnAdPlayed(m_ad, videoPlayed, msg);
        }        
    }    
    
    private bool IsProcessingAnAd()
    {
        return m_ad.Type != AdType.None;        
    }
    
    public AdType GetAdType()
    {
        return m_ad.Type;
    }

    public void OnAdPlayed(Ad ad, bool videoPlayed, string msg=null)
    {        
        Log("Ad type " + ad.Type + " played with success = " + videoPlayed + " msg = " + msg);        

        if (ad == m_ad)
        {
            m_ad.OnPlayed(videoPlayed, msg);
            m_ad.Clear();
        }
    }    

    public abstract string GetId();

    public virtual bool IsWaitingToPlayAnAnd() { return false; }

    public virtual void StopWaitingToPlayAnAd() {}

	public virtual void ShowDebugInfo() {}

    #region log
    private const bool LOG_USE_COLOR = true;
    private const string LOG_CHANNEL = "[AdProvider] ";
    private const string LOG_CHANNEL_COLOR = "<color=cyan>" + LOG_CHANNEL;

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string msg)
    {
        if (LOG_USE_COLOR)
        {
            Debug.Log(LOG_CHANNEL_COLOR + msg + " </color>");
        }
        else
        {
            Debug.Log(LOG_CHANNEL + msg);
        }
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
