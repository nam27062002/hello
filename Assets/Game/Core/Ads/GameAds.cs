using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAds : UbiBCN.SingletonMonoBehaviour<GameAds> {

    public enum EAdPurpose
    {
        NONE,
        REVIVE,
        UPGRADE_MAP,
        REMOVE_MISSION,
		SKIP_MISSION_COOLDOWN,
        EVENT_SCORE_X2
    };

	public static bool adsAvailable {
		get { return Application.internetReachability != NetworkReachability.NotReachable
				  && FeatureSettingsManager.AreAdsEnabled;
		}
	}
	
    private EAdPurpose CurrentAdPurpose { get; set; }

    public delegate void OnPlayVideoCallback(bool giveReward);
    protected OnPlayVideoCallback m_onInterstitialCallback;
    protected OnPlayVideoCallback m_onRewardedCallback;

    private bool IsInited { get; set; }

    private AdProvider m_adProvider;

    private AdProvider GetAdProvider()
    {
        if (m_adProvider == null)
        {
#if MOPUB_SDK_ENABLED
            m_adProvider = new AdProviderMopub();
#elif IRONSOURCE_SDK_ENABLED
            m_adProvider = new AdProviderIronSource();
#else
            m_adProvider = new AdProviderDummy();
#endif
        }

        return m_adProvider;
    }

    public void Init() {
		if (FeatureSettingsManager.AreAdsEnabled) {
			if (!IsInited)  {
				IsInited = true;				

				CurrentAdPurpose = EAdPurpose.NONE;				

                // Age protection disabled by default
                // TODO: true if the user protection should be used
                GetAdProvider().Init(false);                
			}
        }
	}

	public void ShowInterstitial(OnPlayVideoCallback callback)
	{
		// If ads are not available, return immediately
		if(!adsAvailable) {
            if (FeatureSettingsManager.IsDebugEnabled)
                AdProvider.Log("ShowInterstitial can't be performed because there's no ad available");

            // Notify of the error
            if (callback != null) {
				callback.Invoke(false);
			}
			return;
		}

        m_onInterstitialCallback = callback;

		if (FeatureSettingsManager.IsDebugEnabled)
			AdProvider.Log("ShowInterstitial processing...");

        GetAdProvider().ShowInterstitial(onShowInterstitial);       
	}	

    private void onShowInterstitial(bool giveReward, int duration, string msg)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            AdProvider.Log("onShowInterstitial success = " +giveReward + " duration = " + duration + " msg = " + msg);

        if (m_onInterstitialCallback != null)
        {
            m_onInterstitialCallback(giveReward);
            m_onInterstitialCallback = null;
        }
    }

	public void ShowRewarded(EAdPurpose adPurpose, OnPlayVideoCallback callback)
	{
		// If ads are not available, return immediately
		if(!adsAvailable) {
            if (FeatureSettingsManager.IsDebugEnabled)
                AdProvider.Log("ShowRewarded can't be performed because there's no ad available");

            // Notify of the error
            if (callback != null) {
				callback.Invoke(false);
			}
			return;
		}

		// Store setup
		CurrentAdPurpose = adPurpose;
        m_onRewardedCallback = callback;

        AdProvider adProvider = GetAdProvider();

		// Ad has been requested is tracked
        HDTrackingManager.Instance.Notify_AdStarted(Track_EAdPurposeToAdType(adPurpose), Track_EAdPurposeToRewardType(adPurpose), true, adProvider.GetId());

        if (FeatureSettingsManager.IsDebugEnabled)
            AdProvider.Log("ShowRewarded processing...");

        // Request Ad
        adProvider.ShowRewarded(OnShowRewarded);		
	}	

    private void OnShowRewarded(bool giveReward, int duration, string msg)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            AdProvider.Log("onShowRewarded success = " + giveReward + " duration = " + duration + " msg = " + msg);

        HDTrackingManager.Instance.Notify_AdFinished(Track_EAdPurposeToAdType(CurrentAdPurpose), giveReward, false, duration, GetAdProvider().GetId());

        CurrentAdPurpose = EAdPurpose.NONE;

		if (m_onRewardedCallback != null) 
		{
			m_onRewardedCallback (giveReward);
			m_onRewardedCallback = null;
		}
    }

    public bool IsWaitingToPlayAnAd()
    {
        return GetAdProvider().IsWaitingToPlayAnAnd();    	
    }

    public void StopWaitingToPlayAnAd()
    {
    	MopubAdsManager.SharedInstance.StopWaitingToPlayAVideo();
    }

	public void ShowDebugInfo()
	{
		GetAdProvider().ShowDebugInfo();
	}

#region track    
    private string Track_EAdPurposeToAdType(EAdPurpose adPurpose)
    {
        return adPurpose.ToString();
    }

    private string Track_EAdPurposeToRewardType(EAdPurpose adPurpose)
    {
        // Same string is sent for RewardType and AdType
        return Track_EAdPurposeToAdType(adPurpose);
    }
#endregion
}
