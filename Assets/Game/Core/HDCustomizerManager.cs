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
            HDCustomizerManager.instance.OnCustomizerStored();
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

                CustomizerManager.SharedInstance.SetTimestampGetter( sm_instance.GetTimeMillis );
                CustomizerManager.SharedInstance.SetListener(new HDCustomizerListener());
            }

            return sm_instance;
        }
    }
    
    public long GetTimeMillis()
    {
        return GameServerManager.SharedInstance.GetEstimatedServerTimeAsLong();
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
    /// To not recreate the object Customiser every time, losing temp data, we store it
    /// </summary>
    protected Customiser m_lastCustomizer = null;
    
    /// <summary>
    /// The m current experiment appleid code
    /// </summary>
    private long m_currentExperimentCode = -1;

    /// <summary>
    /// To force an apply the api stores a new customizer
    /// </summary>
    private bool m_forceApply = false;

    /// <summary>
    /// Indicated the customization id used on the last popup viewed
    /// </summary>
    private long m_lastSeenPopup = -1;

    public void Initialise()
    {
        Reset();
        m_lastCustomizer = CustomizerManager.SharedInstance.GetCustomiserForCurrentBuild(); // Get Strored Customizer if any
    }

    public void Reset()
    {          
        CustomizerManager.SharedInstance.ResetContentToOriginalValues();

        if ( m_hasBeenApplied && Application.isPlaying)
            UnApplyCustomizer();
        SetTimeToRequest(0f);
        SetState(EState.WaitingToRequestServer);
        m_lastSeenPopup = -1;
    }

    public void Destroy()
    {
        if (ApplicationManager.IsAlive)
        {
            Reset();
        }
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
                if ( !m_hasBeenApplied && GetTimeToRequest() < 0)
                {
                    SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);   
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
            Customiser customiser = m_lastCustomizer;
            if ( customiser != null )
            {
                if ( CustomizerManager.SharedInstance.IsCustomizerValid( customiser ) )
                {
                    bool applyByExperiment = false;
                    if ( m_hasBeenApplied )
                    {
                        // if there was no experiment, check if there is now
                        if ( m_currentExperimentCode == -1 )
                        {
                            ApiExperiment apiExperiment = CustomizerManager.SharedInstance.GetFirstValidExperiment(customiser);
                            if ( apiExperiment != null )
                            {
                                applyByExperiment = true;   
                            }
                        }
                        // if there was an experiment but it's not valid anymore
                        else
                        {
                            applyByExperiment = !CustomizerManager.SharedInstance.IsExperimentCodeValid(customiser, m_currentExperimentCode);
                        }
                    }
                    
                    if(!m_hasBeenApplied  || ( m_hasBeenApplied && applyByExperiment) || m_forceApply)
                    {
                        
                        UnApplyCustomizer();
                        if (CustomizerManager.SharedInstance.ApplyCustomiser())
                        {
                            m_hasBeenApplied = true;
                            ApiExperiment experiment = CustomizerManager.SharedInstance.GetFirstValidExperiment(customiser);
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

                        if ( m_state == EState.Done )
                        {
                            SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);   
                            SetState(EState.WaitingToRequestServer);
                        }
                    }
                }
            }
            else
            {
                if ( m_hasBeenApplied )
                {
                    UnApplyCustomizer();
                    ContentManager.OnRulesUpdated();
                    if ( m_state == EState.Done )
                    {
                        SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);   
                        SetState(EState.WaitingToRequestServer);
                    }
                }
            }
            m_forceApply = false;
            
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
        bool ret = false;
		if ( m_hasBeenApplied )
		{
			long code = CustomizerManager.SharedInstance.GetCustomizerCodeForAvailablePopup(Calety.Customiser.eCustomiserPopupType.E_CUSTOMISER_POPUP_UNKNOWN);
            if ( code > -1 )
            {
                // Check that we haven't seen it already, as we only allow one popup per session
                ret = code != m_lastSeenPopup;
            }
		}

		return ret;
	}

	public CustomiserPopupConfig GetOrRequestCustomiserPopup(string _isoLanguageName)
    {
        CustomiserPopupConfig returnValue = null;

        // Makes sure that there's a customizer and that is has already been applied
        if (m_hasBeenApplied )
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
     
     /// <summary>
     /// Function called when the game dismisses de popup
     /// </summary>
     /// <param name="_config">Config.</param>
     public void NotifyPopupViewed(CustomiserPopupConfig _config)
     {
        m_lastSeenPopup = _config.m_iCustomizerCode;
        // Notify customizer manager
        CustomizerManager.SharedInstance.DiscardPopupResourcesAndSayToServer(_config, true);
        
     }
     
    private void OnCustomizerStored()
    {
        m_lastCustomizer = CustomizerManager.SharedInstance.GetCustomiserForCurrentBuild(); // Load just stored customizer
        SetState( EState.Done );
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


    private void SetState(EState value)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Change state from " + m_state + " to " + value + " at " + Time.realtimeSinceStartup, false);
        }

       
        m_state = value;
        switch(m_state)
        {
            case EState.Done:
            {
                SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);
            }break;
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
