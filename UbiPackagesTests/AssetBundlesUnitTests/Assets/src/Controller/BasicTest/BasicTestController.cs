using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for controlling the SC_BasicTest scene
/// </summary>
public class BasicTestController : MonoBehaviour
{
    public Transform m_cubesRoot;
    private int m_cubesLoadedCount;

    void Start()
    {
        string localAssetBundlesPath = Application.streamingAssetsPath;
        List<string> localAssetBundleIds = new List<string> { "01/asset_cubes", "01/scene_cubes" /*, "ab/logo", "ab/scene_cube"*/ };
        AssetBundlesManager.Instance.Initialize(localAssetBundleIds, localAssetBundlesPath, null);
        m_cubesLoadedCount = 0;

        SceneCubes_Init();
        Ui_Init();        
    }    

    private ELoadResourceMode GetLoadResourceModeFromDropdown(Dropdown dropdown)
    {
        int value = (dropdown == null) ? 0 : dropdown.value;
        return (ELoadResourceMode)value;
    }

    #region asset_cube
    private static string ASSET_CUBE_AB_NAME = "01/asset_cubes";
    private static string ASSET_CUBE_ASSET_NAME = "UbiCube";

    public void AssetCube_OnAddCube()
    {
        Ui_SetEnabled(false);
        Ui_SetOperationResultProcessing();        
        LoadResource_Start(ASSET_CUBE_AB_NAME, ASSET_CUBE_ASSET_NAME, true, GetLoadResourceModeFromDropdown(m_uiCubeDropdown), AssetCube_OnAddCubeDone);
    }

    private void AssetCube_OnAddCubeDone(AssetBundlesOp.EResult result, object data)
    {
        if (result == AssetBundlesOp.EResult.Success)
        {
            GameObject go = Instantiate(data as Object, m_cubesRoot) as GameObject;            
            go.transform.localPosition = new Vector3(m_cubesLoadedCount * 1.5f, 0f, 0f);
            m_cubesLoadedCount++;
        }
        
        Ui_SetEnabled(true);
        Ui_SetOperationResult(result);
    }

    private void AssetCube_UnloadAssetBundle()
    {
        Ui_SetEnabled(false);
        Ui_SetOperationResultProcessing();
        AssetBundlesManager.Instance.UnloadAssetBundle(ASSET_CUBE_AB_NAME, AssetCube_OnUnloadAssetBundleDone);
    }

    private void AssetCube_OnUnloadAssetBundleDone(AssetBundlesOp.EResult result, object data)
    {
        Ui_SetEnabled(true);
        Ui_SetOperationResult(result);
    }
    #endregion

            #region scene_cubes
    private static string SCENE_CUBES_AB_NAME = "01/scene_cubes";
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
            
