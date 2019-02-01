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

    private void Initialize()
    {
        string localAssetBundlesPath = Application.streamingAssetsPath;
        List<string> localAssetBundleIds = new List<string> { "so_medieval_castle" };
        AssetBundlesManager.Instance.Initialize(localAssetBundleIds, localAssetBundlesPath, null);        
    }

    #region ingame
    public class Ingame_SwitchAreaHandle
    {        
        private List<string> PrevAreaRealSceneNames { get; set; }
        private List<string> NextAreaRealSceneNames { get; set; }

        private List<string> AssetBundlesToUnload { get; set; }
        private List<string> AssetBundlesToLoad { get; set; }

        private List<UbiAsyncOperation> m_prevAreaScenesToUnload;
        private List<UbiAsyncOperation> m_nextAreaScenesToLoad;        

        public Ingame_SwitchAreaHandle(List<string> prevAreaRealSceneNames, List<string> nextAreaRealSceneNames)
        {
            PrevAreaRealSceneNames = prevAreaRealSceneNames;
            NextAreaRealSceneNames = nextAreaRealSceneNames;

            List<string> prevAreaSceneAssetBundleIds = Instance.GetSceneAssetBundleIdList(prevAreaRealSceneNames);
            List<string> nextAreaSceneAssetBundleIds = Instance.GetSceneAssetBundleIdList(nextAreaRealSceneNames);

            List<string> assetBundleIdsToStay;
            List<string> assetBundleIdsToUnload;
            UbiListUtils.SplitIntersectionAndDisjoint(prevAreaSceneAssetBundleIds, nextAreaSceneAssetBundleIds, out assetBundleIdsToStay, out assetBundleIdsToUnload);

            List<string> assetBundleIdsToLoad;            
            UbiListUtils.SplitIntersectionAndDisjoint(nextAreaSceneAssetBundleIds, assetBundleIdsToStay, out assetBundleIdsToStay, out assetBundleIdsToLoad);

            AssetBundlesToLoad = assetBundleIdsToLoad;
            AssetBundlesToUnload = assetBundleIdsToUnload;            
        }       

        public bool NeedsToUnloadPrevAreaDependencies()
        {
            return AssetBundlesToUnload != null && AssetBundlesToUnload.Count > 0;
        }

        public void UnloadPrevAreaDependencies()
        {
            AssetBundlesManager.Instance.UnloadAssetBundleList(AssetBundlesToUnload, null);
        }

        public bool NeedsToUnloadPrevAreaScenes()
        {
            return PrevAreaRealSceneNames != null && PrevAreaRealSceneNames.Count > 0;
        }

        public List<UbiAsyncOperation> UnloadPrevAreaScenes()
        {            
            if (m_prevAreaScenesToUnload == null)
            {
                m_prevAreaScenesToUnload = new List<UbiAsyncOperation>();
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
            return AssetBundlesToLoad != null && AssetBundlesToLoad.Count > 0;
        }

        public UbiAsyncOperation LoadNextAreaDependencies()
        {
            return AssetBundlesManager.Instance.LoadAssetBundleList(AssetBundlesToLoad, null, true);
        }

        public bool NeedsToLoadNextAreaScenes()
        {
            return NextAreaRealSceneNames != null && NextAreaRealSceneNames.Count > 0;
        }

        public List<UbiAsyncOperation> LoadNextAreaScenesAsync()
        {            
            if (m_nextAreaScenesToLoad == null)
            {
                m_nextAreaScenesToLoad = new List<UbiAsyncOperation>();
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

    private string GetAssetBundleIdFromScenName(string sceneName)
    {
        return sceneName.ToLower();
    }

    public void LoadScene(string sceneName, LoadSceneMode mode)
    {
        if (IsSceneInAssetBundles(sceneName))
        {
            LoadSceneFromAssetBundles(GetAssetBundleIdFromScenName(sceneName), sceneName, mode);
        }
        else
        {
            LoadSceneFromBuild(sceneName, LoadSceneMode.Additive);
        }
    }    

    public UbiAsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
    {
        UbiAsyncOperation returnValue;
        if (IsSceneInAssetBundles(sceneName))
        {
            returnValue = LoadSceneFromAssetBundlesAsync(GetAssetBundleIdFromScenName(sceneName), sceneName, mode);
        }
        else
        {
            returnValue = LoadSceneFromBuildAsync(sceneName, LoadSceneMode.Additive);
        }

        return returnValue;
    }

    public UbiAsyncOperation UnloadSceneAsync(string sceneName)
    {
        UbiAsyncOperation returnValue;
        if (IsSceneInAssetBundles(sceneName))
        {
            returnValue = UnloadSceneFromAssetBundlesAsync(GetAssetBundleIdFromScenName(sceneName), sceneName);
        }
        else
        {
            returnValue = UnloadSceneFromBuildAsync(sceneName);
        }

        return returnValue;        
    }

    private bool IsSceneInAssetBundles(string sceneName)
    {
        return sceneName == "SO_Medieval_Castle";
    }

    /// <summary>
    /// Returns the id of the asset bundle where this scene is stored.
    /// </summary>
    /// <param name="sceneName">Name of a scene</param>
    /// <returns>id of the asset bundle where this scene is stored.</returns>
    private string GetSceneAssetBundleId(string sceneName)
    {
        return (IsSceneInAssetBundles(sceneName)) ? sceneName.ToLower() : null;
    }

    private List<string> GetSceneAssetBundleIdList(List<string> sceneNames)
    {
        List<string> returnValue = null;
        if (sceneNames != null)
        {
            returnValue = new List<string>();

            int count = sceneNames.Count;
            for (int i = 0; i < count; i++)
            {
                UbiListUtils.AddRange(returnValue, GetSceneDependenciesIds(sceneNames[i]), false, true);
            }
        }

        return returnValue;
    }

    private List<string> GetSceneDependenciesIds(string sceneName)
    {
        string sceneAssetBundleId = GetSceneAssetBundleId(sceneName);
        return AssetBundlesManager.Instance.GetDependenciesIncludingSelf(sceneAssetBundleId);
    }

    private void LoadSceneFromBuild(string sceneName, LoadSceneMode mode)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    private void LoadSceneFromAssetBundles(string assetBundleId, string sceneName, LoadSceneMode mode)
    {
        AssetBundlesManager.Instance.LoadScene(assetBundleId, sceneName, mode);
    }

    private UbiAsyncOperation LoadSceneFromAssetBundlesAsync(string assetBundleId, string sceneName, LoadSceneMode mode)
    {
        return AssetBundlesManager.Instance.LoadSceneAsync(assetBundleId, sceneName, mode, OnLoadSceneFromAssetBundleDone, true);        
    }

    private void OnLoadSceneFromAssetBundleDone(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("OnLoaded " + result);
    }    

    private UbiAsyncOperation LoadSceneFromBuildAsync(string sceneName, LoadSceneMode mode)
    {
        AsyncOperation loadingTask = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        if (DebugUtils.SoftAssert(loadingTask != null, "The common scene " + sceneName + " couldn't be found (probably mispelled or not added to Build Settings)"));      

        return new UbiUnityAsyncOperation(loadingTask);        
    }    

    private UbiAsyncOperation UnloadSceneFromAssetBundlesAsync(string assetBundleId, string sceneName)
    {
        return AssetBundlesManager.Instance.UnloadSceneAsync(assetBundleId, sceneName, null, true);        
    }

    private UbiAsyncOperation UnloadSceneFromBuildAsync(string sceneName)
    {
        AsyncOperation loadingTask = SceneManager.UnloadSceneAsync(sceneName);
        return new UbiUnityAsyncOperation(loadingTask);
    }

    public void Update()
    {
        AssetBundlesManager.Instance.Update();

        if (Input.GetKeyDown(KeyCode.M))
        {
            //m_request = AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(SCENE_AB, Request_OnDone, true);
            m_request = AssetBundlesManager.Instance.LoadSceneAsync(SCENE_AB, SCENE_NAME, LoadSceneMode.Additive, Request_OnDone, true);
        }

        if (m_request != null && !m_request.isDone)
        {
            Debug.Log(m_request.progress);
        }
    }

    private static string SCENE_NAME = "SO_Medieval_Castle";
    private static string SCENE_AB = SCENE_NAME.ToLower();

    private AssetBundlesOpRequest m_request;

    private void Request_OnDone(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("Request_OnDone " + result);
    }
}
