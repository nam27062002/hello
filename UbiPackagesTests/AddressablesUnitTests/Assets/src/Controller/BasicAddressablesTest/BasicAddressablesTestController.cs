using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BasicAddressablesTestController : MonoBehaviour
{
    public Transform m_cubesRoot;
    private int m_cubesLoadedCount;
    public GameObject m_cube;

    public SpriteRenderer m_spriteRenderer;

    private enum ELoadResourceMode
    {
        Sync,
        Async        
    };

    private enum EAddressableIds
    {
        HDCube,
        UbiCube
    };

    void Start ()
    {
        Ui_Init();
        //LoadUbiCubeFromtxtAsync();

        // From Resources
        //Cube_AddMaterialFromResources();
        //Cube_AddTextureFromResources();

        // From Addressables Async
        //Cube_AddMaterialAsyncFromAddressables();
        //Cube_AddTextureAsyncFromAddressables();

        // From Addressables Sync
        //Cube_AddMaterialFromAddressables();
        Cube_AddTextureFromAddressables();        
    }

    void OnAddressablesInit()
    {
        Debug.Log("OnAddressablesInit");
        //LoadSpriteSync();
        LoadSpriteAsync();
    }

    private void LoadSpriteSync()
    {
        Sprite sprite = m_addressablesManager.LoadAsset<Sprite>("UbiLogoSprite");
        OnSpriteLoaded(sprite);
    }

    private void LoadSpriteAsync()
    {
        AddressablesOp op = m_addressablesManager.LoadAssetAsync("UbiLogoSprite");
        op.OnDone = OnSpriteAsyncLoaded;
    }

    private void OnSpriteAsyncLoaded(AddressablesOp op)
    {
        Sprite sprite = op.GetAsset<Sprite>();
        OnSpriteLoaded(sprite);
    }

    private void OnSpriteLoaded(Sprite sprite)
    {
        if (sprite == null)
        {
            Debug.Log("Sprite is null");
        }
        else
        {
            m_spriteRenderer.sprite = sprite;
        }
    }

    void Update ()
    {
        if (m_request != null && m_request.isDone)
        {
            AssetBundle ab = m_request.assetBundle;
            Object ubiCube = ab.LoadAsset("UbiCube");
            Instantiate(ubiCube);
            m_request = null;
        }        

        m_addressablesManager.Update();

        if (Input.GetKeyDown(KeyCode.M))
        {
            //List<string> list = m_addressablesManager.GetDependencyIds(SCENE_CUBES_SCENE_NAME);
            //List<string> ids = new List<string>() { /*SCENE_CUBES_SCENE_NAME,*/ "HDCube", "UbiCube" };
            //List<string> list = m_addressablesManager.GetDependencyIdsList(ids);            

            //List<string> list = m_addressablesManager.Areas_GetDependencyIds("area_asset_cubes");
            //Debug.Log("dependencies = " + UbiListUtils.GetListAsString(list));            

#if UNITY_EDITOR
            //AssetBundlesManager.Instance.GetMockNetworkDriver().IsMockNetworkReachabilityEnabled = !AssetBundlesManager.Instance.GetMockNetworkDriver().IsMockNetworkReachabilityEnabled;
#endif       
            //AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled = !AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled;

#if USE_DUMPER
            Downloadables.Dumper dumper = AssetBundlesManager.Instance.GetDownloadablesDumper();
            if (dumper != null)
            {
                dumper.IsFillingUp = !dumper.IsFillingUp;
            }
#endif
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
#if USE_DUMPER
            Downloadables.Dumper dumper = AssetBundlesManager.Instance.GetDownloadablesDumper();
            if (dumper != null)
            {
                dumper.CleanUp();
            }
#endif
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            LoadUbiCubeFromTxt();
        }
    }

    private void LoadUbiCubeFromTxt()
    {
        TextAsset abAsText = (TextAsset)Resources.Load("Addressables/AssetBundles/asset_cubes");
        byte[] abBytes = abAsText.bytes;
        AssetBundle ab = AssetBundle.LoadFromMemory(abBytes);
        Object ubiCube = ab.LoadAsset("UbiCube");
        Instantiate(ubiCube);
    }

    AssetBundleCreateRequest m_request;
    private void LoadUbiCubeFromtxtAsync()
    {
        TextAsset abAsText = (TextAsset)Resources.Load("Addressables/AssetBundles/asset_cubes");
        byte[] abBytes = abAsText.bytes;
        m_request = AssetBundle.LoadFromMemoryAsync(abBytes);        
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

        Ui_UpdateResolutionDropdown();
        //AssetBundlesManager.Instance.GetMockNetworkDriver().IsMockNetworkReachabilityEnabled = true;
        //AssetBundlesManager.Instance.GetMockNetworkDriver().MockNetworkReachability = NetworkReachability.NotReachable;
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
        return File.ReadAllText(path);
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

#region cube
    private static string CUBE_ADDRESSABLES_MATERIAL = "unknownMaterial";
    private static string CUBE_ADDRESSABLES_TEXTURE = "HDLogo";

    private void Cube_AddMaterial(Material material)
    {
        Renderer renderer = m_cube.GetComponent<Renderer>();
        renderer.material = material;
    }

    private void Cube_AddTexture(Texture texture)
    {
        Renderer renderer = m_cube.GetComponent<Renderer>();
        renderer.material.mainTexture = texture;
    }    

    private void Cube_AddMaterialFromResources()
    {     
        Material material = Resources.Load<Material>("Materials/UnityLogoMaterial");
        Cube_AddMaterial(material);
    }

    private void Cube_AddTextureFromResources()
    {        
        Renderer renderer = m_cube.GetComponent<Renderer>();
        Material material = renderer.material;

        Texture texture = Resources.Load<Texture>("Textures/HDLogo");
        material.mainTexture = texture;        
    }

    private void Cube_AddMaterialAsyncFromAddressables()
    {
        Addressables_Init();

        AddressablesOp op = m_addressablesManager.LoadAssetAsync(CUBE_ADDRESSABLES_MATERIAL);
        op.OnDone = Cube_OnAddMaterialAsyncFromAddressables;        
    }

    private void Cube_OnAddMaterialAsyncFromAddressables(AddressablesOp op)
    {
        if (op.Error == null)
        {
            Material material = op.GetAsset<Material>();
            Cube_AddMaterial(material);
        }
        else
        {
            Debug.Log("Error " + op.Error.ToString());
        }
    }

    private void Cube_AddTextureAsyncFromAddressables()
    {
        // We need the cube to have a material before changing its texture to the one loaded from addressables
        Cube_AddMaterialFromResources();

        Addressables_Init();

        AddressablesOp op = m_addressablesManager.LoadAssetAsync(CUBE_ADDRESSABLES_TEXTURE);
        op.OnDone = Cube_OnAddTextureAsyncFromAddressables;
    }

    private void Cube_OnAddTextureAsyncFromAddressables(AddressablesOp op)
    {
        if (op.Error == null)
        {
            Texture texture = op.GetAsset<Texture>();
            Cube_AddTexture(texture);
        }
        else
        {
            Debug.Log("Error " + op.Error.ToString());
        }
    }

    private void Cube_AddMaterialFromAddressables()
    {
        Addressables_Init();

        AddressablesOp op = m_addressablesManager.LoadDependenciesAsync(CUBE_ADDRESSABLES_MATERIAL);
        op.OnDone = Cube_OnAddMaterialFromAddressables;
    }

    private void Cube_OnAddMaterialFromAddressables(AddressablesOp op)
    {
        if (op.Error == null)
        {
            Material material = m_addressablesManager.LoadAsset<Material>(CUBE_ADDRESSABLES_MATERIAL);
            Cube_AddMaterial(material);
        }
        else
        {
            Debug.Log("Error " + op.Error.ToString());
        }
    }

    private void Cube_AddTextureFromAddressables()
    {
        Addressables_Init();

        AddressablesOp op = m_addressablesManager.LoadDependenciesAsync(CUBE_ADDRESSABLES_TEXTURE);
        op.OnDone = Cube_OnAddTextureFromAddressables;
    }

    private void Cube_OnAddTextureFromAddressables(AddressablesOp op)
    {
        if (op.Error == null)
        {
            Texture texture = m_addressablesManager.LoadAsset<Texture2D>(CUBE_ADDRESSABLES_TEXTURE);
            Cube_AddTexture(texture);
        }
        else
        {
            Debug.Log("Error " + op.Error.ToString());
        }
    }
#endregion

#region scene_cubes    
    private static string SCENE_CUBES_SCENE_NAME = "SC_Cubes";

    public void SceneCubes_Init()
    {
        SceneCubes_IsLoaded = false;
    }

    public bool SceneCubes_IsLoaded { get; set; }

    private string SceneCubes_GetSceneId()
    {
        return SCENE_CUBES_SCENE_NAME;
    }

    private string SceneCubes_GetVariant()
    {
        string returnValue = null;

        //if (m_addressablesManager.HasResourceVariants(AssetCubes_GetAssetId()))
        {
            switch (m_uiSceneCubesResolutionDropdown.value)
            {
                case 0:
                    returnValue = "low";
                    break;

                case 1:
                    returnValue = "high";
                    break;
            }
        }

        //returnValue = null;

        return returnValue;
    }

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
                    op = m_addressablesManager.DownloadDependenciesAsync(SceneCubes_GetSceneId(), SceneCubes_GetVariant());
                    op.OnDone = Scene_OnDependenciesDownloaded;
                    break;

                case ELoadResourceMode.Async:
                    op = m_addressablesManager.LoadSceneAsync(SceneCubes_GetSceneId(), SceneCubes_GetVariant(), UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    op.OnDone = SceneCubes_OnDoneByOp;
                    break;
            }                                    
        }
    }

    private void Scene_OnDependenciesDownloaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            op = m_addressablesManager.LoadDependenciesAsync(SceneCubes_GetSceneId(), SceneCubes_GetVariant());
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
            m_addressablesManager.LoadScene(SceneCubes_GetSceneId(), SceneCubes_GetVariant(), UnityEngine.SceneManagement.LoadSceneMode.Additive);
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
            AddressablesOp op = m_addressablesManager.UnloadSceneAsync(SceneCubes_GetSceneId(), SceneCubes_GetVariant());            
            op.OnDone = SceneCubes_OnDoneByOp;                        
        }
    }
