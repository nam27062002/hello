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
        EVENT_SCORE_X2,
        INTERSTITIAL,
		DAILY_REWARD_DOUBLE
    };

	public static bool adsAvailable {
		get { return DeviceUtilsManager.SharedInstance.internetReachability != NetworkReachability.NotReachable
                  && FeatureSettingsManager.AreAdsEnabled && DebugSettings.areAdsEnabled;
		}
	}
	
    private EAdPurpose CurrentAdPurpose { get; set; }

    public delegate void OnPlayVideoCallback(bool giveReward);
    protected OnPlayVideoCallback m_onInterstitialCallback;
    protected OnPlayVideoCallback m_onRewardedCallback;

    private bool IsInited { get; set; }

    private AdProvider m_adProvider;

    private const string INTERSTITIAL_RUNS_KEY = "GameAds.InterstitialRuns";
    private const string RUNS_WITHOUT_ADS_KEY = "GameAds.RunsWithoutAds";

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
            m_adProvider.onVideoAdOpen += onVideoOpen;
            m_adProvider.onVideoAdClosed += onVideoClosed;
        }

        return m_adProvider;
    }

    public string GetInfo() {
        return GetAdProvider().GetInfo();
    }
    
    public AdProvider.AdType GetAdType()
    {
        return GetAdProvider().GetAdType();
    }

    public void Init() {
		if (FeatureSettingsManager.AreAdsEnabled) {
			if (!IsInited)  {
				IsInited = true;				

				CurrentAdPurpose = EAdPurpose.NONE;

                // Age protection disabled by default
                // true if the user protection should be used
                bool ageProtection = GDPRManager.SharedInstance.IsAgeRestrictionEnabled() || GDPRManager.SharedInstance.IsConsentRestrictionEnabled();
                GetAdProvider().Init(ageProtection, GDPRManager.SharedInstance.IsConsentRestrictionEnabled());
			}
        }
	}

    public void onVideoOpen()
    {
        AudioListener.volume = 0;    
    }
    public void onVideoClosed()
    {
        AudioListener.volume = 1;
    }

	public void ShowInterstitial(OnPlayVideoCallback callback)
	{
		// If ads are not available, return immediately
		if(!adsAvailable) {            
            AdProvider.Log("ShowInterstitial can't be performed because there's no ad available");

            // Notify of the error
            if (callback != null) {
				callback.Invoke(false);
			}
			return;
		}

        m_onInterstitialCallback = callback;
		
	    AdProvider.Log("ShowInterstitial processing...");

        AdProvider adProvider = GetAdProvider();        
        CurrentAdPurpose = EAdPurpose.INTERSTITIAL;

        // Ad has been requested is tracked
        HDTrackingManager.Instance.Notify_AdStarted(false, Track_EAdPurposeToAdType(CurrentAdPurpose), Track_EAdPurposeToRewardType(CurrentAdPurpose), true, adProvider.GetId());

        adProvider.ShowInterstitial(onShowInterstitial);       
	}	

    private void onShowInterstitial(bool giveReward, int duration, string msg)
    {        
        AdProvider.Log("onShowInterstitial success = " +giveReward + " duration = " + duration + " msg = " + msg);

        HDTrackingManager.Instance.Notify_AdFinished(false, Track_EAdPurposeToAdType(CurrentAdPurpose), giveReward, false, duration, GetAdProvider().GetId());

        if ( giveReward ){
            PlayerPrefs.SetInt(RUNS_WITHOUT_ADS_KEY, 0);
        }
        
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
        HDTrackingManager.Instance.Notify_AdStarted(true, Track_EAdPurposeToAdType(adPurpose), Track_EAdPurposeToRewardType(adPurpose), true, adProvider.GetId());
        
        AdProvider.Log("ShowRewarded processing...");

        // Request Ad
        adProvider.ShowRewarded(OnShowRewarded);		
	}	

    private void OnShowRewarded(bool giveReward, int duration, string msg)
    {
        AdProvider.Log("onShowRewarded success = " + giveReward + " duration = " + duration + " msg = " + msg);

        HDTrackingManager.Instance.Notify_AdFinished(true, Track_EAdPurposeToAdType(CurrentAdPurpose), giveReward, false, duration, GetAdProvider().GetId());

        CurrentAdPurpose = EAdPurpose.NONE;
        
        if ( giveReward )
        {
            PlayerPrefs.SetInt(RUNS_WITHOUT_ADS_KEY, 0);
        }

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
        GetAdProvider().StopWaitingToPlayAnAd();
    }

	public void ShowDebugInfo()
	{
		GetAdProvider().ShowDebugInfo();
	}

    #region track    
    private string Track_EAdPurposeToAdType(EAdPurpose adPurpose)
    {
        return (adPurpose == EAdPurpose.INTERSTITIAL) ? "Display_Video" : adPurpose.ToString();        
    }

    private string Track_EAdPurposeToRewardType(EAdPurpose adPurpose)
    {
        // Must be empty for interstitial videos and the same as AdType for rewarded videos.
        return (adPurpose == EAdPurpose.INTERSTITIAL) ? "" : Track_EAdPurposeToAdType(adPurpose);
    }
    #endregion

    #region interstitial

    public bool IsValidUserForInterstitials()
    {
        // this variable will say if the player is a target of the interstitial system
        bool ret = false;
        if (FeatureSettingsManager.AreAdsEnabled)
        {
            DefinitionNode interstitialsSetup = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.INTERSTITIALS_SETUP, "intersitialsSetup");
            int minRuns = interstitialsSetup.GetAsInt("runsToStart");
            if (UsersManager.currentUser.gamesPlayed >= minRuns)
            {
                bool cleanCounter = true;
                bool checkHacker = interstitialsSetup.GetAsBool("checkHacker");
                if ( checkHacker &&  UsersManager.currentUser.isHacker)
                {
                    ret = true;
                }
                else
                {
                    TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
                    int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
                    long lastPurchaseTimestamp = (trackingPersistence == null) ? 0 : trackingPersistence.LastPurchaseTimestamp * 1000;  // to milliseconds
                    long timestamp = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
                    long timeNoPaying = interstitialsSetup.GetAsLong("daysNoPaying") * 24 * 60 * 60 * 1000; // to milliseconds
                    if ( totalPurchases <= 0 || timestamp - lastPurchaseTimestamp > timeNoPaying )
                    {
                        cleanCounter = false;
                        int runs = PlayerPrefs.GetInt(INTERSTITIAL_RUNS_KEY, 0);
                        ret = runs <= 0;
                    }
                }
                if (cleanCounter)
                {
                    PlayerPrefs.SetInt(INTERSTITIAL_RUNS_KEY, 0);
                    PlayerPrefs.Save();
                }
            }
        }
        
        return ret;
    }

    public void ResetIntersitialCounter()
    {
        DefinitionNode intertitialsSetup = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.INTERSTITIALS_SETUP, "intersitialsSetup");
        int intersitialRuns = intertitialsSetup.GetAsInt("interstitialRuns");
        PlayerPrefs.SetInt(INTERSTITIAL_RUNS_KEY, intersitialRuns);
        PlayerPrefs.Save();
    }

    public void ReduceRunsToInterstitial()
    {
        int runs = 0;
        runs = PlayerPrefs.GetInt(INTERSTITIAL_RUNS_KEY, 0);
        runs--;
        PlayerPrefs.SetInt(INTERSTITIAL_RUNS_KEY, runs);
        PlayerPrefs.Save();
    }

    #endregion
}
