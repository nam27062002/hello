using System;
using System.Collections.Generic;
using UnityEngine;
public class PersistenceFacade : UbiBCN.SingletonMonoBehaviour<PersistenceFacade>
{    
    private PersistenceLocalManager LocalManager;
    private PersistenceCloudManager CloudManager;

    private string m_debugManagersKey = null;   
    
    public bool IsLoadCompleted { get; set; } 

    public void Init()
    {
        // If you want to test a particular flow you just need to set the id of the flow to test to m_debugManagerKey
        m_debugManagersKey = DEBUG_PM_CLOUD_SOCIAL_NOT_LOGIN;

        IsLoadCompleted = false;        

        if (FeatureSettingsManager.IsDebugEnabled && !string.IsNullOrEmpty(m_debugManagersKey))
        {
            // Test a particular flow
            LocalManager = Debug_GetLocalManager(m_debugManagersKey);                        
            CloudManager = Debug_GetCloudManager(m_debugManagersKey);
        }
        else
        {
            LocalManager = new PersistenceLocalManager();
            CloudManager = new PersistenceCloudManager();
        }

        LocalManager.Init();
        CloudManager.Init();

        Systems_Init();
        Save_Reset();
        Sync_Init();        
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
            Save_TimeLeftToSave -= Time.deltaTime;
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
        Launch,
        Settings
    }

    private ESyncFrom Sync_From { get; set; }

    private bool Sync_IsSilent
    {
        get
        {
            return Sync_From == ESyncFrom.Settings;
        }
    }

    private bool Sync_IsCloudLoaded { get; set; }

    private enum ESyncState
    {
        None,
        SavingLocalPersistence,
        GettingPersistences,
        Syncing,
        Error            
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
                case ESyncState.SavingLocalPersistence:
                    Save_Request(true);
                    Sync_State = ESyncState.GettingPersistences;
                    break;

                case ESyncState.GettingPersistences:
                    // Loads the local persistence
                    Sync_LoadLocalPersistence();

                    // Logs in to the cloud to get the cloud persistence
                    CloudManager.Load(Sync_From == ESyncFrom.Launch, Sync_IsSilent, Sync_OnCloudLoaded);
                    break;

