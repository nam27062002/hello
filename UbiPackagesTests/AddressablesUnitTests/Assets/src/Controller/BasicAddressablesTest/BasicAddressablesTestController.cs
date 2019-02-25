using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BasicAddressablesTestController : MonoBehaviour
{
    public Transform m_cubesRoot;
    private int m_cubesLoadedCount;

    private enum ELoadResourceMode
    {
        Sync,
        Async        
    };
    
    void Start ()
    {
        Ui_Init();
    }
	
	void Update ()
    {
        m_addressablesManager.Update();

        if (Input.GetKeyDown(KeyCode.M))
        {
            //List<string> list = m_addressablesManager.GetDependencyIds(SCENE_CUBES_SCENE_NAME);
            //List<string> ids = new List<string>() { /*SCENE_CUBES_SCENE_NAME,*/ "HDCube", "UbiCube" };
            //List<string> list = m_addressablesManager.GetDependencyIdsList(ids);            

            List<string> list = m_addressablesManager.Areas_GetDependencyIds("area_asset_cubes");
            Debug.Log("dependencies = " + UbiListUtils.GetListAsString(list));            
        }
    }

    void OnApplicationQuit()
    {
        m_addressablesManager.Reset();
    }

    private ELoadResourceMode GetLoadResourceModeFromDropdown(Dropdown dropdown)
    {
        int value = (dropdown == null) ? 0 : dropdown.value;
        return (ELoadResourceMode)value;
    }

    #region addressables
    private AddressablesManager m_addressablesManager = new AddressablesManager();

    private void Addressables_Init()
    {            
        Logger logger = new ConsoleLogger("Addressables");
        string addressablesPath = "Addressables";/*Path.Combine(Application.streamingAssetsPath, "Addressables");*/
        string assetBundlesPath = Path.Combine(addressablesPath, "AssetBundles");
        string addressablesCatalogPath = Path.Combine(addressablesPath, "addressablesCatalog.json");

        string catalogAsText = null;
        logger.Log("AddressablesCatalog path = " + addressablesCatalogPath + " Exists = " + File.Exists(addressablesCatalogPath) + " platform = " + Application.platform);

        BetterStreamingAssets.Initialize();
        
        if (BetterStreamingAssets.FileExists(addressablesCatalogPath))
        {
            catalogAsText = BetterStreamingAssets.ReadAllText(addressablesCatalogPath);            
        }
        
        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        
        string downloadablesPath = addressablesPath;
        string downloadablesCatalogPath = Path.Combine(downloadablesPath, "downloadablesCatalog.json");
        if (BetterStreamingAssets.FileExists(downloadablesCatalogPath))
        {
            catalogAsText = BetterStreamingAssets.ReadAllText(downloadablesCatalogPath);
        }
        else
        {
            catalogAsText = null;
        }

        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        Downloadables.Tracker tracker = new Downloadables.DummyTracker(logger);
        m_addressablesManager.Initialize(catalogASJSON, assetBundlesPath, downloadablesCatalogASJSON, false, tracker, logger);        
    }

    public void Addressables_Reset()
    {
        m_addressablesManager.Reset();     
    }
    #endregion

    #region scene_cubes    
    private static string SCENE_CUBES_SCENE_NAME = "SC_Cubes";

    public void SceneCubes_Init()
    {
        SceneCubes_IsLoaded = false;
    }

    public bool SceneCubes_IsLoaded { get; set; }

    public void SceneCubes_OnAdd()
    {
        if (!SceneCubes_IsLoaded)
        {
            Ui_SetEnabled(false);
            Ui_SetOperationResultProcessing();

            ELoadResourceMode mode = GetLoadResourceModeFromDropdown(m_uiSceneCubesDropdown);
            AddressablesOp op;
            switch (mode)
            {
                case ELoadResourceMode.Sync:
                    op = m_addressablesManager.DownloadDependenciesAsync(SCENE_CUBES_SCENE_NAME);
                    op.OnDone = Scene_OnDependenciesDownloaded;
                    break;

                case ELoadResourceMode.Async:
                    op = m_addressablesManager.LoadSceneAsync(SCENE_CUBES_SCENE_NAME, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    op.OnDone = SceneCubes_OnDoneByOp;
                    break;
            }                                    
        }
    }

    private void Scene_OnDependenciesDownloaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            op = m_addressablesManager.LoadDependenciesAsync(SCENE_CUBES_SCENE_NAME);
            op.OnDone = Scene_OnDependenciesLoaded;
        }
        else
        {
            SceneCubes_OnDone(op.Error);
        }
    }

    private void Scene_OnDependenciesLoaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            m_addressablesManager.LoadScene(SCENE_CUBES_SCENE_NAME, UnityEngine.SceneManagement.LoadSceneMode.Additive);
            SceneCubes_OnDone(null);
        }
        else
        {
            SceneCubes_OnDone(op.Error);
        }        
    }


    private void SceneCubes_OnDoneByOp(AddressablesOp op)
    {
        SceneCubes_OnDone(op.Error);        
    }

    private void SceneCubes_OnDone(AddressablesError error)
    {
        if (error == null)
        {
            SceneCubes_IsLoaded = !SceneCubes_IsLoaded;
        }

        Ui_SetEnabled(true);
        Ui_SetOperationResult(error);
    }

    private void SceneCubes_OnRemove()
    {
        if (SceneCubes_IsLoaded)
        {
            AddressablesOp op = m_addressablesManager.UnloadSceneAsync(SCENE_CUBES_SCENE_NAME);            
            op.OnDone = SceneCubes_OnDoneByOp;                        
        }
    }
    #endregion

    #region asset_cubes    
    private static string ASSET_CUBES_NAME = "HDCube";

    public void AssetCubes_Init()
    {     
    }    

    public void AssetCubes_OnAdd()
    {        
        Ui_SetEnabled(false);
        Ui_SetOperationResultProcessing();

        ELoadResourceMode mode = GetLoadResourceModeFromDropdown(m_uiAssetCubesDropdown);
        AddressablesOp op;
        switch (mode)
        {
            case ELoadResourceMode.Sync:
                op = m_addressablesManager.DownloadDependenciesAsync(ASSET_CUBES_NAME);                
                op.OnDone = AssetCubes_OnDependenciesDownloaded;
                break;            
            
            case ELoadResourceMode.Async:
                op = m_addressablesManager.LoadAssetAsync(ASSET_CUBES_NAME);
                op.OnDone = AssetCubes_OnDoneByOp;
                break;
        }        
    }

    private void AssetCubes_OnDependenciesDownloaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            op = m_addressablesManager.LoadDependenciesAsync(ASSET_CUBES_NAME);
            op.OnDone = AssetCubes_OnDependenciesLoaded;
        }
        else
        {
            AssetCubes_OnDone(op.Error);
        }
    }

    private void AssetCubes_OnDependenciesLoaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            GameObject prefab = m_addressablesManager.LoadAsset<GameObject>(ASSET_CUBES_NAME);
            AssetCubes_InstantiateCube(prefab);
            AssetCubes_OnDone(null);
        }
        else
        {
            AssetCubes_OnDone(op.Error);
        }
    }

    private void AssetCubes_OnDoneByOp(AddressablesOp op)
    {        
        if (op.Error == null)
        {
            AssetCubes_InstantiateCube(op.GetAsset<Object>());                                    
        }

        AssetCubes_OnDone(op.Error);
    }

    private void AssetCubes_InstantiateCube(Object prefab)
    {
        Debug.Log("OnAssetCubes prefab is null " + (prefab == null));

        if (prefab != null)
        {
            GameObject go = Instantiate(prefab as Object, m_cubesRoot) as GameObject;
            go.transform.localPosition = new Vector3(m_cubesLoadedCount * 1.5f, 0f, 0f);
            m_cubesLoadedCount++;
        }
    }

    private void AssetCubes_OnDone(AddressablesError error)
    {        
        Ui_SetEnabled(true);
        Ui_SetOperationResult(error);
    }

    /*
    private void SceneCubes_OnRemove()
    {
        if (SceneCubes_IsLoaded)
        {
            AddressablesOp op = m_addressablesManager.UnloadSceneAsync(SCENE_CUBES_SCENE_NAME);
            op.OnDone = SceneCubes_OnDoneByOp;
        }
    }
    */
    #endregion


    #region ui
    public List<UIButton> m_uiButtons;    
    public Dropdown m_uiSceneCubesDropdown;
    public Dropdown m_uiAssetCubesDropdown;
    public Text m_uiOperationResult;    

    private void Ui_Init()
    {
        if (m_uiButtons != null)
        {
            int count = m_uiButtons.Count;
            for (int i = 0; i < count; i++)
            {
                if (m_uiButtons[i] != null)
                {
                    switch (m_uiButtons[i].m_id)
                    {
                        case UIButton.EId.ABInit:
                            m_uiButtons[i].Button.onClick.AddListener(Addressables_Init);
                            break;

                        case UIButton.EId.ABReset:
                            m_uiButtons[i].Button.onClick.AddListener(Addressables_Reset);
                            break;

                        case UIButton.EId.AddSceneCubes:
                            m_uiButtons[i].Button.onClick.AddListener(SceneCubes_OnAdd);
                            break;

                        case UIButton.EId.RemoveSceneCubes:
                            m_uiButtons[i].Button.onClick.AddListener(SceneCubes_OnRemove);
                            break;

                        case UIButton.EId.AddAssetCube:
                            m_uiButtons[i].Button.onClick.AddListener(AssetCubes_OnAdd);
                            break;
                    }
                }
            }
        }
        
        Ui_SetEnabled(true);
        Ui_SetOperationResultEmpty();        
        Ui_SetupDropdownWithLoadResourceModeValues(m_uiSceneCubesDropdown);
        Ui_SetupDropdownWithLoadResourceModeValues(m_uiAssetCubesDropdown);
    }

    private void Ui_SetupDropdownWithLoadResourceModeValues(Dropdown value)
    {
        if (value != null)
        {
            value.ClearOptions();

            string[] names = System.Enum.GetNames(typeof(ELoadResourceMode));
            if (names != null)
            {
                List<Dropdown.OptionData> optionsList = new List<Dropdown.OptionData>();
                Dropdown.OptionData optionData;
                int count = names.Length;
                for (int i = 0; i < count; i++)
                {
                    optionData = new Dropdown.OptionData();
                    optionData.text = names[i];
                    optionsList.Add(optionData);
                }

                value.AddOptions(optionsList);
            }
        }
    }

    private void Ui_SetEnabled(bool value)
    {
        if (m_uiButtons != null)
        {
            int count = m_uiButtons.Count;
            bool thisValue;
            for (int i = 0; i < count; i++)
            {
                switch (m_uiButtons[i].m_id)
                {
                    case UIButton.EId.AddSceneCubes:
                        thisValue = !SceneCubes_IsLoaded;
                        break;

                    case UIButton.EId.RemoveSceneCubes:
                        thisValue = SceneCubes_IsLoaded;                        
                        break;

                    default:
                        thisValue = value;
                        break;
                }

                m_uiButtons[i].interactable = thisValue;
            }
        }
    }

    private void Ui_SetOperationResultEmpty()
    {
        Ui_SetOperationResultValue("", Color.black);
    }

    private void Ui_SetOperationResultProcessing()
    {
        Ui_SetOperationResultValue("Processing", Color.black);
    }

    private void Ui_SetOperationResult(AddressablesError error)
    {
        Color color = (error == null) ? Color.green : Color.red;
        string msg;
        if (error == null)
        {
            msg = "Success";
        }
        else
        {
            msg = error.Type.ToString();
            if (error.Type == AddressablesError.EType.Error_Asset_Bundles)
            {
                msg += " abError: " + error.AssetBundlesError.ToString();
            }
        }
        
        Ui_SetOperationResultValue(msg, color);
    }

    private void Ui_SetOperationResultValue(string value, Color color)
    {
        if (m_uiOperationResult != null)
        {
            m_uiOperationResult.text = value;
            m_uiOperationResult.color = color;
        }
    }
    #endregion
}
