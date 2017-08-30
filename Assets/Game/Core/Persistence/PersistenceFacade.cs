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
        //m_debugManagerKey = DEBUG_PM_LOCAL_PERMISSION_ERROR;                

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
        Sync_Reset();

        GameServerManager.SharedInstance.Configure();
        GameServerManager.SharedInstance.Auth(null);
    }

    public void Reset()
    {
        Systems_Reset();        
    }    
    
    public void Update()
    {        
        if (Sync_IsRunning())
        {
            Sync_Update();
        }
        else
        {
            // Save is not allowed while syncing
            Save_TimeLeftToSave -= Time.unscaledDeltaTime;
            if (Save_TimeLeftToSave <= 0f)
            {
                Save_Perform();
            }
        }
    }    

    #region sync
    public enum ESyncFrom
    {
        None,
        FirstLoading,
        FromSettings
    }

    private ESyncFrom Sync_From { get; set; }

    private enum ESyncState
    {
        None,
        GettingPersistences,
        Syncing              
    }

    private ESyncState m_syncState;
    private ESyncState Sync_State
    {
        get
        {
            return m_syncState;
        }

        set
        {
            m_syncState = value;
            switch (m_syncState)
            {
                case ESyncState.GettingPersistences:
                    Sync_LoadLocalPersistence();

                    // The application start notification is sent to the TrackingManager if we're in the first loading and the local persistence is ok (if it's corrupted then
                    // some critical data required by tracking are not going to be available, so we have to fix the problem before sending tracking events
                    if (Sync_From == ESyncFrom.FirstLoading && Manager.LocalProgress_Data != null && Manager.LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK)
                    {
                        HDTrackingManager.Instance.Notify_ApplicationStart();
                    }
                    /*
                    GameServerManager.SharedInstance.Configure();
                    GameServerManager.SharedInstance.Auth(null);
                    */
                    break;

                case ESyncState.Syncing:
                    Sync_ProcessSyncing();
                    break;
            }
        }
    }

    private void Sync_LoadLocalPersistence()
    {
        Manager.LocalProgress_Load(LocalPersistence_ActiveProfileID);        
    }

    private void Sync_ProcessSyncing()
    {
        bool cloudPersistenceIsReady = false;
        bool cloudPersistenceIsValid = false;

        // Checks local persistence status
        switch (Manager.LocalProgress_Data.LoadState)
        {
            case PersistenceStates.LoadState.Corrupted:
            {
                bool useCloud = cloudPersistenceIsReady && cloudPersistenceIsValid;

                Action solveProblem = delegate ()
                {
                    if (!useCloud)
                    {
                        // Local persistence has to be reseted to the default one
                        Manager.LocalProgress_ResetToDefault(LocalPersistence_ActiveProfileID, PersistenceUtils.GetDefaultDataFromProfile());
                    }
                    Sync_OnLoadedSuccessfully();
                };

                Popups_OpenLoadSaveCorruptedError(useCloud, solveProblem);                
            }
            break;

            case PersistenceStates.LoadState.NotFound:
            {
                // If it hasn't been found then the default persistence is stored locally and we proces the Syncing state again
                Manager.LocalProgress_ResetToDefault(LocalPersistence_ActiveProfileID, PersistenceUtils.GetDefaultDataFromProfile());
                Sync_ProcessSyncing();
            }
            break;

            case PersistenceStates.LoadState.PermissionError:
            {
                Action solveProblem = delegate ()
                {
                    // We need to try to read local persistence again
                    Sync_LoadLocalPersistence();

                    // And check its status again
                    Sync_ProcessSyncing();
                };

                // A popup asking the user to check internal storage permissions and try again
                Popups_OpenLocalSavePermissionError(solveProblem);
            }
            break;

            case PersistenceStates.LoadState.OK:
            {
                Sync_OnLoadedSuccessfully();
            }
            break;
        }
    }

    private bool Sync_IsRunning() { return Sync_State != ESyncState.None; }
    private Action Sync_OnDone { get; set; }

    private void Sync_Reset()
    {
        Sync_From = ESyncFrom.None;
        Sync_State = ESyncState.None;
        Sync_OnDone = null;
    }

    public void Sync_Persistences(ESyncFrom from, Action onDone)
    {
        if (!Sync_IsRunning())
        {
            AuthManager.Instance.LoadUser();

            Sync_From = from;
            Sync_State = ESyncState.GettingPersistences;
            Sync_OnDone = onDone;                        
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            LogError("Sync is already running");
        }
    }
    

    private void Sync_OnLoadedSuccessfully()
    {
        // Initialize managers needing data from the loaded profile
        GlobalEventManager.SetupUser(UsersManager.currentUser);

        IsLoadCompleted = true;        

        if (Sync_OnDone != null)
        {
            Sync_OnDone();            
        }

        Sync_Reset();
    }

    private void Sync_Update()
    {
        switch (Sync_State)
        {
            case ESyncState.GettingPersistences:
            {
                bool localPersistenceIsReady = Manager.LocalProgress_Data != null;
                bool needsCloudPersistence = false;
                bool cloudPersistenceIsReady = false;

                // Examine local progress
                if (localPersistenceIsReady)
                {
                    switch (Manager.LocalProgress_Data.LoadState)
                    {
                        case PersistenceStates.LoadState.Corrupted:
                            // We need to wait for the cloud response because if the user has saved some progress in the cloud then the user's local progress
                            // can be restored to the cloud progress
                            //needsCloudPersistence = true;                        
                            break;
                    }
                }

                if (localPersistenceIsReady &&
                (!needsCloudPersistence || cloudPersistenceIsReady))
                {
                    Sync_State = ESyncState.Syncing;
                }
            }
            break;            
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
        Manager.LocalProgress_ResetToDefault(LocalPersistence_ActiveProfileID, PersistenceUtils.GetDefaultDataFromProfile());        
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

    #region local_persistence

    // Default persistence profile - it's stored in the player preferences, that way can be set from the editor and read during gameplay
    public static string LocalPersistence_ActiveProfileID
    {
        get { return PlayerPrefs.GetString("activeProfile", PersistenceProfile.DEFAULT_PROFILE); }
        set
        {
            PlayerPrefs.SetString("activeProfile", value);            
        }
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
    private const string DEBUG_PM_LOCAL_NOT_FOUND = "LocalNotFound";
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

            case DEBUG_PM_LOCAL_NOT_FOUND:
            {
                Queue<PersistenceStates.LoadState> states = new Queue<PersistenceStates.LoadState>();
                states.Enqueue(PersistenceStates.LoadState.NotFound);
                manager.ForcedLoadStates = states;
            }
            break;

            case DEBUG_PM_LOCAL_PERMISSION_ERROR:
            {
                Queue<PersistenceStates.LoadState> states = new Queue<PersistenceStates.LoadState>();

                // Two are enqueued so we can test the case where the problem is not fixed and the case where the problem is fixed
                for (int i = 0; i < 2; i++)
                {
                    states.Enqueue(PersistenceStates.LoadState.PermissionError);
                }

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

    /// <summary>
    /// This popup is shown when the access to the local save file is not authorized by the device
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/18%29No+access+to+local+data
    /// </summary>    
    public static void Popups_OpenLocalSavePermissionError(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOAD_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOAD_FAILED_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }
    #endregion    
}
