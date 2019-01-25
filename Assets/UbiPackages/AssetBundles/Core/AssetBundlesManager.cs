using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Asset Bundles API facade. This class is able to manage both local and remote asset bundles.
/// </summary>
public class AssetBundlesManager
{
    private static AssetBundlesManager sm_instance;
    public static AssetBundlesManager Instance
    {
        get
        {
            if (sm_instance == null)
            {
                sm_instance = new AssetBundlesManager();
            }

            return sm_instance;
        }
    }       

    /// <summary>
    /// List of the local asset bundle ids.
    /// </summary>
    private List<string> m_localAssetBundleIds;
    
    private Dictionary<string, AssetBundleHandle> m_assetBundleHandles;

    public static string DependenciesFileName = "dependencies";        

    /// <summary>
    /// Initialize the system.
    /// </summary>
    /// <param name="localAssetBundleNames">List of the local asset bundle ids.</param>
    public void Initialize(List<string> localAssetBundleIds, string localAssetBundlesPath, Logger logger)
    {
        // Just in case this is not the first time Initialize is called        
        Reset();

        sm_logger = logger;

        if (sm_logger == null)
        {
#if UNITY_EDITOR
            sm_logger = new ConsoleLogger("ASSET_BUNDLES");
#else
            sm_logger = new DummyLogger();        
#endif
        }

        if (Logger.CanLog())
        {
            Logger.Log("Initializing AssetBundlesManager");
        }

        Loader_Init();
        Ops_Init();

        m_localAssetBundleIds = localAssetBundleIds;

        AssetBundleManifest manifest = null;
        string path = Path.Combine(localAssetBundlesPath, DependenciesFileName);        
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(path);
        if (manifestBundle != null)
        {
            string assetName = "AssetBundleManifest";
            manifest = manifestBundle.LoadAsset<AssetBundleManifest>(assetName);
            if (manifest == null)
            {
                if (Logger.CanLog())
                {
                    Logger.LogError("Error when initializing AssetBundlesManager because asset " + assetName + " wasn't found in manifest asset bundle");
                }
            }
            else
            {
                m_assetBundleHandles = new Dictionary<string, AssetBundleHandle>();

                string id;
                AssetBundleHandle handle;
                string[] arr;
                List<string> dependencies;
                List<string> manifestAssetBundleIds = new List<string>(manifest.GetAllAssetBundles());
                if (m_localAssetBundleIds != null)
                {
                    int count = m_localAssetBundleIds.Count;
                    for (int i = 0; i < count; i++)
                    {
                        id = m_localAssetBundleIds[i];

                        // Verifies that the asset bundle is defined in the manifest
                        if (!manifestAssetBundleIds.Contains(id))
                        {
                            if (Logger.CanLog())
                            {
                                Logger.LogError("Asset bundle with id " + m_localAssetBundleIds[i] + " can't be found in asset bundles manifest");
                            }
                        }
                        else
                        {
                            arr = manifest.GetAllDependencies(id);
                            dependencies = new List<string>(arr);

                            // id is also included because it makes downloading logic easier
                            dependencies.Add(id);

                            handle = new AssetBundleHandle();
                            handle.SetupLocal(id, Path.Combine(localAssetBundlesPath, id), dependencies);
                            m_assetBundleHandles.Add(m_localAssetBundleIds[i], handle);
                        }
                    }
                }

                // No needed anymore
                manifestBundle.Unload(true);

                if (Logger.CanLog())
                {
                    Logger.Log("AssetBundlesManager initialized successfully");
                }
            }
        }
        else
        {
            if (Logger.CanLog())
            {
                Logger.LogError("Error when initializing AssetBundlesManager: Path for AssetBundleManifest is not valid: " + path);
            }
        }       
    }

    public void Reset()
    {
        UnloadAssetBundleList(m_localAssetBundleIds, null);
        m_localAssetBundleIds = null;
        m_assetBundleHandles = null;

        Ops_Reset();
        Loader_Reset();
    }

    private bool IsInitialized()
    {
        return m_assetBundleHandles != null;
    }

    public AssetBundleHandle GetAssetBundleHandle(string id)
    {
        AssetBundleHandle returnValue = null;
        if (m_assetBundleHandles != null)
        {
            m_assetBundleHandles.TryGetValue(id, out returnValue);
        }

        return returnValue;
    }

