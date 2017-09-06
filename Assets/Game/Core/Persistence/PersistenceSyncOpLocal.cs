public class PersistenceSyncOpLocal : PersistenceSyncOp
{   
    public string ForcedPersistenceString { get; set; }     

    protected override void ExtendedReset()
    {
        ForcedPersistenceString = null;
        NeedsToPerform = true;
    }

    protected override void ExtendedPerform()
	{        
        switch (Type)
        {
            case EType.Load:
            {
                if (Data != null)
                {
                    if (string.IsNullOrEmpty(ForcedPersistenceString))
                    {
                        Data.Load();
                    }
                    else
                    {
                        Data.LoadFromString(ForcedPersistenceString);
                    }
                }
            }
            break;

            case EType.Save:
            {
                if (Data != null && Data.LoadState == PersistenceStates.LoadState.OK)
                {
                    Data.Save();
                }
            }
            break;
        }        

		OnPerformDone();
	}

	protected override void ExtendedDiscard() {}    
}
