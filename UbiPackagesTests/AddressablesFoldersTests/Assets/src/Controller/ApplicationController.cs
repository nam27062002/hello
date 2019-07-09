using SimpleJSON;
using System;
using System.IO;
using UnityEngine;

public class ApplicationController : Singleton<ApplicationController>
{   
    public enum EScene
    {     
        Menu,   
        Village,
        Castle
    };

    private int SCENES_COUNT = Enum.GetNames(typeof(EScene)).Length;

    private EScene m_prevScene = EScene.Menu;
    private EScene m_scene = EScene.Menu;    

    public void SetScene(EScene scene)
    {
        m_scene = scene;
    }

    void Start()
    {
        Addressables_Init();
    }

    void OnAddressablesInit()
    {
        Debug.Log("OnAddressablesInit");     
    }    

    void Update()
    {
        m_addressablesManager.Update();

        switch (OpType)
        {
            case EOpType.None:
                // Changes scenes
                if (Input.GetKeyDown(KeyCode.C))
                {
                    LevelScenes_Switch();
                }
                else if (Input.GetKeyDown(KeyCode.I))
                {
                    LevelScenes_Instantiate();
                }
                break;            
        }
        
        if (m_levelScenesAsyncOp != null && m_levelScenesAsyncOp.isDone)
        {
            LevelScenes_OnOpDone();            
        }       
    }

    protected override void OnApplicationQuitExtended()
    {        
        m_addressablesManager.Reset();        
    }

    #region level_scenes
    private enum EOpType
    {
        None,
        LoadingLoadingScene,
        LoadingLevelScene,
        InstantiatingScene
    };

    private EOpType OpType { get; set; }

    private UbiAsyncOperation m_levelScenesAsyncOp;

    public void LevelScenes_Switch()
    {
        m_prevScene = m_scene;
        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("SC_Loading");
        m_levelScenesAsyncOp = new UbiUnityAsyncOperation(op);

        OpType = EOpType.LoadingLoadingScene;
    }

    private string LevelScenes_GetSceneName(EScene scene)
    {
        return "SC_" + scene.ToString();
    }

    private string LevelScenes_GetPrefabName(EScene scene)
    {
        return "PF_" + scene.ToString();
    }

    private void LevelScenes_Instantiate()
    {
        string prefabName = LevelScenes_GetPrefabName(m_scene);
        AddressablesOp op = m_addressablesManager.LoadAssetAsync(prefabName);
        op.OnDone = LevelScenes_OnInstantiated;

        m_levelScenesAsyncOp = op;
        OpType = EOpType.InstantiatingScene;
    }    

    private void LevelScenes_OnInstantiated(AddressablesOp op)
    {
        GameObject prefab = op.GetAsset<GameObject>();
        if (prefab != null)
        {
            Instantiate(prefab);
        }
    }

    private void LevelScenes_OnOpDone()
    {
        switch (OpType)
        {
            case EOpType.LoadingLoadingScene:
                OpType = EOpType.LoadingLevelScene;

                int sceneAsInt = (int)m_prevScene;
                m_scene = (EScene)((sceneAsInt + 1) % SCENES_COUNT);

                string currentSceneId = LevelScenes_GetSceneName(m_scene);
                m_levelScenesAsyncOp = m_addressablesManager.LoadSceneAsync(currentSceneId);
                break;

            default:
                OpType = EOpType.None;
                m_levelScenesAsyncOp = null;
                break;            
        }
    }
    #endregion

    #region addressables
    private AddressablesManager m_addressablesManager = new AddressablesManager();

    private void Addressables_Init()
    {
        Logger logger = new ConsoleLogger("Addressables");

        // Addressables catalog         
        string catalogAsText = GetAddressablesFileText("addressablesCatalog", true);
        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Downloadables catalog        
        catalogAsText = GetAddressablesFileText("downloadablesCatalog", true);
        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Downloadables config        
        catalogAsText = GetAddressablesFileText("downloadablesConfig", false);
        JSONNode downloadablesConfigASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        Downloadables.Config downloadablesConfig = new Downloadables.Config();
        downloadablesConfig.Load(downloadablesConfigASJSON, logger);

        // AssetBundles catalog
        catalogAsText = GetAddressablesFileText("assetBundlesCatalog", true);
        JSONNode abCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        Downloadables.Tracker tracker = new Downloadables.DummyTracker(downloadablesConfig, logger);
        m_addressablesManager.Initialize(catalogASJSON, abCatalogASJSON, downloadablesConfig, downloadablesCatalogASJSON, true, tracker, logger);

        OnAddressablesInit();
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

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        else
        {
            return null;
        }
#else
        string path = "Addressables/" + fileName;                        
        TextAsset targetFile = Resources.Load<TextAsset>(path);
        return (targetFile == null) ? null : targetFile.text;
#endif
    }

    public void Addressables_Reset()
    {
        m_addressablesManager.Reset();
    }
    #endregion
}