    public List<string> GetDependenciesIncludingSelf(string id)
    {
        List<string> returnValue = null;
        AssetBundleHandle handle = GetAssetBundleHandle(id);
        if (handle != null)
        { 
            returnValue = handle.GetDependenciesIncludingSelf();            
        }

        return returnValue;        
    }    
    
    public void LoadAssetBundleAndDependencies(string id, AssetBundlesOp.OnDoneCallback onDone)
    {
        if (IsAssetBundleValid(id))
        {
            List<string> abIds = GetDependenciesIncludingSelf(id);
            LoadAssetBundleList(abIds, onDone);
        }
        else
        {
            onDone(AssetBundlesOp.EResult.Error_AB_Handle_Not_Found, null);
        }
    }

    public void LoadAssetBundleList(List<string> ids, AssetBundlesOp.OnDoneCallback onDone)
    {
        if (ids != null && ids.Count > 0)
        {
            if (IsAssetBundleListValid(ids))
            {
                if (IsAssetBundleListLoaded(ids))
                {
                    if (onDone != null)
                    {
                        onDone(AssetBundlesOp.EResult.Success, null);
                    }
                }
                else
                {                    
                    Loader_RequestAssetBundleList(ids);

                    // Creates an op that will be done and will call onDone callback when all requests are done
                    LoadAssetBundleListOp op = new LoadAssetBundleListOp();
                    op.Setup(ids, onDone);
                    Ops_PerformOp(op);
                }
            }
            else if (onDone != null)
            {
                onDone(AssetBundlesOp.EResult.Error_AB_Handle_Not_Found, null);
            }            
        }
        else if (onDone != null)
        {
            onDone(AssetBundlesOp.EResult.Success, null);
        }
    }

    public bool IsAssetBundleValid(string id)
    {
        return GetAssetBundleHandle(id) != null;        
    }

    /// <summary>
    /// Checks if all asset bundle ids in the list are valid. An asset bundle id is valid if it has an asset bundle handle.
    /// </summary>
    /// <param name="ids">List of asset bundle ids to check.</param>
    /// <returns>Returns <c>true</c> if all asset bundles in the list are valid.</returns>
    public bool IsAssetBundleListValid(List<string> ids)
    {
        bool returnValue = true;
        if (ids != null && ids.Count > 0)
        {            
            int count = ids.Count;
            for (int i = 0; i < count && returnValue; i++)
            {                
                returnValue = IsAssetBundleValid(ids[i]);
            }
        }

        return returnValue;
    }

    /// <summary>
    /// Checks if all asset bundle ids in the list are loaded.
    /// </summary>
    /// <param name="ids">List of asset bundle ids to check.</param>
    /// <returns>Returns <c>true</c> if all asset bundles in the list are loaded.</returns>
    public bool IsAssetBundleListLoaded(List<string> ids)
    {
        bool returnValue = IsAssetBundleListValid(ids);
        if (returnValue && ids != null && ids.Count > 0)
        {
            AssetBundleHandle handle;
            int count = ids.Count;
            for (int i = 0; i < count && returnValue; i++)
            {
                handle = GetAssetBundleHandle(ids[i]);
                returnValue = handle != null && handle.IsLoaded();
            }
        }

        return returnValue;
    }    

    /// <summary>
    /// Loads synchronously an asset called <c>assetName</c> from an asset bundle with <c>assetBundleId</c> as id. This method assumes that the asset bundle has already been downloaded and loaded.
    /// This method makes it easier to migrate from loading an asset from Resources to loading it from an asset bundle but client needs to have downloaded and loaded this asset bundle before calling this method.    
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="assetName">Name of the asset to load.</param>
    /// <returns>The asset required if everything went ok, otherwise <c>null</c> is returned.</returns>
    public Object LoadAsset(string assetBundleId, string assetName)
    {
        Object returnValue = null;
        AssetBundleHandle handle = GetAssetBundleHandle(assetBundleId);
        if (handle != null)
        {
            if (handle.IsLoaded())
            {
                AssetBundle assetBundle = handle.AssetBundle;
                if (assetBundle != null)
                {
                    returnValue = assetBundle.LoadAsset(assetName);
                }
            }
        }

        return returnValue;                    
    }

