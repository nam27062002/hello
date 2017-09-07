public abstract class PersistenceSyncOp
{
	public enum EType
	{
		None,
		Load,
		Save,
		Merge
	};

	public EType Type { get; set; }

	public enum EState
	{
		None,
		Running,
		Ready
	};

	public EState State { get; set; }

	public bool IsSilent { get; set; }	

	public PersistenceData Data { get; set; }

	public PersistenceStates.ESyncResult Result { get; set; }

    public bool NeedsToPerform { get; set; }

    private void Reset()
	{
		Type = EType.None;
		State = EState.None;
		IsSilent = false;		
		Data = null;
		Result = PersistenceStates.ESyncResult.None;
        NeedsToPerform = true;

		ExtendedReset();
	}

	protected virtual void ExtendedReset() {}

	public virtual void Setup(EType type, PersistenceData data, bool isSilent)
	{
		Discard();

		Type = type;
		Data = data;
		IsSilent = isSilent;		
	}

	public void Perform()
	{
        if (NeedsToPerform)
        {
            State = EState.Running;
            ExtendedPerform();
        }
        else
        {
            OnPerformDone();
        }
	}

	protected abstract void ExtendedPerform();	
	protected void OnPerformDone()
	{
        State = EState.Ready;
        CalculateResult();		
	}

	public void Discard()
	{
		if (State != EState.None)
		{
			ExtendedDiscard();
		}

		Reset();
	}

	protected abstract void ExtendedDiscard();

	public virtual void Update() {}

    public void CalculateResult()
    {
        if (State == EState.Ready)
        {
            Result = ExtendedCalculateResult();
        }
        else if (FeatureSettingsManager.IsDebugEnabled)
        {
            PersistenceFacade.Log("Result can't be calculated because the operation is not Ready");
        }
    }

    protected virtual PersistenceStates.ESyncResult ExtendedCalculateResult()
    {
        PersistenceStates.ESyncResult returnValue = PersistenceStates.ESyncResult.Error_Default;
        if (Data != null)
        {
            if (Type == EType.Load)
            {
                switch (Data.LoadState)
                {
                    case PersistenceStates.LoadState.Corrupted:
                        returnValue = PersistenceStates.ESyncResult.Error_Local_Load_Corrupted;
                        break;

                    case PersistenceStates.LoadState.PermissionError:
                        returnValue = PersistenceStates.ESyncResult.Error_Local_Load_Permission;
                        break;

                    case PersistenceStates.LoadState.NotFound:
                        returnValue = PersistenceStates.ESyncResult.Error_Local_Load_NotFound;
                        break;

                    case PersistenceStates.LoadState.OK:
                        returnValue = PersistenceStates.ESyncResult.Success;
                        break;
                }
            }
            else if (Type == EType.Save)
            {
                switch (Data.SaveState)
                {
                    case PersistenceStates.SaveState.DiskSpace:
                        returnValue = PersistenceStates.ESyncResult.Error_Local_Save_DiskSpace;
                        break;

                    case PersistenceStates.SaveState.PermissionError:
                        returnValue = PersistenceStates.ESyncResult.Error_Local_Save_Permission;
                        break;

                    case PersistenceStates.SaveState.OK:
                        returnValue = PersistenceStates.ESyncResult.Success;
                        break;
                }
            }
        }

        return returnValue;
    }
}
