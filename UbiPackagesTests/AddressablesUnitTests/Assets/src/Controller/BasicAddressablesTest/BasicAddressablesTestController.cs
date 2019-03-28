﻿using SimpleJSON;
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

    private enum EAddressableIds
    {
        HDCube,
        UbiCube
    };

    void Start ()
    {
        Ui_Init();
        //LoadUbiCubeFromtxtAsync();
    }
	
	void Update ()
    {
        if (m_request != null && m_request.isDone)
        {
            AssetBundle ab = m_request.assetBundle;
            Object ubiCube = ab.LoadAsset("UbiCube");
            Instantiate(ubiCube);
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
            AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled = !AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled;
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

        string addressablesPath = "Addressables";
        string assetBundlesPath = addressablesPath;
        string addressablesCatalogPath = Path.Combine(addressablesPath, "addressablesCatalog");

        // Addressables catalog 
        TextAsset targetFile = Resources.Load<TextAsset>(addressablesCatalogPath);
        string catalogAsText = (targetFile == null) ? null : targetFile.text;
        JSONNode catalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);

        // Downloadables catalog
        string downloadablesPath = addressablesPath;
        string downloadablesCatalogPath = Path.Combine(downloadablesPath, "downloadablesCatalog");        
        targetFile = Resources.Load<TextAsset>(downloadablesCatalogPath);
        catalogAsText = (targetFile == null) ? null : targetFile.text;        
        JSONNode downloadablesCatalogASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        
        // Downloadables config
        string downloadablesConfigPath = Path.Combine(downloadablesPath, "downloadablesConfig");        
        targetFile = Resources.Load<TextAsset>(downloadablesConfigPath);
        catalogAsText = (targetFile == null) ? null : targetFile.text;
        JSONNode downloadablesConfigASJSON = (string.IsNullOrEmpty(catalogAsText)) ? null : JSON.Parse(catalogAsText);
        Downloadables.Config downloadablesConfig = new Downloadables.Config();
        downloadablesConfig.Load(downloadablesConfigASJSON, logger);        

        Downloadables.Tracker tracker = new Downloadables.DummyTracker(downloadablesConfig, logger);
        m_addressablesManager.Initialize(catalogASJSON, assetBundlesPath, downloadablesConfig, downloadablesCatalogASJSON, true, tracker, logger);

        Ui_UpdateResolutionDropdown();
        //AssetBundlesManager.Instance.GetMockNetworkDriver().IsMockNetworkReachabilityEnabled = true;
        //AssetBundlesManager.Instance.GetMockNetworkDriver().MockNetworkReachability = NetworkReachability.NotReachable;
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
    #endregion


    #region ui
    public List<UIButton> m_uiButtons;    
    public Dropdown m_uiSceneCubesDropdown;
    public Dropdown m_uiSceneCubesResolutionDropdown;

    public Dropdown m_uiAssetCubesDropdown;
    public Dropdown m_uiAssetCubesAddressableIds;
    public Dropdown m_uiAssetCubesResolutionDropdown;
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
