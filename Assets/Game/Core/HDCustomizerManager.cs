// When enabled the customizer is applied when Apply() is called by the game
// When disabled the customizer is applied as soon as the response is received by server. This is Calety's original implementation.
//#define APPLY_ON_DEMAND

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible is an intermediary between the game and <c>CustomizerManager</c>, which is the Calety class responsible for handling customizer related stuff.
/// </summary>
public class HDCustomizerManager
{
#if APPLY_ON_DEMAND
    private class HDCustomizerListener : CyCustomiser.CustomizerListener
#else
    private class HDCustomizerListener : CustomizerManager.CustomizerListener
#endif
    {
#if APPLY_ON_DEMAND
        public override void onCustomizationError(CyCustomiser.eCustomizerError eError, string strSKU, string strAttrib)
#else
        public override void onCustomizationError(CustomizerManager.eCustomizerError eError, string strSKU, string strAttrib)
#endif
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                LogWarning("Error " + eError.ToString() + " when processing category: " + strSKU + " attribute: " + strAttrib);
            }
        }

        public override void onCustomizationChangedFiles(List<string> kChangedContentFiles)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
				string msg = "Files changed: ";
				for(int i = 0; i < kChangedContentFiles.Count; ++i) {
					if(i > 0) msg += ", ";
					msg += kChangedContentFiles[i];
				}
				Log(msg);
            }

            HDCustomizerManager.instance.NotifyFilesChanged(kChangedContentFiles);
        }

        public override void onNewPopupReceived() { }

#if APPLY_ON_DEMAND
        public override void onPopupIsPrepared(CyCustomiser.CustomiserPopupConfig kPopupConfig) { }
#else
        public override void onPopupIsPrepared(CustomizerManager.CustomiserPopupConfig kPopupConfig)
		{
			HDCustomizerManager.instance.NotifyPopupIsPrepared(kPopupConfig);
		}
#endif

        public override void onCustomizationFinished()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onCustomizationFinished");
            }

            HDCustomizerManager.instance.NotifyCustomizationFinished();
        }

        public override void onTimeToEndReceived(long iSecondsToEnd)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onTimeToEndReceived " + iSecondsToEnd);
            }

            HDCustomizerManager.instance.NotifyTimeToEndReceived(iSecondsToEnd);
        }

        public override void onTimeToNextReceived(long iSecondsToNext)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onTimeToNextReceived " + iSecondsToNext);
            }

            HDCustomizerManager.instance.NotifyTimeToNextReceived(iSecondsToNext);
        }
    }

    private const bool DEBUG_ENABLED = false;

    private static HDCustomizerManager sm_instance;

    public static HDCustomizerManager instance
    {
        get
        {
            if (sm_instance == null)
            {
                sm_instance = new HDCustomizerManager();

#if APPLY_ON_DEMAND
                CustomizerManager.SharedInstance.Initialise(false);
#endif
                CustomizerManager.SharedInstance.SetListener(new HDCustomizerListener());
            }

            return sm_instance;
        }
    }
    
    private enum EState
    {
        None, 
        WaitingToRequestCache,       
        WaitingToRequestServer,
        WaitingForResponse,
        Done
    };

    private EState m_state;

    /// <summary>
    /// List of files changed by the customizer
    /// </summary>
    private List<string> m_filesChangedByCustomizer;    

    /// <summary>
    /// List containing the files to revert because they were changed by previous customizers that don't have to be applied anymore, typically because those customizers have expired
    /// </summary>
    private List<string> m_filesToRevert;


	/// <summary>
	/// At this point we only support one popup. This will have a reference after the callback from CustomizerMAnager is received.
	/// </summary>
	private CustomizerManager.CustomiserPopupConfig m_lastPreparedPopupConfig;


    /// <summary>
    /// Time in seconds left to make customizer expire
    /// </summary>
    private double m_timeToExpire;

    /// <summary>
    /// Bool used to know whether or not m_timeToExpire was changed by customizer
    /// </summary>
    private bool m_timeToExpireModified;

    private const float TIME_TO_WAIT_BETWEEN_REQUESTS = (DEBUG_ENABLED) ? 20f : 10 * 60f;

    /// <summary>
    /// Time in seconds to wait to request customizer
    /// </summary>
    private float m_timeToRequest;

    /// <summary>
    /// Whether or not the changes defined by the current customizer have been applied by the client
    /// </summary>
    private bool m_hasBeenApplied;

    /// <summary>
    /// Whether or not the notification when a file is changed by CustomizerManager should be considered. It's used to avoid files reverted to the original content from being considered as files changed by Customizer
    /// </summary>
    private bool m_isNotifyFilesChangedEnabled;

    private bool m_needsToNotifyRulesChanged;

    private CustomizerManager.Experiment m_currentExperiment;

    public void Initialise()
    {
        Reset();
    }

    public void Reset()
    {        
        if (m_filesToRevert != null)
        {
            m_filesToRevert.Clear();
        }

        SetTimeToRequest(0f);

#if APPLY_ON_DEMAND        
        SetState(EState.WaitingToRequestCache);
#else
        SetState(EState.WaitingToRequestServer);
#endif
        SetIsNotifyFilesChangedEnabled(true);
        SetNeedsToNotifyRulesChanged(false);

        m_currentExperiment = null;
    }

    public void Destroy()
    {
        Reset();
    }

    public void Update()
    {
        float timeToRequest = GetTimeToRequest();
        if (timeToRequest > 0f)
        {
            timeToRequest -= Time.deltaTime;
            SetTimeToRequest(timeToRequest);
        }

        switch (m_state)
        {
            case EState.WaitingToRequestCache:
                UpdateWaitingToRequestCache();
                break;

            case EState.WaitingToRequestServer:                
                // Checks if it's a good moment to request the customizer
                if (IsRequestCustomizerAllowed() &&
                    GameSessionManager.SharedInstance.IsLogged() &&  // CustomizerManager requires the user to be logged in. This is checked here because CustomizerManager doesn't call any callback if the user is not logged in
                    timeToRequest <= 0f)
                {                                     
                    RequestCustomizer();                    
                }
                break;

            case EState.Done:
                // Checks if customizer has to be requested again because former data have expired
                double timeToExpire = GetTimeToExpire();
                if (timeToExpire > 0)
                {
                    timeToExpire -= Time.deltaTime;
                    SetTimeToExpire(timeToExpire);
                }

                if (timeToExpire <= 0f)
                {
                    SetState(EState.WaitingToRequestServer);
                }                
                break;
        }

#if UNITY_EDITOR
        if (DEBUG_ENABLED)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SetTimeToRequest(0f);
                SetState(EState.WaitingToRequestServer);
            }
            /*else if (Input.GetKeyDown(KeyCode.A))
            {
                Apply();
            }*/
        }
