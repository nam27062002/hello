﻿#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for tayloring Addressables UbiPackage to meet Hungry Dragon needs
/// </summary>
public class HDAddressablesManager
{
    private static HDAddressablesManager sm_instance;

    public static HDAddressablesManager Instance
    {
        get
        {
            if (sm_instance == null)
            {
                sm_instance = new HDAddressablesManager();
            }

            return sm_instance;
        }
    }

    private AddressablesManager m_addressablesManager;

    private HDDownloadablesTracker m_tracker;

    private string LastSceneId { get; set; }

    private HDAddressablesManager()
    {
        m_addressablesManager = new AddressablesManager();
        Flavour_Init();
    }

    /// <summary>
    /// Make sure this method is called after ContentDeltaManager.OnContentDelta() was called since this method uses data from assetsLUT to create downloadables catalog.
    /// </summary>
    public void Initialize()
    {
        LastSceneId = null;

#if ENABLE_LOGS
        Logger logger = new CPLogger(ControlPanel.ELogChannel.Addressables);
#else
        Logger logger = null;
#endif

        // Addressables catalog     
        string catalogAsText = GetAddressablesFileText("addressablesCatalog", true);
        JSONNode catalogAsJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Retrieves downloadables catalog
        // TODO: Retrieve this information from the assetsLUT cached
        // Downloadables catalog        
        /*
        catalogAsText = GetAddressablesFileText("downloadablesCatalog", true);
        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        */

        // Downloadables config        
        catalogAsText = GetAddressablesFileText("downloadablesConfig", false);
        JSONNode downloadablesConfigAsJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        Downloadables.Config downloadablesConfig = new Downloadables.Config();
        downloadablesConfig.Load(downloadablesConfigAsJSON, logger);

        // downloadablesCatalog is created out of latest assetsLUT
        ContentDeltaManager.ContentDeltaData assetsLUT = ContentDeltaManager.SharedInstance.m_kServerDeltaData;
        if (assetsLUT == null)
        {
            assetsLUT = ContentDeltaManager.SharedInstance.m_kLocalDeltaData;
        }

        m_tracker = new HDDownloadablesTracker(downloadablesConfig, logger);
        JSONNode downloadablesCatalogAsJSON = AssetsLUTToDownloadablesCatalog(assetsLUT);

        // AssetBundles catalog
        catalogAsText = GetAddressablesFileText("assetBundlesCatalog", true);
        JSONNode abCatalogAsJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

#if UNITY_EDITOR && false
        bool useMockDrivers = true;
#else
        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
        bool useMockDrivers = (kServerConfig != null && kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION);
#endif
        m_addressablesManager.Initialize(catalogAsJSON, abCatalogAsJSON, downloadablesConfig, downloadablesCatalogAsJSON, useMockDrivers, m_tracker, logger);

        InitDownloadableHandles();
        InitAddressablesAreas();
    }

    private string GetAddressablesFileText(string fileName, bool platformDependent)
    {
#if UNITY_EDITOR
        string path = "Assets/Editor/Addressables/generated/";
        if (platformDependent)
        {
            path += UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        }

        path += fileName + ".json";
        return File.ReadAllText(path);
#else
        string path = "Addressables/" + fileName;                        
        TextAsset targetFile = Resources.Load<TextAsset>(path);
        return (targetFile == null) ? null : targetFile.text;
#endif
    }

    public bool IsInitialized()
    {
        return m_addressablesManager.IsInitialized();
    }

    public bool IsReady()
    {
        return m_addressablesManager.IsReady();
    }

    public void Reset()
    {
        m_addressablesManager.Reset();
        Flavour_Reset();
    }

    private string GetEnvironmentUrlBase()
    {
        string urlBase = "";

        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
        if (kServerConfig != null)
        {
            urlBase = "http://hdragon-assets-s3.akamaized.net/";
            switch (kServerConfig.m_eBuildEnvironment)
            {
                case CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION:
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";
                    //urlBase = "http://hdragon-assets-s3.akamaized.net/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_STAGE_QC:
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";

                    // Link to CDN (it uses cache)
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";

                    // Direct link to the bucket (no cache involved, which might make downloads more expensive)
                    //urlBase = "https://s3.us-east-2.amazonaws.com/hdragon-assets/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_STAGE:
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_INTEGRATION:
                case CaletyConstants.eBuildEnvironments.BUILD_DEV:
                case CaletyConstants.eBuildEnvironments.BUILD_LOCAL:
                    urlBase = "http://bcn-mb-services1.ubisoft.org/hungrydragon";
                    break;
            }

            //http://10.44.4.69:7888/                                
        }

        return urlBase;
    }    

    private static string GetUrlWithEnvironment(string urlBase)
    {
        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
        return CaletyConstants.GetUrlWithEnvironment(urlBase, kServerConfig.m_eBuildEnvironment);
    }

    private static string GetUrlWithoutEnvironment(string urlBase)
    {
        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
        return CaletyConstants.GetUrlWithoutEnvironment(urlBase, kServerConfig.m_eBuildEnvironment);
    }

    private JSONNode AssetsLUTToDownloadablesCatalog(ContentDeltaManager.ContentDeltaData assetsLUT)
    {
        if (assetsLUT != null)
        {
            // urlBase is taken from assetLUT unless it's empty or "localhost", which means that assetsLUT is the one in the build, which must be adapted to the environment
            string urlBase = assetsLUT.m_strURLBase;

            bool calculateProdUrl = true;

#if UNITY_EDITOR
            // We don't need to use prod url when using the local server 
            calculateProdUrl = !urlBase.Contains(":7888");            
            if (!calculateProdUrl)            
            {
                urlBase = GetUrlWithoutEnvironment(urlBase);
                Downloadables.Downloader.USE_CRC_IN_URL = false;
            }
#endif
            if (calculateProdUrl)
            {
                if (string.IsNullOrEmpty(urlBase) || urlBase.Contains("localhost"))
                {
                    assetsLUT.m_strURLBase = GetUrlWithEnvironment(GetEnvironmentUrlBase());
                }

                urlBase = assetsLUT.GetAssetsBundleUrlBase();
            }

            Downloadables.Catalog catalog = new Downloadables.Catalog();
            catalog.UrlBase = urlBase;

            Downloadables.CatalogEntry entry;
            string key;
            foreach (KeyValuePair<string, long> pair in assetsLUT.m_kAssetCRCs)
            {
                // Only bundles are considered
                if (!ContentManager.USE_ASSETS_LUT_V2 || assetsLUT.m_kAssetTypes[pair.Key] == ContentDeltaManager.ContentDeltaData.EAssetType.Bundle)
                {
                    entry = new Downloadables.CatalogEntry();
                    entry.CRC = pair.Value;
                    entry.Size = assetsLUT.m_kAssetSizes[pair.Key];
                    key = pair.Key;
                    catalog.AddEntry(pair.Key, entry);
                }
            }

            if (!ContentManager.USE_ASSETS_LUT_V2 || !calculateProdUrl)
            {
                return Downloadables.Manager.GetCatalogFromAssetsLUT(catalog.ToJSON(), calculateProdUrl);
            }
            else
            {
                return catalog.ToJSON();
            }
        }
        else
        {
            return null;
        }
    }