    /// <summary>
    /// Loads asynchronously an asset called <c>assetName</c> from an asset bundle with <c>assetBundleId</c> as id. This method, if required, will download and load this asset bundle and its dependencies before 
    /// loading the asset from the asset bundle.
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="assetName">Name of the asset to load.</param>    
    /// <param name="onDone">Callback that will be called when the asset has been loaded or if an error happened throughout the process.</param>    
    public void LoadAssetAsync(string assetBundleId, string assetName, AssetBundlesOp.OnDoneCallback onDone)
    {   
        if (!LoadAssetFromAssetBundlesFullOp.EarlyExit(assetBundleId, assetName, onDone))
        {
            LoadAssetFromAssetBundlesFullOp op = new LoadAssetFromAssetBundlesFullOp();
            op.Setup(assetBundleId, assetName, onDone);
            Ops_PerformOp(op);
        }        
    }

    /// <summary>
    /// Loads asynchronously an asset called <c>assetName</c> from an asset bundle. 
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="assetName">Name of the asset to load.</param>    
    /// <param name="onDone">Callback that will be called when the asset has been loaded or if an error happened throughout the process.</param>    
    public void LoadAssetFromAssetBundleAsync(AssetBundle assetBundle, string assetName, AssetBundlesOp.OnDoneCallback onDone)
    {
        LoadAssetFromAssetBundleOp op = new LoadAssetFromAssetBundleOp();
        op.Setup(assetBundle, assetName, onDone);
        Ops_PerformOp(op);
    }

    public void UnloadAssetBundle(string assetBundleId, AssetBundlesOp.OnDoneCallback onDone)
    {
        AssetBundlesOp.EResult result = AssetBundlesOp.EResult.Success;        
        AssetBundleHandle handle = GetAssetBundleHandle(assetBundleId);
        if (handle == null)
        {
            result = AssetBundlesOp.EResult.Error_AB_Handle_Not_Found;            
        }
        else if (handle.IsLoaded() || handle.IsLoading())
        {                        
            handle.Unload();                               
        }        
        else
        {
            result = AssetBundlesOp.EResult.Error_AB_Is_Not_Loaded;
        }

        if (onDone != null)
        {
            onDone(result, null);
        }
    }

    public void UnloadAssetBundleList(List<string> assetBundleIds, AssetBundlesOp.OnDoneCallback onDone)
    {
        if (assetBundleIds != null)
        {
            int count = assetBundleIds.Count;
            AssetBundleHandle handle;
            for (int i = 0; i < count; i++)
            {
                handle = GetAssetBundleHandle(assetBundleIds[i]);
                if (handle != null)
                {
                    if (handle.IsLoaded())
                    {
                        handle.Unload();
                    }                    
                }
            }
        }
    }

