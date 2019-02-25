using System.IO;
using SimpleJSON;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for tayloring Addressables UbiPackage to meet Hungry Dragon needs
/// </summary>
public class HDAddressablesManager : AddressablesManager
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

    private float m_pollAutomaticDownloaderAt;

    /// <summary>
    /// Make sure this method is called after ContentDeltaManager.OnContentDelta() was called since this method uses data from assetsLUT to create downloadables catalog.
    /// </summary>
    public void Initialise()
    {
        Logger logger = (FeatureSettingsManager.IsDebugEnabled) ? new CPLogger(ControlPanel.ELogChannel.Addressables) : null;
        
        string addressablesPath = "Addressables";
        string assetBundlesPath = addressablesPath;
        string addressablesCatalogPath = Path.Combine(addressablesPath, "addressablesCatalog.json");

        BetterStreamingAssets.Initialize();

        // Retrieves addressables catalog
        string catalogAsText = null;
        if (BetterStreamingAssets.FileExists(addressablesCatalogPath))
        {
            catalogAsText = BetterStreamingAssets.ReadAllText(addressablesCatalogPath);            
        }

        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Retrieves downloadables catalog
        // TODO: Retrieve this information from the assetsLUT cached
        /*
        string downloadablesPath = addressablesPath;
        string downloadablesCatalogPath = Path.Combine(downloadablesPath, "downloadablesCatalog.json");
        if (BetterStreamingAssets.FileExists(downloadablesCatalogPath))
        {
            catalogAsText = BetterStreamingAssets.ReadAllText(downloadablesCatalogPath);
        }
        JSONNode downloadablesCatalogAsJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        */

        // downloadablesCatalog is created out of latest assetsLUT
        ContentDeltaManager.ContentDeltaData assetsLUT = ContentDeltaManager.SharedInstance.m_kServerDeltaData;
        if (assetsLUT == null)
        {
            assetsLUT = ContentDeltaManager.SharedInstance.m_kLocalDeltaData;
        }

        Downloadables.Tracker tracker = new HDDownloadablesTracker(5, null, logger);
        JSONNode downloadablesCatalogAsJSON = AssetsLUTToDownloadablesCatalog(assetsLUT);               
        Initialize(catalogASJSON, assetBundlesPath, downloadablesCatalogAsJSON, false, tracker, logger);

        m_pollAutomaticDownloaderAt = 0f;
    }

    private JSONNode AssetsLUTToDownloadablesCatalog(ContentDeltaManager.ContentDeltaData assetsLUT)
    {
        string urlBase = null;

        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
        if (kServerConfig != null)
        {
            switch (kServerConfig.m_eBuildEnvironment)
            {
                case CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION:
                    urlBase = "http://hdragon-assets.s3.amazonaws.com/prod/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_STAGE_QC:
                    urlBase = "http://hdragon-assets.s3.amazonaws.com/qc/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_STAGE:
                    urlBase = "http://hdragon-assets.s3.amazonaws.com/stage/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_DEV:
                    urlBase = "http://hdragon-assets.s3.amazonaws.com/dev/";
                    break;
            }

            //http://10.44.4.69:7888/            
        }

        if (string.IsNullOrEmpty(urlBase))
        {
            urlBase = assetsLUT.m_strURLBase;
        }

        Downloadables.Catalog catalog = new Downloadables.Catalog();
        catalog.UrlBase = urlBase + assetsLUT.m_iReleaseVersion + "/";

        Downloadables.CatalogEntry entry;        
        foreach (KeyValuePair<string, long> pair in assetsLUT.m_kAssetCRCs)
        {
            entry = new Downloadables.CatalogEntry();
            entry.CRC = pair.Value;
            entry.Size = assetsLUT.m_kAssetSizes[pair.Key];

            catalog.AddEntry(pair.Key, entry);
        }

        return Downloadables.Manager.GetCatalogFromAssetsLUT(catalog.ToJSON());        
    }

    // This method has been overridden in order to let the game load a scene before AddressablesManager has been initialized, typically the first loading scene, 
    // which may be called when rebooting the game
    public override bool LoadScene(string id, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsInitialized())
        {
            return base.LoadScene(id, mode);
        }
        else
        {
            SceneManager.LoadScene(id, mode);
            return true;
        }
    }

    // This method has been overridden in order to let the game load a scene before AddressablesManager has been initialized, typically the first loading scene,
    // which may be called when rebooting the game
    public override AddressablesOp LoadSceneAsync(string id, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsInitialized())
        {
            return base.LoadSceneAsync(id, mode);
        }
        else
        {
            AddressablesAsyncOp op = new AddressablesAsyncOp();            
            op.Setup(new UbiUnityAsyncOperation(SceneManager.LoadSceneAsync(id, mode)));
            return op;
        }        
    }

    public override AddressablesOp UnloadSceneAsync(string id)
    {
        if (IsInitialized())
        {
            return base.UnloadSceneAsync(id);
        }
        else
        {
            AddressablesAsyncOp op = new AddressablesAsyncOp();
            op.Setup(new UbiUnityAsyncOperation(SceneManager.UnloadSceneAsync(id)));
            return op;
        }
    }

    protected override void ExtendedUpdate()
    {
        if (Time.realtimeSinceStartup >= m_pollAutomaticDownloaderAt)
        {
            // We don't want the automatic downloader to interfere with the ingame experience
            // We don't want downloadables to interfere with the first user experience, so the user must have played at least two runs for the automatic downloading to be enabled        
            bool value = !FlowManager.IsInGameScene() && UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN);
            if (value != IsAutomaticDownloaderEnabled)
                IsAutomaticDownloaderEnabled = value;

            m_pollAutomaticDownloaderAt = Time.realtimeSinceStartup + 3f;
        }
    }

    #region ingame
    public class Ingame_SwitchAreaHandle
    {        
        private List<string> PrevAreaRealSceneNames { get; set; }
        private List<string> NextAreaRealSceneNames { get; set; }

        private List<string> DependencyIdsToUnload { get; set; }
        private List<string> DependencyIdsToLoad { get; set; }

        private List<AddressablesOp> m_prevAreaScenesToUnload;
        private List<AddressablesOp> m_nextAreaScenesToLoad;        

        public Ingame_SwitchAreaHandle(string prevArea, string nextArea, List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
        {
            PrevAreaRealSceneNames = prevAreaRealSceneNames;
            NextAreaRealSceneNames = nextAreaRealSceneNames;

            // Retrieves the dependencies of the previous area scenes
            List<string> prevAreaSceneDependencyIds = Instance.GetDependencyIdsList(prevAreaRealSceneNames);

            // Retrieves the dependencies of the next area scenes
            List<string> nextAreaSceneDependencyIds = Instance.GetDependencyIdsList(nextAreaRealSceneNames);

            // Retrieves the dependencies of the previous area defined in addressablesCatalog.json
            List<string> prevAreaDependencyIds = Instance.Areas_GetDependencyIds(prevArea);

            // Adds the dependencies of the scenes and the dependencies of the addressables areas
            prevAreaDependencyIds = UbiListUtils.AddRange(prevAreaDependencyIds, prevAreaSceneDependencyIds, true, true);

            // Retrieves the dependencies of the next area defined in addressablesCatalog.json
            List<string> nextAreaDependencyIds = Instance.Areas_GetDependencyIds(nextArea);

            // Adds the dependencies of the scenes and the dependencies of the addressables areas
            nextAreaDependencyIds = UbiListUtils.AddRange(nextAreaDependencyIds, nextAreaSceneDependencyIds, true, true);

            List<string> assetBundleIdsToStay;
            List<string> assetBundleIdsToUnload;
            UbiListUtils.SplitIntersectionAndDisjoint(prevAreaDependencyIds, nextAreaDependencyIds, out assetBundleIdsToStay, out assetBundleIdsToUnload);

            List<string> assetBundleIdsToLoad;            
            UbiListUtils.SplitIntersectionAndDisjoint(nextAreaDependencyIds, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

            DependencyIdsToLoad = assetBundleIdsToLoad;
            DependencyIdsToUnload = assetBundleIdsToUnload;            
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
                        m_nextAreaScenesToLoad.Add(Instance.LoadSceneAsync(NextAreaRealSceneNames[i], LoadSceneMode.Additive));
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
                    Instance.LoadScene(NextAreaRealSceneNames[i], LoadSceneMode.Additive);
                }
            }
        }
    }

    public Ingame_SwitchAreaHandle Ingame_SwitchArea(string prevArea, string newArea, List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
    {
        // Makes sure that all scenes are available. For now a scene is not scheduled to be loaded if it's not available because there's no support
        // for downloading stuff during the loading screen. 
        // TODO: To delete this stuff when downloading unavailable stuff flow is implemented
        if (nextAreaRealSceneNames != null)
        {            
            for (int i = 0; i < nextAreaRealSceneNames.Count;)
            {
                if (!IsResourceAvailable(nextAreaRealSceneNames[i]))
                {
                    nextAreaRealSceneNames.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        return new Ingame_SwitchAreaHandle(prevArea, newArea, prevAreaRealSceneNames, nextAreaRealSceneNames);
    }
    #endregion                       
}
