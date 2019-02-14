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

        string catalogAsText = null;
        if (BetterStreamingAssets.FileExists(addressablesCatalogPath))
        {
            catalogAsText = BetterStreamingAssets.ReadAllText(addressablesCatalogPath);            
        }

        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);        
        m_addressablesManager.Initialize(catalogASJSON, assetBundlesPath, logger);        
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

        public Ingame_SwitchAreaHandle(List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
        {
            PrevAreaRealSceneNames = prevAreaRealSceneNames;
            NextAreaRealSceneNames = nextAreaRealSceneNames;

            List<string> prevAreaSceneDependencyIds = Instance.GetSceneDependencyIdsList(prevAreaRealSceneNames);
            List<string> nextAreaSceneDependencyIds = Instance.GetSceneDependencyIdsList(nextAreaRealSceneNames);

            List<string> assetBundleIdsToStay;
            List<string> assetBundleIdsToUnload;
            UbiListUtils.SplitIntersectionAndDisjoint(prevAreaSceneDependencyIds, nextAreaSceneDependencyIds, out assetBundleIdsToStay, out assetBundleIdsToUnload);

            List<string> assetBundleIdsToLoad;            
            UbiListUtils.SplitIntersectionAndDisjoint(nextAreaSceneDependencyIds, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

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

    public Ingame_SwitchAreaHandle Ingame_SwitchArea(List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
    {
            return new Ingame_SwitchAreaHandle(prevAreaRealSceneNames, nextAreaRealSceneNames);
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
