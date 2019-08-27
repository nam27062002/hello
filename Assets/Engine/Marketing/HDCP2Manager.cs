#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;

public class HDCP2Manager
{
    private static HDCP2Manager smInstance = null;

    public static HDCP2Manager Instance
    {
        get
        {
            if (smInstance == null)
            {
                smInstance = new HDCP2Manager();
            }

            return smInstance;
        }
    }

    private enum EState
    {
        None,
        PlayingPromo
    };

    private EState m_state = EState.None;	   

    private CP2Listener m_listener = null;

    private UnityAction<bool> m_onPlayPromoDone;

	private int NumPromosFailedSoFar = 0;

    public void Initialise()
    {
        if (!IsInitialised() && CanBeInitialised())
        {            
            Log("INIT CP2......");				          

			NumPromosFailedSoFar = 0;

            m_listener = new CP2Listener();
            CP2Manager.SharedInstance.SetListener(m_listener);

            SetState(EState.None);

            CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
            if (settingsInstance != null)
            {
                int totalPurchases = (HDTrackingManager.Instance.TrackingPersistenceSystem == null) ? 0 : HDTrackingManager.Instance.TrackingPersistenceSystem.TotalPurchases;
                int playerProgress = (UsersManager.currentUser != null) ? UsersManager.currentUser.GetPlayerProgress() : 0;
                string countryCode = DeviceUtilsManager.SharedInstance.GetDeviceCountryCode();
                if (string.IsNullOrEmpty(countryCode))
                {
                    countryCode = "UNKNOWN";
                }

                CP2Manager.CrossPromotionConfig kCrossPromotionConfig = new CP2Manager.CrossPromotionConfig();
                kCrossPromotionConfig.m_strLocalCP2DataPath = "data.zip";
                kCrossPromotionConfig.m_bIsDEVEnvironment = (settingsInstance.m_iBuildEnvironmentSelected != (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION);
				kCrossPromotionConfig.m_strGameCode = "728";
                kCrossPromotionConfig.m_strAdZone = "7: Gameplay to Main Menu loading";
                kCrossPromotionConfig.m_strCountry = countryCode;
                kCrossPromotionConfig.m_strLevelReached = "" + playerProgress;
                kCrossPromotionConfig.m_strIAPCount = "" + totalPurchases;

                CP2Manager.SharedInstance.Initialise(kCrossPromotionConfig);
            }
        }
    }

    private bool IsInitialised()
    {
		return CP2Manager.SharedInstance.CheckIfInitialised();
    }

    private bool CanBeInitialised()
    {
        bool returnValue = FeatureSettingsManager.instance.IsCP2Enabled();
        if (returnValue)
        {
            // At the moment CP2 doesn't support api level 17 (it uses a function that doesn't exist under api level 19)
#if !UNITY_EDITOR && UNITY_ANDROID
            var clazz = new AndroidJavaClass("android.os.Build$VERSION");
            if (clazz != null)
            {
                int apiLevel = clazz.GetStatic<int>("SDK_INT");

                if (apiLevel < 19)
                {                    
                    Log("CP2 can't be initialized because apiLevel (" + apiLevel + ") is lower than 19");
                    returnValue = false;
                }
            }
#endif
        }

        return returnValue;
    }

    private void SetState(EState state)
    {
        m_state = state;
    }

    /// <summary>
    /// Returns whether or not there's a CP2 interstitial available to be played
    /// </summary>
    /// <returns></returns>
    private bool IsInterstitialAvailable()
    {
		return FeatureSettingsManager.instance.IsCP2InterstitialEnabled() && IsInitialised() && m_state == EState.None && 
			//CP2Manager.SharedInstance.CanShowPromo(CrossPromo.PromoType.INTERSTITIAL);               
			NumPromosFailedSoFar < 2;
    }

    private bool CanUserPlayInterstitial()
    {
        // We want to make sure the user has already passed first time user xp so interstitial doesn't interfere with ftux
        // The user has to have been playing for a while (3 runs) in the current session so she won't get annoyed by interstitial
        // We make sure that the minimum time since a cp2 interstitial was last played has passed so the user doesn't get too spammed                     
        TrackingPersistenceSystem trackingSystem = HDTrackingManager.Instance.TrackingPersistenceSystem;
        bool ftuxPassed = trackingSystem != null && trackingSystem.GameRoundCount >= 3;
        return ftuxPassed && HDTrackingManager.Instance.Session_GameRoundCount >= FeatureSettingsManager.instance.GetCP2InterstitialMinRounds() && GetUserRestrictionTimeToWait() <= 0f;        
    }   

    private float GetUserRestrictionTimeToWait()
    {
        float returnValue = 0f;

        // If the user has already seen a cp2 interstitial then checks that the minimum time between two consecutive cp2 interstitials has passed
        long latestTimestamp = PersistencePrefs.GetCp2InterstitialLatestAt();
        if (latestTimestamp > 0)
        {
            long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - latestTimestamp;
            returnValue = (FeatureSettingsManager.instance.GetCP2InterstitialFrequency() * 1000 - diff) / 1000f;            
        }

        return returnValue;
    }

    public string GetDebugInfo()
    {
        TrackingPersistenceSystem trackingSystem = HDTrackingManager.Instance.TrackingPersistenceSystem;
        bool ftuxPassed = trackingSystem != null && trackingSystem.GameRoundCount >= 3;

		return "IsInterstitialAvailable = " + IsInterstitialAvailable() + 
			//" CanShowPromo = " + CP2Manager.SharedInstance.CanShowPromo(CrossPromo.PromoType.INTERSTITIAL) + 
			" CP2InterstitialEnabled = " + FeatureSettingsManager.instance.IsCP2InterstitialEnabled() + " Inititalized = " + IsInitialised() + " state = " + m_state +
            " CanUserPlayInterstitial = " + CanUserPlayInterstitial() + " timeToWait = " + GetUserRestrictionTimeToWait() + " ftuxPassed= " + ftuxPassed +
            " minRoundsSoFar = " + HDTrackingManager.Instance.Session_GameRoundCount + " minRoundsRequired = " + FeatureSettingsManager.instance.GetCP2InterstitialMinRounds();
    }

    private void PlayInterstitialInternal(Action<bool> onDone)
    {
        if (IsInterstitialAvailable())
        {
            PlayPromo(CrossPromo.PromoType.INTERSTITIAL, onDone);

            // onDone is simulated in editor becuase CP2 doesn't work in editor
#if UNITY_EDITOR
            if (onDone != null)
                onDone(true);
#endif
        }
        else 
        {
            Log("Can't play CP2 interstitial because it's not available: cp2Enabled = " + FeatureSettingsManager.instance.IsCP2Enabled() + 
                " cp2InterstitialEnabled = " +  FeatureSettingsManager.instance.IsCP2InterstitialEnabled() + " initialised = " + IsInitialised() + 
				" state = " + m_state + " numPromosFailedSoFar = " + NumPromosFailedSoFar);
        }
    }

    private void PlayPromo(CrossPromo.PromoType promoType, Action<bool> onDone)
    {                
	    Log("Playing promo " + promoType.ToString() + " listener is not null = " + (m_listener != null));        

        SetState(EState.PlayingPromo);
        if (m_listener != null)
        {
            m_listener.m_onPlayPromo = onDone;                
        }

        CP2Manager.SharedInstance.ShowPromo(promoType);
    }

    private void OnPlayPromo(bool success)
    {        
        Log("OnPlayPromo success = " + success);        

		if (!success) 
		{
			NumPromosFailedSoFar++;
		}

        SetState(EState.None);
        if (m_onPlayPromoDone != null)
        {
            m_onPlayPromoDone(success);
            m_onPlayPromoDone = null;
        }
    }

    private void OnRestrictedPlayPromo(bool success)
    {
        OnPlayPromo(success);

        if (success)
        {
            long time = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
            PersistencePrefs.SetCp2InterstitialLatestAt(time);
        }            
    }

    public bool CanPlayInterstitial(bool checkRestrictionPerUser)
    {
        bool returnValue = IsInterstitialAvailable();
        if (returnValue && checkRestrictionPerUser)
        {
            returnValue = CanUserPlayInterstitial();
        }

        return returnValue;
    }

    /// <summary>
    /// Plays a cp2 interstitial if there's one available.
    /// </summary>
    /// <param name="checkRestrictionPerUser">Whether or not user's restrictions should be checked too</param>
    public void PlayInterstitial(bool checkRestrictionPerUser, UnityAction<bool> onDone)
    {
        m_onPlayPromoDone = onDone;

        if (checkRestrictionPerUser)
        {
            if (CanUserPlayInterstitial())
            {
                PlayInterstitialInternal(OnRestrictedPlayPromo);
            }
            else if (FeatureSettingsManager.IsDebugEnabled)
            {
                OnPlayPromo(false);
                Log("Can't play CP2 interstitial because of user's restriction. The user has to wait " + GetUserRestrictionTimeToWait() + " seconds more");
            }
        }
        else
        {
            PlayInterstitialInternal(OnPlayPromo);
        }
    }

    #region listener
    public class CP2Listener : CP2Manager.CP2Listener
    {
        public Action<bool> m_onPlayPromo;        

        public override void onInGameLocation(string strURL)
        {            
            Log("CP2Listener onInGameLocation: url = " + strURL);            
        }

        public override void onTrackingCallback(string strURL, string strType, string strPromoID)
        {            
            Log("CP2Listener onTrackingCallback: " + strURL + " " + strType + " " + strPromoID);            
        }

        public override void onDownloadDelegate(bool bSuccess, string strMessage)
        {            
            Log("CP2Listener onDownloadDelegate: " + bSuccess + " " + strMessage);            
        }

        public override void onCompletionCallback(CrossPromo.CrossPromoInstance kPromo, string strAction)
        {
            bool success = kPromo.currentStatus != CrossPromo.PromoStatus.ERROR;
            if (success)
            {
                if (kPromo.isThereContentToDisplay)
                {                    
                    Log("CP2Listener onCompletionCallback Content to display: " + strAction);                    
                }
                else
                {                    
                    Log("CP2Listener onCompletionCallback Finished: " + strAction);                    
                }
            }
            else
            {                                    
                Log("CP2Listener onCompletionCallback: " + kPromo.errorMessage);
            }
                            
            if (m_onPlayPromo != null)
            {
                m_onPlayPromo(success);
            }
        }
    };
    #endregion

    #region debug
#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    private static void Log(string msg)
    {
        ControlPanel.Log(msg, ControlPanel.ELogChannel.CP2);
    }
    #endregion
}
