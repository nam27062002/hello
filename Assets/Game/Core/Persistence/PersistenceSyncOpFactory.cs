using System.Collections.Generic;
public class PersistenceSyncOpFactory
{
    public PersistenceSyncOpFactory(bool isCacheEnabled, PersistenceSyncOpFactory syncOpFactory)
    {
        Cache_IsEnabled = isCacheEnabled;
        SyncOpFactory = syncOpFactory;
    }

    public enum EOpType
    {
        LoadLocal,
        SaveLocal,
        LoadCloud,
        SaveCloud,
        Sync
    };

    /// <summary>
    /// A sync operation might need to create its own operations (for example saveLocal op after reseting persistence to default). 
    /// This is the factory that a sync operation will use so it won't interfere with any other sync operation.
    /// 
    /// </summary>
    private PersistenceSyncOpFactory SyncOpFactory { get; set; }

    #region cache
    // This region is responsible for storing one operation per type so they can be reused. This is done to reduce the memory impact of recreating operation objects every time
    // a sync operation is performed
    private Dictionary<EOpType, PersistenceSyncOp> Cache_Ops { get; set; }

    protected void Cache_RegisterOp(EOpType type, PersistenceSyncOp op)
    {
        if (Cache_Ops == null)
        {
            Cache_Ops = new Dictionary<EOpType, PersistenceSyncOp>();
        }

        if (Cache_Ops.ContainsKey(type))
        {
            Cache_Ops[type] = op;
        }
        else
        {
            Cache_Ops.Add(type, op);
        }
    }

    private PersistenceSyncOp Cache_GetOp(EOpType type)
    {
        PersistenceSyncOp returnValue = null;
        if (Cache_Ops != null && Cache_Ops.ContainsKey(type))
        {
            returnValue = Cache_Ops[type];
        }

        return returnValue;
    }

    private bool Cache_IsEnabled { get; set; }
    #endregion    

    protected PersistenceSyncOp GetNewOp(EOpType type)
    {
        PersistenceSyncOp returnValue = null;

        switch (type)
        {
            case EOpType.LoadLocal:
            case EOpType.SaveLocal:
                returnValue = new PersistenceSyncOpLocal();
                break;

            case EOpType.LoadCloud:
            case EOpType.SaveCloud:
                returnValue = new PersistenceSyncOpCloud();
                break;

            case EOpType.Sync:
                returnValue = new PersistenceSyncOpMerge();
                break;
        }

        return returnValue;
    }

    private PersistenceSyncOp GetOp(EOpType type)
    {        
        PersistenceSyncOp returnValue = null;
        if (Cache_IsEnabled)
        {
            returnValue = Cache_GetOp(type);
        }

        if (returnValue == null)
        {
            returnValue = GetNewOp(type);
            if (Cache_IsEnabled)
            {
                Cache_RegisterOp(type, returnValue);
            }
        }

        return returnValue;
    }
    
    public PersistenceSyncOp GetLoadLocalOp(PersistenceData data, bool needsToPerform = true)
	{
        PersistenceSyncOp op = GetOp(EOpType.LoadLocal);
        op.Setup(PersistenceSyncOp.EType.Load, data, false);
        op.NeedsToPerform = needsToPerform;   
		return op;
	}    	

	public PersistenceSyncOp GetSaveLocalOp(PersistenceData data, bool isSilent)
	{
        PersistenceSyncOp op = GetOp(EOpType.SaveLocal);        
		op.Setup(PersistenceSyncOp.EType.Save, data, isSilent);
		return op;
	}	

	public PersistenceSyncOp GetLoadCloudOp(PersistenceData data, bool isSilent)
	{
        PersistenceSyncOp op = GetOp(EOpType.LoadCloud);
        op.Setup(PersistenceSyncOp.EType.Load, data, isSilent);
		return op;
	}	

	public PersistenceSyncOp GetSaveCloudOp(PersistenceData data, bool isSilent)
	{
        PersistenceSyncOp op = GetOp(EOpType.SaveCloud);
        op.Setup(PersistenceSyncOp.EType.Save, data, isSilent);
		return op;
	}
	
	public PersistenceSyncOp GetSyncOp(PersistenceSyncOp localOp, PersistenceSyncOp cloudOp, 
	                                   bool isSilent)
	{        
        PersistenceSyncOpMerge op = GetOp(EOpType.Sync) as PersistenceSyncOpMerge; 
		op.Setup(localOp, cloudOp, isSilent, SyncOpFactory);
		return op;
	}    
}
