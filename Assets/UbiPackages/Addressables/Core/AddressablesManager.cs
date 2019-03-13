using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for serving a resource (either an assets or an scene) given a name 
/// </summary>
public class AddressablesManager
{
#if UNITY_EDITOR
    public static AddressablesCatalog GetCatalog(string catalogPath, bool editorMode)
    {        
        AddressablesCatalog returnValue = null;

        // Loads the catalog        
        StreamReader reader = new StreamReader(catalogPath);
        string content = reader.ReadToEnd();
        reader.Close();

        JSONNode catalogJSON = JSON.Parse(content);
        returnValue = new AddressablesCatalog(editorMode);
        returnValue.Load(catalogJSON, sm_logger);

        return returnValue;
    }

    public const string ADDRESSSABLES_CATALOG_FILENAME = "addressablesCatalog.json";
    public const string ADDRESSABLES_EDITOR_CATALOG_FILENAME = "editor_" + ADDRESSSABLES_CATALOG_FILENAME;
    public const string ADDRESSABLES_EDITOR_CATALOG_PATH = "Assets/Editor/Addressables/" + ADDRESSABLES_EDITOR_CATALOG_FILENAME;

    private const string EDITOR_MODE_KEY = "EditorMode";

    /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
    private static int sm_editorMode = -1;
    public static bool EditorMode
    {
        get
        {
            if (sm_editorMode == -1)
                sm_editorMode = UnityEditor.EditorPrefs.GetBool(EDITOR_MODE_KEY, true) ? 1 : 0;

            return sm_editorMode != 0;
        }

        set
        {
            int newValue = value ? 1 : 0;
            if (newValue != sm_editorMode)
            {
                sm_editorMode = newValue;
                UnityEditor.EditorPrefs.SetBool(EDITOR_MODE_KEY, value);
            }
        }
    }
#endif

    private AddressablesCatalog m_catalog;
    private bool m_isInitialized = false;
    private AddressablesCatalogEntry m_entryHelper;    
        
    public void Initialize(JSONNode catalogJSON, string localAssetBundlesPath, Downloadables.Config downloadablesConfig, JSONNode downloadablesCatalogJSON, Downloadables.Tracker tracker, Logger logger)
    {
        sm_logger = logger;

        bool buildCatalog = true;

#if UNITY_EDITOR
        // editor catalog is used instead in editor mode
        if (EditorMode)
        {
            m_catalog = GetCatalog(ADDRESSABLES_EDITOR_CATALOG_PATH, true);
            buildCatalog = false;
        }
#endif
        if (buildCatalog)
        {
            // Loads the catalog
            if (m_catalog == null)
            {
                m_catalog = new AddressablesCatalog();
            }

            m_catalog.Load(catalogJSON, logger);
        }

        // Loads the providers
        AddressablesProvider.Logger = logger;

        m_providerFromAB = new AddressablesFromAssetBundlesProvider();
        m_providerFromAB.Initialize(localAssetBundlesPath, downloadablesConfig, downloadablesCatalogJSON, tracker, logger);

        m_providerFromResources = new AddressablesFromResourcesProvider();

#if UNITY_EDITOR
        m_providerFromEditor = new AddressablesFromEditorProvider();
#endif

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
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    public List<string> GetDependencyIds(string id, string variant = null)
    {
        List<string> returnValue = null;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
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
    /// <param name="variant">Variant of the addressables which dependencies are requested.</param>    
    public List<string> GetDependencyIdsList(List<string> ids, string variant = null)
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
                    provider = Providers_GetProvider(ids[i], variant, out entry);
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
    /// Whether or not a resource (either scene or asset) is available, which means that this resource and all its dependencies are either local or remote and already downloaded
    /// </summary>
    /// <param name="id">Resource id (either scene or asset)</param>
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    /// <param name="track">Whether or not the result should be tracked</param>
    /// <returns>Whether or not the resource (either scene or asset) is available, which means that this resource and all its dependencies are either local or remote and already downloaded</returns>
    public bool IsResourceAvailable(string id, string variant = null, bool track = false)
    {        
        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
            return provider.IsResourceAvailable(entry, track);            
        }
        else
        {
            return false;
        }     
    }

    public bool IsResourceListAvailable(List<string> ids, string variant = null, bool track = false)
    {
        bool returnValue = false;

        if (IsInitialized())
        {
            returnValue = true;
            if (ids != null)
            {
                int count = ids.Count;
                for (int i = 0; i < count; i++)
                {
                    // IsResourceAvailable() must be called for every id so that the result can be tracked if it needs to
                    returnValue = IsResourceAvailable(ids[i], variant, track) && returnValue;
                }
            }        
        }

        return returnValue;
    }

    /// <summary>
    /// Returns whether or not there are variants defined for the addressable which id is passed as a parameter.
    /// </summary>
    /// <param name="id">Addressable id</param>
    /// <returns>Returns whether or not there are variants defined for the addressable which id is passed as a parameter.</returns>
    public bool HasResourceVariants(string id)
    {
        bool returnValue = false;

        if (IsInitialized())
        {
            returnValue = m_catalog.HasEntryVariants(id);
        }

        return returnValue;
    }

    /// <summary>
    /// Downloads asynchronously the dependencies (typically asset bundles if the addressable is stored in an asset bundle) required to be loaded before loading the addressable with <c>id</c> as an identifier.
    /// </summary>
    /// <param name="id">Addressable id which dependencies are requested to be downloaded</param>
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public AddressablesOp DownloadDependenciesAsync(string id, string variant = null)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
            returnValue = provider.DownloadDependenciesAsync(entry);

            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously the dependencies (typically asset bundles if the addressable is stored in an asset bundle) required to be loaded before loading the addressable with <c>id</c> as an identifier.
    /// </summary>
    /// <param name="id">Addressable id which dependencies are requested to be loaded</param>
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public AddressablesOp LoadDependenciesAsync(string id, string variant = null)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
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
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public void UnloadDependencies(string id, string variant)
    {        
        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
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

    public List<string> GetAssetBundlesGroupDependencyIds(string groupId)
    {
        List<string> returnValue = null;

        if (IsInitialized())
        {
            AssetBundlesGroup abGroup = GetAssetBundlesGroup(groupId);
            if (abGroup != null)
            {
                returnValue = abGroup.AssetBundleIds;
            }            
        }
        else
        {
            Errors_ProcessManagerNotInitialized(false);
        }

        return returnValue;
    }

    public AssetBundlesGroup GetAssetBundlesGroup(string groupId)
    {
        AssetBundlesGroup returnValue = null;
        if (IsInitialized())
        {
            returnValue = m_providerFromAB.GetAssetBundlesGroup(groupId);
        }
        else
        {
            Errors_ProcessManagerNotInitialized(false);
        }

        return returnValue;
    }

    public AssetBundlesGroup CreateAssetBundlesGroupFromList(string groupId, List<string> groupIds)
    {
        AssetBundlesGroup returnValue = null;
        if (IsInitialized())
        {
            returnValue = m_providerFromAB.CreateAssetBundlesGroupFromList(groupId, groupIds);
        }
        else
        {
            Errors_ProcessManagerNotInitialized(false);
        }

        return returnValue;
    }

    /// <summary>
    /// Loads synchronously the scene corresponding to the addressable id <c>id</c>. This method assumes that all possible dependencies such as asset bundles needed to load the scene have already been downloaded and loaded.    
    /// </summary>    
    /// <param name="id">Addressable id corresponding to the scene to load.</param>    
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    /// <param name="mode">Allows you to specify whether or not to load the scene additively.</param>        
    /// <returns><c>true</c> if the scene has been loaded successfully.</returns>
    public virtual bool LoadScene(string id, string variant = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        bool returnValue = false;
        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
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
    /// <param name="id">Addressable id corresponding to the scene to load.</param>    
    /// <param name="variant">Variant of the addressable which dependencies are requested.</param>    
    /// <param name="mode">Allows you to specify whether or not to load the scene additively.</param>            
    /// <returns>Returns an <c>AddressablesOp</c> to handle the operation.</returns>
    public virtual AddressablesOp LoadSceneAsync(string id, string variant = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
            returnValue = provider.LoadSceneAsync(entry, mode);

            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }        

        return returnValue;
    }    

    public virtual AddressablesOp UnloadSceneAsync(string id, string variant = null)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
            returnValue =  provider.UnloadSceneAsync(entry);

            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }
        
        return returnValue;
    }   
    