#endif
    }

    private void UpdateWaitingToRequestCache()
    {
        if (IsRequestCustomizerAllowed())
        {
#if APPLY_ON_DEMAND            
            bool applyOk = CustomizerManager.SharedInstance.ApplyStoredCustomiserFile();
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("Customizer applied with success: " + applyOk);
            }

            if (applyOk)
            {
                SetHasBeenApplied(true);
            }
            else
            {
                SetState(EState.WaitingToRequestServer);
            }            
#else                 
            SetState(EState.WaitingToRequestServer);
#endif  
        }
    }

    private void RequestCustomizer()
    {        
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Requesting customizer...", true);
        }

        SetState(EState.WaitingForResponse);        
        CustomizerManager.SharedInstance.GetCustomizationsFromServer();
    }

    private bool IsRequestCustomizerAllowed()
    {        
        return !FlowManager.IsInGameScene() &&                  // We don't want this request to interfere with performance when the user is playing a run
               ContentManager.ready;                            // We need the content to be loaded because CustomizerManager may reload some rules
    }

    private bool NeedsToReloadRules()
    {
        return NeedsToRevertAnyFiles() || NeedsToApplyCustomizerChanges();
    }

    private bool NeedsToRevertAnyFiles()
    {
        return m_filesToRevert != null && m_filesToRevert.Count > 0;
    }

    private bool NeedsToApplyCustomizerChanges()
    {
        bool returnValue = !GetHasBeenApplied();
        if (returnValue)
        {
#if APPLY_ON_DEMAND
            returnValue = m_state == EState.WaitingForResponse;
#else                 
            returnValue = m_state == EState.Done && m_filesChangedByCustomizer != null && m_filesChangedByCustomizer.Count > 0;
#endif            
        }

        return returnValue;
    }

    /// <summary>
    /// Returns whether or not the customizer has been received
    /// </summary>
    /// <returns></returns>
    public bool IsReady()
    {
        return m_state == EState.Done;
    }

    /// <summary>
    /// Applies changes to rules if current customizer requires so
    /// </summary>
    /// <returns><c>true</c> if any rules have changed as a consequence of applying the current customizer. <c>false</c> otherwise</returns>
    private bool InternalApply()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Applying customizer needsToRevertAnyFiles = " + NeedsToRevertAnyFiles() + " needsToApplyCustomizerChanges = " + NeedsToApplyCustomizerChanges(), false);
        }

        bool returnValue = false;

        if (NeedsToRevertAnyFiles())
        {
            SetIsNotifyFilesChangedEnabled(false);
            CustomizerManager.SharedInstance.ResetContentToOriginalValues();
            SetIsNotifyFilesChangedEnabled(true);

            if (m_filesToRevert != null)
            {
                m_filesToRevert.Clear();
            }

            returnValue = true;
        }
        
        if (NeedsToApplyCustomizerChanges())
        {
#if APPLY_ON_DEMAND
            bool applyOk = CustomizerManager.SharedInstance.ApplyStoredCustomiserFile();
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("Customizer applied with success: " + applyOk);
            }