    #region flavour

    public const string FLAVOUR_VARIANT_SEPARATOR = "/";

    private Dictionary<string, Dictionary<string, string>> m_flavourVariants;

    private void Flavour_Init()
    {
        m_flavourVariants = new Dictionary<string, Dictionary<string, string>>();
    }

    private void Flavour_Reset()
    {
        if (m_flavourVariants != null)
        {
            m_flavourVariants.Clear();
        }
    }

    private string Flavour_GetVariantPerFlavour(string flavourSku, string variant)
    {
        string returnValue = variant;
        
        if (string.IsNullOrEmpty(flavourSku))
        {
            AddressablesManager.LogError("Flavour mustn't be empty");
        }
        else
        {
            // If variant is empty then we need to use the flavourSku as flavoured variant
            // otherwise the flavoured variant looks like: flavourSku-variant
            if (string.IsNullOrEmpty(variant))
            {
                returnValue = flavourSku;
            }
            else
            {
                // Flavoured variants are stored in order to prevent memory from being allocated every time
                // a variant needs to be flavoured
                if (!m_flavourVariants.ContainsKey(flavourSku))
                {
                    m_flavourVariants.Add(flavourSku, new Dictionary<string, string>());
                }

                Dictionary<string, string> flavourDict = m_flavourVariants[flavourSku];
                if (!flavourDict.ContainsKey(variant))
                {
                    flavourDict.Add(variant, flavourSku + FLAVOUR_VARIANT_SEPARATOR + variant);
                }

                returnValue = flavourDict[variant];
            }
        }

        return returnValue;
    }

    private string Flavour_GetVariant(string addressablesId, string variant)
    {
        string addressablesVariant = FlavourManager.Instance.GetCurrentFlavour().AddressablesVariantAsString;
        string returnValue = Flavour_GetVariantPerFlavour(addressablesVariant, variant);
        
        // Default flavour is used if flavourSku is not already the default one and no resouce is
        // defined for flavouredSku
        if (addressablesVariant != Flavour.ADDRESSABLES_VARIANT_DEFAULT_SKU && !m_addressablesManager.ExistsResource(addressablesId, returnValue))
        {
            returnValue = Flavour_GetVariantPerFlavour(Flavour.ADDRESSABLES_VARIANT_DEFAULT_SKU, variant);
        }

        return returnValue;
    }
    #endregion

    public List<string> GetDependencyIds(string id, string variant = null)
    {
        return m_addressablesManager.GetDependencyIds(id, Flavour_GetVariant(id, variant));
    }

    public List<string> GetDependencyIdsList(List<string> ids, string variant = null)
    {
        List<string> returnValue = null;
        if (ids != null)
        {
            returnValue = new List<string>();

            // We need to loop through all ids because we can not assume that they all share the same flavoured variant
            List<string> thisDependencies;
            int count = ids.Count;
            for (int i = 0; i < count; i++)
            {
                thisDependencies = GetDependencyIds(ids[i], variant);
                UbiListUtils.AddRange(returnValue, thisDependencies, false, true);    
            }
        }

        return returnValue;
    }

    public bool IsDependencyListAvailable(List<string> dependencyIds)
    {
        return m_addressablesManager.IsDependencyListAvailable(dependencyIds);
    }

    public AddressablesOp LoadDependencyIdsListAsync(List<string> dependencyIds)
    {
        return m_addressablesManager.LoadDependencyIdsListAsync(dependencyIds);
    }

    public void UnloadDependencyIdsList(List<string> dependencyIds)
    {
        m_addressablesManager.UnloadDependencyIdsList(dependencyIds);
    }

    public List<string> GetAssetBundlesGroupDependencyIds(string groupId)
    {
        return m_addressablesManager.GetAssetBundlesGroupDependencyIds(groupId);
    }

    public AddressablesOp LoadDependenciesAsync(string id, string variant = null)
    {
        return m_addressablesManager.LoadDependenciesAsync(id, Flavour_GetVariant(id, variant));
    }

    public UbiAsyncOperation LoadDependenciesAndSceneAsync(string id, string variant = null)
    {
        if (IsInitialized())
        {
            return LoadAddressablesBatchAsynch(id);            
        }
        else
        {
            return LoadSceneAsync(id, Flavour_GetVariant(id, variant), LoadSceneMode.Single);
        }
    }

    public bool IsResourceAvailable(string id, string variant = null, bool track = false)
    {
        return m_addressablesManager.IsResourceAvailable(id, Flavour_GetVariant(id, variant), track);
    }

    public bool ExistsResource(string id, string variant)
    {
        return m_addressablesManager.ExistsResource(id, Flavour_GetVariant(id, variant));
    }

    public bool ExistsDependencyId(string dependencyId)
    {
        return m_addressablesManager.ExistsDependencyId(dependencyId);
    }

    public void AddResourceListDownloadableIdsToHandle(Downloadables.Handle handle, List<string> addressableIds, string variant = null)
    {
        if (handle != null && addressableIds != null)
        {
            // We need to loop through all ids because we can not assume that they all share the same flavoured variant
            int count = addressableIds.Count;
            for (int i = 0; i < count; i++)
            {
                m_addressablesManager.AddResourceDownloadableIdsToHandle(handle, addressableIds[i], Flavour_GetVariant(addressableIds[i], variant));
            }
        }
    }

