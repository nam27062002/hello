﻿using SimpleJSON;
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
    
    public static AssetBundleManifest LoadManifest(string path, out AssetBundle manifestBundle)
    {
        AssetBundleManifest abManifest = null;
        manifestBundle = null;
        if (path != null && File.Exists(path))
        {            
            manifestBundle = AssetBundle.LoadFromFile(path);
            if (manifestBundle != null)
            {
                string assetName = "AssetBundleManifest";
                abManifest = manifestBundle.LoadAsset<AssetBundleManifest>(assetName);
            }            
        }

        return abManifest;
    }
   
    private Dictionary<string, AssetBundleHandle> m_assetBundleHandles;

    public static string ASSET_BUNDLES_CATALOG_FILENAME_NO_EXTENSION = "assetBundlesCatalog";
    public static string ASSET_BUNDLES_CATALOG_FILENAME = ASSET_BUNDLES_CATALOG_FILENAME_NO_EXTENSION + ".json";    
    public static string ASSET_BUNDLES_PATH_RELATIVE = "AssetBundles";

    private Downloadables.Manager m_downloadablesManager;        

#if UNITY_EDITOR
    private MockNetworkDriver m_networkDriver;
    private MockDiskDriver m_diskDriver;
#endif
    
    private Dictionary<string, AssetBundlesGroup> m_groups;

    /// <summary>
    /// Initialize the system.
    /// </summary>
    /// <param name="localAssetBundlesPath">Path to assetBundlesCatalog.json and where the local asset bundles are stored.</param>    
    /// <param name="downloadablesConfig">Downloadables configuration</param>        
    /// <param name="downloadablesCatalog">JSON containing the catalog of downloadables.</param>        
    /// <param name="tracker">Downloadables tracker that is notified with downloadables related events.</param>
    /// <param name="logger">Logger</param>
    public void Initialize(string localAssetBundlesPath, Downloadables.Config downloadablesConfig, JSONNode downloadablesCatalog, Downloadables.Tracker tracker, Logger logger)
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
            Logger.Log("Initializing AssetBundlesManager...");
        }

        Loader_Init();
        Ops_Init();

#if UNITY_EDITOR
        MockNetworkDriver networkDriver = new MockNetworkDriver(null);
        m_networkDriver = networkDriver;        

        MockDiskDriver diskDriver = new MockDiskDriver(null);
        m_diskDriver = diskDriver;
        //m_diskDriver.SetExceptionTypeToThrow(MockDiskDriver.EExceptionType.UnauthorizedAccess);
#else
        NetworkDriver networkDriver = new ProductionNetworkDriver();
        DiskDriver diskDriver = new ProductionDiskDriver();
#endif
        Logger downloadablesLogger = logger;
        m_downloadablesManager = new Downloadables.Manager(downloadablesConfig, networkDriver, diskDriver, null, tracker, downloadablesLogger);
        m_downloadablesManager.Initialize(downloadablesCatalog);

        LoadCatalog(localAssetBundlesPath);

        if (m_groups != null)
        {
            foreach (KeyValuePair<string, AssetBundlesGroup> pair in m_groups)
            {
                pair.Value.ResolveDependencies();
            }
        }

        if (CanLog())
        {
            Logger.Log("AssetBundlesManager initialized successfully");
        }
    }

#if UNITY_EDITOR
    public MockNetworkDriver GetMockNetworkDriver()
    {
        return m_networkDriver;
    }

    public MockDiskDriver GetMockDiskDriver()
    {
        return m_diskDriver;
    }
