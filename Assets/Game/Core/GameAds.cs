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
		RESULTS_GET_KEY
    };

	public static bool adsAvailable {
		get { return Application.internetReachability != NetworkReachability.NotReachable
				  && FeatureSettingsManager.AreAdsEnabled;
		}
	}

	public delegate void OnPlayVideoCallback(bool giveReward);
	private OnPlayVideoCallback m_onInterstitialCallback;
	private OnPlayVideoCallback m_onRewardedCallback;
    private EAdPurpose CurrentAdPurpose { get; set; }
    private float CurrentAdStartTimestamp { get; set; }

	private bool IsInited { get; set; }

    public void Init() {
		if (FeatureSettingsManager.AreAdsEnabled) {
			if (!IsInited)  {
				IsInited = true;

				string interstitialId = "af85208c87c746e49cb88646d60a11f9";
				string rewardId = "242e5f30622549f0ae85de0921842b71";
				bool isPhone = true;
				// TODO: Check if tablet
				if (Application.platform == RuntimePlatform.Android) {
					if (isPhone) {
						interstitialId = "af85208c87c746e49cb88646d60a11f9";
						rewardId = "242e5f30622549f0ae85de0921842b71";
					}
				} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
					if (isPhone) {
						interstitialId = "c3c79080175c42da94013bccf8b0c9a2";
						rewardId = "5e6b8e4e20004d2c97c8f3ffd0ed97e2";
					}
				}

				CurrentAdPurpose = EAdPurpose.NONE;
				CurrentAdStartTimestamp = 0f;
				AdsManager.SharedInstance.Init (interstitialId, rewardId, true, 30);
			}
        }
	}

	public void ShowInterstitial(OnPlayVideoCallback callback)
	{
		// If ads are not available, return immediately
		if(!adsAvailable) {
			// Notify of the error
			if(callback != null) {
				callback.Invoke(false);
			}
			return;
		}

        m_onInterstitialCallback = callback;
		AdsManager.SharedInstance.PlayNotRewarded(OnInsterstitialResult, 5);
	}

	private void OnInsterstitialResult(AdsManager.EPlayResult result)
	{
		if (m_onInterstitialCallback != null){
			m_onInterstitialCallback(result == AdsManager.EPlayResult.PLAYED );	
			m_onInterstitialCallback = null;
		}
	}

	public void ShowRewarded(EAdPurpose adPurpose, OnPlayVideoCallback callback)
	{
		// If ads are not available, return immediately
		if(!adsAvailable) {
			// Notify of the error
			if(callback != null) {
				callback.Invoke(false);
			}
			return;
		}

		// Store setup
		CurrentAdPurpose = adPurpose;
		CurrentAdStartTimestamp = Time.unscaledTime;
		m_onRewardedCallback = callback;

		// Ad has been requested is tracked
        HDTrackingManager.Instance.Notify_AdStarted(Track_EAdPurposeToAdType(adPurpose), Track_EAdPurposeToRewardType(adPurpose), true, TRACK_AD_PROVIDER_ID);

		// Request Ad
		AdsManager.SharedInstance.PlayRewarded(OnRewardedResult, 5);
	}

	private void OnRewardedResult(AdsManager.EPlayResult result)
	{        
        if ( m_onRewardedCallback != null ){
			m_onRewardedCallback(result == AdsManager.EPlayResult.PLAYED );
			m_onRewardedCallback = null;
		}

        // Ad has been finished is tracked
        bool videoWatched = result == AdsManager.EPlayResult.PLAYED;
        int duration = (int)(Time.unscaledTime - CurrentAdStartTimestamp);
        HDTrackingManager.Instance.Notify_AdFinished(Track_EAdPurposeToAdType(CurrentAdPurpose), videoWatched, false, duration, TRACK_AD_PROVIDER_ID);

        CurrentAdPurpose = EAdPurpose.NONE;
    }

    #region track
    private const string TRACK_AD_PROVIDER_ID = "MoPub";

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