            LoadResource_Start(SCENE_CUBES_AB_NAME, SCENE_CUBES_SCENE_NAME, false, GetLoadResourceModeFromDropdown(m_uiCubeDropdown), SceneCubes_OnDone, LoadSceneMode.Additive);
        }
    }

    private void SceneCubes_OnDone(AssetBundlesOp.EResult result, object data)
    {
        if (result == AssetBundlesOp.EResult.Success)
        {
            SceneCubes_IsLoaded = !SceneCubes_IsLoaded;
        }
        
        Ui_SetEnabled(true);
        Ui_SetOperationResult(result);
    }

    private void SceneCubes_OnRemove()
    {
        if (SceneCubes_IsLoaded)
        {
            AssetBundlesManager.Instance.UnloadSceneAsync(SCENE_CUBES_AB_NAME, SCENE_CUBES_SCENE_NAME, SceneCubes_OnDone);
            SceneManager.UnloadSceneAsync("SC_Cubes");
        }
    }
    #endregion

    #region load_resource
    private enum ELoadResourceMode
    {
        Sync,
        Async,
        AsyncFull
    };

    private string m_loadResourceABId;
    private string m_loadResourceName;
    private bool m_loadResourceIsAsset;
    private ELoadResourceMode m_loadResourceMode;
    private LoadSceneMode m_loadResourceSceneMode;
    private AssetBundlesOp.OnDoneCallback m_loadResourceOnDone;

    private void LoadResource_Start(string abId, string resourceName, bool isAsset, ELoadResourceMode mode, AssetBundlesOp.OnDoneCallback onDone, LoadSceneMode sceneMode = LoadSceneMode.Single)
    {
        m_loadResourceABId = abId;
        m_loadResourceName = resourceName;
        m_loadResourceIsAsset = isAsset;

        m_loadResourceMode = mode;
        m_loadResourceSceneMode = sceneMode;

        m_loadResourceOnDone = onDone;

        //string abId = "ab/logo";
        if (m_loadResourceMode != ELoadResourceMode.AsyncFull)
        {
            AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(abId, LoadResource_OnAssetBundleLoaded);
        }
        else
        {
            AssetBundlesManager.Instance.LoadResourceAsync(m_loadResourceABId, m_loadResourceName, m_loadResourceIsAsset, LoadResource_OnAssetLoaded, m_loadResourceSceneMode);
        }
    }

    private void LoadResource_OnAssetBundleLoaded(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("LoadResource_OnAssetBundleLoaded " + result);
        if (result == AssetBundlesOp.EResult.Success)
        {
            if (m_loadResourceMode == ELoadResourceMode.Sync)
            {                
                Object asset = AssetBundlesManager.Instance.LoadResource(m_loadResourceABId, m_loadResourceName, m_loadResourceIsAsset, m_loadResourceSceneMode);
                LoadResource_OnDone((m_loadResourceIsAsset && asset == null) ? AssetBundlesOp.EResult.Error_Asset_Not_Found_In_AB : AssetBundlesOp.EResult.Success, asset);                
            }
            else if (m_loadResourceMode == ELoadResourceMode.Async)
            {
                AssetBundleHandle handle = AssetBundlesManager.Instance.GetAssetBundleHandle(m_loadResourceABId);
                AssetBundlesManager.Instance.LoadResourceFromAssetBundleAsync(handle.AssetBundle, m_loadResourceName, m_loadResourceIsAsset, LoadResource_OnAssetLoaded, m_loadResourceSceneMode);
            }
        }
        else 
        {
            LoadResource_OnDone(result, data);
        }
    }

    private void LoadResource_OnAssetLoaded(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("On resource " + m_loadResourceName + " loaded from asset bundle " + m_loadResourceABId + " result = " + result);        
        LoadResource_OnDone(result, data);
    }

    private void LoadResource_OnDone(AssetBundlesOp.EResult result, object data)
    {
        if (m_loadResourceOnDone != null)
        {
            m_loadResourceOnDone(result, data);
        }
    }
    #endregion

    #region ui
    public List<UIButton> m_uiButtons;    
    public Dropdown m_uiCubeDropdown;
    public Dropdown m_uiSceneCubeDropdown;
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
                        case UIButton.EId.AddAssetCube:
                            m_uiButtons[i].Button.onClick.AddListener(AssetCube_OnAddCube);
                            break;

                        case UIButton.EId.UnloadCubeAssetBundle:
                            m_uiButtons[i].Button.onClick.AddListener(AssetCube_UnloadAssetBundle);
                            break;

                        case UIButton.EId.AddSceneCubes:
                            m_uiButtons[i].Button.onClick.AddListener(SceneCubes_OnAdd);
                            break;

                        case UIButton.EId.RemoveSceneCubes:
                            m_uiButtons[i].Button.onClick.AddListener(SceneCubes_OnRemove);
                            break;
                    }
                }                
            }
        }

        Ui_SetEnabled(true);
        Ui_SetOperationResultEmpty();
        Ui_SetupDropdownWithLoadResourceModeValues(m_uiCubeDropdown);
        Ui_SetupDropdownWithLoadResourceModeValues(m_uiSceneCubeDropdown);                
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

    private void Ui_SetOperationResult(AssetBundlesOp.EResult result)
    {
        Color color = (result == AssetBundlesOp.EResult.Success) ? Color.green : Color.red;
        Ui_SetOperationResultValue(result.ToString(), color);
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

    public void Update()
    {
        AssetBundlesManager.Instance.Update();
    }
}
