using System;
using System.Collections.Generic;
using UnityEngine;
public class PersistenceFacade : UbiBCN.SingletonMonoBehaviour<PersistenceFacade>
{
    private GameProgressManager Manager;
    private string m_debugManagerKey = null;   
    
    public bool IsLoadCompleted { get; set; } 

    public void Init()
    {
        // If you want to test a particular flow you just need to set the id of the flow to test to m_debugManagerKey
        //m_debugManagerKey = DEBUG_PM_LOCAL_CORRUPTED;        

        IsLoadCompleted = false;

        if (FeatureSettingsManager.IsDebugEnabled && !string.IsNullOrEmpty(m_debugManagerKey))
        {
            // Test a particular flow
            Manager = Debug_GetManager(m_debugManagerKey);            
            if (Manager == null)
            {
                LogError("No manager defined for " + m_debugManagerKey);
            }
        }
        else
        {
            Manager = new PersistenceManagerImp();            
        }

        Manager.Init();

        Systems_Init();
        Save_Reset();

        GameServerManager.SharedInstance.Configure();
        GameServerManager.SharedInstance.Auth(null);
    }

    public void Reset()
    {
        Systems_Reset();        
    }    
    
    public void Update()
    {
        Save_TimeLeftToSave -= Time.unscaledDeltaTime;
        if (Save_TimeLeftToSave <= 0f)
        {
            Save_Perform();
        }
    }

    #region sync
    public void Sync_GameProgress(Action onDone)
    {
        AuthManager.Instance.LoadUser();

        PersistenceStates.LoadState localState = Manager.LocalProgress_Load();
        Sync_ProcessLocalProgress(localState, onDone);        
    }

    private void Sync_ProcessLocalProgress(PersistenceStates.LoadState localState, Action onDone)
    {
        Action solveConflict = delegate ()
        {
            // Local persistence has to be reseted to the default one
            localState = Manager.LocalProgress_ResetToDefault();
            Sync_ProcessLocalProgress(localState, onDone);
        };

        switch (localState)
        {            
            case PersistenceStates.LoadState.Corrupted:
                Popups_OpenLoadSaveCorruptedError(false, solveConflict);
                break;

            case PersistenceStates.LoadState.OK:
                Sync_OnLoadedSuccessfully(onDone);
                break;
        }        
    }

    private void Sync_OnLoadedSuccessfully(Action onDone)
    {
        // Initialize managers needing data from the loaded profile
        GlobalEventManager.SetupUser(UsersManager.currentUser);

        IsLoadCompleted = true;

        if (onDone != null)
        {
            onDone();
        }
    }
    #endregion

    #region save
    // This region is responsible for saving persistence, which is saved periodically or on demand

    /// <summary>
    /// Time in seconds between persistence saves. Persistence is saved periodically every SAVE_PERIOD_TIME seconds
    /// </summary>
    private const float SAVE_PERIOD_TIME = 60.0f;

    /// <summary>
    /// Time left in seconds for the next persistence save
    /// </summary>
    private float Save_TimeLeftToSave { get; set; }

    private enum ESaveType
    {
        Automatic,
        Requested
    }

    private void Save_Reset()
    {
        Save_TimeLeftToSave = SAVE_PERIOD_TIME;
    }

    public void Save_ResetToDefault()
    {
        Manager.LocalProgress_ResetToDefault();
        Save_Request(true);
    }

    /// <summary>
    /// Request a persistence save.
    /// </summary>
    /// <param name="immediately">When <c>true</c> the save is performed immediately. When <c>false</c> the save is performed at next tick. Use <c>true</c> only 
    /// when you can't wait a tick, for example, when the game is shutting down.</param>
    public void Save_Request(bool immediately=false)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Save_Request");
        }

        // Immediately means that we need to perform the persistence save right now because next tick might not happen (exemple when the game is shutting down),
        // otherwise we delay the save until the next tick. This way several requests done in the same tick will be resolved with a single save
        if (immediately)
        {
            Save_Perform();
        }
        else
        {
            Save_TimeLeftToSave = 0f;
        }
    }

    private void Save_Perform()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            Log("Save_Perform");
        }

        Save_Reset();
        Manager.LocalProgress_SaveToDisk();        
    }
    #endregion

    #region systems
    private UserPersistenceSystem Systems_User { get; set; }
    private TrackingPersistenceSystem Systems_Tracking { get; set; }

    private void Systems_Init()
    {        
        Systems_User = UsersManager.currentUser;
        Manager.Systems_RegisterSystem(Systems_User);

        Systems_Tracking = HDTrackingManager.Instance.TrackingPersistenceSystem;
        if (Systems_Tracking != null)
        {
            Manager.Systems_RegisterSystem(Systems_Tracking);
        }
    }

    private void Systems_Reset()
    {
        // If has to be unregistered because another user object will be created every time the user is led to the loading scene
        if (Systems_User != null)
        {
            Manager.Systems_UnregisterSystem(Systems_User);
            Systems_User = null;
        }

        if (Systems_Tracking != null)
        {
            Systems_Tracking.Reset();
            Manager.Systems_UnregisterSystem(Systems_Tracking);
            Systems_Tracking = null;
        }
    }
    #endregion

    #region debug
    private const string LOG_CHANNEL = "PersistenceFacade:";

    private void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }

    private void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }

    private const string DEBUG_PM_LOCAL_CORRUPTED = "LocalCorrupted";
    private const string DEBUG_PM_LOCAL_PERMISSION_ERROR = "LocalPermissionError";    

    private GameProgressManager Debug_GetManager(string managerKey)
    {                
        PersistenceManagerDebug manager = new PersistenceManagerDebug(managerKey);

        switch (managerKey)
        {
            case DEBUG_PM_LOCAL_CORRUPTED:
            {
                Queue<PersistenceStates.LoadState> states = new Queue<PersistenceStates.LoadState>();
                states.Enqueue(PersistenceStates.LoadState.Corrupted);
                manager.ForcedLoadStates = states;
            }
            break;

            case DEBUG_PM_LOCAL_PERMISSION_ERROR:
            {
                Queue<PersistenceStates.LoadState> states = new Queue<PersistenceStates.LoadState>();
                states.Enqueue(PersistenceStates.LoadState.PermissionError);
                manager.ForcedLoadStates = states;
            }
            break;

            default:
                manager = null;
                break;
        }

        return manager;        
    }
    #endregion

    #region popups
    /// <summary>
    /// This popup is shown when the local save is corrupted when the game was going to continue locally
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/20%29Local+save+corrupted
    /// </summary>
    /// <param name="cloudEver">Whether or not the user has synced with server</param>    
    private void Popups_OpenLoadSaveCorruptedError(bool cloudEver, Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = (cloudEver) ? "TID_SAVE_ERROR_LOCAL_CORRUPTED_OFFLINE_DESC" : "TID_SAVE_ERROR_LOCAL_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }
    #endregion
}
