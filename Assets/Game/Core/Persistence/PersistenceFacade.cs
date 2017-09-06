using UnityEngine;
using System;
public class PersistenceFacade : UbiBCN.SingletonMonoBehaviour<PersistenceFacade>
{		 
    private PersistenceFacadeConfig Config { get; set; }   

    public void Init()
    {
        PersistenceFacadeConfigDebug.EUserCaseId userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Production;
        userCaseId = PersistenceFacadeConfigDebug.EUserCaseId.Error_Local_Save_Permission;
        if (FeatureSettingsManager.IsDebugEnabled && userCaseId != PersistenceFacadeConfigDebug.EUserCaseId.Production)
        {
            Config = new PersistenceFacadeConfigDebug(userCaseId);
        }
        else
        {
            Config = new PersistenceFacadeConfig();
        }

        Sync_IsFromLaunchApplicationDone = false;
        Sync_Init();
        Local_Init();
        Save_Init();
    }
    
    protected void Update()
    {
        if (Sync_Syncer != null)
        {
            Sync_Syncer.Update();
        }

        Save_Update();
	}

	#region sync
	private PersistenceSyncer Sync_Syncer { get; set; }

	public PersistenceData Sync_LocalData { get; set; }
	public PersistenceData Sync_CloudData { get; set; }

	public bool Sync_AreDataInSync { get; set; }

    private bool Sync_IsLoadingCloudFromLaunch { get; set; }

	private void Sync_Init()
	{
        Sync_IsLoadingCloudFromLaunch = false;
        Sync_AreDataInSync = false;

        if (Sync_Syncer == null)
        {
            Sync_Syncer = new PersistenceSyncer();
        }
        else
        {
            Sync_Syncer.Reset(true);
        }

        string dataName = PersistencePrefs.ActiveProfileName;
        Sync_LocalData = new PersistenceData(dataName);
        Sync_CloudData = new PersistenceData(dataName);        
    }

    public bool Sync_IsFromLaunchApplicationDone { get; set; }    

	public void Sync_FromLaunchApplication(Action<PersistenceStates.ESyncResult> onDone)
	{
        // Local load is done first in order to make sure the application is launched as quick as possible
        PersistenceSyncOpFactory factory = Config.SyncFromLaunchFactory;
		PersistenceSyncOp localOp = factory.GetLoadLocalOp(Sync_LocalData);
        PersistenceSyncOp syncOp = factory.GetSyncOp(localOp, null, false);

        Action<PersistenceStates.ESyncResult> onLocalLoad = delegate (PersistenceStates.ESyncResult result)
        {
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                Log("FROM LAUNCH: Local loaded");
            }

            Sync_IsFromLaunchApplicationDone = true;
            Sync_IsLoadingCloudFromLaunch = true;
            if (onDone != null)
            {
                onDone(result);
            }

            Action<PersistenceStates.ESyncResult> onCloudLoad = delegate (PersistenceStates.ESyncResult r)
            {
                Sync_IsLoadingCloudFromLaunch = false;

                if (FeatureSettingsManager.IsDebugEnabled)
                {
                    Log("FROM LAUNCH: CLOUD loaded");
                }
            };

            // It needs to load the cloud and merge. We use a dummy local load op since it's already loaded. We just want to provide the syncer with the local data
            localOp = factory.GetLoadLocalOp(Sync_LocalData, false);
            PersistenceSyncOp cloudOp = factory.GetLoadCloudOp(Sync_CloudData, true);
            syncOp = factory.GetLoadCloudOp(Sync_CloudData, true);
            Sync_Perform(localOp, cloudOp, syncOp, onCloudLoad);
        };

