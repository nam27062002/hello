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

    private HDDownloadablesTracker m_tracker;

    /// <summary>
    /// Make sure this method is called after ContentDeltaManager.OnContentDelta() was called since this method uses data from assetsLUT to create downloadables catalog.
    /// </summary>
    public void Initialise()
    {
        Logger logger = (FeatureSettingsManager.IsDebugEnabled) ? new CPLogger(ControlPanel.ELogChannel.Addressables) : null;
        
        string addressablesPath = "Addressables";
        string assetBundlesPath = addressablesPath;
        string addressablesCatalogPath = Path.Combine(addressablesPath, "addressablesCatalog");        

        // Addressables catalog 
        TextAsset targetFile = Resources.Load<TextAsset>(addressablesCatalogPath);
        string catalogAsText = (targetFile == null) ? null : targetFile.text;        
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

        // Downloadables config
        string downloadablesConfigPath = Path.Combine(addressablesPath, "downloadablesConfig");
        targetFile = Resources.Load<TextAsset>(downloadablesConfigPath);
        catalogAsText = (targetFile == null) ? null : targetFile.text;
        JSONNode downloadablesConfigJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        Downloadables.Config downloadablesConfig = new Downloadables.Config();
        downloadablesConfig.Load(downloadablesConfigJSON, logger);

        // downloadablesCatalog is created out of latest assetsLUT
        ContentDeltaManager.ContentDeltaData assetsLUT = ContentDeltaManager.SharedInstance.m_kServerDeltaData;
        if (assetsLUT == null)
        {
            assetsLUT = ContentDeltaManager.SharedInstance.m_kLocalDeltaData;
        }

        m_tracker = new HDDownloadablesTracker(downloadablesConfig, logger);
        JSONNode downloadablesCatalogAsJSON = AssetsLUTToDownloadablesCatalog(assetsLUT);

#if UNITY_EDITOR && false
        bool useMockDrivers = true;
#else
        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
	    bool useMockDrivers = (kServerConfig != null && kServerConfig.m_eBuildEnvironment != CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION);                       
#endif
        Initialize(catalogASJSON, assetBundlesPath, downloadablesConfig, downloadablesCatalogAsJSON, useMockDrivers, m_tracker, logger);

        InitDownloadableHandles();        
        InitAddressablesAreas();
    }

    private string GetEnvironmentUrlBase()
    {
        string urlBase = "";

        Calety.Server.ServerConfig kServerConfig = ServerManager.SharedInstance.GetServerConfig();
        if (kServerConfig != null)
        {
            switch (kServerConfig.m_eBuildEnvironment)
            {
                case CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION:
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";
                    urlBase = "http://hdragon-assets-s3.akamaized.net/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_STAGE_QC:
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";

                    // Link to CDN (it uses cache)
                    //urlBase = "http://hdragon-assets.s3.amazonaws.com/";

                    // Direct link to the bucket (no cache involved, which might make downloads more expensive)
                    urlBase = "https://s3.us-east-2.amazonaws.com/hdragon-assets/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_STAGE:
                    urlBase = "http://hdragon-assets.s3.amazonaws.com/";
                    break;

                case CaletyConstants.eBuildEnvironments.BUILD_DEV:
                    urlBase = "http://hdragon-assets.s3.amazonaws.com/";
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

    public UbiAsyncOperation LoadDependenciesAndSceneAsync(string id, string variant = null)
    {
        if (IsInitialized())
        {
            return LoadAddressablesBatchAsynch(id);            
        }
        else
        {
            return LoadSceneAsync(id, variant, LoadSceneMode.Single);
        }
    }

    // This method has been overridden in order to let the game load a scene before AddressablesManager has been initialized, typically the first loading scene, 
    // which may be called when rebooting the game
    public override bool LoadScene(string id, string variant = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsInitialized())
        {            
            return base.LoadScene(id, variant, mode);           
        }
        else
        {
            SceneManager.LoadScene(id, mode);
            return true;
        }
    }

    // This method has been overridden in order to let the game load a scene before AddressablesManager has been initialized, typically the first loading scene,
    // which may be called when rebooting the game
    public override AddressablesOp LoadSceneAsync(string id, string variant = null, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (IsInitialized())
        {            
            return base.LoadSceneAsync(id, variant, mode);                        
        }
        else
        {
            AddressablesAsyncOp op = new AddressablesAsyncOp();            
            op.Setup(new UbiUnityAsyncOperation(SceneManager.LoadSceneAsync(id, mode)));
            return op;
        }        
    }

    public override AddressablesOp UnloadSceneAsync(string id, string variant = null)
    {
        if (IsInitialized())
        {
            return base.UnloadSceneAsync(id, variant);
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
        // We don't want the downloader to interfere with the ingame experience
        IsDownloaderEnabled = !FlowManager.IsInGameScene();

		// Downloader is disabled while the app is loading in order to let it load faster and smoother
		IsAutomaticDownloaderEnabled = !GameSceneManager.isLoading && IsAutomaticDownloaderAllowed() && DebugSettings.isAutomaticDownloaderEnabled;

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

        m_addressablesAreaLoader.Setup(handle, sceneId);

        return m_addressablesAreaLoader;
    }
    
    private AddressablesBatchHandle GetAddressablesAreaBatchHandle(string sceneId)
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

        public Ingame_SwitchAreaHandle(string prevArea, string nextArea, List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
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

    public Ingame_SwitchAreaHandle Ingame_SwitchArea(string prevArea, string newArea, List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
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

        return new Ingame_SwitchAreaHandle(prevArea, newArea, prevAreaRealSceneNames, nextAreaRealSceneNames);
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
        Downloadables.Handle handle = CreateDownloadablesHandle(DOWNLOADABLE_GROUP_LEVEL_AREA_1);
        m_downloadableHandles.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_1, handle);

        // Areas 1 and 2
        HashSet<string> groupIds = new HashSet<string>();
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_1);
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_2);
        handle = CreateDownloadablesHandle(groupIds);
        m_downloadableHandles.Add(DOWNLOADABLE_GROUP_LEVEL_AREAS_1_2, handle);

        // Areas 1,2 and 3
        groupIds = new HashSet<string>();
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_1);
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_2);
        groupIds.Add(DOWNLOADABLE_GROUP_LEVEL_AREA_3);
        handle = CreateDownloadablesHandle(groupIds);
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
        
		// Get target dragon info
		IDragonData dragonData = DragonManager.GetDragonData(_dragonSku);

		// Figure out group dependencies for this dragon
		// 1. Level area dependencies: will depend basically on the dragon's tier
		//    Some powers allow dragons to go to areas above their tier, check for those cases as well
		List<string> equippedPowers = GetPowersList(dragonData.persistentDisguise, dragonData.pets);

		// No more dependencies to be checked: return handler!
		return GetHandleForDragonTier(dragonData.tier, equippedPowers);
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
		// Get target dragon info
		IDragonData dragonData = _tournament.tournamentData.tournamentDef.dragonData;

		// Figure out group dependencies for this dragon
		// 1. Level area dependencies: will depend basically on the dragon's tier
		//    Some powers allow dragons to go to areas above their tier, check for those cases as well
		List<string> equippedPowers = GetPowersList(dragonData.disguise, dragonData.pets);

		// No more dependencies to be checked: return handler!
		return GetHandleForDragonTier(dragonData.tier, equippedPowers);
	}

	/// <summary>
	/// Get the handle for all dowloadables required for a dragon tier and a list of equipped powers.
	/// </summary>
	/// <returns>The handle for all downloadables required for the given setup.</returns>
	/// <param name="_tier">Tier.</param>
	/// <param name="_powers">Powers sku list.</param>
	private Downloadables.Handle GetHandleForDragonTier(DragonTier _tier, List<string> _powers) {
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
            if (FeatureSettingsManager.IsDebugEnabled)
            {
                LogError("You need to call HDADdressablesManager.Instance.Initialise() before calling this method");
            }
        }
        else
        {
            if (!m_downloadableHandles.TryGetValue(DOWNLOADABLE_GROUP_ALL, out returnValue))
            {
                returnValue = CreateAllDownloadablesHandle();
                m_downloadableHandles.Add(DOWNLOADABLE_GROUP_ALL, returnValue);
            }            
        }

        return returnValue;
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
            List<string> optionalBundles = new List<string>();
            foreach (KeyValuePair<string, DefinitionNode> pair in dragons)
            {
                mandatoryBundles.Add( pair.Key + "_local" );
                optionalBundles.Add( pair.Key ); // for disguises
            }
            handle.AddDependencyIds( mandatoryBundles );
            handle.AddDependencyIds( optionalBundles );
            
        }
    }

    private void AddGameDependenciesToAddressablesBatchHandle(AddressablesBatchHandle handle)
    { 
        if (handle != null)
        {
            // Area 1 level dependencies
            handle.AddGroup(GROUP_LEVEL_AREA_1);

            // Current dragon ingame dependencies
            IDragonData data = GetCurrentDragonData();
            handle.AddAddressable(data.gamePrefab);

            AddDisguiseDependencies( handle, data, true );
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
        }
        handle.AddAddressable( resultPrefab );
        AddDisguiseDependencies( handle, dragon, false );
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
            dragon = DragonManager.currentDragon;
        }
        return dragon;
    }

    private void AddDisguiseDependencies( AddressablesBatchHandle handle, IDragonData data, bool ingame = true )
    {
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, data.disguise);
        // Materials
        string skin = def.Get("skin");
        if ( ingame )
        {
            handle.AddAddressable( skin + "_ingame_body" );
            handle.AddAddressable( skin + "_ingame_wings" );
        }
        else
        {
            handle.AddAddressable( skin + "_body" );
            handle.AddAddressable( skin + "_wings" );
        }

        // Body Parts
        List<string> bodyParts = def.GetAsList<string>("body_parts");
        for (int j = 0; j < bodyParts.Count; j++)
        {
            handle.AddAddressable(bodyParts[j]);
        }

        // Icon
        handle.AddAddressable( def.Get("icon") );


    }
#endregion
}