#endif

    private void LoadCatalog(string directory)
    {        
        string path = Path.Combine(directory, ASSET_BUNDLES_CATALOG_FILENAME_NO_EXTENSION);        
        TextAsset targetFile = Resources.Load<TextAsset>(path);
        JSONNode json = (targetFile != null) ? JSON.Parse(targetFile.text) : null;        

        if (json != null)
        {
            AssetBundlesCatalog catalog = new AssetBundlesCatalog();          
            catalog.Load(json, sm_logger);

            m_assetBundleHandles = new Dictionary<string, AssetBundleHandle>();

            // Full directory is used so we can use AssetBundle.LoadFromFileAsync() to load asset bundles from streaming assets and from downloadables folder in device storage
			string fullLocalDirectory = Application.streamingAssetsPath;//Path.Combine(Application.streamingAssetsPath, directory);
            fullLocalDirectory = Path.Combine(fullLocalDirectory, ASSET_BUNDLES_PATH_RELATIVE);

            List<string> assetBundleIds = catalog.GetAllAssetBundleIds();

            string id;
            AssetBundleHandle handle;
            List<string> dependencies;
            int count = assetBundleIds.Count;
            for (int i = 0; i < count; i++)
            {
                id = assetBundleIds[i];

                dependencies = new List<string>();
                catalog.GetAllDependencies(id, dependencies);

                // id is also included because it makes downloading logic easier
                dependencies.Add(id);

                handle = new AssetBundleHandle();

                // The path depends on whether or not the asset bundle is local
                if (m_downloadablesManager.Catalog_ContainsEntryStatus(id))
                {
                    handle.Setup(id, m_downloadablesManager.GetPathToDownload(id), dependencies, true);
                }
                else
                {
                    handle.Setup(id, Path.Combine(fullLocalDirectory, id), dependencies, false);
                }

                m_assetBundleHandles.Add(assetBundleIds[i], handle);
            }

            m_groups = catalog.GetGroups();         
        }
        else
        {
            if (CanLog())
            {
                Logger.Log("No " + ASSET_BUNDLES_CATALOG_FILENAME + " file found");
            }
        }
    }    

    public void Reset()
    {
        m_groups = null;

        if (m_assetBundleHandles != null)
        {
            foreach (KeyValuePair<string, AssetBundleHandle> pair in m_assetBundleHandles)
            {
                UnloadAssetBundle(pair.Key, null);
            }
        }

        m_assetBundleHandles = null;

        Ops_Reset();
        Loader_Reset();

        if (m_downloadablesManager != null)
        {
            m_downloadablesManager.Reset();
            m_downloadablesManager = null;
        }
    }

    private bool IsInitialized()
    {
        return m_assetBundleHandles != null;
    }

    public bool IsAutomaticDownloaderEnabled
    {
        get
        {
            return (m_downloadablesManager == null) ? false : m_downloadablesManager.IsAutomaticDownloaderEnabled;
        }

        set
        {
            if (m_downloadablesManager != null)
            {
                m_downloadablesManager.IsAutomaticDownloaderEnabled = value;
            }
        }
    }

    public bool IsDownloaderEnabled
    {
        get
        {
            return (m_downloadablesManager == null) ? false : m_downloadablesManager.IsEnabled;
        }

        set
        {
            if (m_downloadablesManager != null)
            {
                m_downloadablesManager.IsEnabled = value;
            }
        }
    }

    public AssetBundleHandle GetAssetBundleHandle(string id)
    {
        AssetBundleHandle returnValue = null;
        if (m_assetBundleHandles != null && !string.IsNullOrEmpty(id))
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

    public List<string> GetDependenciesIncludingSelfList(List<string> ids)
    {
        List<string> returnValue = null;
        if (ids != null)
        {
            returnValue = new List<string>();

            int count = ids.Count;
            List<string> abIds;
            for (int i = 0; i < count; i++)
            {
                abIds = GetDependenciesIncludingSelf(ids[i]);
                UbiListUtils.AddRange(returnValue, abIds, false, true);
            }
        }

        return returnValue;
    }

    private AssetBundlesOpRequest PreprocessRequest(bool buildRequest, ref AssetBundlesOp.OnDoneCallback onDone)
    {
        AssetBundlesOpRequest returnValue = null;
        if (buildRequest)
        {
            returnValue = new AssetBundlesOpRequest();
            returnValue.Setup(onDone);
            onDone = returnValue.NotifyResult;
        }

        return returnValue;
    }

    private void PostprocessRequest(AssetBundlesOpRequest request, AssetBundlesOp op)
    {
        if (request != null)
        {
            request.Op = op;
        }
    }

    /// <summary>
    /// Returns whether or not the asset bundle which id is passed as a parameter is available to be loaded,
    /// which means that either the asset bundle is local or it's remote and it has already been downloaded.
    /// </summary>  
    public bool IsAssetBundleAvailable(string id, bool track = false)
    {
        bool returnValue = IsAssetBundleValid(id);
        if (returnValue)
        {
            AssetBundleHandle handle = GetAssetBundleHandle(id);
            if (handle.IsRemote())
            {
                returnValue = m_downloadablesManager.IsIdAvailable(id, track);
            }
        }

        return returnValue;
    }

    public bool IsAssetBundleListAvailable(List<string> ids, bool track = false)
    {
        bool returnValue = IsAssetBundleListValid(ids);
        if (returnValue)
        {
			if(ids != null) 
			{
				int count = ids.Count;
				for(int i = 0; i < count; i++) 
				{
                    // IsAssetBundleAvailable() must be called for every id so that the result will be tracked if it needs to
                    returnValue = IsAssetBundleAvailable(ids[i], track) && returnValue;
                }
			}
        }

        return returnValue;
    }

    private float GetAssetBundleMbDownloadedSoFar(string id)
    {
        float returnValue = 0f;
        if (IsAssetBundleValid(id))
        {
            AssetBundleHandle handle = GetAssetBundleHandle(id);
            if (handle.IsRemote())
            {
                m_downloadablesManager.GetIdMbLeftToDownload(id);
            }
        }         

        return returnValue;
    }

    public float GetAssetBundleMbLeftToDownload(string id)
    {
        float returnValue = 0f;
        if (IsAssetBundleValid(id))
        {
            AssetBundleHandle handle = GetAssetBundleHandle(id);
            if (handle.IsRemote())
            {
                m_downloadablesManager.GetIdMbLeftToDownload(id);
            }
        }

        return returnValue;
    }

    public float GetAssetBundleTotalMb(string id)
    {
        float returnValue = 0f;
        if (IsAssetBundleValid(id))
        {
            AssetBundleHandle handle = GetAssetBundleHandle(id);
            if (handle.IsRemote())
            {
                m_downloadablesManager.GetIdTotalMb(id);
            }
        }

        return returnValue;
    }

    public float GetAssetBundleListMbDownloadedSoFar(List<string> ids)
    {
        float returnValue = 0f;
        if (ids != null)
        {
            int count = ids.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += GetAssetBundleMbDownloadedSoFar(ids[i]);
            }
        }

        return returnValue;
    }

    public float GetAssetBundleListMbLeftToDownload(List<string> ids)
    {
        float returnValue = 0f;
        if (ids != null)
        {
            int count = ids.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += GetAssetBundleMbLeftToDownload(ids[i]);
            }
        }

        return returnValue;
    }

    public float GetAssetBundleListTotalMb(List<string> ids)
    {
        float returnValue = 0f;
        if (ids != null)
        {
            int count = ids.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += GetAssetBundleTotalMb(ids[i]);
            }
        }

        return returnValue;
    }

    public AssetBundlesGroup GetAssetBundlesGroup(string groupId)
    {
        AssetBundlesGroup returnValue = null;
        if (!string.IsNullOrEmpty(groupId) && m_groups != null)
        {
            m_groups.TryGetValue(groupId, out returnValue);
        }

        return returnValue;
    }

    public AssetBundlesGroup CreateAssetBundlesGroupFromList(string groupId, List<string> groupIds)
    {
        AssetBundlesGroup returnValue = new AssetBundlesGroup();
        List<string> assetBundleIds = new List<string>();
        if (groupIds != null)
        {
            int count = groupIds.Count;
            AssetBundlesGroup group;
            for (int i = 0; i < count; i++)
            {
                group = GetAssetBundlesGroup(groupId);
                if (group != null)
                {
                    UbiListUtils.AddRange(assetBundleIds, group.AssetBundleIds, false, true);
                }
            }
        }

        returnValue.Setup(groupId, groupIds);

        return returnValue;
    }

    public AssetBundlesOpRequest DownloadAssetBundleAndDependencies(string id, AssetBundlesOp.OnDoneCallback onDone, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue;

        if (IsAssetBundleValid(id))
        {
            List<string> abIds = GetDependenciesIncludingSelf(id);
            returnValue = DownloadAssetBundleList(abIds, onDone, buildRequest);
        }
        else
        {
            returnValue = PreprocessRequest(buildRequest, ref onDone);
            onDone(AssetBundlesOp.EResult.Error_AB_Handle_Not_Found, null);
        }

        return returnValue;
    }

    public AssetBundlesOpRequest DownloadAssetBundleList(List<string> ids, AssetBundlesOp.OnDoneCallback onDone, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);
        AssetBundlesOp op = DownloadAssetBundleListInternal(ids, onDone);
        PostprocessRequest(returnValue, op);

        return returnValue;
    }

    private AssetBundlesOp DownloadAssetBundleListInternal(List<string> ids, AssetBundlesOp.OnDoneCallback onDone)
    {
        AssetBundlesOp returnValue = null;

        if (ids != null && ids.Count > 0)
        {
            if (IsAssetBundleListValid(ids))
            {
                if (IsAssetBundleListAvailable(ids))
                {
                    if (onDone != null)
                    {
                        onDone(AssetBundlesOp.EResult.Success, null);
                    }
                }
                else
                {
                    int count = ids.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (!IsAssetBundleAvailable(ids[i]))
                        {
                            m_downloadablesManager.RequestId(ids[i]);
                        }
                    }

                    // Creates an op that will be done and will call onDone callback when all downloads are done
                    // or when any downloads fail
                    DownloadAssetBundleListOp op = new DownloadAssetBundleListOp();
                    op.Setup(ids, onDone);
                    Ops_PerformOp(op);

                    returnValue = op;
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

        return returnValue;
    }


    public AssetBundlesOpRequest LoadAssetBundleAndDependencies(string id, AssetBundlesOp.OnDoneCallback onDone, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue;

        if (IsAssetBundleValid(id))
        {
            List<string> abIds = GetDependenciesIncludingSelf(id);
            returnValue = LoadAssetBundleList(abIds, onDone, buildRequest);
        }
        else
        {
            returnValue = PreprocessRequest(buildRequest, ref onDone);
            onDone(AssetBundlesOp.EResult.Error_AB_Handle_Not_Found, null);
        }

        return returnValue;
    }        

    private AssetBundlesOp LoadAssetBundleListInternal(List<string> ids, AssetBundlesOp.OnDoneCallback onDone)
    {
        AssetBundlesOp returnValue = null;

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

                    returnValue = op;
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

        return returnValue;
    }

    public AssetBundlesOpRequest LoadAssetBundleList(List<string> ids, AssetBundlesOp.OnDoneCallback onDone, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);
        AssetBundlesOp op = LoadAssetBundleListInternal(ids, onDone);
        PostprocessRequest(returnValue, op);

        return returnValue;
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
    public AssetBundlesOpRequest LoadAssetAsync(string assetBundleId, string assetName, AssetBundlesOp.OnDoneCallback onDone, bool buildRequest = false)
    {        
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);

        if (!LoadAssetFromAssetBundlesFullOp.EarlyExit(assetBundleId, assetName, onDone))
        {
            LoadAssetFromAssetBundlesFullOp op = new LoadAssetFromAssetBundlesFullOp();
            op.Setup(assetBundleId, assetName, onDone);
            Ops_PerformOp(op);
            PostprocessRequest(returnValue, op);
        }

        return returnValue;
    }   

    /// <summary>
    /// Loads asynchronously an asset called <c>assetName</c> from an asset bundle. 
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="assetName">Name of the asset to load.</param>    
    /// <param name="onDone">Callback that will be called when the asset has been loaded or if an error happened throughout the process.</param>    
    public AssetBundlesOpRequest LoadAssetFromAssetBundleAsync(AssetBundle assetBundle, string assetName, AssetBundlesOp.OnDoneCallback onDone, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);        

        LoadAssetFromAssetBundleOp op = new LoadAssetFromAssetBundleOp();
        op.Setup(assetBundle, assetName, onDone);
        Ops_PerformOp(op);

        PostprocessRequest(returnValue, op);

        return returnValue;
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
    public AssetBundlesOp.EResult LoadScene(string assetBundleId, string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AssetBundlesOp.EResult result = AssetBundlesOp.EResult.Error_Internal;

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
                        result = AssetBundlesOp.EResult.Success;
                    }
                }
            }
            else
            {
                result = AssetBundlesOp.EResult.Error_AB_Is_Not_Loaded;
            }
        }  
        else
        {
            result = AssetBundlesOp.EResult.Error_AB_Handle_Not_Found;
        }

        return result;
    }   

    /// <summary>
    /// Loads asynchronously a scene called <c>sceneName</c> from an asset bundle with <c>assetBundleId</c> as id. This method will download and load the asset bundle before loading
    /// the scene from the asset bundle if it needs to.
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>    
    /// <param name="loadSceneMode">Allows you to specify whether or not to load the scene additively.</param>    
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public AssetBundlesOpRequest LoadSceneAsync(string assetBundleId, string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, AssetBundlesOp.OnDoneCallback onDone = null, bool buildRequest=false)
    {
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);

        if (!LoadSceneFromAssetBundlesFullOp.EarlyExit(assetBundleId, sceneName, onDone))
        {
            LoadSceneFromAssetBundlesFullOp op = new LoadSceneFromAssetBundlesFullOp();
            op.Setup(assetBundleId, sceneName, loadSceneMode, onDone);
            Ops_PerformOp(op);
            PostprocessRequest(returnValue, op);
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously a scene called <c>sceneName</c> from an asset bundle. 
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>        
    /// <param name="loadSceneMode">Allows you to specify whether or not to load the scene additively.</param>    
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public AssetBundlesOpRequest LoadSceneFromAssetBundleAsync(AssetBundle assetBundle, string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single, AssetBundlesOp.OnDoneCallback onDone = null, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);

        LoadSceneFromAssetBundleOp op = new LoadSceneFromAssetBundleOp();
        op.SetupLoad(assetBundle, sceneName, loadSceneMode, onDone);
        Ops_PerformOp(op);

        PostprocessRequest(returnValue, op);

        return returnValue;
    }

    /// <summary>
    /// Unloads asynchronously a scene called <c>sceneName</c> from an asset bundle with <c>assetBundleId</c> as id. There's no <c>UnloadScene()</c> method to unload synchronously a scene because    
    /// that option has been deprecated by Unity.
    /// </summary>    
    /// <param name="assetBundleId">Asset bundle id that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>        
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public AssetBundlesOpRequest UnloadSceneAsync(string assetBundleId, string sceneName, AssetBundlesOp.OnDoneCallback onDone = null, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue;
        if (LoadSceneFromAssetBundlesFullOp.EarlyExit(assetBundleId, sceneName, onDone))
        {
            returnValue = PreprocessRequest(buildRequest, ref onDone);
        }
        else
        {
            AssetBundleHandle handle = GetAssetBundleHandle(assetBundleId);
            returnValue = UnLoadSceneFromAssetBundleAsync(handle.AssetBundle, sceneName, onDone, buildRequest);            
        }

        return returnValue;
    }    

    /// <summary>
    /// Unloads asynchronously a scene called <c>sceneName</c>.
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="sceneName">Name of the scene to load.</param>            
    /// <param name="onDone">Callback that will be called when the scene has been loaded or if an error happened throughout the process.</param>    
    public AssetBundlesOpRequest UnLoadSceneFromAssetBundleAsync(AssetBundle assetBundle, string sceneName, AssetBundlesOp.OnDoneCallback onDone = null, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue = PreprocessRequest(buildRequest, ref onDone);

        LoadSceneFromAssetBundleOp op = new LoadSceneFromAssetBundleOp();
        op.SetupUnload(assetBundle, sceneName, onDone);
        Ops_PerformOp(op);

        PostprocessRequest(returnValue, op);

        return returnValue;
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
    public AssetBundlesOpRequest LoadResourceAsync(string assetBundleId, string resourceName, bool isAsset, AssetBundlesOp.OnDoneCallback onDone, LoadSceneMode mode = LoadSceneMode.Single, bool buildRequest = false)
    {
        AssetBundlesOpRequest returnValue;
        if (isAsset)
        {
            returnValue = LoadAssetAsync(assetBundleId, resourceName, onDone, buildRequest);
        }
        else
        {
            returnValue = LoadSceneAsync(assetBundleId, resourceName, mode, onDone, buildRequest);
        }

        return returnValue;
    }

    /// <summary>
    /// Loads asynchronously a resource called <c>resourceName</c> from an asset bundle 
    /// </summary>
    /// <param name="assetBundle">Asset bundle that contains the asset.</param>
    /// <param name="resourceName">Name of the resource to load.</param>    
    /// <param name="isAsset"><c>true</c> is the resource to load is an asset. <c>false</c> if it's a scene</param>    
    /// <param name="onDone">Callback that will be called when the resource has been loaded or if an error happened throughout the process.</param>    
    public void LoadResourceFromAssetBundleAsync(AssetBundle assetBundle, string resourceName, bool isAsset, AssetBundlesOp.OnDoneCallback onDone, LoadSceneMode mode = LoadSceneMode.Single, bool buildRequest = false)
    {
        if (isAsset)
        {
            LoadAssetFromAssetBundleAsync(assetBundle, resourceName, onDone, buildRequest);
        }
        else
        {
            LoadSceneFromAssetBundleAsync(assetBundle, resourceName, mode, onDone, buildRequest);
        }        
    }

    public Downloadables.CatalogEntryStatus GetDownloadablesCatalogEntryStatus(string id)
    {
        return m_downloadablesManager.Catalog_GetEntryStatus(id);
    }

    public Dictionary<string, Downloadables.CatalogEntryStatus> GetDownloadablesCatalog()
    {
        return m_downloadablesManager.Catalog_GetEntryStatusList();
    }

    public void Update()
    {
        if (IsInitialized())
        {            
            Loader_Update();
            m_downloadablesManager.Update();
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
                if (handle != null && handle.NeedsToRequestToLoad())
                {
                    handle.OnPendingToRequestToLoad();
                    m_loaderRequests.Enqueue(id);
                }                
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

    public float Loader_GetProgress(string id)
    {
        float returnValue = 0f;
        AssetBundleHandle handle = GetAssetBundleHandle(id);
        if (handle != null)
        {
            if (handle.IsLoaded())
            {
                returnValue = 1f;
            }
            else if (handle.IsLoading())
            {
                if (m_loaderLoadAssetBundleOp != null)
                {
                    AssetBundleHandle loaderHandle = m_loaderLoadAssetBundleOp.GetHandle();
                    if (loaderHandle != null && loaderHandle.Id == id)
                    {
                        returnValue = m_loaderLoadAssetBundleOp.Progress;
                    }
                }
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

    public static bool CanLog()
    {
        return sm_logger != null && sm_logger.CanLog();
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
