﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAds : Singleton<GameAds>, IBroadcastListener {

    public enum EAdPurpose
    {
        NONE,
        REVIVE,
        UPGRADE_MAP,
        REMOVE_MISSION,
		SKIP_MISSION_COOLDOWN,
        EVENT_SCORE_X2,
        INTERSTITIAL,
		DAILY_REWARD_DOUBLE,
		FREE_OFFER_PACK,
        RUN_REWARD_MULTIPLIER
    };

    private const string INTERSTITIAL_RUNS_KEY = "GameAds.InterstitialRuns";
    private const string RUNS_WITHOUT_ADS_KEY = "GameAds.RunsWithoutAds";
    private const string DEFAULT_SETTINGS_SKU = "defaultAdsSettings";

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

    private DefinitionNode m_interstitialSettings = null;

    private AdProvider m_adProvider;

    private AdProvider GetAdProvider()
    {
        if (m_adProvider == null)
        {
#if UNITY_EDITOR
			m_adProvider = new AdProviderDummy();
#elif MOPUB_SDK_ENABLED
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
            // If settings not initialized, do it now
            if(m_interstitialSettings == null) {
                LoadSettingsForCluster(UsersManager.currentUser.GetClusterId());
			}

            int minRuns = m_interstitialSettings.GetAsInt("runsToStart");
            if (UsersManager.currentUser.gamesPlayed >= minRuns)
            {
                bool cleanCounter = true;
                bool checkHacker = m_interstitialSettings.GetAsBool("checkHacker");
                if ( checkHacker &&  UsersManager.currentUser.isHacker)
                {
                    ret = true;
                }
                else
                {
                    TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
                    int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
                    long lastPurchaseTimestamp = (trackingPersistence == null) ? 0 : trackingPersistence.LastPurchaseTimestamp * 1000;  // to milliseconds
                    long timestamp = GameServerManager.GetEstimatedServerTimeAsLong();
                    long timeNoPaying = m_interstitialSettings.GetAsLong("daysNoPaying") * 24 * 60 * 60 * 1000; // to milliseconds
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
        // If settings not initialized, do it now
        if(m_interstitialSettings == null) {
            LoadSettingsForCluster(UsersManager.currentUser.GetClusterId());
        }

        int intersitialRuns = m_interstitialSettings.GetAsInt("interstitialRuns");
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

    /// <summary>
    /// Load the interstitial ad settings corresponding to the given cluster.
    /// </summary>
    /// <param name="_clusterId">The cluster whose settings we want.</param>
    private void LoadSettingsForCluster(string _clusterId) {
        // Get all settings definitions
        List<DefinitionNode> settingsDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.INTERSTITIALS_SETUP);
        bool found = false;
        for(int i = 0; i < settingsDefs.Count; ++i) {
            // Is this setting valid for this cluster?
            List<string> clusterIds = settingsDefs[i].GetAsList<string>("clusterIds");
            if(clusterIds.Contains(_clusterId)) {
                // Yes! store it and break the loop
                m_interstitialSettings = settingsDefs[i];
                found = true;
                break;
			}
		}

        // If we couldn't find any settings for the given cluster, use default
        if(!found) {
            // Default settings
            m_interstitialSettings = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.INTERSTITIALS_SETUP, DEFAULT_SETTINGS_SKU);
        }
    }

    #endregion

    #region callbacks
    /// <summary>
    /// An event has been received.
    /// </summary>
    /// <param name="_eventType"></param>
    /// <param name="_eventData"></param>
    public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _eventData) {
        switch(_eventType) {
            case BroadcastEventType.CLUSTER_ID_ASSIGNED: {
                // Load interstitial settings for the new cluster
                LoadSettingsForCluster((_eventData as ClusterIdEventInfo).clusterId);
			} break;
		}
	}
	#endregion
}