    // This method has been overridden in order to let the game load a scene before AddressablesManager has been initialized, typically the first loading scene, 
    // which may be called when rebooting the game
    public bool LoadScene(string id, string variant = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsInitialized())
        {            
            return m_addressablesManager.LoadScene(id, Flavour_GetVariant(id, variant), mode);           
        }
        else
        {
            SceneManager.LoadScene(id, mode);
            return true;
        }
    }

    // This method has been overridden in order to let the game load a scene before AddressablesManager has been initialized, typically the first loading scene,
    // which may be called when rebooting the game
    public AddressablesOp LoadSceneAsync(string id, string variant = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsInitialized())
        {            
            return m_addressablesManager.LoadSceneAsync(id, Flavour_GetVariant(id, variant), mode);                        
        }
        else
        {
            AddressablesAsyncOp op = new AddressablesAsyncOp();            
            op.Setup(new UbiUnityAsyncOperation(SceneManager.LoadSceneAsync(id, mode)));
            return op;
        }        
    }

    public AddressablesOp UnloadSceneAsync(string id, string variant = null)
    {
        if (IsInitialized())
        {
            return m_addressablesManager.UnloadSceneAsync(id, Flavour_GetVariant(id, variant));
        }
        else
        {
            AddressablesAsyncOp op = new AddressablesAsyncOp();
            op.Setup(new UbiUnityAsyncOperation(SceneManager.UnloadSceneAsync(id)));
            return op;
        }
    }

    public T LoadAsset<T>(string id, string variant = null) where T : UnityEngine.Object
    {       
        return m_addressablesManager.LoadAsset<T>(id, Flavour_GetVariant(id, variant));
    }

    public object LoadAsset(string id, string variant = null)
    {        
        return m_addressablesManager.LoadAsset(id, Flavour_GetVariant(id, variant));
    }

    public AddressablesOp LoadAssetAsync(string id, string variant = null)
    {        
        return m_addressablesManager.LoadAssetAsync(id, Flavour_GetVariant(id, variant));
    }

    public void FillWithLoadedAssetBundleIdList(List<string> ids)
    {
        m_addressablesManager.FillWithLoadedAssetBundleIdList(ids);
    }

    public void Update()
    {
        m_addressablesManager.Update();

        // We don't want the downloader to interfere with the ingame experience
        m_addressablesManager.IsDownloaderEnabled = !FlowManager.IsInGameScene();

        // Downloader is disabled while the app is loading in order to let it load faster and smoother
        m_addressablesManager.IsAutomaticDownloaderEnabled = !GameSceneManager.isLoading && IsAutomaticDownloaderAllowed();

        UpdateAddressablesAreas();
    }

    public bool IsAutomaticDownloaderAllowed()
    {
        if (FeatureSettingsManager.AreCheatsEnabled && !DebugSettings.isAutomaticDownloaderEnabled)
            return false;

        // If permission has already been requested for any downloadable handle then automatic downloader should be enabled so that request can be served
        if (HasPermissionRequestedForAnyDownloadableHandle())
            return true;

		// Automatic downloader gets unlocked when the user buys a dragon or when the user unlocks the second dragon. This is done to save up on traffic since many user won't made it to the second dragon.
        if (UsersManager.currentUser.GetNumOwnedDragons() > 1) {
			return true;
		}

        // Second progression dragon is unlocked
        IDragonData dragonData = DragonManager.GetClassicDragonsByOrder(1);
        return dragonData != null && !dragonData.isLocked;
    }

    #region addressables_areas
    private HDAddressablesAreaLoader m_addressablesAreaLoader;

    private void InitAddressablesAreas()
    {
        m_addressablesAreaLoader = new HDAddressablesAreaLoader();       
    }

    private void ResetAddressablesAreas()
    {
        if (m_addressablesAreaLoader != null)
        {
            m_addressablesAreaLoader.Reset();
        }
    }    
    
    private HDAddressablesAreaLoader LoadAddressablesBatchAsynch(string sceneId)
    {
        AddressablesBatchHandle handle = GetAddressablesAreaBatchHandle(sceneId);

        if (m_addressablesAreaLoader != null)
        {
            m_addressablesAreaLoader = new HDAddressablesAreaLoader();
        }

        // First loading mustn't show the dragon icon
        m_addressablesAreaLoader.Setup(handle, sceneId, ((sceneId == MenuSceneController.NAME && !string.IsNullOrEmpty(LastSceneId)) || sceneId == GameSceneController.NAME));

        LastSceneId = sceneId;

        return m_addressablesAreaLoader;
    }
    
    public AddressablesBatchHandle GetAddressablesAreaBatchHandle(string sceneId)
    {
        AddressablesBatchHandle returnValue = new AddressablesBatchHandle();        
        if (sceneId == MenuSceneController.NAME)
        {
            // Menu            
            AddMenuDependenciesToAddressablesBatchHandle(returnValue);
        }
        else if (sceneId == GameSceneController.NAME)
        {
            // Game
            AddGameDependenciesToAddressablesBatchHandle(returnValue);            
        }
        else if (sceneId == ResultsScreenController.NAME)
        {
            // Results
            AddResultsDependenciesToAddressablesBatchHandle(returnValue);
        }

        return returnValue;
    }

    public void UpdateAddressablesAreas()
    {
        if (m_addressablesAreaLoader != null)
        {
            m_addressablesAreaLoader.Update();
        }
    }
#endregion


