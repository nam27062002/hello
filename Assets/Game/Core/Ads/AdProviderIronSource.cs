﻿#if IRONSOURCE_SDK_ENABLED
using UnityEngine;
using System;
using System.Collections;

public class AdProviderIronSource : AdProvider
{
    private const string IronSourceGameObjectName = "IronSourceEvents";

    private static HSEIronSourceEngine mIronSourceEngine = null;

    private static IronSourceEvents mIronSourceEvents = null;

    protected override void ExtendedInit(bool useAgeProtection)
    {
        string appId = null;        

        // Ad units depend on the user's age (<13)
        if (useAgeProtection)
        {
#if UNITY_IPHONE		    
            appId = "757a7605"; // HD
#elif UNITY_ANDROID            
            appId = "757aaf8d"; // HD
#endif
        }
        else
        {
#if UNITY_IPHONE
		    //appId = "6d850bb5"; // HSE
            appId = "757a3c7d"; // HD
#elif UNITY_ANDROID
            //appId = "6be092bd"; // HSE
            appId = "7579c96d"; // HD
#endif
        }

        //  Initialize game object (for IronSource engine)
        GameObject go = GameObject.Find(IronSourceGameObjectName);
        if (go == null)
        {
            go = new GameObject(IronSourceGameObjectName);
            GameObject.DontDestroyOnLoad(go);
        }

        // Get/Add Component (IronSourceEvents)
        mIronSourceEvents = go.GetComponent<IronSourceEvents>();
        if (mIronSourceEvents == null)
        {
            mIronSourceEvents = go.AddComponent<IronSourceEvents>();
        }

        // Get/Add Component (HSEIronSourceEngine)
        mIronSourceEngine = go.GetComponent<HSEIronSourceEngine>();
        if (mIronSourceEngine == null)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("IronSource:: ... AdProviderIronSource() started");
            }