    public T LoadAsset<T>(string id, string variant = null)
    {        
        T returnValue = default(T);

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
            returnValue = provider.LoadAsset<T>(entry);            
        }
        else
        {
            LogErrorManagerNotInitialized();
        }

        return returnValue;
    }

    public AddressablesOp LoadAssetAsync(string id, string variant = null)
    {
        AddressablesOp returnValue;

        if (IsInitialized())
        {
            AddressablesCatalogEntry entry;
            AddressablesProvider provider = Providers_GetProvider(id, variant, out entry);
            returnValue = provider.LoadAssetAsync(entry);

            Ops_AddOp(returnValue);
        }
        else
        {
            returnValue = Errors_ProcessManagerNotInitialized(true);
        }

        return returnValue;
    }

    public bool IsAutomaticDownloaderEnabled
    {
        get
        {
            return (m_providerFromAB == null) ? false : m_providerFromAB.IsAutomaticDownloaderEnabled;
        }

        set
        {
            if (m_providerFromAB != null)
            {
                m_providerFromAB.IsAutomaticDownloaderEnabled = value;
            }
        }
    }

    public bool IsDownloaderEnabled
    {
        get
        {
            return (m_providerFromAB == null) ? false : m_providerFromAB.IsDownloaderEnabled;
        }

        set
        {
            if (m_providerFromAB != null)
            {
                m_providerFromAB.IsDownloaderEnabled = value;
            }
        }
    }

    public void Update()
    {
        if (IsInitialized())
        {            
            m_providerFromAB.Update();
            Ops_Update();

            ExtendedUpdate();
        }
    }

    protected virtual void ExtendedUpdate() {}    

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

#if UNITY_EDITOR
    private AddressablesFromEditorProvider m_providerFromEditor;
#endif

    private AddressablesProvider Providers_GetProvider(string id, string variant, out AddressablesCatalogEntry entry)
    {
        AddressablesProvider returnValue = m_providerFromResources;

        entry = m_catalog.GetEntry(id, variant);

        bool entryWasFound = (entry != null);

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

#if UNITY_EDITOR
        // Editor mode must be used only when there's an entry defined for the addressable requested, otherwise we need to use the default provider (fromResources)
        if (EditorMode && entryWasFound)
        {
            returnValue = m_providerFromEditor;
        }
#endif

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
