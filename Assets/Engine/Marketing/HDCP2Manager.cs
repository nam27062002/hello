﻿using System;
using UnityEngine;

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

    private Action<bool> m_onPlayPromoDone;

    public void Initialise()
    {
        if (!IsInitialised() && CanBeInitialised())
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                ControlPanel.Log("INIT CP2......");				          

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
                    if (FeatureSettingsManager.IsDebugEnabled)
                        ControlPanel.Log("CP2 can't be initialized because apiLevel (" + apiLevel + ") is lower than 19");

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
		return FeatureSettingsManager.instance.IsCP2InterstitialEnabled() && IsInitialised() && m_state == EState.None;               
    }

    private bool CanUserPlayInterstitial()
    {
        // Checks that the minimum time since a cp2 interstitial was last played has passed        
        long latestTimestamp = PersistencePrefs.GetCp2InterstitialLatestAt();
        long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - latestTimestamp;
        return GetUserRestrictionTimeToWait() <= 0f;
    }   

    private float GetUserRestrictionTimeToWait()
    {
        // Checks that the minimum time since a cp2 interstitial was last played has passed        
        long latestTimestamp = PersistencePrefs.GetCp2InterstitialLatestAt();
        long diff = GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong() - latestTimestamp;
        float timeToWait = (FeatureSettingsManager.instance.GetCP2InterstitialFrequency() * 1000 - diff) / 1000f;
        return timeToWait;
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
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Can't play CP2 interstitial because it's not available: cp2Enabled = " + FeatureSettingsManager.instance.IsCP2Enabled() + 
                " cp2InterstitialEnabled = " +  FeatureSettingsManager.instance.IsCP2InterstitialEnabled() + " initialised = " + IsInitialised() + 
                " state = " + m_state);
        }
    }

    private void PlayPromo(CrossPromo.PromoType promoType, Action<bool> onDone)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Playing promo " + promoType.ToString());
        }

        SetState(EState.PlayingPromo);
        if (m_listener != null)
        {
            m_listener.m_onPlayPromo = onDone;                
        }

        CP2Manager.SharedInstance.ShowPromo(promoType);
    }

    private void OnPlayPromo(bool success)
    {
        SetState(EState.None);
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
    public void PlayInterstitial(bool checkRestrictionPerUser)
    {
        if (checkRestrictionPerUser)
        {
            if (CanUserPlayInterstitial())
            {
                PlayInterstitialInternal(OnRestrictedPlayPromo);
            }
            else if (FeatureSettingsManager.IsDebugEnabled)
            {

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
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("CP2Listener onInGameLocation: url = " + strURL);            
        }

        public override void onTrackingCallback(string strURL, string strType, string strPromoID)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("CP2Listener onTrackingCallback: " + strURL + " " + strType + " " + strPromoID);            
        }

        public override void onDownloadDelegate(bool bSuccess, string strMessage)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("CP2Listener onDownloadDelegate: " + bSuccess + " " + strMessage);            
        }

        public override void onCompletionCallback(CrossPromo.CrossPromoInstance kPromo, string strAction)
        {
            bool success = kPromo.currentStatus != CrossPromo.PromoStatus.ERROR;
            if (success)
            {
                if (kPromo.isThereContentToDisplay)
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                        Log("CP2Listener onCompletionCallback Content to display: " + strAction);                    
                }
                else
                {
                    if (FeatureSettingsManager.IsDebugEnabled)
                        Log("CP2Listener onCompletionCallback Finished: " + strAction);                    
                }
            }
            else
            {                
                    if (FeatureSettingsManager.IsDebugEnabled)
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
    private static void Log(string msg)
    {
        ControlPanel.Log(msg, ControlPanel.ELogChannel.CP2);
    }
    #endregion
}
