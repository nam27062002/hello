public class PersistenceSyncOpFactoryDebug : PersistenceSyncOpFactory
{
    public PersistenceSyncOpFactoryDebug(PersistenceSyncOpFactory syncOpFactory) : base(true, syncOpFactory)
    {
    }

    public PersistenceSyncOpDebug RegisterOp(EOpType type)
    {
        PersistenceSyncOp normalOp = GetNewOp(type);
        PersistenceSyncOpDebug returnValue = new PersistenceSyncOpDebug(normalOp);

        Cache_RegisterOp(type, returnValue);
        return returnValue;
    }    
}
