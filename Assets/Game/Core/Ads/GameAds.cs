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
        INTERSTITIAL
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

    private const string INTERSTITIAL_RUNS_KEY = "GameAds.InterstitialRuns";

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
                GetAdProvider().Init(ageProtection);
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
        GetAdProvider().StopWaitingToPlayAnAd();
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
    #region interstitial

    public bool ShouldShowInterstitial(bool advanceCountIfTrue = true)
    {
        bool ret = false;

        bool isValidProfile = false;
        // Check profiles
        List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList("");
        int max = defs.Count;
        for (int i = 0; i < max && !isValidProfile; i++)
        {
            if (defs[i].GetAsBool("check"))
            {
                string sku = defs[i].Get("sku");
                switch (sku)
                {
                    case "hacker":
                        isValidProfile = UsersManager.currentUser.isBadUser;
                        break;
                    case "non_payer":
                        int progress = UsersManager.currentUser.GetPlayerProgress();
                        int progressCheck = defs[i].GetAsInt("params");
                        if (progress >= progressCheck)
                        {
                            TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
                            int totalPurchases = (trackingPersistence == null) ? 0 : trackingPersistence.TotalPurchases;
                            isValidProfile = totalPurchases <= 0;
                        }
                        break;
                    case "unnoficial":
#if UNITY_EDITOR                        
                        isValidProfile = !DeviceUtilsManager.SharedInstance.CheckIsAppFromStore();
#endif
                        break;
                }
            }
        }

        // if one profile is valid check number of runs
        if ( isValidProfile )
        {
            int runs = 0;
            if (PlayerPrefs.HasKey(INTERSTITIAL_RUNS_KEY))
            {
                runs = PlayerPrefs.GetInt(INTERSTITIAL_RUNS_KEY);
            }
            else
            {
                runs = Random.Range(1,3);
            }

            if ( runs <= 0 )
            {
                ret = true;
            }
            // if we play an insterstitial reduce number of runs?
            if (advanceCountIfTrue)
                ReduceInterstitialRuns();
        }
        return ret;
    }

    public void ReduceInterstitialRuns()
    {
        int runs = 0;
        if (PlayerPrefs.HasKey(INTERSTITIAL_RUNS_KEY))
        {
            runs = PlayerPrefs.GetInt(INTERSTITIAL_RUNS_KEY);
        }
        runs--;
        if ( runs < 0 )
        {
            runs = Random.Range(1, 3);
        }
        PlayerPrefs.SetInt(INTERSTITIAL_RUNS_KEY, runs);
    }

    #endregion
}
