using SimpleJSON;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for serving a resource (either an assets or an scene) given a name 
/// </summary>
public class AddressablesManager
{
    private AddressablesCatalog m_catalog;
    private bool m_isInitialized = false;
    private AddressablesCatalogEntry m_entryHelper;
    
    public void Initialize(JSONNode catalogJSON, string assetBundlesManifestPath, List<string> localAssetBundleIds, Logger logger)
    {        
        sm_logger = logger;

        // Loads the catalog
        if (m_catalog == null)
        {
            m_catalog = new AddressablesCatalog();
        }

        m_catalog.Load(catalogJSON, logger);        

        // Loads the providers
        m_providerFromAB = new AddressablesFromAssetBundlesProvider();
        m_providerFromAB.Initialize(localAssetBundleIds, assetBundlesManifestPath, logger);

        m_providerFromResources = new AddressablesFromResourcesProvider();

        Ops_Init();

        m_entryHelper = new AddressablesCatalogEntry();

        m_isInitialized = true;
    }    

    public void Reset()
    {
        if (IsInitialized())
        {
            m_catalog.Reset();          
            m_providerFromAB.Reset();

            Ops_Reset();     

            m_isInitialized = false;
        }
    }

    public bool IsInitialized()
    {
        return m_isInitialized;
    }