#endregion

#region asset_cubes        

    public void AssetCubes_Init()
    {        
    }    

    private string AssetCubes_GetAssetId()
    {       
        return ((EAddressableIds)m_uiAssetCubesAddressableIds.value).ToString();
    }

    private string AssetCubes_GetVariant()
    {
        string returnValue = null;

        if (m_addressablesManager.HasResourceVariants(AssetCubes_GetAssetId()))        
        {
            switch (m_uiAssetCubesResolutionDropdown.value)
            {
                case 0:
                    returnValue = "low";
                    break;

                case 1:
                    returnValue = "high";
                    break;
            }
        }

        return returnValue;
    }

    public void AssetCubes_OnAdd()
    {        
        Ui_SetEnabled(false);
        Ui_SetOperationResultProcessing();

        string assetId = AssetCubes_GetAssetId();
        string variant = AssetCubes_GetVariant();

        ELoadResourceMode mode = GetLoadResourceModeFromDropdown(m_uiAssetCubesDropdown);
        AddressablesOp op;
        switch (mode)
        {
            case ELoadResourceMode.Sync:
                op = m_addressablesManager.DownloadDependenciesAsync(assetId);                
                op.OnDone = AssetCubes_OnDependenciesDownloaded;
                break;            
            
            case ELoadResourceMode.Async:
                op = m_addressablesManager.LoadAssetAsync(assetId, variant);
                op.OnDone = AssetCubes_OnDoneByOp;
                break;
        }        
    }

    private void AssetCubes_OnDependenciesDownloaded(AddressablesOp op)
    {
        if (op.Error == null)
        {
            op = m_addressablesManager.LoadDependenciesAsync(AssetCubes_GetAssetId(), AssetCubes_GetVariant());
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
            GameObject prefab = m_addressablesManager.LoadAsset<GameObject>(AssetCubes_GetAssetId(), AssetCubes_GetVariant());
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
            AssetCubes_InstantiateCube(op.GetAsset<GameObject>());                                    
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
#endregion


#region ui
    public List<UIButton> m_uiButtons;    
    public Dropdown m_uiSceneCubesDropdown;
    public Dropdown m_uiSceneCubesResolutionDropdown;

    public Dropdown m_uiAssetCubesDropdown;
    public Dropdown m_uiAssetCubesAddressableIds;
    public Dropdown m_uiAssetCubesResolutionDropdown;
    public Text m_uiOperationResult;
    public Text m_uiDiskFreeSpace;

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
        Ui_SetupDropdownWithEnumValues(m_uiAssetCubesAddressableIds, System.Enum.GetNames(typeof(EAddressableIds)));

        m_uiAssetCubesAddressableIds.onValueChanged.AddListener(delegate { Ui_OnAddressableIdChanged(m_uiAssetCubesAddressableIds); });
    }
    
    private void Ui_OnAddressableIdChanged(Dropdown dropDown)
    {
        Ui_UpdateResolutionDropdown();
    }

    private void Ui_UpdateResolutionDropdown()
    {
        m_uiAssetCubesResolutionDropdown.gameObject.SetActive(m_addressablesManager.HasResourceVariants(AssetCubes_GetAssetId()));
    }

    private void Ui_SetupDropdownWithEnumValues(Dropdown value, string[] names)
    {
        if (value != null)
        {
            value.ClearOptions();
            
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

    private void Ui_SetupDropdownWithLoadResourceModeValues(Dropdown value)
    {
        string[] names = System.Enum.GetNames(typeof(ELoadResourceMode));
        Ui_SetupDropdownWithEnumValues(value, names);
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
