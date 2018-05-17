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

				string interstitialId = "";
				string rewardId = "";
				bool isPhone = !MiscUtils.IsDeviceTablet( FeatureSettingsManager.m_OriginalScreenWidth, FeatureSettingsManager.m_OriginalScreenHeight, Screen.dpi );
				if ( UnityEngine.Debug.isDebugBuild )
				{
					if (Application.platform == RuntimePlatform.Android) {
						if (isPhone) {
							rewardId = "242e5f30622549f0ae85de0921842b71";
							interstitialId = "af85208c87c746e49cb88646d60a11f9";
						}else{
							rewardId = "b83b87dab28b42a98abace9b7a3a8c15";
							interstitialId = "03a11207d6ff4e70a11ddaf63b12ef8a";
						}
					} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
						if (isPhone) {
							rewardId = "5e6b8e4e20004d2c97c8f3ffd0ed97e2";
							interstitialId = "c3c79080175c42da94013bccf8b0c9a2"; // WRONG CONFIGURATION. Ask Alexis Rosa to check it!!!!
						}else{
                            rewardId = "3ee1afec3ef5468ab65d65e3dc85025a";
                            interstitialId  = "1ff446d39b244c25ab7fa62638d592fb";                            
						}
					}
				}
				else
				{
					if (Application.platform == RuntimePlatform.Android) {
						if (isPhone) {
							rewardId = "f572e1fc37274ac6a8eb45bfecdd65c8";
							interstitialId = "a45750f42c864575b54afeb96a408818";
						}else{
							rewardId = "4248c1e6ec54409e9935b4ae08887e2b";
							interstitialId = "5bbda1a7d3634cbf9281e6f2104c9134";
						}
					} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
						if (isPhone) {
							rewardId = "0d4a0085682948748fc8b162e8cbbae8";
							interstitialId = "bf6b94c26863449e9347369bdb31362e";
						}else{
							rewardId = "31188271f9ab404cb2346472a9e93d8d";
							interstitialId = "203f4feba4104e88a400519d731589ed";
						}
					}
				}

				CurrentAdPurpose = EAdPurpose.NONE;
				CurrentAdStartTimestamp = 0f;

                // TODO: Validate all interstitialId values are configurated correctly (Ask Juan how to validate configuration for these ids)
				MopubAdsManager.SharedInstance.Init (interstitialId, false, rewardId, true, 30);
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
		MopubAdsManager.SharedInstance.PlayNotRewarded(OnInsterstitialResult, 5);
	}

	private void OnInsterstitialResult(MopubAdsManager.EPlayResult result)
	{
		if (m_onInterstitialCallback != null){
			m_onInterstitialCallback(result == MopubAdsManager.EPlayResult.PLAYED );	
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
		MopubAdsManager.SharedInstance.PlayRewarded(OnRewardedResult, 5);
	}

	private void OnRewardedResult(MopubAdsManager.EPlayResult result)
	{        
        if ( m_onRewardedCallback != null ){
			m_onRewardedCallback(result == MopubAdsManager.EPlayResult.PLAYED );
			m_onRewardedCallback = null;
		}

        // Ad has been finished is tracked
		bool videoWatched = result == MopubAdsManager.EPlayResult.PLAYED;
        int duration = (int)(Time.unscaledTime - CurrentAdStartTimestamp);
        HDTrackingManager.Instance.Notify_AdFinished(Track_EAdPurposeToAdType(CurrentAdPurpose), videoWatched, false, duration, TRACK_AD_PROVIDER_ID);

        CurrentAdPurpose = EAdPurpose.NONE;
    }


    public bool IsWaitingToPlayAnAd()
    {
    	return MopubAdsManager.SharedInstance.IsWaitingToPlayAVideo();
    }

    public void StopWaitingToPlayAnAd()
    {
    	MopubAdsManager.SharedInstance.StopWaitingToPlayAVideo();
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
