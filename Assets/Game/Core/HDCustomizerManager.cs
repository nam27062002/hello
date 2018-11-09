// When enabled the customizer is applied when Apply() is called by the game
// When disabled the customizer is applied as soon as the response is received by server. This is Calety's original implementation.
//#define APPLY_ON_DEMAND

using System.Collections.Generic;
using UnityEngine;
using Calety.Customiser;
using Calety.Customiser.Api;
/// <summary>
/// This class is responsible is an intermediary between the game and <c>CustomizerManager</c>, which is the Calety class responsible for handling customizer related stuff.
/// </summary>
public class HDCustomizerManager
{
    private class HDCustomizerListener : Calety.Customiser.CustomizerListener
    {
        public override void onCustomizationError(eCustomizerError eError, string strSKU, string strAttrib)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                LogWarning("Error " + eError.ToString() + " when processing category: " + strSKU + " attribute: " + strAttrib);
            }
        }

        public override void onCustomizationStored () 
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                LogWarning("onCustomizationStored");
            }
            HDCustomizerManager.instance.ForceApplyOnNextCheck();
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

        }

        public override void onNewPopupReceived() { }


        public override void onPopupIsPrepared(CustomiserPopupConfig kPopupConfig)
		{
			HDCustomizerManager.instance.NotifyPopupIsPrepared(kPopupConfig);
		}

        public override void onCustomizationFinished()
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onCustomizationFinished");
            }
        }

        public override void onTimeToEndReceived(long iSecondsToEnd)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onTimeToEndReceived " + iSecondsToEnd);
            }
        }

        public override void onTimeToNextReceived(long iSecondsToNext)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onTimeToNextReceived " + iSecondsToNext);
            }
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

                CustomizerManager.SharedInstance.Initialise();
                CustomizerManager.SharedInstance.SetListener(new HDCustomizerListener());
            }

            return sm_instance;
        }
    }
    
    private enum EState
    {
        None,       
        WaitingToRequestServer,
        WaitingForResponse,
        Done
    };

    private EState m_state;


	/// <summary>
	/// At this point we only support one popup. This will have a reference after the callback from CustomizerMAnager is received.
	/// </summary>
	private CustomiserPopupConfig m_lastPreparedPopupConfig;

    private const float TIME_TO_WAIT_BETWEEN_REQUESTS = (DEBUG_ENABLED) ? 20f : 10 * 60f;

    /// <summary>
    /// Time in seconds to wait to request customizer
    /// </summary>
    private float m_timeToRequest;

    /// <summary>
    /// Whether or not the changes defined by the current customizer have been applied by the client
    /// </summary>
    private bool m_hasBeenApplied = false;
    
    /// <summary>
    /// The m current experiment appleid code
    /// </summary>
    private long m_currentExperimentCode = -1;

    /// <summary>
    /// To force an apply the api stores a new customizer
    /// </summary>
    private bool m_forceApply = false;

    public void Initialise()
    {
        Reset();
    }

    public void Reset()
    {        
        if ( m_hasBeenApplied )
            UnApplyCustomizer();
        SetTimeToRequest(0f);
        SetState(EState.WaitingToRequestServer);
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

    
    public void CheckAndApply()
    {
        if ( CustomizerManager.SharedInstance.CheckIfInitialised() )
        {
            Customiser customiser = CustomizerManager.SharedInstance.GetCustomiserForCurrentBuild();
            if ( customiser != null )
            {
                if ( customiser.IsValid())
                {
                    if(!m_hasBeenApplied  || ( m_hasBeenApplied && ! customiser.IsExperimentCodeValid( m_currentExperimentCode ) ) || m_forceApply)
                    {
                        m_forceApply = false;
                        UnApplyCustomizer();
                        if (CustomizerManager.SharedInstance.ApplyCustomiser())
                        {
                            m_hasBeenApplied = true;
                            ApiExperiment experiment = customiser.GetFirstValidExperiment();
                            if (experiment != null)
                            {
                                
                                Log("New experiment applied: name = " + experiment.GetName() + " groupName = " + experiment.GetGroupName(), true);
                                HDTrackingManager.Instance.Notify_ExperimentApplied(experiment.GetName(), experiment.GetGroupName());
                                m_currentExperimentCode = experiment.GetCode();
                            }
                        }
                        ContentManager.OnRulesUpdated();
                    }
                }
                else
                {
                    if ( m_hasBeenApplied )
                    {
                        UnApplyCustomizer();
                        ContentManager.OnRulesUpdated();

                        // request customizer
                        SetState(EState.WaitingToRequestServer);
                    }
                }
            }
            else
            {
                if ( m_hasBeenApplied )
                {
                    UnApplyCustomizer();
                    ContentManager.OnRulesUpdated();
                }
            }
            
        }
    }
    
    private void UnApplyCustomizer()
    {
        CustomizerManager.SharedInstance.ResetContentToOriginalValues();
        m_hasBeenApplied = false;
        m_currentExperimentCode = -1;
        // if ( trackRemove ) Tell tracking there is no experiment
        // Remove Experiment Code m_experimentCode = "";
        
    }

	public bool IsCustomiserPopupAvailable()
	{
		if (m_state == EState.Done)
		{
			return CustomizerManager.SharedInstance.IsCustomiserPopupAvailable(Calety.Customiser.eCustomiserPopupType.E_CUSTOMISER_POPUP_UNKNOWN);
		}

		return false;
	}

	public CustomiserPopupConfig GetOrRequestCustomiserPopup(string _isoLanguageName)
    {
        CustomiserPopupConfig returnValue = null;

        // Makes sure that there's a customizer and that is has already been applied
        if (m_state == EState.Done)
        {
			returnValue = CustomizerManager.SharedInstance.PrepareOrGetCustomiserPopupByType(Calety.Customiser.eCustomiserPopupType.E_CUSTOMISER_POPUP_UNKNOWN, _isoLanguageName);
        }

        return returnValue;
    }

	public CustomiserPopupConfig GetLastPreparedPopupConfig()
	{		
		CustomiserPopupConfig config = m_lastPreparedPopupConfig;
		m_lastPreparedPopupConfig = null;
		return config;
	}

	private void NotifyPopupIsPrepared(CustomiserPopupConfig _config) 
	{
		m_lastPreparedPopupConfig = _config;
	}
     
    private void ForceApplyOnNextCheck()
    {
        m_forceApply = true;
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


    private void SetState(EState value)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Change state from " + m_state + " to " + value + " at " + Time.realtimeSinceStartup, false);
        }

       
        m_state = value;
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