#region ingame
public class Ingame_SwitchAreaHandle
    {        
        private List<string> PrevAreaRealSceneNames { get; set; }
        private List<string> NextAreaRealSceneNames { get; set; }

        private List<string> DependencyIdsToUnload { get; set; }
        private List<string> DependencyIdsToLoad { get; set; }

        private List<AddressablesOp> m_prevAreaScenesToUnload;
        private List<AddressablesOp> m_nextAreaScenesToLoad;        

        public Ingame_SwitchAreaHandle(string prevArea, string nextArea, List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames, List<string> dependenciesToStayInMemory)
        {
            PrevAreaRealSceneNames = prevAreaRealSceneNames;
            NextAreaRealSceneNames = nextAreaRealSceneNames;

            // Retrieves the dependencies of the previous area scenes
            List<string> prevAreaSceneDependencyIds = Instance.GetDependencyIdsList(prevAreaRealSceneNames);

            // Retrieves the dependencies of the next area scenes
            List<string> nextAreaSceneDependencyIds = Instance.GetDependencyIdsList(nextAreaRealSceneNames);

            // Retrieves the dependencies of the previous area defined in addressablesCatalog.json
            List<string> prevAreaDependencyIds = Instance.GetAssetBundlesGroupDependencyIds(prevArea);

            // Adds the dependencies of the scenes and the dependencies of the addressables areas
            prevAreaDependencyIds = UbiListUtils.AddRange(prevAreaDependencyIds, prevAreaSceneDependencyIds, true, true);

            // Retrieves the dependencies of the next area defined in addressablesCatalog.json
            List<string> nextAreaDependencyIds = Instance.GetAssetBundlesGroupDependencyIds(nextArea);

            // Adds the dependencies of the scenes and the dependencies of the addressables areas
            nextAreaDependencyIds = UbiListUtils.AddRange(nextAreaDependencyIds, nextAreaSceneDependencyIds, true, true);

            List<string> assetBundleIdsToStay;
            List<string> assetBundleIdsToUnload;
            UbiListUtils.SplitIntersectionAndDisjoint(prevAreaDependencyIds, nextAreaDependencyIds, out assetBundleIdsToStay, out assetBundleIdsToUnload);

            List<string> assetBundleIdsToLoad;            
            UbiListUtils.SplitIntersectionAndDisjoint(nextAreaDependencyIds, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

            // Makes sure that the dependencyIds (typically the ones that player's dragon and pets depend on) that need to stay in memory are not unloaded
            List<string> actualAssetBundleIdsToUnload;
            UbiListUtils.SplitIntersectionAndDisjoint(assetBundleIdsToUnload, dependenciesToStayInMemory, out assetBundleIdsToStay, out actualAssetBundleIdsToUnload);

            List<string> actualAssetBundleIdsToLoad;
            UbiListUtils.SplitIntersectionAndDisjoint(assetBundleIdsToLoad, dependenciesToStayInMemory, out assetBundleIdsToStay, out actualAssetBundleIdsToLoad);

            DependencyIdsToLoad = actualAssetBundleIdsToLoad;
            DependencyIdsToUnload = actualAssetBundleIdsToUnload;            
        }       

        public bool NeedsToUnloadPrevAreaDependencies()
        {
            return DependencyIdsToUnload != null && DependencyIdsToUnload.Count > 0;
        }

        public void UnloadPrevAreaDependencies()
        {
            Instance.UnloadDependencyIdsList(DependencyIdsToUnload);            
        }

        public bool NeedsToUnloadPrevAreaScenes()
        {
            return PrevAreaRealSceneNames != null && PrevAreaRealSceneNames.Count > 0;
        }

        public List<AddressablesOp> UnloadPrevAreaScenes()
        {            
            if (m_prevAreaScenesToUnload == null)
            {
                m_prevAreaScenesToUnload = new List<AddressablesOp>();
                if (PrevAreaRealSceneNames != null)
                {
                    int count = PrevAreaRealSceneNames.Count;
                    for (int i = 0; i < count; i++)
                    {
                        m_prevAreaScenesToUnload.Add(Instance.UnloadSceneAsync(PrevAreaRealSceneNames[i]));
                    }
                }
            }

            return m_prevAreaScenesToUnload;            
        }

        public bool NeedsToLoadNextAreaDependencies()
        {
            return DependencyIdsToLoad != null && DependencyIdsToLoad.Count > 0;
        }

        public AddressablesOp LoadNextAreaDependencies()
        {
            return Instance.LoadDependencyIdsListAsync(DependencyIdsToLoad);
        }

        public bool NeedsToLoadNextAreaScenes()
        {
            return NextAreaRealSceneNames != null && NextAreaRealSceneNames.Count > 0;
        }

        public List<AddressablesOp> LoadNextAreaScenesAsync()
        {            
            if (m_nextAreaScenesToLoad == null)
            {
                m_nextAreaScenesToLoad = new List<AddressablesOp>();
                if (NextAreaRealSceneNames != null)
                {
                    int count = NextAreaRealSceneNames.Count;
                    for (int i = 0; i < count; i++)
                    {                        
                        m_nextAreaScenesToLoad.Add(Instance.LoadSceneAsync(NextAreaRealSceneNames[i], null, LoadSceneMode.Additive));
                    }
                }
            }

            return m_nextAreaScenesToLoad;            
        }

        public void LoadNextAreaScenes()
        {
            if (NextAreaRealSceneNames != null)
            {
                int count = NextAreaRealSceneNames.Count;
                for (int i = 0; i < count; i++)
                {
                    Instance.LoadScene(NextAreaRealSceneNames[i], null, LoadSceneMode.Additive);
                }
            }
        }

        public void UnloadNextAreaDependencies()
        {
            Instance.UnloadDependencyIdsList(DependencyIdsToLoad);
        }
    }    

    public Ingame_SwitchAreaHandle Ingame_SwitchArea(string prevArea, string newArea, List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames, List<string> dependencyIdsToStayInMemory)
    {
        // Makes sure that all scenes are available. For now a scene is not scheduled to be loaded if it's not available because there's no support
        // for downloading stuff during the loading screen. 
        // TODO: To delete this stuff when downloading unavailable stuff flow is implemented
        if (nextAreaRealSceneNames != null)
        {            
            for (int i = 0; i < nextAreaRealSceneNames.Count;)
            {
                if (IsResourceAvailable(nextAreaRealSceneNames[i], null, true))
                {
                    i++;                    
                }
                else
                {
                    // Avoid this scene to be loaded since it's not available
                    nextAreaRealSceneNames.RemoveAt(i);                    
                }
            }
        }

        return new Ingame_SwitchAreaHandle(prevArea, newArea, prevAreaRealSceneNames, nextAreaRealSceneNames, dependencyIdsToStayInMemory);
    }

    /// <summary>
    /// Method called when the user leaves ingame and the whole level has been unloaded.
    /// </summary>
    public void Ingame_NotifyLevelUnloaded()
    {
        // We need to track the result of every downloadable required by ingame only once per run, so we need to reset it to leave it prepared for the next run
        m_tracker.ResetIdsLoadTracked();
    }
    #endregion

    #region groups
    private const string GROUP_MENU = "menu";
    private const string GROUP_LEVEL_AREA_1 = "area1";
    private const string GROUP_LEVEL_AREA_2 = "area2";
    private const string GROUP_LEVEL_AREA_3 = "area3";
    #endregion

    #region DOWNLOADABLE_GROUPS_HANDLERS
    // [AOC] Keep it hardcoded? For now it's fine since there aren't so many 
    //		 groups and rules can be tricky to represent in content

    private const string DOWNLOADABLE_GROUP_LEVEL_AREA_1        = GROUP_LEVEL_AREA_1;
    private const string DOWNLOADABLE_GROUP_LEVEL_AREA_2        = GROUP_LEVEL_AREA_2;
    private const string DOWNLOADABLE_GROUP_LEVEL_AREA_3        = GROUP_LEVEL_AREA_3;
    
    // Combined    
    private const string DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2     = "areas1_2";
    private const string DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2_3   = "areas1_2_3";

    // All downloadables
    private const string DOWNLOADABLE_GROUP_ALL                 = "all";

    /// <summary>
    /// Downloadable.Handle objects are cached because they're going to be requested ofter and generating them often may trigger garbage collector often
    /// </summary>
    private Dictionary<string, Downloadables.Handle> m_downloadableHandles;

    /// <summary>
    /// Initializes and stores all downloadable handles that will be required throughout the game session
    /// </summary>
    private void InitDownloadableHandles()
    {
        m_downloadableHandles = new Dictionary<string, Downloadables.Handle>();

        // All handles should be created and stored here

        //
        // Level groups
        //
        // Area 1
        Downloadables.Handle handle = m_addressablesManager.CreateDownloadablesHandle(DOWNLOADABLE_GROUP_LEVEL_AREA_1);
        m_downloadableHandles.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_1, handle);

        // Areas 1 and 2
        HashSet<string> groupIds = new HashSet<string>();
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_1);
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_2);
        handle = m_addressablesManager.CreateDownloadablesHandle(groupIds);
        m_downloadableHandles.Add(DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2, handle);

        // Areas 1,2 and 3
        groupIds = new HashSet<string>();
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_1);
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_2);
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_3);
        handle = m_addressablesManager.CreateDownloadablesHandle(groupIds);
        m_downloadableHandles.Add(DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2_3, handle);
    }
    
    /// <summary>
    /// Returns the downloadables handle corresponding to the id passed as a parameter
    /// </summary>
    /// <param name="handleId">Id of the handle requested. This id is the one used in <c>InitDownloadableHandles()</c> when the handles were created.</param>
    /// <returns>Returns a <c>Downloadables.Handle</c> object which handles the downloading of all downloadables associated to the groups used when <c>handleId</c> was created.</returns>
    private Downloadables.Handle GetDownloadablesHandle(string handleId)
    {
        Downloadables.Handle returnValue = null;
        if (m_downloadableHandles != null && !string.IsNullOrEmpty(handleId))
        {
            m_downloadableHandles.TryGetValue(handleId, out returnValue);
        }

        return returnValue;
    }

    /// <summary>
    /// Get the handle for all downloadables required for a specific dragon in its
    /// current state: owned status, skin equipped, pets equipped, etc...
    /// </summary>
    /// <returns>The handle for all downloadables required for that dragon.</returns>
    /// <param name="_dragonSku">Classic dragon sku.</param>
    public Downloadables.Handle GetHandleForClassicDragon(string _dragonSku) {
		// Create new handle. We will fill it with downloadable Ids based on target dragon status
		Downloadables.Handle handle = m_addressablesManager.CreateDownloadablesHandle();
		List<string> resourceIDs = new List<string>();

		// Get target dragon info
		IDragonData dragonData = DragonManager.GetDragonData(_dragonSku);

		// Figure out group dependencies for this dragon
		// 1. Level area dependencies: will depend basically on the dragon's tier
		//    Some powers allow dragons to go to areas above their tier, check for those cases as well
		List<string> equippedPowers = GetPowersList(dragonData.persistentDisguise, dragonData.pets);
		Downloadables.Handle levelHandle = GetLevelHandle(dragonData.tier, equippedPowers);
		handle.AddDownloadableIds(levelHandle.GetDownloadableIds());

		// 2. Dragon bundles
		resourceIDs.AddRange(GetResourceIDsForDragon(_dragonSku));

		// 3. Equipped pets bundles
		for(int i = 0; i < dragonData.pets.Count; ++i) {
			resourceIDs.AddRange(GetResourceIDsForPet(dragonData.pets[i]));
		}

		// No more dependencies to be checked: Add resource IDs and return!
		Instance.AddResourceListDownloadableIdsToHandle(handle, resourceIDs);
		return handle;
	}

	/// <summary>
	/// Same as <see cref="GetHandleForClassicDragon(string)"/> but for a special dragon.
	/// </summary>
	/// <returns>The handle for all downloadables required for that dragon.</returns>
	/// <param name="_dragonSku">Special dragon sku.</param>
	public Downloadables.Handle GetHandleForSpecialDragon(string _dragonSku) {
		// For now, same logic as with the classic dragons applies :)
		return GetHandleForClassicDragon(_dragonSku);
	}

	/// <summary>
	/// Same as <see cref="GetHandleForClassicDragon(string)"/> but for a dragon in a tournament.
	/// </summary>
	/// <returns>The handle for all downloadables required for that dragon.</returns>
	/// <param name="_tournament">Tournament data.</param>
	public Downloadables.Handle GetHandleForTournamentDragon(HDTournamentManager _tournament) {
		// Create new handle. We will fill it with downloadable Ids based on target dragon status
		Downloadables.Handle handle = m_addressablesManager.CreateDownloadablesHandle();
		List<string> resourceIDs = new List<string>();

		// Get target dragon info
		IDragonData dragonData = _tournament.tournamentData.tournamentDef.dragonData;

		// Figure out group dependencies for this dragon
		// 1. Level area dependencies: will depend basically on the dragon's tier
		//    Some powers allow dragons to go to areas above their tier, check for those cases as well
		List<string> equippedPowers = GetPowersList(dragonData.disguise, dragonData.pets);
		Downloadables.Handle levelHandle = GetLevelHandle(dragonData.tier, equippedPowers);
		handle.AddDownloadableIds(levelHandle.GetDownloadableIds());

		// 2. Dragon bundles
		resourceIDs.AddRange(GetResourceIDsForDragon(dragonData.sku));

		// 3. Equipped pets bundles
		for(int i = 0; i < dragonData.pets.Count; ++i) {
			resourceIDs.AddRange(GetResourceIDsForPet(dragonData.pets[i]));
		}

		// 4. [AOC] TODO!! Level bundles for target spawn point. Feature not yet implemented.

		// No more dependencies to be checked: Add resource IDs and return!
		HDAddressablesManager.Instance.AddResourceListDownloadableIdsToHandle(handle, resourceIDs);
		return handle;
	}


    /// <summary>
    /// Get a handle for the specified disguise
    /// </summary>
    /// <returns>The handle for all downloadables required for that disguise.</returns>
    public AddressablesBatchHandle GetHandleForDragonDisguise(IDragonData _dragonData)
    {
        AddressablesBatchHandle handle = new AddressablesBatchHandle();
        AddDisguiseDependencies(handle, _dragonData);
        return handle;
    }

    /// <summary>
    /// Get the handle for all level dowloadables required for a dragon tier and a list of equipped powers.
    /// </summary>
    /// <returns>The handle for all downloadables required for the given setup.</returns>
    /// <param name="_tier">Tier.</param>
    /// <param name="_powers">Powers sku list.</param>
    private Downloadables.Handle GetLevelHandle(DragonTier _tier, List<string> _powers) {
		// Check equipped powers to detect those that might modify the target tier
		for(int i = 0; i < _powers.Count; ++i) {
			// Is it one of the tier-modifying powers?
			// [AOC] TODO!! Check whether having multiple powers of this type would accumulate max tier or not
			switch(_powers[i]) {
				case "dragonram": {
					// Increaase tier by 1, don't go out of bounds!
					_tier = _tier + 1;
					if(_tier >= DragonTier.COUNT) {
						_tier = DragonTier.COUNT - 1;
					}
				} break;
			}
		}

        // Now select target asset groups based on final tier
        string handleId = null;
        if (_tier >= DragonTier.TIER_3)
        {    // L and bigger
            handleId = DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2_3; // Village, Castle and Dark
        }        
        else if(_tier >= DragonTier.TIER_2) {    // M and bigger
            handleId = DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2; // Village and Castle
        }
        else if (_tier >= DragonTier.TIER_0)
        {  // XS and bigger
            handleId = DOWNLOADABLE_GROUP_LEVEL_AREA_1; // Village
        }

        // Finally get / create the handle for these assets groups
        if (DebugSettings.useDownloadablesMockHandlers) {
			// Debug!
			Downloadables.MockHandle handle = new Downloadables.MockHandle(
				0f,	// Bytes
				50f * 1024f * 1024f,	// Bytes
				2f * 60f, 	// Seconds
				false, false
			);

			return handle;
		} else {
            // The real deal
            return GetDownloadablesHandle(handleId);
        }
	}

    public Downloadables.Handle GetHandleForAllDownloadables()
    {
        Downloadables.Handle returnValue = null;
        if (m_downloadableHandles == null)
        {            
            AddressablesManager.LogError("You need to call HDADdressablesManager.Instance.Initialise() before calling this method");            
        }
        else
        {
            if (!m_downloadableHandles.TryGetValue(DOWNLOADABLE_GROUP_ALL, out returnValue))
            {
                returnValue = m_addressablesManager.CreateAllDownloadablesHandle();
                m_downloadableHandles.Add(DOWNLOADABLE_GROUP_ALL, returnValue);
            }            
        }

        // Are we debugging?
        if (DebugSettings.useDownloadablesMockHandlers)
        {

            // Use the singleton all downloadables mock handle
            if (! Downloadables.AllDownloadablesMockHandle.Instance.MockupInitialized)
            {
                // Initialize mock
                Downloadables.AllDownloadablesMockHandle.Instance.Initialize (
                0f, // Bytes
                50f * 1024f * 1024f,    // Bytes
                1f * 60f,   // Seconds
                false, false
                //,new List<Downloadables.AllDownloadablesMockHandle.Action> { new Downloadables.AllDownloadablesMockHandle.Action (10, Downloadables.Handle.EError.NO_CONNECTION)}
                );
            }

            return Downloadables.AllDownloadablesMockHandle.Instance;
        }
        else
        {
            // The real deal
            return returnValue;
        }
        
    }

    private bool HasPermissionRequestedForAnyDownloadableHandle()
    {
        if (m_downloadableHandles != null)
        {
            foreach (KeyValuePair<string, Downloadables.Handle> pair in m_downloadableHandles)
            {
                if (!pair.Value.NeedsToRequestPermission())
                {
                    return true;
                }
            }
        }

        return false;
    }

	/// <summary>
	/// Given a skin and a list of pets, create a list with all the powers derived from such equipment.
	/// </summary>
	/// <returns>List of power skus derived from given equipment. The first entry will correspond to the skin power.</returns>
	/// <param name="_skinSku">Skin sku.</param>
	/// <param name="_pets">Pets skus.</param>
	private List<string> GetPowersList(string _skinSku, List<string> _pets) {
		// Aux vars
		List<string> powers = new List<string>();
		DefinitionNode def = null;

		// Check skin power
		def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _skinSku);
		if(def != null) {
			powers.Add(def.GetAsString("powerup"));
		}

		// Check pets powers
		for(int i = 0; i < _pets.Count; i++) {
			def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _pets[i]);
			if(def != null) {
				powers.Add(def.Get("powerup"));
			}
		}

		// Done!
		return powers;
	}

    #endregion

    #region Asset Ids Getters
    /// <summary>
    /// Dictionary containing the list of dependencies per definition sku. They're stored to save performance when looking up if the assets handled by addressables are available, which happens a lot while the user is in menu
    /// </summary>
    private Dictionary<string, List<string>> m_dependecyIdsPerDefSku = new Dictionary<string, List<string>>();

    private void AddDependencyIdsPerDefSku(string _defSku, List<string> _dependencyIds) {        
        if (m_dependecyIdsPerDefSku.ContainsKey(_defSku)) {
            m_dependecyIdsPerDefSku[_defSku] = _dependencyIds;
        } else {
            m_dependecyIdsPerDefSku.Add(_defSku, _dependencyIds);
        }
    }

    private bool AreDependecyIdsOfDefSkuAvailable(string _defSku) {        
        if (m_dependecyIdsPerDefSku.ContainsKey(_defSku)) {
            return IsDependencyListAvailable(m_dependecyIdsPerDefSku[_defSku]);
        } else {
            return false;
        }        
    }

    public bool AreResourcesForDragonAvailable(string _dragonSku) {
        if (!m_dependecyIdsPerDefSku.ContainsKey(_dragonSku)) {
            List<string> _resourceIds = GetResourceIDsForDragon(_dragonSku);
            List<string> _dependencyIds = GetDependencyIdsList(_resourceIds);
            AddDependencyIdsPerDefSku(_dragonSku, _dependencyIds);            
        }

        return AreDependecyIdsOfDefSkuAvailable(_dragonSku);
    }

    public bool AreResourcesForPetAvailable(string _dragonSku) {
        if (!m_dependecyIdsPerDefSku.ContainsKey(_dragonSku)) {
            List<string> _resourceIds = GetResourceIDsForPet(_dragonSku);            
            List<string> _dependencyIds = GetDependencyIdsList(_resourceIds);
            AddDependencyIdsPerDefSku(_dragonSku, _dependencyIds);
        }

        return AreDependecyIdsOfDefSkuAvailable(_dragonSku);
    }

    public bool AreResourcesForSkinAvailable(string _skinSku) {
        if (!m_dependecyIdsPerDefSku.ContainsKey(_skinSku)) {
            List<string> _resourceIds = GetResourceIDsForDragonSkin(_skinSku, false);
            List<string> _dependencyIds = GetDependencyIdsList(_resourceIds);
            AddDependencyIdsPerDefSku(_skinSku, _dependencyIds);
        }

        return AreDependecyIdsOfDefSkuAvailable(_skinSku);
    }


    public bool AreResourcesForRewardAvailable( Metagame.Reward _reward )
    {
        bool ret = true;
        switch( _reward.type )
        {
            case Metagame.RewardPet.TYPE_CODE:
            {
                ret = AreResourcesForPetAvailable(_reward.sku);
            }break;
            case Metagame.RewardSkin.TYPE_CODE:
            {
                ret = AreResourcesForSkinAvailable( _reward.sku );
            }break;
            case Metagame.RewardDragon.TYPE_CODE:
            {
                ret = AreResourcesForDragonAvailable(_reward.sku);
            }break;
            case Metagame.RewardMulti.TYPE_CODE:
            {
                Metagame.RewardMulti multi = _reward as Metagame.RewardMulti;
                int l = multi.rewards.Count;
                for (int i = 0; i < l && ret; i++)
                {
                    ret = AreResourcesForRewardAvailable( multi.rewards[i] );
                }
            }break;
        }
        return ret;
    }

    /// <summary>
    /// Obtain a list of all the resources needed for a dragon.
    /// Doesn't include equipped pets.
    /// </summary>
    /// <returns>The list of all the resources needed for a dragon.</returns>
    /// <param name="_dragonSku">Dragon sku.</param>
    private List<string> GetResourceIDsForDragon(string _dragonSku) {
		// Aux vars
		HashSet<string> ids = new HashSet<string>();
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _dragonSku);

		// [AOC] In theory all assets go in the same bundle, but just in case add them all
		if(def != null) {
			// Dragon prefabs
			// A bit different for special dragons
			if(def.GetAsString("type") == DragonDataSpecial.TYPE_CODE) {
				// Get all special tier definitions linked to this dragon and add the resource ids for each of them
				List<DefinitionNode> specialTierDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SPECIAL_DRAGON_TIERS, "specialDragon", def.sku);
				for(int i = 0; i < specialTierDefs.Count; ++i) {
					// It is a hash set, so we're ensuring no duplicates
					ids.Add(specialTierDefs[i].GetAsString("gamePrefab"));
					ids.Add(specialTierDefs[i].GetAsString("menuPrefab"));
					ids.Add(specialTierDefs[i].GetAsString("resultsPrefab"));
				}
			} else {
				ids.Add(def.GetAsString("gamePrefab"));
				ids.Add(def.GetAsString("menuPrefab"));
				ids.Add(def.GetAsString("resultsPrefab"));
			}

			// Skin assets
			// [AOC] Don't need to add them as they go with the same bundle as the dragon prefabs

			// Icons
			// [AOC] Don't need to add them as they go with the same bundle as the dragon prefabs
		}

		return ids.ToList();
	}

	/// <summary>
	/// Obtain a list of all the resources needed for a pet.
	/// </summary>
	/// <returns>The list of all the resources needed for a pet.</returns>
	/// <param name="_petSku">Pet sku.</param>
	public List<string> GetResourceIDsForPet(string _petSku) {
		// Aux vars
		List<string> ids = new List<string>();
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _petSku);

		// [AOC] In theory all assets go in the same bundle, but just in case add them all
		if (def != null) {
			// Pet prefabs
			ids.Add(def.GetAsString("gamePrefab"));
			ids.Add(def.GetAsString("menuPrefab"));

			// Icon
			ids.Add(def.GetAsString("icon"));
		}

		return ids;
	}

    /// <summary>
    /// Returns the list of adderssable ids for a dragon skin
    /// </summary>
    /// <param name="skinSku">Sku of the dragon skin which addressable ids need to be retrieved</param>
    /// <param name="ingame">When <c>true</c> the dragon skin used ingame is considered, otherwise the dragon skin for menu is considered instead</param>
    /// <returns></returns>
    public List<string> GetResourceIDsForDragonSkin(string skinSku, bool ingame) 
    {
        List<string> returnValue = new List<string>();

        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, skinSku);
        if (def != null)
        {
            // Materials
            string skin = def.Get("skin");
            if (ingame)
            {
                returnValue.Add(skin + "_ingame_body");
                returnValue.Add(skin + "_ingame_wings");
            }
            else
            {
                returnValue.Add(skin + "_body");
                returnValue.Add(skin + "_wings");                
            }

            // Body Parts
            List<string> bodyParts = def.GetAsList<string>("body_parts");
            for (int j = 0; j < bodyParts.Count; j++)
            {
                returnValue.Add(bodyParts[j]);
            }
        }

        return returnValue;
    }
	#endregion

	#region ADDRESSABLES_BATCH_HANDLERS            
	private void AddMenuDependenciesToAddressablesBatchHandle(AddressablesBatchHandle handle)
    {
        if (handle != null)
        {
            // Menu group
            handle.AddGroup(GROUP_MENU);

            // All menu prefabs dependencies
            Dictionary<string, DefinitionNode> dragons = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DRAGONS);

            List<string> mandatoryBundles = new List<string>();
            mandatoryBundles.Add("shared"); // We need animations
            mandatoryBundles.Add("particles_shared");   // for the particles
			handle.AddDependencyIds( mandatoryBundles ); 

			// menu dragons
            foreach (KeyValuePair<string, DefinitionNode> pair in dragons)
            {
                Handle_AddAddressable(handle, pair.Value.Get("menuPrefab"));            
            }                      

            Dictionary<string, IDragonData> dragonDatas = DragonManager.dragonsBySku;
            foreach (KeyValuePair<string, IDragonData> pair in dragonDatas) {
                if (pair.Value.isOwned) {
                    AddPetDependencies(handle, pair.Value);
                }

				// Disguises
				AddDisguiseDependencies (handle, pair.Value, false);
            }
        }
    }

    private void Handle_AddAddressable(AddressablesBatchHandle handle, string addressableId)
    {
        handle.AddAddressable(addressableId, Flavour_GetVariant(addressableId, null));
    }

    private void AddGameDependenciesToAddressablesBatchHandle(AddressablesBatchHandle handle)
    { 
        if (handle != null)
        {
            // Area 1 level dependencies
            handle.AddGroup(GROUP_LEVEL_AREA_1);

            // Current dragon ingame dependencies
            IDragonData data = GetCurrentDragonData();
            Handle_AddAddressable(handle, data.gamePrefab);

            AddDisguiseDependencies( handle, data, true );

            AddPetDependencies(handle, data);
        }        
    }

    private void AddResultsDependenciesToAddressablesBatchHandle(AddressablesBatchHandle handle)
    {
        // Current dragon results dependencies 
        IDragonData dragon = GetCurrentDragonData(); 
        string resultPrefab = "";
        if ( dragon.type == IDragonData.Type.SPECIAL )
        {
            DefinitionNode specialTierDef = DragonDataSpecial.GetDragonTierDef(dragon.sku, dragon.tier);
            resultPrefab = specialTierDef.GetAsString("resultsPrefab");
        }
        else
        {
            resultPrefab = dragon.def.GetAsString("resultsPrefab");
            // Take next skin into account just in case we unlock it this run
            int initialLevel = RewardManager.dragonInitialLevel;
            DragonProgression progression = (dragon as DragonDataClassic).progression;
            int finalLevel = progression.level;
            if ( initialLevel != finalLevel )
            {
                List<DefinitionNode> newSkins = UsersManager.currentUser.wardrobe.GetUnlockedSkins( DragonManager.CurrentDragon.def.sku, initialLevel, finalLevel );
                for (int i = 0; i < newSkins.Count; i++)
                {
                    AddDisguiseDependencies( handle, newSkins[i], false );
                }
            }
        }
        Handle_AddAddressable(handle, resultPrefab);
        string animController = resultPrefab.Replace("PF_", "AC_");
        Handle_AddAddressable(handle, animController);
        AddDisguiseDependencies( handle, dragon, false );

        // Add next dragon icon
        IDragonData nextDragonData = DragonManager.GetNextDragonData(dragon.sku);
		if(nextDragonData != null) {
            string defaultIcon = IDragonData.GetDefaultDisguise(nextDragonData.def.sku).Get("icon");
            Handle_AddAddressable(handle, defaultIcon);
        }
    }    

    private IDragonData GetCurrentDragonData()
    {
        IDragonData dragon = null;
        if ( HDLiveDataManager.tournament.isActive)
        {
            dragon = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData;
        }
        else
        {
            dragon = DragonManager.CurrentDragon;
        }
        return dragon;
    }

    private void AddDisguiseDependencies( AddressablesBatchHandle handle, IDragonData data, bool ingame = true )
    {
        AddDisguiseDependencies( handle, data.disguise, ingame );
    }

    private void AddDisguiseDependencies( AddressablesBatchHandle handle, string disguiseSku, bool ingame = true  )
    {
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, disguiseSku);
        AddDisguiseDependencies( handle, def, ingame );
    }

    private void AddDisguiseDependencies( AddressablesBatchHandle handle, DefinitionNode def, bool ingame = true  )
    {
        // Materials
        List<string> resourceIDs = GetResourceIDsForDragonSkin(def.sku, ingame);        
        int count = resourceIDs.Count;
        for (int i = 0; i < count; i++)
        {
            Handle_AddAddressable(handle, resourceIDs[i]);
        }

        // Icon
        Handle_AddAddressable(handle, def.Get("icon"));
    }

    private void AddPetDependencies( AddressablesBatchHandle handle, IDragonData data )
    {
        foreach (string pet in data.pets)
        {
            DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, pet);
            if (petDef != null) {
                string id = petDef.Get("gamePrefab");
                Handle_AddAddressable(handle, id);
            }
        }
    }
#endregion
}