        Sync_Perform(localOp, null, syncOp, onLocalLoad);       
    }

	public void Sync_FromSettings(Action<PersistenceStates.ESyncResult> onDone)
	{
        PersistenceSyncOpFactory factory = Config.SyncFromSettingsFactory;

        // We need to save the current progress so the user won't lose it during the process
        PersistenceSyncOp localOp = factory.GetSaveLocalOp(Sync_LocalData, false);
		PersistenceSyncOp cloudOp = factory.GetLoadCloudOp(Sync_CloudData, false);
		PersistenceSyncOp syncOp = factory.GetSyncOp(localOp, cloudOp, false);
		Sync_Perform(localOp, cloudOp, syncOp, onDone);
	}

	private void Sync_Perform(PersistenceSyncOp localOp, PersistenceSyncOp cloudOp, 
	                          PersistenceSyncOp syncOp, Action<PersistenceStates.ESyncResult> onDone)
	{
		Action<PersistenceStates.ESyncResult> onSyncDone = delegate(PersistenceStates.ESyncResult result)
		{
			if (onDone != null)
			{
				onDone(result);
				onDone = null;
			}

            // Syncs are considered in sync if all ops have been successful
            Sync_AreDataInSync = (localOp != null && localOp.Result == PersistenceStates.ESyncResult.Success &&
                                  cloudOp != null && cloudOp.Result == PersistenceStates.ESyncResult.Success &&
                                  syncOp != null && syncOp.Result == PersistenceStates.ESyncResult.Success);

            Log("Sync DONE AreDataInSync = " + Sync_AreDataInSync);
		};

		Sync_Syncer.Sync(localOp, cloudOp, syncOp, onSyncDone);
	}

    public bool Sync_IsSyncing()
    {
        return Sync_Syncer.IsSyncing();
    }
	#endregion

	#region save
    private float Save_TimeToPerform { get; set; }

    private bool Save_IsPerforming { get; set; }

    private void Save_Init()
    {
        Save_TimeToPerform = -1;
        Save_IsPerforming = false;
    }

	public void Save_Request(bool immediate=false)
	{        
		if (immediate)
		{
            Save_Perform();
		}
        else
        {
            Save_TimeToPerform = 0f;
        }
	}

    /// <summary>
    /// Performs save game. Only local save supported so far
    /// </summary>    
	private void Save_Perform()
	{
        if (FeatureSettingsManager.IsDebugEnabled)
        {        
            Log("SAVE Trying to save");            
        }

        Save_TimeToPerform = -1;

        // Since the cloud load is not blocker because we want the application to start as soon as the local persistence has been loaded then, taking into condiseration that loading the 
        // cloud persistence can take long, it could happen that a Save is requested. In this case we just save the local persistence in order to avoid any losses
        if (Sync_IsLoadingCloudFromLaunch)
        {
            // It's saved only if the local persistence is not locked
            if (!Sync_Syncer.IsLocalLocked())
            {
                Sync_LocalData.Save();
                Save_OnPerformed(true);
            }
        }
        else
        {        
            PersistenceSyncOpFactory factory = Config.SaveFactory;
            PersistenceSyncOp localOp = factory.GetSaveLocalOp(Sync_LocalData, false);
            PersistenceSyncOp cloudOp = null;//factory.GetLoadCloudOp(Sync_CloudData, false, true);
            PersistenceSyncOp syncOp = factory.GetSyncOp(localOp, cloudOp, false);

            Action<PersistenceStates.ESyncResult> onSaveDone = delegate (PersistenceStates.ESyncResult result)
            {
                Save_OnPerformed(result == PersistenceStates.ESyncResult.Success);
            };

            Sync_Perform(localOp, cloudOp, syncOp, onSaveDone);
        }        
	}

    private void Save_OnPerformed(bool success)
    {
        if (FeatureSettingsManager.IsDebugEnabled)
        {
            if (success)
            {
                Log("SAVE completed successfully");
            }
            else
            {
                LogWarning("SAVE couldn't be completed successfully");
            }
        }

        // If success then 
        Save_TimeToPerform = (success) ? -1 : 30f;
    }

    private void Save_Update()
    {
        if (Save_TimeToPerform > -1)
        {
            Save_TimeToPerform -= Time.deltaTime;
            if (Save_TimeToPerform < 0)
            {                
                Save_Perform();
            }
        }
    }
	#endregion

	#region local
	public UserPersistenceSystem Local_UserPersistenceSystem { get; set; }

	private void Local_Init()
	{
        Local_UserPersistenceSystem = UsersManager.currentUser;
        Sync_LocalData.Systems_RegisterSystem(Local_UserPersistenceSystem);        

        TrackingPersistenceSystem trackingSystem = HDTrackingManager.Instance.TrackingPersistenceSystem;
        if (trackingSystem != null)
        {
            Sync_LocalData.Systems_RegisterSystem(trackingSystem);            
        }        
	}

    /// <summary>
    /// Resets the current local persistence to the default one. This method should be called only for DEBUG purposes.
    /// </summary>
    public void Local_ResetToDefault()
    {
        if (Sync_LocalData != null)
        {
            SimpleJSON.JSONClass defaultPersistence = PersistenceUtils.GetDefaultDataFromProfile(Sync_LocalData.Key);
            Sync_LocalData.LoadFromString(defaultPersistence.ToString());

            // Saves the new local persistence. We don't need to use any syncer because this method is called only for debug purposes
            Sync_LocalData.Save();
        }
    }
	#endregion

	#region social
	public bool Social_IsLoggedIn()
	{
		return false;
	}
	#endregion	

	#region popups
	/// <summary>
    /// This popup is shown when the local save is corrupted when the game was going to continue locally
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/20%29Local+save+corrupted
    /// </summary>
    /// <param name="cloudEver">Whether or not the user has synced with server</param>    
	public static void Popups_OpenLoadSaveCorruptedError(bool cloudEver, Action onConfirm)
	{        
		PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME";
        config.MessageTid = (cloudEver) ? "TID_SAVE_ERROR_LOCAL_CORRUPTED_OFFLINE_DESC" : "TID_SAVE_ERROR_LOCAL_CORRUPTED_DESC";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);        

        //Popup popup = FlowController.Instance.OpenPopup("Local save corrupted. Reset?");
		//popup.AddButton("Ok", onConfirm, true);
	}

	/// <summary>
    /// This popup is shown when the access to the local save file is not authorized by the device
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/18%29No+access+to+local+data
    /// </summary>    
    public static void Popups_OpenLocalLoadPermissionError(Action onConfirm)
    {		
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_LOAD_FAILED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_LOAD_FAILED_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
        
		//Popup popup = FlowController.Instance.OpenPopup("No permission to load local persistence. Try again?");
		//popup.AddButton("Retry", onConfirm, true);
    }

	/// <summary>
    /// This popup is shown when starting the game if there's no free disk space to store the local save
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/27%29No+disk+space
    /// </summary>    
    public static void Popups_OpenLocalSaveDiskOutOfSpaceError(Action onConfirm)
    {		
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_SPACE_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
     
		//Popup popup = FlowController.Instance.OpenPopup("No space to save local persistence. Try again?");
		//popup.AddButton("Retry", onConfirm, true);
    }

    /// <summary>
    /// This popup is shown when starting the game if there's no access to disk to store the local save.
    /// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/28%29No+disk+access
    /// </summary>    
    public static void Popups_OpenLocalSavePermissionError(Action onConfirm)
    {		
        PopupMessage.Config config = PopupMessage.GetConfig();
        config.TitleTid = "TID_SAVE_ERROR_DISABLED_NAME";
        config.MessageTid = "TID_SAVE_ERROR_DISABLED_ACCESS_DESC";
        config.ConfirmButtonTid = "TID_GEN_RETRY";
        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
        config.OnConfirm = onConfirm;
        config.IsButtonCloseVisible = false;
        PopupManager.PopupMessage_Open(config);
     
		//Popup popup = FlowController.Instance.OpenPopup("No permission to save local persistence. Try again?");
		//popup.AddButton("Retry", onConfirm, true);
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
        
		//Popup popup = FlowController.Instance.OpenPopup("No connection.");
		//popup.AddButton("Ok", onConfirm, true);
    } 
    #endregion    

	#region log
	private const string LOG_CHANNEL = "Persistence:";

    public static void Log(string msg)
    {
        Debug.Log(LOG_CHANNEL + msg);
    }

    public static void LogError(string msg)
    {
        Debug.LogError(LOG_CHANNEL + msg);
    }

	public static void LogWarning(string msg)
    {
        Debug.LogWarning(LOG_CHANNEL + msg);
    }
	#endregion
}