#endif            

            SetHasBeenApplied(true);

            returnValue = true;
        }

/*
#if UNITY_EDITOR
        if (DEBUG_ENABLED)
        {
            DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.OFFER_PACKS, "offer_pack_1");
            Log("offer_pack_1 enabled = " + def.GetAsBool("enabled"));
        }
#endif
*/

        return returnValue;
    }

    /// <summary>
    /// Applies changes to rules if current customizer requires so
    /// </summary>
    /// <returns><c>true</c> if any rules have changed as a consequence of applying the current customizer. <c>false</c> otherwise</returns>
    public bool Apply()
    {        
#if APPLY_ON_DEMAND
        return InternalApply();
#else
        bool returnValue = NeedsToReloadRules();
        if (returnValue)
        {
            InternalApply();
        }
        else
        {
            returnValue = GetNeedsToNotifyRulesChanged();
            SetNeedsToNotifyRulesChanged(false);
        }

        if (FeatureSettingsManager.IsDebugEnabled && returnValue)
        {
            Log("Applying changes...", true);
        }

        return returnValue;
#endif
    }

	public bool IsCustomiserPopupAvailable()
	{
		if (m_state == EState.Done)
		{
			return CustomizerManager.SharedInstance.IsCustomiserPopupAvailable(CustomizerManager.eCustomiserPopupType.E_CUSTOMISER_POPUP_UNKNOWN);
		}

		return false;
	}

	public CustomizerManager.CustomiserPopupConfig GetOrRequestCustomiserPopup(string _isoLanguageName)
    {
        CustomizerManager.CustomiserPopupConfig returnValue = null;

        // Makes sure that there's a customizer and that is has already been applied
        if (m_state == EState.Done)
        {
			returnValue = CustomizerManager.SharedInstance.PrepareOrGetCustomiserPopupByType(CustomizerManager.eCustomiserPopupType.E_CUSTOMISER_POPUP_UNKNOWN, _isoLanguageName);
        }

        return returnValue;
    }

	public CustomizerManager.CustomiserPopupConfig GetLastPreparedPopupConfig()
	{		
		CustomizerManager.CustomiserPopupConfig config = m_lastPreparedPopupConfig;
		m_lastPreparedPopupConfig = null;
		return config;
	}

	private void NotifyPopupIsPrepared(CustomizerManager.CustomiserPopupConfig _config) 
	{
		m_lastPreparedPopupConfig = _config;
	}

    private void NotifyFilesChanged(List<string> files)
    {
        if (files != null && files.Count > 0 && m_isNotifyFilesChangedEnabled)
        {
            if (m_filesChangedByCustomizer == null)
            {
                m_filesChangedByCustomizer = new List<string>();
            }

            int count = files.Count;
            for (int i = 0; i < count; i++)
            {
                // Makes sure this file is not already in the list
                if (!m_filesChangedByCustomizer.Contains(files[i]))
                {
                    m_filesChangedByCustomizer.Add(files[i]);
                }
            }
        }
    }

    private void NotifyTimeToEndReceived(long secondsToEnd)
    {
        if (secondsToEnd < GetTimeToExpire())
        {
            SetTimeToExpireModified(true);
            SetTimeToExpire(secondsToEnd);
        }
    }

    private void NotifyTimeToNextReceived(long secondsToNext)
    {
        // Time to next received is stored in Time to Expire because if it's smaller than Time to Expire then we need to request for customizer again
        if (secondsToNext < GetTimeToExpire())
        {
            SetTimeToExpireModified(true);
            SetTimeToExpire(secondsToNext);
        }        
    }        

    private void NotifyCustomizationFinished()
    {
        // Gets the experiment and if it has changed then tracking is notified
        CustomizerManager.Experiment experiment = CustomizerManager.SharedInstance.GetExperiment();
        if (experiment != null && (m_currentExperiment == null || (m_currentExperiment.m_strName == experiment.m_strName && m_currentExperiment.m_strGroupName == experiment.m_strGroupName)))
        {
            Log("New experiment applied: name = " + experiment.m_strName + " groupName = " + experiment.m_strGroupName, true);
            HDTrackingManager.Instance.Notify_ExperimentApplied(experiment.m_strName, experiment.m_strGroupName);            
        }

        m_currentExperiment = experiment;

        SetState(EState.Done);

        if (FeatureSettingsManager.IsDebugEnabled)
        {
            bool needsToApply = m_filesChangedByCustomizer != null && m_filesChangedByCustomizer.Count > 0;
            if (needsToApply)
            {
                Log("Customizer WITH changes received. These changes are PENDING to be applied", true);
            }
            else
            {
                Log("Customizer WITH NO changes received", true);
            }
        }
    }

    public void NotifyServerDown()
    {        
        if (m_state == EState.WaitingForResponse)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("Customizer request cancelled because server is down at " + Time.realtimeSinceStartup);
            }

            // We wait some time before requesting again in order to avoid spamming
            if (GetTimeToRequest() <= 0f)
            {
                SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);
            }

            SetState(EState.WaitingToRequestServer);
        }
    }

    private double GetTimeToExpire()
    {
        return m_timeToExpire;
    }

    private void SetTimeToExpire(double value)
    {
        m_timeToExpire = value;
    }

    private bool GetTimeToExpireModified()
    {
        return m_timeToExpireModified;
    }

    private void SetTimeToExpireModified(bool value)
    {
        m_timeToExpireModified = value;
    }

    private float GetTimeToRequest()
    {
        return m_timeToRequest;
    }    

    private void SetTimeToRequest(float value)
    {       
        m_timeToRequest = value;
    }

    private bool GetHasBeenApplied()
    {
        return m_hasBeenApplied;
    }

    private void SetHasBeenApplied(bool value)
    {
        m_hasBeenApplied = value;
    }    

    private void SetIsNotifyFilesChangedEnabled(bool value)
    {
        m_isNotifyFilesChangedEnabled = value;
    }

    private bool GetNeedsToNotifyRulesChanged()
    {
        return m_needsToNotifyRulesChanged;
    }

    private void SetNeedsToNotifyRulesChanged(bool value)
    {
        m_needsToNotifyRulesChanged = value;
    }

    private void SetState(EState value)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Change state from " + m_state + " to " + value + " at " + Time.realtimeSinceStartup, false);
        }

        switch (m_state)
        {
            case EState.Done:
                if (GetHasBeenApplied())
                {
                    // Files changed by current customizer need to be undone only if those changed had been applied
                    AddFilesChangedByCustomizerToFilesToRevert();
                }                                  
                break;
        }

        m_state = value;

        switch (m_state)
        {            
            case EState.WaitingForResponse:
            case EState.WaitingToRequestCache:
                if (m_filesChangedByCustomizer != null)
                {
                    m_filesChangedByCustomizer.Clear();
                }

                SetHasBeenApplied(false);
                SetIsNotifyFilesChangedEnabled(true);
                SetTimeToExpire(double.MaxValue);
                SetTimeToExpireModified(false);

                // We want to apply customizer changes as soon as possible in order to change rules before the rest of the game access to them
                if (m_state == EState.WaitingToRequestCache)
                {
                    UpdateWaitingToRequestCache();
                }
                break;

            case EState.Done:
#if !APPLY_ON_DEMAND
                bool needsToNotifyRulesChanged = InternalApply();
                // SetNeedsToNotifyRulesChanged(needsToNotifyRulesChanged);
                
                if ( needsToNotifyRulesChanged )
                    OnRulesUpdated();
#endif
                // If there's no customizer then we need to schedule the next request
                if (!GetTimeToExpireModified())
                {
                    if (GetTimeToRequest() <= 0f)
                    {
                        SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);
                    }                    

                    SetState(EState.WaitingToRequestServer);
                }    
                
                
                
                break;     
        }
    }  
    
    private void OnRulesUpdated()
    {
        // Cached data need to be reloaded
        OffersManager.InitFromDefinitions(true);
    }
    

    private void AddFilesChangedByCustomizerToFilesToRevert()
    {        
        if (m_filesChangedByCustomizer != null)
        {
            if (m_filesToRevert == null)
            {
                m_filesToRevert = new List<string>();
            }

            int count = m_filesChangedByCustomizer.Count;
            string file;
            for (int i = 0; i < count; i++)
            {
                file = m_filesChangedByCustomizer[i];
                if (!m_filesToRevert.Contains(file))
                {
                    m_filesToRevert.Add(file);
                }
            }
        }
    }

#region log    
    public static void Log(string msg, bool logToCPConsole=false)
    {
        ControlPanel.Log(msg, ControlPanel.ELogChannel.Customizer, logToCPConsole);
    }

    public static void LogWarning(string msg)
    {
        ControlPanel.LogWarning(msg, ControlPanel.ELogChannel.Customizer);
    }

    public static void LogError(string msg)
    {
        ControlPanel.LogError(msg, ControlPanel.ELogChannel.Customizer);
    }
#endregion
}
