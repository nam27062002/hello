using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible is an intermediary between the game and <c>CustomizerManager</c>, which is the Calety class responsible for handling customizer related stuff.
/// </summary>
public class HDCustomizerManager
{
    private class HDCustomizerListener : CustomizerManager.CustomizerListener
    {
        public override void onCustomizationError(CustomizerManager.eCustomizerError eError, string strSKU, string strAttrib)
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
                Log("Files changed: " + kChangedContentFiles.ToString());
            }

            HDCustomizerManager.instance.NotifyFilesChanged(kChangedContentFiles);
        }

        public override void onNewPopupReceived() { }

        public override void onPopupIsPrepared(CustomizerManager.CustomiserPopupConfig kPopupConfig) { }

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
                Log("onTimeToEndReceived");
            }

            HDCustomizerManager.instance.NotifyTimeToEndReceived(iSecondsToEnd);
        }

        public override void onTimeToNextReceived(long iSecondsToNext)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("onTimeToNextReceived");
            }

            HDCustomizerManager.instance.NotifyTimeToEndReceived(iSecondsToNext);
        }
    }

    private static HDCustomizerManager sm_instance;

    public static HDCustomizerManager instance
    {
        get
        {
            if (sm_instance == null)
            {
                sm_instance = new HDCustomizerManager();
                CustomizerManager.SharedInstance.SetListener(new HDCustomizerListener());
            }

            return sm_instance;
        }
    }

    private enum EState
    {
        None,        
        WaitingToRequest,
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
    /// Time in seconds left to make customizer expire
    /// </summary>
    private double m_timeToExpire;

    private const float TIME_TO_WAIT_BETWEEN_REQUESTS = 20f;

    /// <summary>
    /// Time in seconds to wait to request customizer
    /// </summary>
    private float m_timeToRequest;

    /// <summary>
    /// Whether or not the changes defined by the current customizer have been applied by the client
    /// </summary>
    private bool m_hasBeenApplied;    

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
        SetState(EState.WaitingToRequest);
    }

    public void Destroy()
    {
        Reset();
    }

    public void Update()
    {
        switch (m_state)
        {
            case EState.WaitingToRequest:
                float timeToRequest = GetTimeToRequest();
                if (timeToRequest > 0f)
                {
                    timeToRequest -= Time.deltaTime;
                    SetTimeToRequest(timeToRequest);
                }

                // Checks if it's a good moment to request the customizer
                if (IsRequestCustomizerAllowed() && timeToRequest <= 0f)
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
                }

                if (timeToExpire < 0f)
                {
                    SetState(EState.WaitingToRequest);
                }                
                break;
        }

#if UNITY_EDITOR
        if (DEBUG_ENABLED)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {                
                SetTimeToRequest(0f);
                SetState(EState.WaitingToRequest);
            }                
        }
#endif
    }

    private void RequestCustomizer()
    {        
        SetState(EState.WaitingForResponse);
        CustomizerManager.SharedInstance.GetCustomizationsFromServer();
    }

    private bool IsRequestCustomizerAllowed()
    {        
        return GameSessionManager.SharedInstance.IsLogged() &&  // CustomizerManager requires the user to be logged in. This is checked here because CustomizerManager doesn't call any callback if the user is not logged in
               !FlowManager.IsInGameScene() &&                  // We don't want this request to interfere with performance when the user is playing a run
               ContentManager.ready;                            // We need the content to be loaded because CustomizerManager may reload some rules
    }

    public bool NeedsToReloadRules()
    {
        return NeedsToRevertAnyFiles() || NeedsToApplyCustomizerChanges();
    }

    private bool NeedsToRevertAnyFiles()
    {
        return m_filesToRevert != null && m_filesToRevert.Count > 0;
    }

    private bool NeedsToApplyCustomizerChanges()
    {
        return !GetHasBeenApplied() && m_state == EState.Done && m_filesChangedByCustomizer != null && m_filesChangedByCustomizer.Count > 0;
    }

    private void NotifyFilesChanged(List<string> files)
    {
        if (files != null && files.Count > 0)
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
        if (secondsToEnd < m_timeToExpire)
        {
            SetTimeToExpire(secondsToEnd);
        }
    }

    private void NotifyTimeToNextReceived(long secondsToNext)
    {
        if (secondsToNext < m_timeToExpire)
        {
            SetTimeToExpire(secondsToNext);
        }
    }

    public void NotifyRulesReloaded()
    {
        if (NeedsToRevertAnyFiles())
        {
            if (m_filesToRevert != null)
            {
                m_filesToRevert.Clear();
            }
        }

        if (NeedsToApplyCustomizerChanges())
        {
            SetHasBeenApplied(true);
        }
    }

    private void NotifyCustomizationFinished()
    {        
        SetState(EState.Done);
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
            SetTimeToRequest(TIME_TO_WAIT_BETWEEN_REQUESTS);
            SetState(EState.WaitingToRequest);
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
            Log("Change state from " + m_state + " to " + value + " at " + Time.realtimeSinceStartup);
        }

        switch (m_state)
        {
            case EState.Done:
                if (GetHasBeenApplied())
                {
                    // Files changed by current customizer need to be undone only if those changed had been applied
                    AddyFilesChangedByCustomizerToFilesToRevert();
                }                
                break;
        }

        m_state = value;

        switch (m_state)
        {            
            case EState.WaitingForResponse:
                if (m_filesChangedByCustomizer != null)
                {
                    m_filesChangedByCustomizer.Clear();
                }

                SetHasBeenApplied(false);

                SetTimeToExpire(double.MaxValue);
                break;            
        }
    }  

    private void AddyFilesChangedByCustomizerToFilesToRevert()
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
    private const bool DEBUG_ENABLED = true;
    private const string LOG_CHANNEL = "[HDCustomizerManager] ";
    public static void Log(string msg)
    {
        msg = LOG_CHANNEL + msg;

        if (DEBUG_ENABLED)
        {
            msg = "<color=yellow>" + msg + "</color>";
        }

        Debug.Log(msg);
    }

    public static void LogWarning(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogWarning(msg);
    }

    public static void LogError(string msg)
    {
        msg = LOG_CHANNEL + msg;
        Debug.LogError(msg);
    }
    #endregion

    #region offline
    // This region is responsible for mocking server response so client can be tested before server
    #endregion
}