                case ESyncState.Syncing:
                    Sync_ProcessSyncing();
                    break;
            }
        }
    }

    private void Sync_LoadLocalPersistence()
    {
        LocalManager.LocalProgress_Load(LocalPersistence_ActiveProfileID);        
    }

    private void Sync_ProcessSyncing()
    {                
        bool cloudPersistenceIsValid = CloudManager.Cloud_Persistence != null && CloudManager.Cloud_Persistence.LoadState == PersistenceStates.LoadState.OK;

        // Checks local persistence status
        switch (LocalManager.LocalProgress_Data.LoadState)
        {            
            case PersistenceStates.LoadState.Corrupted:
            {                
                bool useCloud = Sync_IsCloudLoaded && cloudPersistenceIsValid;

                Action solveProblem = delegate ()
                {                    
                    if (!useCloud)
                    {
                        // Local persistence has to be reseted to the default one
                        LocalManager.LocalProgress_ResetToDefault(LocalPersistence_ActiveProfileID, PersistenceUtils.GetDefaultDataFromProfile());
                    }

                    Sync_State = ESyncState.Syncing;
                };

                Sync_State = ESyncState.Error;
                Popups_OpenLoadSaveCorruptedError(useCloud, solveProblem);                
            }
            break;

            case PersistenceStates.LoadState.NotFound:
            {
                // If it hasn't been found then the default persistence is stored locally and we proces the Syncing state again
                LocalManager.LocalProgress_ResetToDefault(LocalPersistence_ActiveProfileID, PersistenceUtils.GetDefaultDataFromProfile());
                Sync_ProcessSyncing();
            }
            break;

            case PersistenceStates.LoadState.PermissionError:
            {
                Action solveProblem = delegate ()
                {                    
                    // We need to try to read local persistence again
                    Sync_LoadLocalPersistence();
                    Sync_State = ESyncState.Syncing;
                };

                Sync_State = ESyncState.Error;

                // A popup asking the user to check internal storage permissions and try again
                Popups_OpenLocalSavePermissionError(solveProblem);
            }
            break;

            case PersistenceStates.LoadState.OK:
            {
                bool useCloud = Sync_From == ESyncFrom.Settings;
                if (!useCloud || Sync_IsCloudLoaded)
                {
                    Sync_ComparePersistences();                        
                }
            }
            break;
        }        
    }

    private bool Sync_IsRunning() { return Sync_State != ESyncState.None; }
    private Action Sync_OnDone { get; set; }
    
    private PersistenceComparator Sync_Comparator { get; set; }    
    
    private void Sync_Init()
    {
        Sync_Comparator = new HDPersistenceComparator();
        Sync_Reset();
    }

    private void Sync_Reset()
    {
        Sync_From = ESyncFrom.None;
        Sync_State = ESyncState.None;
        Sync_OnDone = null;
        Sync_IsCloudLoaded = false;
    }

    public void Sync_Persistences(ESyncFrom from, Action onDone)
    {
        if (!Sync_IsRunning())
        {
            AuthManager.Instance.LoadUser();

            Sync_Reset();

            Sync_From = from;

            // If there's a local progress then we have to store it before syncing persistences in order to make sure that no progress is lost
            if (LocalManager.LocalProgress_Data != null && LocalManager.LocalProgress_Data.LoadState == PersistenceStates.LoadState.OK)
            {
                Sync_State = ESyncState.SavingLocalPersistence;
            }
            else
            {
                Sync_State = ESyncState.GettingPersistences;
            }

            Sync_OnDone = onDone;                        
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            LogError("Sync is already running");
        }
    }
    

    private void Sync_OnLoadedSuccessfully(bool needsToReload)
    {
        // We've just synced persistences so we can reset the save timer
        Save_Reset();

        if (needsToReload)
        {
            Sync_Reset();
            ApplicationManager.instance.NeedsToRestartFlow = true;
        }
        else
        {
            // The application start notification is sent to the TrackingManager if we're in the first loading and the local persistence is ok (if it's corrupted then
            // some critical data required by tracking are not going to be available, so we have to fix the problem before sending tracking events
            if (Sync_From == ESyncFrom.Launch)
            {
                HDTrackingManager.Instance.Notify_ApplicationStart();
            }

            // Initialize managers needing data from the loaded profile
            GlobalEventManager.SetupUser(UsersManager.currentUser);

            IsLoadCompleted = true;

            if (Sync_OnDone != null)
            {
                Sync_OnDone();
            }

            Sync_Reset();
        }
    }

    private void Sync_Update()
    {
        switch (Sync_State)
        {
            case ESyncState.GettingPersistences:
            {
                bool localPersistenceIsReady = LocalManager.LocalProgress_Data != null;
                bool needsCloudPersistence = Sync_From == ESyncFrom.Settings;                

                // Examine local progress
                if (localPersistenceIsReady)
                {
                    switch (LocalManager.LocalProgress_Data.LoadState)
                    {
                        case PersistenceStates.LoadState.Corrupted:
                            // We need to wait for the cloud response because if the user has saved some progress in the cloud then the user's local progress
                            // can be restored to the cloud progress
                            needsCloudPersistence = true;                        
                            break;
                    }
                }

                if (localPersistenceIsReady &&
                    (!needsCloudPersistence || Sync_IsCloudLoaded))
                {
                    Sync_State = ESyncState.Syncing;
                }
            }
            break;            
        }
    }

    private void Sync_ComparePersistences()
    {
        PersistenceData cloudSave = CloudManager.Cloud_Persistence;
        PersistenceData localSave = LocalManager.LocalProgress_Data;

        PersistenceStates.LoadState localResult = PersistenceStates.LoadState.NotFound;
        PersistenceStates.LoadState cloudResult = PersistenceStates.LoadState.NotFound;

        if (localSave != null)
        {
            localResult = localSave.LoadState;
        }

        if (cloudSave != null)
        {
            cloudResult = cloudSave.LoadState;
        }

        bool needsToReload = false;

        //Both local and cloud okay so now decide on which should override which!
        if (localResult == PersistenceStates.LoadState.OK && cloudResult == PersistenceStates.LoadState.OK)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
                Log("(Sync_ComparePersistences) :: Both local and cloud save are OK");

            PersistenceStates.ConflictState state = Sync_Comparator.CompareSaves(localSave, cloudSave);            

            //TODO we don't actually have a perfect way of telling the saves are the same short of analysis absolutely everything in the save
            //so this case should never come up but it is possible to be the cause of missing progress (ie coins / gems) because we didn'y consider
            //them when comparing saves
            if (state == PersistenceStates.ConflictState.Equal)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("(Sync_ComparePersistences) :: No change to save data detected just using local");

                Sync_ResolveConflict(PersistenceStates.ConflictResult.Local, localSave, cloudSave);
            }
            else if (state == PersistenceStates.ConflictState.UseLocal)
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("(Sync_ComparePersistences) :: Local Save is newer just use local");

                Sync_ResolveConflict(PersistenceStates.ConflictResult.Local, localSave, cloudSave);
            }
            else if (state == PersistenceStates.ConflictState.UseCloud)
            {
                needsToReload = true;

                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("(Sync_ComparePersistences) :: Cloud save is newer just use cloud!");

                Sync_ResolveConflict(PersistenceStates.ConflictResult.Cloud, localSave, cloudSave);
            }
            else
            {
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("(Sync_ComparePersistences) :: Conflict Found - " + state);

                /*
                onSyncConflict(state, m_comparator.GetLocalProgress(), m_comparator.GetCloudProgress(), delegate (ConflictResult result) {

                    ResolveConflict(result, localSave, cloudSave);

                });
                */
            }         
        }
        /*
        else if (localResult == PersistenceStates.LoadState.OK && cloudResult == PersistenceStates.LoadState.Corrupted)
        {
            Debug.Log("SaveGameManager (SyncCloudSave) :: Local OK, Cloud Corrupted");

            //Cloud save was corrupted so show conflict dialog with the ability to only choose local save and override
            m_comparator.CompareSaves(localSave, null);
            onSyncConflict(ConflictState.CloudSaveCorrupt, m_comparator.GetLocalProgress(), null, delegate (ConflictResult conflictResult) {

                ResolveConflict(conflictResult, localSave, cloudSave);

            });
        }

        else if (localResult == LoadState.OK && cloudResult == LoadState.VersionMismatch)
        {
            Debug.Log("SaveGameManager (SyncCloudSave) :: Local OK, Cloud Newer");

            //Cloud save is newer and we can't deal with it so show upgrade needed and disable cloud saving
            m_uploadEnabled = false;
            m_syncCompleteCallback(null, SyncState.UpgradeNeeded);
        }
        else if (localResult == LoadState.Corrupted && cloudResult == LoadState.OK)
        {
            Debug.Log("SaveGameManager (SyncCloudSave) :: Local Corrupted, Cloud OK");

            m_comparator.CompareSaves(null, cloudSave);

            //The local save is corrupted so show the conflict dialog with the ability to only choose cloud save and override
            onSyncConflict(ConflictState.LocalSaveCorrupt, null, m_comparator.GetCloudProgress(), delegate (ConflictResult conflictResult) {

                ResolveConflict(conflictResult, localSave, cloudSave);

            });
        }
        else if (localResult == LoadState.Corrupted && cloudResult == LoadState.VersionMismatch)
        {
            Debug.Log("SaveGameManager (SyncCloudSave) :: Local Corrupted, Cloud Newer");

            m_uploadEnabled = false;
            onSyncConflict(ConflictState.LocalCorruptUpgradeNeeded, null, null, delegate (ConflictResult conflictResult) {

                ResolveConflict(conflictResult, localSave, cloudSave);

            });
        }
        else if (localResult == LoadState.Corrupted && cloudResult == LoadState.Corrupted)
        {
            Debug.Log("SaveGameManager (SyncCloudSave) :: Both Local and Cloud Corrupted");

            //Both are corrupt
            m_uploadEnabled = false;
            m_syncCompleteCallback(null, SyncState.Corrupted);
        }
        else if (localResult == LoadState.PermissionError)
        {
            Debug.Log("SaveGameManager (SyncCloudSave) :: Both Local permission error");

            //There are permission errors with the local save so we need to inform user and disable saving until this is sorted
            m_uploadEnabled = false;
            m_savingEnabled = false;
            m_syncCompleteCallback(null, SyncState.PermissionError);
        }
        else
        {
            Debug.LogError(string.Format("Unsupported sync state! Local state {0}, Corrupt state {1}", localResult, cloudResult));

            m_uploadEnabled = false;
            m_savingEnabled = false;
            m_syncCompleteCallback(null, SyncState.Error);
        }*/

        Sync_OnLoadedSuccessfully(needsToReload);
    }

    private void Sync_ResolveConflict(PersistenceStates.ConflictResult result, PersistenceData localSave, PersistenceData cloudSave)
    {
        switch (result)
        {
            case PersistenceStates.ConflictResult.Cloud:
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("(Sync_ResolveConflict) :: Resolving conflict with cloud save!");

                LocalManager.LocalProgress_Override(cloudSave);                
                break;

            case PersistenceStates.ConflictResult.Local:
                if (FeatureSettingsManager.IsDebugEnabled)
                    Log("SaveGameManager (ResolveConflict) :: Resolving conflict with local save!");

                CloudManager.Cloud_Override(localSave.ToString(), null);

                /*m_comparator.ReconcileData(localSave, cloudSave);

                localSave.Save();

                LoadState localState = LoadSystems(localSave);
                if (localState == LoadState.OK)
                {
                    UploadSave(m_syncUser, localSave, delegate (Error error)
                    {
                        if (error != null && error.GetType() != typeof(UploadDisallowedError))
                        {
                            m_syncCompleteCallback(error, SyncState.Error);
                        }
                        else
                        {
                            m_saveData = localSave;
                            m_syncCompleteCallback(null, SyncState.Successful);
                        }
                    });
                }
                else
                {
                    //TODO may need more specific errors
                    m_syncCompleteCallback(new SyncError("Failed to load resolved cloud save", ErrorCodes.SaveError), SyncState.Error);
                }
                */

                break;
        }
    }

    private void Sync_OnCloudLoaded()
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Sync_OnCloudLoaded loaded");

        Sync_IsCloudLoaded = true;                
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
        LocalManager.LocalProgress_ResetToDefault(LocalPersistence_ActiveProfileID, PersistenceUtils.GetDefaultDataFromProfile());        
    }

    /// <summary>
    /// Request a persistence save.
    /// </summary>
    /// <param name="immediately">When <c>true</c> the save is performed immediately. When <c>false</c> the save is performed at next tick. Use <c>true</c> only 
    /// when you can't wait a tick, for example, when the game is shutting down.</param>
    public void Save_Request(bool immediately=false)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
            Log("Save_Request");        

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
            Log("Save_Perform");        

        Save_Reset();
        LocalManager.LocalProgress_SaveToDisk();        
        CloudManager.Cloud_Save(null);        
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
        LocalManager.Systems_RegisterSystem(Systems_User);

        Systems_Tracking = HDTrackingManager.Instance.TrackingPersistenceSystem;
        if (Systems_Tracking != null)
        {
            LocalManager.Systems_RegisterSystem(Systems_Tracking);
        }
    }

    private void Systems_Reset()
    {
        // If has to be unregistered because another user object will be created every time the user is led to the loading scene
        if (Systems_User != null)
        {
            LocalManager.Systems_UnregisterSystem(Systems_User);
            Systems_User = null;
        }

        if (Systems_Tracking != null)
        {
            Systems_Tracking.Reset();
            LocalManager.Systems_UnregisterSystem(Systems_Tracking);
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
    private const string DEBUG_PM_CLOUD_SERVER_CHECK_CONNECTION = "CloudSeverCheckConnection";
    private const string DEBUG_PM_CLOUD_SERVER_NOT_LOGIN = "CloudSeverNotLogin";
    private const string DEBUG_PM_CLOUD_SOCIAL_NOT_LOGIN = "CloudSocialNotLogin";

    private PersistenceLocalManager Debug_GetLocalManager(string managersKey)
    {                
        PersistenceLocalManagerDebug manager = new PersistenceLocalManagerDebug(managersKey);

        switch (managersKey)
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
        }

        return manager;        
    }

    private PersistenceCloudManager Debug_GetCloudManager(string managersKey)
    {
        PersistenceCloudManagerDebug manager = new PersistenceCloudManagerDebug(managersKey);

        switch (managersKey)
        {
            case DEBUG_PM_CLOUD_SERVER_CHECK_CONNECTION:
            {
                Queue<bool> states = new Queue<bool>();

                // Two are enqueued so we can test it from launcher and from settings
                for (int i = 0; i < 2; i++)
                {
                    states.Enqueue(false);
                }
                manager.Server_ForcedCheckConnection = states;
            }
            break;

            case DEBUG_PM_CLOUD_SERVER_NOT_LOGIN:
            {
                Queue<bool> states = new Queue<bool>();

                // Two are enqueued so we can test it from launcher and from settings
                for (int i = 0; i < 2; i++)
                {
                    states.Enqueue(false);
                }
                manager.Server_ForcedLogIn = states;
            }
            break;

            case DEBUG_PM_CLOUD_SOCIAL_NOT_LOGIN:
            {
                Queue<bool> states = new Queue<bool>();

                // Two are enqueued so we can test it from launcher and from settings
                for (int i = 0; i < 2; i++)
                {
                    states.Enqueue(false);
                }
                manager.Social_ForcedLogIn = states;
            }
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

    /// <summary>
    /// This popup is shown when there's no internet connection
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/29%29No+internet+connection
    /// </summary>    
    public static void Popups_OpenErrorConnection(Action onConfirm)
    {
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SOCIAL_ERROR_CONNECTION_NAME";
        config.MessageTid = "TID_SOCIAL_ERROR_CONNECTION_DESC";
        config.MessageParams = new string[] { SocialPlatformManager.SharedInstance.GetPlatformName() };
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
    }
    #endregion    
}