    /// <summary>
    /// Returns the list of dependencies (typically asset bundles) ids required to load the addressable with <c>id</c> as an identifier.
    /// </summary>
    /// <param name="id">Addressable id which dependencies are requested.</param>    
    public List<string> GetDependencyIds(string id)
    {
        List<string> returnValue = null;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, out entry);
            returnValue = provider.GetDependencyIds(entry);
        }
        else
        {
            Errors_ProcessManagerNotInitialized(false);
        }

        return returnValue;
    }

    /// <summary>
    /// Returns the list of dependencies (typically asset bundles) ids required to load the addressables which identifiers are passed as a parameter in <c>id</c>.
    /// </summary>
    /// <param name="id">List of addressable ids which dependencies are requested.</param>    
    public List<string> GetDependencyIdsList(List<string> ids)
    {
        List<string> returnValue = null;

        if (IsInitialized())
        {
            if (ids != null && ids.Count > 0)
            {
                returnValue = new List<string>();

                AddressablesCatalogEntry entry;
                AddressablesProvider provider;
                int count = ids.Count;
                for (int i = 0; i < count; i++)
                {
                    provider = Providers_GetProvider(ids[i], out entry);
                    UbiListUtils.AddRange(returnValue, provider.GetDependencyIds(entry), false, true);
                }
            }
        }
        else
        {
            Errors_ProcessManagerNotInitialized(false);
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously the dependencies (typically asset bundles if the addressable is stored in an asset bundle) required to be loaded before loading the addressable with <c>id</c> as an identifier.
    /// </summary>
    /// <param name="id">Addressable id which dependencies are requested to be loaded</param>
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public AddressablesOp LoadDependenciesAsync(string id)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, out entry);
            returnValue = provider.LoadDependenciesAsync(entry);

            Ops_AddOp(returnValue);
        }
        else
        {            
            returnValue = Errors_ProcessManagerNotInitialized(true);         
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously a list of dependencies (typically asset bundles).
    /// </summary>
    /// <param name="id">List of dependency ids to load.</param>
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public AddressablesOp LoadDependencyIdsListAsync(List<string> dependencyIds)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            // Dependencies are only handled by provider from Asset Bundles
            returnValue = m_providerFromAB.LoadDependencyIdsListAsync(dependencyIds);            
            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }

        return returnValue;        
    }

    /// <summary>
    /// Unloads the dependencies (typically asset bundles if the addressable is stored in an asset bundle) that were loaded in order to be able to load the addressable with <c>id</c> as an identifier.
    /// </summary>
    /// <param name="id">Addressable id which dependencies are requested to be unloaded</param>
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public void UnloadDependencies(string id)
    {        
        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, out entry);
            provider.UnloadDependencies(entry);            
        }
        else
        {            
            Errors_ProcessManagerNotInitialized(false);           
        }        
    }  
    
    public void UnloadDependencyIdsList(List<string> dependencyIds)
    {
        if (IsInitialized())
        {
            // Dependencies are only handled by provider from Asset Bundles
            m_providerFromAB.UnloadDependencyIdsList(dependencyIds);                        
        }
        else
        {
            Errors_ProcessManagerNotInitialized(false);
        }
    }

    /// <summary>
    /// Loads synchronously the scene corresponding to the addressable id <c>id</c>. This method assumes that all possible dependencies such as asset bundles needed to load the scene have already been downloaded and loaded.    
    /// </summary>    
    /// <param name="Id">Addressable id corresponding to the scene to load.</param>    
    /// <param name="mode">Allows you to specify whether or not to load the scene additively.</param>        
    /// <returns><c>true</c> if the scene has been loaded successfully.</returns>
    public bool LoadScene(string id, LoadSceneMode mode)
    {
        bool returnValue = false;
        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, out entry);
            returnValue = provider.LoadScene(entry, mode);
        }        
        else 
        {
            LogErrorManagerNotInitialized();
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously the scene corresponding to the addressable id <c>id</c>. If this addressable has some dependencies then they'll be downloaded and loaded before loading it, if required.   
    /// </summary>    
    /// <param name="Id">Addressable id corresponding to the scene to load.</param>    
    /// <param name="mode">Allows you to specify whether or not to load the scene additively.</param>            
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public AddressablesOp LoadSceneAsync(string id, LoadSceneMode mode)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, out entry);
            returnValue = provider.LoadSceneAsync(entry, mode);

            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }        

        return returnValue;
    }    

    public AddressablesOp UnloadSceneAsync(string id)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, out entry);
            returnValue =  provider.UnloadSceneAsync(entry);

            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }
        
        return returnValue;
    }    
    
    public void Update()
    {
        if (IsInitialized())
        {
            m_providerFromAB.Update();
            Ops_Update();
        }
    }

    #region ops
    private List<AddressablesOp> m_ops;

    private void Ops_Init()
    {
        if (m_ops == null)
        {
            m_ops = new List<AddressablesOp>();
        }
    }

    private void Ops_Reset()
    {
        if (m_ops != null)
        {
            m_ops.Clear();
        }
    }

    private void Ops_AddOp(AddressablesOp op)
    {
        if (op.isDone)
        {
            if (op.Error != null)
            {
                Errors_ProcessAddressablesOpError(op.Error);
            }
        }
        else
        {
            m_ops.Add(op);
        }
    }

    private void Ops_Update()
    {
        for (int i = m_ops.Count - 1; i > -1; i--)
        {
            if (m_ops[i].isDone)
            {
                if (m_ops[i].OnDone != null)
                {
                    m_ops[i].OnDone(m_ops[i]);
                }

                m_ops.RemoveAt(i);
            }
        }
    }
    #endregion

    #region errors
    private AddressablesOp Errors_ProcessManagerNotInitialized(bool returnOp)
    {
        AddressablesOp returnValue = null;

        LogErrorManagerNotInitialized();        

        if (returnOp)
        {
            AddressablesError error = new AddressablesError(AddressablesError.EType.Error_Manager_Not_initialized);
            returnValue = new AddressablesOpResult();
            returnValue.Setup(error, null);        
        }

        return returnValue;
    }    
    #endregion

        #region providers
    private AddressablesFromAssetBundlesProvider m_providerFromAB;
    private AddressablesFromResourcesProvider m_providerFromResources;

    private AddressablesProvider Providers_GetProvider(string id, out AddressablesCatalogEntry entry)
    {
        AddressablesProvider returnValue = m_providerFromResources;

        entry = m_catalog.GetEntry(id);

        // If id is not in the catalog we assume that id is a path to the resource
        if (entry == null)
        {
            entry = m_entryHelper;
            entry.SetupAsEntryInResources(id);
        }
        
        switch (entry.LocationType)
        {
            case AddressablesTypes.ELocationType.Resources:
                returnValue = m_providerFromResources;
                break;

            case AddressablesTypes.ELocationType.AssetBundles:
                returnValue = m_providerFromAB;
                break;
        }

        return returnValue;
    }
    #endregion

    #region logger
    private static Logger sm_logger;

    public static bool CanLog()
    {
        return sm_logger != null && sm_logger.CanLog();
    }

    public void Log(string msg)
    {
        if (CanLog())
        {
            sm_logger.Log(msg);
        }
    }

    public void LogWarning(string msg)
    {
        if (CanLog())
        {
            sm_logger.LogWarning(msg);
        }
    }

    public void LogError(string msg)
    {
        if (CanLog())
        {
            sm_logger.LogError(msg);
        }
    }

    private void LogErrorManagerNotInitialized()
    {
        if (CanLog())
        {
            sm_logger.LogError("Manager is not initialized");
        }
    }

    private void LogErrorIdNotFound(string id)
    {
        if (CanLog())
        {
            sm_logger.LogError("Addressables id <" + id + "> is not in the catalog");
        }
    }

    private void Errors_ProcessAddressablesOpError(AddressablesError error)
    {
        if (CanLog())
        {
            sm_logger.LogError("Error when retrieving Addressable operation: " + error.ToString());
        }
    }
    #endregion    
}
