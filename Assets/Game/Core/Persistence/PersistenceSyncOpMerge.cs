using SimpleJSON;
using System;
public class PersistenceSyncOpMerge : PersistenceSyncOp
{
	private PersistenceSyncOp LocalOp { get; set; }
	private PersistenceSyncOp CloudOp { get; set; }
	private PersistenceSyncOpFactory OpFactory { get; set; }

	public void Setup(PersistenceSyncOp localOp, PersistenceSyncOp cloudOp, bool isSilent, PersistenceSyncOpFactory opFactory)
	{
		base.Setup(EType.Merge, null, isSilent);
		LocalOp = localOp;
		CloudOp = cloudOp;
		OpFactory = opFactory;
	}	

	protected override void ExtendedPerform()
	{
		// Check that can be performed
		Sync();
	}

	private void Sync()
	{
		EFlowId currentFlowId = Flow_GetId();
		Flow_ProcessId(currentFlowId);
	}

	private void ResetDataToDefault(PersistenceData data)
	{
		JSONClass defaultPersistence = PersistenceUtils.GetDefaultDataFromProfile(data.Key);
		data.LoadFromString(defaultPersistence.ToString());
	}

	protected override void ExtendedDiscard()
	{
	}

    protected override PersistenceStates.ESyncResult ExtendedCalculateResult()
    {
        return PersistenceStates.ESyncResult.Success;
    }

    public override void Update ()
	{
		if (Syncer != null)
		{
			Syncer.Update();
		}
	}

	#region flow
	private enum EFlowId
	{
		Done,

        // Local
		Error_Local_Load_NotFound,
		Error_Local_Load_Permission,
		Error_Local_Load_Corrupted,
		Error_Local_Save_DiskSpace,
		Error_Local_Save_Permission,

        // Cloud
        Error_Cloud_Not_Connection,
        Error_Cloud_Social_NotLogged,

        // Merge
        Merge
    }

	private EFlowId Flow_GetId()
	{
		EFlowId returnValue = Flow_GetFlowIdBecauseOfLocal();
	
		// If no local flow has to be performed then check if any cloud flow has to be performed
		if (returnValue == EFlowId.Done)
		{
			returnValue = Flow_GetFlowIdBecauseOfCloud();
		}

		return returnValue;
	}

	private EFlowId Flow_GetFlowIdBecauseOfLocal()
	{
		EFlowId returnValue = EFlowId.Done;

		// Check Load data flows first
		if (LocalOp != null)
        {
            LocalOp.CalculateResult();

            if (LocalOp.Result != PersistenceStates.ESyncResult.Success && !LocalOp.IsSilent)
            {                
                switch (LocalOp.Result)
                {
                    case PersistenceStates.ESyncResult.Error_Local_Load_NotFound:
                    {
                        returnValue = EFlowId.Error_Local_Load_NotFound;
                    }
                    break;

                    case PersistenceStates.ESyncResult.Error_Local_Load_Permission:
                    {
                        returnValue = EFlowId.Error_Local_Load_Permission;
                    }
                    break;

                    case PersistenceStates.ESyncResult.Error_Local_Load_Corrupted:
                    {
                        returnValue = EFlowId.Error_Local_Load_Corrupted;
                    }
                    break;                    
                    
                    case PersistenceStates.ESyncResult.Error_Local_Save_DiskSpace:
                    {
                        returnValue = EFlowId.Error_Local_Save_DiskSpace;
                    }
                    break;

                    case PersistenceStates.ESyncResult.Error_Local_Save_Permission:
                    {
                        returnValue = EFlowId.Error_Local_Save_Permission;
                    }
                    break;
                }                                    
            }
		}

		return returnValue;
	}

	private EFlowId Flow_GetFlowIdBecauseOfCloud()
	{
        EFlowId returnValue = EFlowId.Done;

        if (CloudOp != null)
        {
            CloudOp.CalculateResult();

            if (CloudOp.Result != PersistenceStates.ESyncResult.Success && !CloudOp.IsSilent)
            {
                switch (CloudOp.Result)
                {
                    case PersistenceStates.ESyncResult.Error_Cloud_NotConnection:
                    case PersistenceStates.ESyncResult.Error_Cloud_Server_NotLogged:
                    case PersistenceStates.ESyncResult.Error_Cloud_Server_Persistence:                    
                        returnValue = EFlowId.Error_Cloud_Not_Connection;
                        break;

                    case PersistenceStates.ESyncResult.Error_Cloud_Social_NotLogged:
                        returnValue = EFlowId.Error_Cloud_Social_NotLogged;
                        break;

                    case PersistenceStates.ESyncResult.Error_Cloud_Server_MergeShowPopupNeeded:
                        returnValue = EFlowId.Merge;
                        break;
                }
            }
        }
        
		return returnValue;
	}

