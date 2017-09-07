/// <summary>
/// This class is responsible for storing some information used to configure <c>PersistenceFacade</c>. It's been created to make debug easier
/// </summary>
using System;
using System.Collections.Generic;
public class PersistenceFacadeConfig
{    
    protected enum EFactoryType
    {
        SyncFromLaunch,
        SyncFromSettings,
        Save
    };

    private Dictionary<EFactoryType, PersistenceSyncOpFactory> CachedFactories { get; set; }

    protected virtual PersistenceSyncOpFactory GetFactory(EFactoryType type)
    {
        PersistenceSyncOpFactory returnValue = null;
        if (CachedFactories != null && CachedFactories.ContainsKey(type))
        {
            returnValue = CachedFactories[type];
        }        

        return returnValue;
    }

    private void CacheFactory(EFactoryType type, PersistenceSyncOpFactory factory)
    {
        if (CachedFactories == null)
        {
            CachedFactories = new Dictionary<EFactoryType, PersistenceSyncOpFactory>();
        }

        if (CachedFactories.ContainsKey(type))
        {
            CachedFactories[type] = factory;
        }
        else
        {
            CachedFactories.Add(type, factory);
        }
    }

    public PersistenceSyncOpFactory SyncFromLaunchFactory
    {
        get { return GetFactory(EFactoryType.SyncFromLaunch); }    
        set { CacheFactory(EFactoryType.SyncFromLaunch, value); }    
    }

    public PersistenceSyncOpFactory SyncFromSettingsFactory
    {
        get { return GetFactory(EFactoryType.SyncFromSettings); }
        set { CacheFactory(EFactoryType.SyncFromSettings, value); }
    }

    public PersistenceSyncOpFactory SaveFactory
    {
        get { return GetFactory(EFactoryType.Save); }
        set { CacheFactory(EFactoryType.Save, value); }
    }

    public PersistenceFacadeConfig()
    {
        Setup();
    }

    protected virtual void Setup()
    {
        // All use the same factory because only one can be called simultaneously        
        PersistenceSyncOpFactory productionFactory = GetProductionFactory();
        SetFactoryToAllTypes(productionFactory);        
    }        

    protected void SetFactoryToAllTypes(PersistenceSyncOpFactory factory)
    {
        // All use the same factory because only one can be called simultaneously
        int count = Enum.GetValues(typeof(EFactoryType)).Length;
        for (int i = 0; i < count; i++)
        {
            CacheFactory((EFactoryType)i, factory);
        }
    }

    protected PersistenceSyncOpFactory GetProductionFactory()
    {
        // It doesn't need a sync op factory becaused this factory won't be used to get sync operations
        PersistenceSyncOpFactory syncFactory = new PersistenceSyncOpFactory(true, null);        
        return new PersistenceSyncOpFactory(true, syncFactory);
    }    
}