    /// <summary>
    /// Loads synchronously a scene called <c>sceneName</c> from an asset bundle with <c>assetBundleId</c> as id. This method assumes that the asset bundle and its dependencies have already been downloaded and loaded.
    /// This method makes it easier to migrate from loading a scene from the build to loading it from an asset bundle but client needs to have downloaded and loaded this asset bundle and its dependencies before calling 
    /// this method.    
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>
    /// <param name="mode">Allows you to specify whether or not to load the scene additively.</param>        
    public void LoadScene(string assetBundleId, string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {        
        AssetBundleHandle handle = GetAssetBundleHandle(assetBundleId);
        if (handle != null)
        {
            if (handle.IsLoaded())
            {
                AssetBundle assetBundle = handle.AssetBundle;
                if (assetBundle != null)
                {
                    if (assetBundle.isStreamedSceneAssetBundle)
                    {                        
                        SceneManager.LoadScene(sceneName, mode);
                    }
                }
            }
        }        
    }

    /// <summary>
    /// Loads asynchronously a scene called <c>sceneName</c> from an asset bundle with <c>assetBundleId</c> as id. This method will download and load the asset bundle before loading
    /// the scene from the asset bundle if it needs to.
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>    
    /// <param name="loadSceneMode">Allows you to specify whether or not to load the scene additively.</param>    
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public void LoadSceneAsync(string assetBundleId, string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, AssetBundlesOp.OnDoneCallback onDone = null)
    {
        if (!LoadSceneFromAssetBundlesFullOp.EarlyExit(assetBundleId, sceneName, onDone))
        {
            LoadSceneFromAssetBundlesFullOp op = new LoadSceneFromAssetBundlesFullOp();
            op.Setup(assetBundleId, sceneName, loadSceneMode, onDone);
            Ops_PerformOp(op);
        }
    }

    /// <summary>
    /// Loads asynchronously a scene called <c>sceneName</c> from an asset bundle. 
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>        
    /// <param name="loadSceneMode">Allows you to specify whether or not to load the scene additively.</param>    
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public void LoadSceneFromAssetBundleAsync(AssetBundle assetBundle, string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, AssetBundlesOp.OnDoneCallback onDone = null)
    {
        LoadSceneFromAssetBundleOp op = new LoadSceneFromAssetBundleOp();
        op.SetupLoad(assetBundle, sceneName, loadSceneMode, onDone);
        Ops_PerformOp(op);
    }

    /// <summary>
    /// Unloads asynchronously a scene called <c>sceneName</c> from an asset bundle with <c>assetBundleId</c> as id. There's no <c>UnloadScene()</c> method to unload synchronously a scene because    
    /// that option has been deprecated by Unity.
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>        
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public void UnloadSceneAsync(string assetBundleId, string sceneName, AssetBundlesOp.OnDoneCallback onDone = null)
    {
        if (!LoadSceneFromAssetBundlesFullOp.EarlyExit(assetBundleId, sceneName, onDone))
        {
            AssetBundleHandle handle = GetAssetBundleHandle(assetBundleId);
            UnLoadSceneFromAssetBundleAsync(handle.AssetBundle, sceneName, onDone);            
        }
    }

    /// <summary>
    /// Unloads asynchronously a scene called <c>sceneName</c>.
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>            
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public void UnLoadSceneFromAssetBundleAsync(AssetBundle assetBundle, string sceneName, AssetBundlesOp.OnDoneCallback onDone = null)
    {
        LoadSceneFromAssetBundleOp op = new LoadSceneFromAssetBundleOp();
        op.SetupUnload(assetBundle, sceneName, onDone);
        Ops_PerformOp(op);
    }    

    public Object LoadResource(string assetBundleId, string resourceName, bool isAsset, LoadSceneMode mode = LoadSceneMode.Single)
    {
        Object returnValue = null;
        if (isAsset)
        {
            returnValue = LoadAsset(assetBundleId, resourceName);
        }
        else
        {
            LoadScene(assetBundleId, resourceName, mode);
            returnValue = null;
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously an resource called <c>resourceName</c> from an asset bundle with <c>assetBundleId</c> as id. This method, if required, will download and load this asset bundle and its dependencies 
    /// before loading the resource from the asset bundle if it needs to.
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="resourceName">Name of the resource to load.</param>    
    /// <param name="isAsset"><c>true</c> is the resource to load is an asset. <c>false</c> if it's a scene</param>    
    /// <param name="onDone">Callback that will be called when the asset has been loaded or if an error happened throughout the process.</param>    
    public void LoadResourceAsync(string assetBundleId, string resourceName, bool isAsset, AssetBundlesOp.OnDoneCallback onDone, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (isAsset)
        {
            LoadAssetAsync(assetBundleId, resourceName, onDone);
        }
        else
        {
            LoadSceneAsync(assetBundleId, resourceName, mode, onDone);
        }        
    }

    /// <summary>
    /// Loads asynchronously a resource called <c>resourceName</c> from an asset bundle 
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="resourceName">Name of the resource to load.</param>    
    /// <param name="isAsset"><c>true</c> is the resource to load is an asset. <c>false</c> if it's a scene</param>    
    /// <param name="onDone">Callback that will be called when the resource has been loaded or if an error happened throughout the process.</param>    
    public void LoadResourceFromAssetBundleAsync(AssetBundle assetBundle, string resourceName, bool isAsset, AssetBundlesOp.OnDoneCallback onDone, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (isAsset)
        {
            LoadAssetFromAssetBundleAsync(assetBundle, resourceName, onDone);
        }
        else
        {
            LoadSceneFromAssetBundleAsync(assetBundle, resourceName, mode, onDone);
        }        
    }

    public void Update()
    {
        if (IsInitialized())
        {
            Loader_Update();
            Ops_Update();
        }
    }

    #region ops
    private List<AssetBundlesOp> m_ops;

    private void Ops_Init()
    {
        if (m_ops == null)
        {
            m_ops = new List<AssetBundlesOp>();
        }
        else
        {
            m_ops.Clear();
        }
    }

    private void Ops_Reset()
    {
        if (m_ops != null)
        {
            int count = m_ops.Count;
            for (int i = 0; i < count; i++)
            {
                m_ops[i].Reset();
            }

            m_ops.Clear();
        }
    }

    private void Ops_PerformOp(AssetBundlesOp op)
    {
        if (op != null)
        {
            op.Perform();
            m_ops.Add(op);                        
        }
    }

    private void Ops_Update()
    {
        if (m_ops != null)
        {            
            AssetBundlesOp op;
            for (int i = m_ops.Count - 1; i > -1; i--)
            {
                op = m_ops[i];
                op.Update();
                if (!op.IsPerforming)
                {
                    m_ops.RemoveAt(i);
                }                
            }
        }
    }
    #endregion

    #region loader    
    /// <summary>
    /// Queue containing the asset bundle ids to load
    /// </summary>
    private Queue<string> m_loaderRequests;

    /// <summary>
    /// Operation resused to load an asset bundle. Only one asset bundle can be loaded simultaneously
    /// </summary>
    private LoadAssetBundleOp m_loaderLoadAssetBundleOp;    

    private void Loader_Init()
    {
        if (m_loaderRequests == null)
        {
            m_loaderRequests = new Queue<string>();
        }        

        if (m_loaderLoadAssetBundleOp == null)
        {
            m_loaderLoadAssetBundleOp = new LoadAssetBundleOp();
        }

        Loader_Reset();
    }

    private void Loader_Reset()
    {
        if (m_loaderRequests != null)        
        {
            m_loaderRequests.Clear();
        }

        if (m_loaderLoadAssetBundleOp != null)
        {
            m_loaderLoadAssetBundleOp.Reset();
        }
    }

    private void Loader_RequestAssetBundleList(List<string> ids)
    {
        if (ids != null)
        {
            int count = ids.Count;
            for (int i = 0; i < count; i++)
            {
                Loader_RequestAssetBundle(ids[i]);
            }
        }
    }

    private void Loader_RequestAssetBundle(string id)
    {
        if (Loader_NeedsToLoadAssetBundle(id, false))
        {            
            if (!m_loaderRequests.Contains(id))
            {
                AssetBundleHandle handle = GetAssetBundleHandle(id);
                if (handle != null)
                {
                    handle.OnPendingToRequestToLoad();
                }

                m_loaderRequests.Enqueue(id);
            }
        }
    }

    private bool Loader_NeedsToLoadAssetBundle(string id, bool checkDependencies)
    {
        bool returnValue = false;

        if (checkDependencies)
        {
            List<string> abIds = GetDependenciesIncludingSelf(id);
            if (abIds != null)
            {
                int count = abIds.Count;
                for (int i = 0; i < count && !returnValue; i++)
                {
                    returnValue = Loader_NeedsToLoadAssetBundle(abIds[i], false);
                }
            }
        }
        else
        {
            AssetBundleHandle handle = GetAssetBundleHandle(id);
            if (handle != null)
            {
                returnValue = handle.NeedsToRequestToLoad();
            }
        }

        return returnValue;
    }

    private void Loader_Update()
    {
        // Checks if it's ready for serving the next request
        if (m_loaderLoadAssetBundleOp.IsPerforming)
        {
            m_loaderLoadAssetBundleOp.Update();
        }
        else        
        {
            if (m_loaderRequests.Count > 0)
            {
                string id = m_loaderRequests.Dequeue();
                AssetBundleHandle handle = GetAssetBundleHandle(id);
                if (handle != null)
                {
                    handle.OnLoadRequested();
                    
                    m_loaderLoadAssetBundleOp.Setup(handle, null);
                    m_loaderLoadAssetBundleOp.Perform();
                }                 
            }
        }
    }        
    #endregion

    #region logger
    private static Logger sm_logger;

    public static Logger Logger
    {
        get
        {
            return sm_logger;
        } 
    }

    public static void Log_AssertABIsNullButIsLoaded(string assetBundleId)
    {
        if (Logger.CanLog())
        {
            Logger.LogError("ASSERT: asset bundle " + assetBundleId + " is marked as loaded but its AssetBundle object is null");
        }
    }

    public static void Log_AssertABIsNotLoadedButCallbackWasSuccessful(string assetBundleId)
    {
        if (Logger.CanLog())
        {
            Logger.LogError("ASSERT: asset bundle " + assetBundleId + " is not loaded but callback returned success");
        }
    }
    #endregion
}
