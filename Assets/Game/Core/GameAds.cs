using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAds : UbiBCN.SingletonMonoBehaviour<GameAds> {

    public enum EAdPurpose
    {
        NONE,
        REVIVE,
        UPGRADE_MAP,
        REMOVE_MISSION
    };

	public delegate void OnPlayVideoCallback(bool giveReward);
	private OnPlayVideoCallback m_onInterstitialCallback;
	private OnPlayVideoCallback m_onRewardedCallback;
    private EAdPurpose CurrentAdPurpose { get; set; }

	public void Init()
	{
        if (FeatureSettingsManager.AreAdsEnabled)
        {
            string interstitialId = "af85208c87c746e49cb88646d60a11f9";
            string rewardId = "242e5f30622549f0ae85de0921842b71";
            bool isPhone = true;
            // TODO: Check if tablet
            if (Application.platform == RuntimePlatform.Android)
            {
                if (isPhone)
                {
                    interstitialId = "af85208c87c746e49cb88646d60a11f9";
                    rewardId = "242e5f30622549f0ae85de0921842b71";
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (isPhone)
                {
                    interstitialId = "c3c79080175c42da94013bccf8b0c9a2";
                    rewardId = "5e6b8e4e20004d2c97c8f3ffd0ed97e2";
                }
            }

            CurrentAdPurpose = EAdPurpose.NONE;
            AdsManager.SharedInstance.Init(interstitialId, rewardId, true, 30);
        }
	}

	public void ShowInterstitial(OnPlayVideoCallback callback)
	{
        m_onInterstitialCallback = callback;

        if (FeatureSettingsManager.AreAdsEnabled) {            
            AdsManager.SharedInstance.PlayNotRewarded(OnInsterstitialResult, 5);
        } else {
            // 1 second to wait before calling the callback because of an error on the blocker popup that prevents it from being closed when close() is
            // called immediately after open() was called
            UbiBCN.CoroutineManager.DelayedCall(() => OnInsterstitialResult(AdsManager.EPlayResult.TIMEOUT), 1);
        }
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
        CurrentAdPurpose = adPurpose;
		m_onRewardedCallback = callback;

        bool adAvailable = FeatureSettingsManager.AreAdsEnabled;
        // Ad has been requested is tracked
        HDTrackingManager.Instance.Notify_AdStarted(Track_EAdPurposeToAdType(adPurpose), Track_EAdPurposeToRewardType(adPurpose), adAvailable, TRACK_AD_PROVIDER_ID);

        if (adAvailable) {
            // Ad is requested
            AdsManager.SharedInstance.PlayRewarded(OnRewardedResult, 5);
        } else {
            // 1 second to wait before calling the callback because of an error on the blocker popup that prevents the popup from being closed when close() is
            // called immediately after open() was called
            //UbiBCN.CoroutineManager.DelayedCall(() => OnRewardedResult(AdsManager.EPlayResult.TIMEOUT), 1);
			UbiBCN.CoroutineManager.DelayedCall(() => OnRewardedResult(AdsManager.EPlayResult.PLAYED), 1);	// [AOC] TEMP!! Simulate the ad was viewed while we try to fix some weird bug with ads not being given sometimes
        }       
	}

	private void OnRewardedResult(AdsManager.EPlayResult result)
	{        
        if ( m_onRewardedCallback != null ){
			m_onRewardedCallback(result == AdsManager.EPlayResult.PLAYED );
			m_onRewardedCallback = null;
		}

        // Ad has been finished is tracked
        bool videoWatched = result == AdsManager.EPlayResult.PLAYED;
        HDTrackingManager.Instance.Notify_AdFinished(Track_EAdPurposeToAdType(CurrentAdPurpose), videoWatched, false, 0, TRACK_AD_PROVIDER_ID);

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
