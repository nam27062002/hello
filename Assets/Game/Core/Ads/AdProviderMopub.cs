using UnityEngine;

public class AdProviderMopub : AdProvider
{

    ~AdProviderMopub()
    {
        #if MOPUB_SDK_ENABLED
            MoPubManager.OnRewardedVideoClosedEvent -= onRewardedVideoClosedEvent;
            MoPubManager.OnRewardedVideoShownEvent -= onRewardedVideoShownEvent;
            
            MoPubManager.OnInterstitialShownEvent -= onInterstitialShown;
            MoPubManager.OnInterstitialDismissedEvent -= OnInterstitialDismissed;
        #endif            
    }
    
    protected override void ExtendedInit(bool useAgeProtection, bool consentRestriction)
    {
        if (useAgeProtection)
        {
            // No Ad units for < 13 users have been proveded for Mopub (Mopub stopped being deprecated right before age protection was supported)            
            LogError("No unit ads when age protection is enabled haven't been provided.");            
        }
        else
        {
            string interstitialId = "";
            string rewardId = "";
            bool isPhone = !MiscUtils.IsDeviceTablet(FeatureSettingsManager.m_OriginalScreenWidth, FeatureSettingsManager.m_OriginalScreenHeight, Screen.dpi);
            if (UnityEngine.Debug.isDebugBuild)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (isPhone)
                    {
                        rewardId = "242e5f30622549f0ae85de0921842b71";
                        interstitialId = "af85208c87c746e49cb88646d60a11f9";
                    }
                    else
                    {
                        rewardId = "b83b87dab28b42a98abace9b7a3a8c15";
                        interstitialId = "03a11207d6ff4e70a11ddaf63b12ef8a";
                    }
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (isPhone)
                    {
                        rewardId = "5e6b8e4e20004d2c97c8f3ffd0ed97e2";
                        interstitialId = "c3c79080175c42da94013bccf8b0c9a2"; // WRONG CONFIGURATION. Ask Alexis Rosa to check it!!!!
                    }
                    else
                    {
                        rewardId = "3ee1afec3ef5468ab65d65e3dc85025a";
                        interstitialId = "1ff446d39b244c25ab7fa62638d592fb";
                    }
                }
            }
            else
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (isPhone)
                    {
                        rewardId = "f572e1fc37274ac6a8eb45bfecdd65c8";
                        interstitialId = "a45750f42c864575b54afeb96a408818";
                    }
                    else
                    {
                        rewardId = "4248c1e6ec54409e9935b4ae08887e2b";
                        interstitialId = "5bbda1a7d3634cbf9281e6f2104c9134";
                    }
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (isPhone)
                    {
                        rewardId = "0d4a0085682948748fc8b162e8cbbae8";
                        interstitialId = "bf6b94c26863449e9347369bdb31362e";
                    }
                    else
                    {
                        rewardId = "31188271f9ab404cb2346472a9e93d8d";
                        interstitialId = "203f4feba4104e88a400519d731589ed";
                    }
                }
            }

            // TODO: Validate all interstitialId values are configurated correctly (Ask Juan how to validate configuration for these ids)
            MopubAdsManager.SharedInstance.Init(interstitialId, false, rewardId, true, 30);
#if MOPUB_SDK_ENABLED
            MoPubManager.OnRewardedVideoShownEvent += onVideoAdOpen;
            MoPubManager.OnRewardedVideoClosedEvent += OnVideoEnded;
            
            
            MoPubManager.OnInterstitialShownEvent += onVideoAdOpen;
            MoPubManager.OnInterstitialDismissedEvent += OnVideoEnded;
#endif            
        }
    }
    
    void OnVideoStarted( string id )
    {
        if (onVideoAdOpen != null)
            onVideoAdOpen();
    }
    
    void OnVideoEnded( string id )
    {
        if (onVideoAdClosed != null)
            onVideoAdClosed();
    }

    protected override void ExtendedShowInterstitial()
    {
        MopubAdsManager.SharedInstance.PlayNotRewarded(OnInsterstitialResult, 5);
    }

    private void OnInsterstitialResult(MopubAdsManager.EPlayResult result)
    {
        OnShowInterstitial(result == MopubAdsManager.EPlayResult.PLAYED);        
    }

    protected override void ExtendedShowRewarded()
    {
        MopubAdsManager.SharedInstance.PlayRewarded(OnRewardedResult, 5);
    }

    private void OnRewardedResult(MopubAdsManager.EPlayResult result)
    {
        OnShowRewarded(result == MopubAdsManager.EPlayResult.PLAYED);              
    }

    public override string GetId()
    {
        return "MoPub";
    }

    public override bool IsWaitingToPlayAnAnd()
    {
        return MopubAdsManager.SharedInstance.IsWaitingToPlayAVideo();
    }

    public override void StopWaitingToPlayAnAd()
    {
        MopubAdsManager.SharedInstance.StopWaitingToPlayAVideo();
    }
}
