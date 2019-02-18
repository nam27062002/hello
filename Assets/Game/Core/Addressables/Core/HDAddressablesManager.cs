using System.IO;
using SimpleJSON;
using System.Collections.Generic;
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
                sm_instance.Initialize();
            }

            return sm_instance;
        }
    }

    private AddressablesManager m_addressablesManager = new AddressablesManager();

    private void Initialize()
    {
        Logger logger = new ConsoleLogger("Addressables");
        string addressablesPath = "Addressables";
        string assetBundlesPath = Path.Combine(addressablesPath, "AssetBundles");
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
        string downloadablesPath = addressablesPath;
        string downloadablesCatalogPath = Path.Combine(downloadablesPath, "downloadablesCatalog.json");
        if (BetterStreamingAssets.FileExists(downloadablesCatalogPath))
        {
            catalogAsText = BetterStreamingAssets.ReadAllText(downloadablesCatalogPath);
        }

        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        m_addressablesManager.Initialize(catalogASJSON, assetBundlesPath, downloadablesCatalogASJSON, logger);        
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
            List<string> prevAreaSceneDependencyIds = Instance.GetSceneDependencyIdsList(prevAreaRealSceneNames);

            // Retrieves the dependencies of the next area scenes
            List<string> nextAreaSceneDependencyIds = Instance.GetSceneDependencyIdsList(nextAreaRealSceneNames);

            // Retrieves the dependencies of the previous area defined in addressablesCatalog.json
            List<string> prevAreaDependencyIds = Instance.m_addressablesManager.Areas_GetDependencyIds(prevArea);

            // Adds the dependencies of the scenes and the dependencies of the addressables areas
            prevAreaDependencyIds = UbiListUtils.AddRange(prevAreaDependencyIds, prevAreaSceneDependencyIds, true, true);

            // Retrieves the dependencies of the next area defined in addressablesCatalog.json
            List<string> nextAreaDependencyIds = Instance.m_addressablesManager.Areas_GetDependencyIds(nextArea);

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
            return Instance.LoadDependencyIdsList(DependencyIdsToLoad);
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
                if (!m_addressablesManager.IsResourceAvailable(nextAreaRealSceneNames[i]))
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
   
    public void LoadScene(string sceneName, LoadSceneMode mode)
    {
        m_addressablesManager.LoadScene(sceneName, mode);
    }    

    public AddressablesOp LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        return m_addressablesManager.LoadSceneAsync(sceneName, mode);
    }

    public AddressablesOp UnloadSceneAsync(string sceneName)
    {        
        return m_addressablesManager.UnloadSceneAsync(sceneName);
    }
    
    private AddressablesOp LoadDependencyIdsList(List<string> dependencyIds)
    {
        return m_addressablesManager.LoadDependencyIdsListAsync(dependencyIds);
    }

    private void UnloadDependencyIdsList(List<string> dependencyIds)
    {
        m_addressablesManager.UnloadDependencyIdsList(dependencyIds);
    }

    private List<string> GetSceneDependenciyIds(string sceneName)
    {
        return m_addressablesManager.GetDependencyIds(sceneName);
    }

    private List<string> GetSceneDependencyIdsList(List<string> sceneNames)
    {
        return m_addressablesManager.GetDependencyIdsList(sceneNames);        
    }    
    
    public void Update()
    {
        if (m_addressablesManager != null)
        {
            m_addressablesManager.Update();
        }        
    }
}
