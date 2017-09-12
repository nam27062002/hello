/// <summary>
/// This class is responsible for storing some information used to configure <c>PersistenceFacade</c>. It's been created to make debug easier
/// </summary>
using System;
using System.Collections.Generic;
public class PersistenceFacadeConfig
{        
    private Dictionary<PersistenceSyncer.EPurpose, PersistenceSyncOpFactory> CachedFactories { get; set; }

    protected virtual PersistenceSyncOpFactory GetFactory(PersistenceSyncer.EPurpose type)
    {
        PersistenceSyncOpFactory returnValue = null;
        if (CachedFactories != null && CachedFactories.ContainsKey(type))
        {
            returnValue = CachedFactories[type];
        }        

        return returnValue;
    }

    private void CacheFactory(PersistenceSyncer.EPurpose type, PersistenceSyncOpFactory factory)
    {
        if (CachedFactories == null)
        {
            CachedFactories = new Dictionary<PersistenceSyncer.EPurpose, PersistenceSyncOpFactory>();
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
        get { return GetFactory(PersistenceSyncer.EPurpose.SyncFromLaunch); }    
        set { CacheFactory(PersistenceSyncer.EPurpose.SyncFromLaunch, value); }    
    }

    public PersistenceSyncOpFactory SyncFromSettingsFactory
    {
        get { return GetFactory(PersistenceSyncer.EPurpose.SyncFromSettings); }
        set { CacheFactory(PersistenceSyncer.EPurpose.SyncFromSettings, value); }
    }

    public PersistenceSyncOpFactory SaveFactory
    {
        get { return GetFactory(PersistenceSyncer.EPurpose.Save); }
        set { CacheFactory(PersistenceSyncer.EPurpose.Save, value); }
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
        int count = Enum.GetValues(typeof(PersistenceSyncer.EPurpose)).Length;
        for (int i = 0; i < count; i++)
        {
            CacheFactory((PersistenceSyncer.EPurpose)i, factory);
        }
    }

    protected PersistenceSyncOpFactory GetProductionFactory()
    {
        // It doesn't need a sync op factory becaused this factory won't be used to get sync operations
        PersistenceSyncOpFactory syncFactory = new PersistenceSyncOpFactory(true, null);        
        return new PersistenceSyncOpFactory(true, syncFactory);
    }

    #region social    
    public virtual bool Social_IsLoggedIn()
    {
        return SocialPlatformManager.SharedInstance.IsLoggedIn();
    }
    #endregion
}
