using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAds : UbiBCN.SingletonMonoBehaviour<GameAds> {

	public delegate void OnPlayVideoCallback(bool giveReward);
	private OnPlayVideoCallback m_onInterstitialCallback;
	private OnPlayVideoCallback m_onRewardedCallback;

	public void Init()
	{
		string interstitialId = "af85208c87c746e49cb88646d60a11f9";
		string rewardId = "242e5f30622549f0ae85de0921842b71";
		bool isPhone = true;
		// TODO: Check if tablet
		if ( Application.platform == RuntimePlatform.Android )
		{
			if ( isPhone )
			{
				interstitialId = "af85208c87c746e49cb88646d60a11f9";
				rewardId = "242e5f30622549f0ae85de0921842b71";
			}
		}
		else if ( Application.platform == RuntimePlatform.IPhonePlayer )
		{
			if ( isPhone )
			{
				interstitialId = "c3c79080175c42da94013bccf8b0c9a2";
				rewardId = "5e6b8e4e20004d2c97c8f3ffd0ed97e2";
			}
		}

		AdsManager.SharedInstance.Init( interstitialId, rewardId, true, 30);
	}

	public void ShowInterstitial(OnPlayVideoCallback callback)
	{
		m_onInterstitialCallback = callback;
		AdsManager.SharedInstance.PlayNotRewarded(OnInsterstitialResult, 5);
	}

	public void OnInsterstitialResult(AdsManager.EPlayResult result)
	{
		if (m_onInterstitialCallback != null){
			m_onInterstitialCallback(result == AdsManager.EPlayResult.PLAYED );	
			m_onInterstitialCallback = null;
		}
	}

	public void ShowRewarded(OnPlayVideoCallback callback)
	{
		m_onRewardedCallback = callback;
		AdsManager.SharedInstance.PlayRewarded(OnRewardedResult, 5);
	}

	public void OnRewardedResult(AdsManager.EPlayResult result)
	{
		if ( m_onRewardedCallback != null ){
			m_onRewardedCallback(result == AdsManager.EPlayResult.PLAYED );
			m_onRewardedCallback = null;
		}
	}
}