	private void Flow_ProcessId(EFlowId id)
	{
		switch (id)
		{
			case EFlowId.Done:
			{
				Flow_ProcessDone();	
			}
			break;

			case EFlowId.Error_Local_Load_NotFound:
			{
				Flow_ProcessErrorLocalLoadNotFound();
			}
			break;

			case EFlowId.Error_Local_Load_Permission:
			{
				Flow_ProcessErrorLocalLoadPermission();
			}
			break;

			case EFlowId.Error_Local_Load_Corrupted:
			{
				Flow_ProcessErrorLocalLoadCorrupted();
			}
			break;

			case EFlowId.Error_Local_Save_DiskSpace:
			{
				Flow_ProcessErrorLocalSaveDiskSpace();
			}
			break;

			case EFlowId.Error_Local_Save_Permission:
			{
				Flow_ProcessErrorLocalSavePermission();
			}
			break;

            case EFlowId.Error_Cloud_Not_Connection:
            {
                Flow_ProcessErrorCloudNotConnection();
            }
            break;

            case EFlowId.Error_Cloud_Social_NotLogged:
            {
                Flow_ProcessErrorCloudSocialNotLogged();
            }
            break;

            case EFlowId.Merge:
            {
                Flow_ProcessMerge();
            }
            break;           
        }
	}			

	private void Flow_ProcessDone()
	{
		OnPerformDone();
	}    

    private void Flow_ProcessErrorLocalLoadNotFound()
	{
		// Local persistence reseted to default
		PersistenceData localData = LocalOp.Data;
		ResetDataToDefault(localData);

		// New persistence is saved locally
		PersistenceSyncOp saveLocalOp = OpFactory.GetSaveLocalOp(localData, LocalOp.IsSilent);
		PersistenceSyncOp syncOp = OpFactory.GetSyncOp(saveLocalOp, null, LocalOp.IsSilent);

        Action<PersistenceStates.ESyncResult> onSaved = delegate (PersistenceStates.ESyncResult result)
        {
            // We need to reload the persistence again so any problems can be detected
            Syncer_Sync(LocalOp, null, null);
        };

        Syncer.Sync(saveLocalOp, null, syncOp, onSaved);        
	}

	private void Flow_ProcessErrorLocalLoadPermission()
	{
		Action solveProblem = delegate ()
        {                    
            // We need to try to read local persistence again
			Syncer_Sync(LocalOp, null, null);
        };               

        // A popup asking the user to check internal storage permissions and try again
        PersistenceFacade.Popups_OpenLocalLoadPermissionError(solveProblem);
	}

	private void Flow_ProcessErrorLocalLoadCorrupted()
	{
		PersistenceData localData = LocalOp.Data;
		bool useCloud = false;
		Action solveProblem = delegate() 
		{                                
            // Local persistence has to be reseted to the default one
			ResetDataToDefault(localData);	                        
			PersistenceSyncOp saveLocalOp = OpFactory.GetSaveLocalOp(localData, LocalOp.IsSilent);
			PersistenceSyncOp syncOp = OpFactory.GetSyncOp(saveLocalOp, null, LocalOp.IsSilent);

            Action<PersistenceStates.ESyncResult> onSaved = delegate (PersistenceStates.ESyncResult result)
            {
                // We need to load the persistence again so any problems can be detected
                Syncer_Sync(LocalOp, null, null);
            };

            Syncer.Sync(saveLocalOp, null, syncOp, onSaved);                                  
        };
                
        PersistenceFacade.Popups_OpenLoadSaveCorruptedError(useCloud, solveProblem);                
	}

    private void Flow_ProcessErrorLocalSaveDiskSpace()
    {
        Action solveProblem = delegate ()
        {
            // Try to save again			                       
            Syncer_Sync(LocalOp, null, null);
        };

        PersistenceFacade.Popups_OpenLocalSaveDiskOutOfSpaceError(solveProblem);
    }

    private void Flow_ProcessErrorLocalSavePermission()
	{
		Action solveProblem = delegate() 
		{                                
            // Try to save again			                       
			Syncer_Sync(LocalOp, null, null);                       
        };
                
        PersistenceFacade.Popups_OpenLocalSavePermissionError(solveProblem);                
	}	

    private void Flow_ProcessErrorCloudNotConnection()
    {
        Action onDone = delegate ()
        {
            // Disable message errors so the sync operation finishes
            CloudOp.IsSilent = true;
            Sync();
        };

        // A popup notifying the error is shown. When the user clicks on ok button we just need to close it and let the user keep playing
        PersistenceFacade.Popups_OpenErrorConnection(onDone);
    }

    private void Flow_ProcessErrorCloudSocialNotLogged()
    {
        // No popup should be shown. The op needs to just be over
        CloudOp.IsSilent = true;
        Sync();
    }

    private void Flow_ProcessMerge()
    {
        // No popup should be shown. The op needs to just be over
        CloudOp.IsSilent = true;
        Sync();
    }
    #endregion

    private PersistenceSyncer mSyncer;
	private PersistenceSyncer Syncer 
	{ 
		get 
		{
			if (mSyncer == null)
			{
				mSyncer = new PersistenceSyncer();
			}

			return mSyncer;
		}
	}

	private void Syncer_Sync(PersistenceSyncOp localOp, PersistenceSyncOp cloudOp, 
	                         PersistenceSyncOp syncOp)
	{
		Action<PersistenceStates.ESyncResult> resync = delegate(PersistenceStates.ESyncResult result)
		{
			Sync();
		};

		Syncer.Sync(localOp, cloudOp, syncOp, resync);        
	}
}