            mIronSourceEngine = go.AddComponent<HSEIronSourceEngine>();
            mIronSourceEngine.Init(appId, this);
        }
    }

	protected override void ExtendedShowInterstitial()
	{
		if (mIronSourceEngine != null) 
		{
			mIronSourceEngine.TryToPlayVideoAd(m_ad);
		}
	}

	protected override void ExtendedShowRewarded()
	{
		if (mIronSourceEngine != null) 
		{
			mIronSourceEngine.TryToPlayVideoAd(m_ad);
		}
	}

	public override bool IsWaitingToPlayAnAnd() 
	{ 		
		return (mIronSourceEngine == null) ? false : mIronSourceEngine.IsWaitingToPlayAnAnd(); 
	}

	public override void StopWaitingToPlayAnAd() 
	{
		if (mIronSourceEngine != null) 
		{
			mIronSourceEngine.StopWaitingToPlayAnAd ();
		}
	}

    public override string GetId()
    {
        return "IronSource";
    }

	public override void ShowDebugInfo() 
	{
		if (FeatureSettingsManager.IsDebugEnabled) 
		{
			if (mIronSourceEngine == null) 
			{
				Log("Engine step: mIronSourceEngine is null");
			} 
			else 
			{
				Log("Engine step: " + mIronSourceEngine.GetEngineStep ());
				Log("Rewarded status: " + mIronSourceEngine.GetRewardedStatus ());
				Log("Interstitial status: " + mIronSourceEngine.GetInterstitialStatus ());
			}
		}
	}

    // ==================================================== //
    // ==================================================== //
    // ==================================================== //
    // ==================================================== //
    // ==================================================== //


    //  Callbacks mono behavior
    public class HSEIronSourceEngine : MonoBehaviour
    {

        public enum EStep
        {
            NotStarted,
            WaittingInternet,
            Running,
            PlayingVideo,
        };

        public enum EState
        {
            Waitting,
            Caching,
            Loaded,
            Playing,
        };

        public enum PlaybackResult
        {
            NONE,
            SUCCESS,
            FAILED,
        };

        // --------------------------------------------------------------- //

        private Ad mTryingToPlayAd;
        private bool mTryingToPlayAdIsPlaying = false;
        private DateTime mTryingToPlayAdStartedAt = DateTime.Now;        

        public EStep mStep = EStep.NotStarted;

        public EState mStateRewarded = EState.Waitting;
        public EState mStateInterstitial = EState.Waitting;

        private int mInterstitialCachingTry = 1;

        private DateTime mStepStartedAt = DateTime.Now;
        private DateTime mlastRewardedReadyCheckAt = DateTime.Now;
        private DateTime mlastRewardedCachingCheckAt = DateTime.Now;
        private DateTime mlastInterstitialReadyCheckAt = DateTime.Now;
        private DateTime mlastInterstitialCachingCheckAt = DateTime.Now;

        private PlaybackResult mPlaybackResult = PlaybackResult.NONE;

        private bool mTimeout_appWasPaused = false;

        private int mPlayVideoAdCount = 0;

        private AdProvider mAdProvider;

        // --------------------------------------------------------------- //


        public void Init(string appId, AdProvider adProvider)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("IronSource::: Init...");
            }

            mAdProvider = adProvider;

            IronSource.Agent.reportAppStarted();

			if (FeatureSettingsManager.IsDebugEnabled) 
			{
				IronSource.Agent.validateIntegration();
				IronSource.Agent.setAdaptersDebug(true);
			}

            //			IronSource.Agent.setUserId ("uniqueUserId");
            IronSource.Agent.init(appId, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL);

            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("IronSource::: Init... DONE! (using AppId: " + appId + ")");
            }            

            SetStep(EStep.WaittingInternet);
        }


        // ==================================================== //
        
        public bool CanShowAdNow(AdType adType)
        {
            if (adType == AdType.V4VC)
            {
                return mStateRewarded == EState.Loaded;
            }
            else if (adType == AdType.Interstitial)
            {
                return mStateInterstitial == EState.Loaded;
            }

            return false;
        }  
			
        public void TryToPlayVideoAd(Ad ad)
        {           
            mTryingToPlayAdIsPlaying = false;
            mTryingToPlayAdStartedAt = DateTime.Now;
            mTryingToPlayAd = ad;                     
        }

		public bool IsWaitingToPlayAnAnd() 
		{ 		
			return mTryingToPlayAd != null;
		}

		public void StopWaitingToPlayAnAd() 
		{
			// The video can be cancelled only if it hasn't started being played
			if (mTryingToPlayAd != null && !mTryingToPlayAdIsPlaying)
			{
				mAdProvider.OnAdPlayed(mTryingToPlayAd, false, "(IS) Cancelled by the user");                
				mTryingToPlayAd = null;
			}
		}

        private void SetStep(EStep newStep)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("IronSource::: -----------------------");
                Log("IronSource::: ... NewStep: " + newStep);
            }

            mStep = newStep;
            mStepStartedAt = DateTime.Now;

            switch (newStep)
            {
                case EStep.PlayingVideo:

                    mPlaybackResult = PlaybackResult.NONE;

                    if (FeatureSettingsManager.IsDebugEnabled)
                        Log("IronSource::: ... ... ... Playing " + mTryingToPlayAd.Type);                                                                   

#if UNITY_ANDROID
                    // Start a timeout to skip an infinity-loop when IronSource tries to start a videoAd
                    StartCoroutine(StartPlayAdTimeOut());
#endif
                    if (mTryingToPlayAd.Type == AdType.V4VC)
                    {
                        mStateRewarded = EState.Playing;
                        IronSource.Agent.showRewardedVideo();
                    }
                    else if (mTryingToPlayAd.Type == AdType.Interstitial)
                    {
                        mStateInterstitial = EState.Playing;
                        IronSource.Agent.showInterstitial();
                    }
                    break;
            }
        }			

        private IEnumerator StartPlayAdTimeOut()
        {
            int currentPlayVideoAdCount = mPlayVideoAdCount;

            // this flag will be set to true when the videoAd starts,
            mTimeout_appWasPaused = false;

            // so... if previous flag remains false after the 5 seconds timeout... we will notify a PlayVideo Failef by timeout...
            yield return new WaitForSecondsRealtime(5f);
            //			yield return new WaitForSecondsRealtime(5f);

            if (!mTimeout_appWasPaused && currentPlayVideoAdCount == mPlayVideoAdCount && mStep == EStep.PlayingVideo && mPlaybackResult == PlaybackResult.NONE)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("IronSource::: ---> ... ... Playing TIMEOUT");
                NotifyPlaybackResult(PlaybackResult.FAILED);
            }
        }


        private void LogicUpdate()
        {
            switch (mStep)
            {
                case EStep.WaittingInternet:

                    //  Make sure SDK is initialized when internet connection is enabled
                    if (Application.internetReachability != NetworkReachability.NotReachable)
                    {
                        SetStep(EStep.Running);
                    }
                    break;

                case EStep.Running:

                    // If a video must be played, first search if any already cached and play it...
                    if (mTryingToPlayAd != null)
                    {
                        if ((mTryingToPlayAd.Type == AdType.V4VC && mStateRewarded == EState.Loaded)
                        || (mTryingToPlayAd.Type == AdType.Interstitial && mStateInterstitial == EState.Loaded))
                        {
                            mTryingToPlayAdIsPlaying = true;

                            SetStep(EStep.PlayingVideo);
                            return;
                        }
                    }

                    // If Waitting timeout is over and internet is reachable find a non-cached video and try to chache it...
                    if (((DateTime.Now - mStepStartedAt).TotalSeconds >= 3)
                    && (Application.internetReachability != NetworkReachability.NotReachable))
                    {
                        // if caching state... check if videoAd was cached (aka loaded)
                        if (mStateRewarded == EState.Caching && (DateTime.Now - mlastRewardedReadyCheckAt).TotalSeconds >= 3)
                        {
                            mlastRewardedReadyCheckAt = DateTime.Now;
                            if (IronSource.Agent.isRewardedVideoAvailable())
                            {
                                mStateRewarded = EState.Loaded;

                                if (FeatureSettingsManager.IsDebugEnabled)
                                    Log("IronSource::: ... ... Loaded  " + "(Rewarded)");
                            }
                        }
                        else if (mStateInterstitial == EState.Caching && (DateTime.Now - mlastInterstitialReadyCheckAt).TotalSeconds >= 5)
                        {
                            if ((DateTime.Now - mlastInterstitialCachingCheckAt).TotalSeconds >= 60)
                            {
                                mStateInterstitial = EState.Waitting;
                                mInterstitialCachingTry++;
                            }
                            else
                            {
                                mlastInterstitialReadyCheckAt = DateTime.Now;
                                if (IronSource.Agent.isInterstitialReady())
                                {
                                    mStateInterstitial = EState.Loaded;
                                    if (FeatureSettingsManager.IsDebugEnabled)
                                        Log("IronSource::: ... ... Loaded  " + "(Interstitial)");

                                    mInterstitialCachingTry = 1;
                                }
                            }
                        }


                        // if 'Waitting' State, call to start caching
                        if (mStateRewarded == EState.Waitting)
                        {
                            mStateRewarded = EState.Caching;
                            mlastRewardedReadyCheckAt = DateTime.Now;
                            mlastRewardedCachingCheckAt = DateTime.Now;

                            if (FeatureSettingsManager.IsDebugEnabled)
                                Log("IronSource::: ... ... Caching...  " + "(Rewarded)");
                        }
                        else if (mStateInterstitial == EState.Waitting)
                        {
                            mStateInterstitial = EState.Caching;
                            mlastInterstitialReadyCheckAt = DateTime.Now;
                            mlastInterstitialCachingCheckAt = DateTime.Now;

                            if (FeatureSettingsManager.IsDebugEnabled)
                                Log("IronSource::: ... ... Caching...  " + "(Interstitial)");

                            IronSource.Agent.loadInterstitial();
                        }
                    }
                    break;

                case EStep.PlayingVideo:

                    if (mPlaybackResult != PlaybackResult.NONE)
                    {
                        mPlayVideoAdCount++;

                        if (FeatureSettingsManager.IsDebugEnabled)
                            Log("IronSource::: ... ... ... Played  " + mTryingToPlayAd.Type + ": " + mPlaybackResult);

                        if (mTryingToPlayAd.Type == AdType.V4VC)
                        {
                            mStateRewarded = EState.Waitting;
                        }
                        else if (mTryingToPlayAd.Type == AdType.Interstitial)
                        {
                            mStateInterstitial = EState.Waitting;
                        }

                        SetStep(EStep.Running);
                        
                        if (mPlaybackResult == PlaybackResult.SUCCESS)
                        {
                            mAdProvider.OnAdPlayed(mTryingToPlayAd, true, null);                            
                        }
                        else
                        {
                            mAdProvider.OnAdPlayed(mTryingToPlayAd, false, "(IS) Playback attent failed with TimeOut");                            
                        }
                        mTryingToPlayAd = null;
                        return;
                    }
                    break;
            }

            // check if 'TryToPlayVideoAd' task has reached Timeout
            if (mTryingToPlayAd != null && !mTryingToPlayAdIsPlaying && (DateTime.Now - mTryingToPlayAdStartedAt).TotalSeconds >= (mTryingToPlayAd.Timeout < 1 ? 1 : mTryingToPlayAd.Timeout))
            {
                mAdProvider.OnAdPlayed(mTryingToPlayAd, false, "(IS) Load TimeOut... No videoAd available");                
                mTryingToPlayAd = null;
            }
        }


        private void NotifyPlaybackResult(PlaybackResult playbackResult)
        {
            if (mPlaybackResult == PlaybackResult.NONE && mStep == EStep.PlayingVideo)
            {
                mPlaybackResult = playbackResult;
            }
        }



        // --------------------------------------------------------------- //



        // ==================================================== //

        void Update()
        {
            LogicUpdate();
        }


        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                // Needed to know if a videoAd was started to play:
                // When a VideoAd starts to play, our app is paused and this flag is set.
                // so, we can detect if a videoAd don't start to play and entering in an infinite loop...
                mTimeout_appWasPaused = true;
            }

            IronSource.Agent.onApplicationPause(paused);
        }


        void OnEnable()
        {
            Debug.Log("AdProviderIronSourceCallbacks enabled");

            //  Register listerners
            IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;

            IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;
        }


        void OnDisable()
        {
            Debug.Log("AdProviderIronSourceCallbacks disable");

            //  UnRegister listerners
            IronSourceEvents.onInterstitialAdReadyEvent -= InterstitialAdReadyEvent;
            IronSourceEvents.onInterstitialAdLoadFailedEvent -= InterstitialAdLoadFailedEvent;
            IronSourceEvents.onInterstitialAdShowSucceededEvent -= InterstitialAdShowSucceededEvent;
            IronSourceEvents.onInterstitialAdShowFailedEvent -= InterstitialAdShowFailedEvent;
            IronSourceEvents.onInterstitialAdClickedEvent -= InterstitialAdClickedEvent;
            IronSourceEvents.onInterstitialAdOpenedEvent -= InterstitialAdOpenedEvent;
            IronSourceEvents.onInterstitialAdClosedEvent -= InterstitialAdClosedEvent;

            IronSourceEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent -= RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent -= RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= RewardedVideoAdShowFailedEvent;
            IronSourceEvents.onRewardedVideoAdClickedEvent -= RewardedVideoAdClickedEvent;
        }


        #region Interstitial IronSource callbacks

        void InterstitialAdReadyEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource:: --> Interstitial Loaded");

            //			NotifyCachingResult(CachingResult.VIDEO_AD_LOADED, AdType.Interstitial);
        }

        void InterstitialAdLoadFailedEvent(IronSourceError error)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource:: --> Interstitial FAILED: " + error);

            //			NotifyCachingResult(CachingResult.VIDEO_AD_NOT_FOUND, AdType.Interstitial);
        }

        void InterstitialAdShowSucceededEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource:: --> Interstitial PLAY-Back SUCCESS");

            NotifyPlaybackResult(PlaybackResult.SUCCESS);
        }

        void InterstitialAdShowFailedEvent(IronSourceError error)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource:: --> Interstitial PLAY-Back FAILED: " + error);

            NotifyPlaybackResult(PlaybackResult.FAILED);
        }

        void InterstitialAdClickedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Interstitial Ad Clicked Event");
        }

        void InterstitialAdOpenedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Interstitial Ad Opened Event");
        }

        void InterstitialAdClosedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Interstitial Ad Closed Event");
        }

        void InterstitialAdRewardedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Interstitial Ad Rewarded Event");
        }
        #endregion

        #region Rewarded IronSource callbacks

        void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
        }

        void RewardedVideoAdOpenedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> I got RewardedVideoAdOpenedEvent");
        }

        void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
        }

        void RewardedVideoAdClosedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> I got RewardedVideoAdClosedEvent");

            NotifyPlaybackResult(PlaybackResult.SUCCESS);
        }

        void RewardedVideoAdStartedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Rewarded VideoAd Started Event");
        }

        void RewardedVideoAdEndedEvent()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Rewarded video closed");
        }

        void RewardedVideoAdShowFailedEvent(IronSourceError error)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource:: --> Failed To Play rewarded video: " + error);

            NotifyPlaybackResult(PlaybackResult.FAILED);
        }

        void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("IronSource: --> Rewarded VideoAd Clicked Event");
        }
        #endregion

        // ==================================================== //
		public string GetEngineStep()
		{
			string stepStr = mStep.ToString();

			switch (mStep)
			{
			case EStep.Running:

				if (mTryingToPlayAd != null && !mTryingToPlayAdIsPlaying)
				{
					int timeOut = (mTryingToPlayAd.Timeout<1?1:mTryingToPlayAd.Timeout) - (int)((DateTime.Now - mTryingToPlayAdStartedAt).TotalSeconds);

					stepStr = "Trying to play: " + mTryingToPlayAd.Type.ToString() + " (timeOut: " + timeOut + ")";
				}
				break;
/*
			case EStep.CachingVideo:

				stepStr = "Caching " + mCurrentVideoAd.adType + " (" + (int)((DateTime.Now - mStepStartedAt).TotalSeconds) + " sec)";

				if (mTryingToPlayAd != null && !mTryingToPlayAdIsPlaying)
				{
					int timeOut = (mTryingToPlayAd.Timeout<1?1:mTryingToPlayAd.Timeout) - (int)((DateTime.Now - mTryingToPlayAdStartedAt).TotalSeconds);

					stepStr += " & Try PlayAd: " + mTryingToPlayAd.Type.ToString() + " (timeOut: " + timeOut + ")";
				}

				break;
*/
			case EStep.PlayingVideo:

				stepStr = "Playing " + mTryingToPlayAd.Type;
				break;
			}

			return stepStr;
		}

		public string GetRewardedStatus()
		{
			string stateStr = mStateRewarded.ToString();

			if (mStateRewarded == EState.Caching)
			{
				stateStr +=  " (" + (int)((DateTime.Now - mlastRewardedCachingCheckAt).TotalSeconds) + " sec)";
			}

			return stateStr;
		}

		public string GetInterstitialStatus()
		{
			string stateStr = mStateInterstitial.ToString();

			if (mStateInterstitial == EState.Caching)
			{
				stateStr +=  " (" + (int)((DateTime.Now - mlastInterstitialCachingCheckAt).TotalSeconds) + " sec" + (mInterstitialCachingTry>1? ", try " + mInterstitialCachingTry:"" + ")");
			}

			return stateStr;
		}
        // ==================================================== //


    }	// class end
}
#endif
