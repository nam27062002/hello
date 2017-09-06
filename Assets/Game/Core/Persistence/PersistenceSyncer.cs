using System;
using System.Collections.Generic;
public class PersistenceSyncer
{
	private enum EState
	{
		None,
		Starting,
		WaitingForLocalOp,
		WaitingForCloudOp,
		Syncing,
		Disabled
	};

	private EState mState;
	private EState State 
	{
		get 
		{
			return mState;
		}

		set 
		{
			if (mState != value)
			{
				mState = value;

				switch (mState)
				{
					case EState.Starting:
					{					
						if (LocalOp != null)
						{
							LocalOp.Perform();
						}

						if (CloudOp != null)
						{
							CloudOp.Perform();						
						}		

						State = EState.WaitingForLocalOp;
					}
					break;

					case EState.Syncing:
					{
						if (SyncOp != null)
						{
							SyncOp.Perform();
						}                        
					}
					break;

					case EState.Disabled:
					{
						int count = Ops.Count;
						for (int i = 0; i < count; i++)
						{
							Ops[i].Discard();
						}						

						Notify_OnDone(PersistenceStates.ESyncResult.Error_Disabled);					
					}
					break;
				}
			}
		}
	}

	private Action<PersistenceStates.ESyncResult> OnDone { get; set; }

	private enum EOpType
	{
		LocalOp,
		CloudOp,
		SyncOp
	};

	private List<PersistenceSyncOp> Ops { get; set;}

	private PersistenceSyncOp LocalOp 
	{
		get 
		{
			return Ops[(int)EOpType.LocalOp];
		}

		set 
		{
			Ops[(int)EOpType.LocalOp] = value;
		}
	}

	private PersistenceSyncOp CloudOp 
	{
		get 
		{
			return Ops[(int)EOpType.CloudOp];
		}

		set 
		{
			Ops[(int)EOpType.CloudOp] = value;
		}
	}

	private PersistenceSyncOp SyncOp
	{
		get 
		{
			return Ops[(int)EOpType.SyncOp];
		}

		set 
		{
			Ops[(int)EOpType.SyncOp] = value;
		}
	}

	public PersistenceSyncer ()
	{
		int count = Enum.GetValues(typeof(EOpType)).Length;
		Ops = new List<PersistenceSyncOp>();
		for (int i = 0; i < count; i++)
		{
			Ops.Add(null);
		}
	}

	public void Reset(bool full)
	{
		OnDone = null;
		ResetOps ();

		if (full)
		{
			State = EState.None;
		}
	}

	private void ResetOps()
	{
		int count = Ops.Count;
		for (int i = 0; i < count; i++)
		{
			if (Ops[i] != null)
			{
				//Ops[i].Discard();
				Ops[i] = null;
			}
		}
	}

	public void Enable()
	{
		if (State == EState.Disabled)
		{
			State = EState.None;
		} 
		else if (FeatureSettingsManager.IsDebugEnabled)
		{
			PersistenceFacade.LogWarning("Syncer was already enabled!!");
		}
	}

	public void Disable ()
	{
		if (State != EState.Disabled)
		{
			State = EState.Disabled;
		} 
		else if (FeatureSettingsManager.IsDebugEnabled)
		{
			PersistenceFacade.LogWarning("Syncer was already disabled!!");
		}
	}

	public void Sync(PersistenceSyncOp localOp, PersistenceSyncOp cloudOp, 
	                 PersistenceSyncOp syncOp, Action<PersistenceStates.ESyncResult> onDone)
	{
		if (State == EState.None)
		{
			Reset(false);

			LocalOp = localOp;
			CloudOp = cloudOp;
			SyncOp = syncOp;
			OnDone = onDone;

			State = EState.Starting;
		}
		else if (onDone != null)
		{
			PersistenceStates.ESyncResult result = (State == EState.Disabled) ? 
				PersistenceStates.ESyncResult.Error_Disabled : PersistenceStates.ESyncResult.Error_Already_Syncing;

			onDone(result);
		}
	}

    public bool IsSyncing()
    {
        return State != EState.None && State != EState.Disabled;
    }

    public bool IsLocalLocked()
    {
        bool isNotLocked = LocalOp == null || LocalOp.Result == PersistenceStates.ESyncResult.Success || State == EState.WaitingForCloudOp;
        return !isNotLocked;
    }

	public void Update()
	{
		int count = Ops.Count;
		for (int i = 0; i < count; i++)
		{
			if (Ops[i] != null)
			{
				Ops[i].Update();
			}
		}

		switch (mState)
		{
			case EState.WaitingForLocalOp:
			{
				if (LocalOp == null || LocalOp.State == PersistenceSyncOp.EState.Ready)
				{				
                    State = EState.WaitingForCloudOp;					
				}
			}
			break;

			case EState.WaitingForCloudOp:
			{
				if (CloudOp == null || CloudOp.State == PersistenceSyncOp.EState.Ready)
				{
					State = EState.Syncing;
				}
			}
			break;

			case EState.Syncing:
			{
				if (SyncOp == null || SyncOp.State == PersistenceSyncOp.EState.Ready)
				{
                    State = EState.None;
                    PersistenceStates.ESyncResult result = PersistenceStates.ESyncResult.Success;
                    if (LocalOp != null && LocalOp.Result != PersistenceStates.ESyncResult.Success ||
                        CloudOp != null && CloudOp.Result != PersistenceStates.ESyncResult.Success ||
                        SyncOp != null && SyncOp.Result != PersistenceStates.ESyncResult.Success)
                    {
                        result = PersistenceStates.ESyncResult.Error_Default;
                    }
                     
                    Notify_OnDone(result);						
				}				
			}
			break;
		}
	}

	private void Notify_OnDone(PersistenceStates.ESyncResult result)
	{
        Action<PersistenceStates.ESyncResult> onDone = OnDone;
		Reset(false);

        if (onDone != null)
        {
            onDone(result);
        }
    }
}
