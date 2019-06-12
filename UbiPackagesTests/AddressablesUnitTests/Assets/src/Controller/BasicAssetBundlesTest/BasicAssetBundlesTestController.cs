using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for controlling the SC_BasicTest scene
/// </summary>
public class BasicAssetBundlesTestController : MonoBehaviour
{
    public Transform m_cubesRoot;
    private int m_cubesLoadedCount;

    void Start()
    {        
        Camera.main.eventMask = 0;

        m_cubesLoadedCount = 0;

        SceneCubes_Init();
        Ui_Init();        
    }    

    private ELoadResourceMode GetLoadResourceModeFromDropdown(Dropdown dropdown)
    {
        int value = (dropdown == null) ? 0 : dropdown.value;
        return (ELoadResourceMode)value;
    }

    private bool m_useRequests = true;

    #region asset_cube
    private static string ASSET_CUBE_AB_NAME = "asset_cubes";
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
    private static string SCENE_CUBES_AB_NAME = "scene_cubes_low";
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
        }
    }

    private void SceneCubes_UnloadAB()
    {
        AssetBundlesManager.Instance.UnloadAssetBundle(SCENE_CUBES_AB_NAME, SceneCubes_OnDone);
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
            AssetBundlesManager.Instance.DownloadAssetBundleAndDependencies(abId, LoadResource_OnAssetBundleDownloaded, m_useRequests);            
        }
        else
        {
            AssetBundlesManager.Instance.LoadResourceAsync(m_loadResourceABId, m_loadResourceName, m_loadResourceIsAsset, LoadResource_OnAssetLoaded, m_loadResourceSceneMode, m_useRequests);
        }
    }

    private void LoadResource_OnAssetBundleDownloaded(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("LoadResource_OnAssetBundleDownloaded " + result);
        if (result == AssetBundlesOp.EResult.Success)
        {
            AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(m_loadResourceABId, LoadResource_OnAssetBundleLoaded, m_useRequests);            
        }
        else
        {
            LoadResource_OnDone(result, data);
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
                AssetBundlesManager.Instance.LoadResourceFromAssetBundleAsync(handle.AssetBundle, m_loadResourceName, m_loadResourceIsAsset, LoadResource_OnAssetLoaded, m_loadResourceSceneMode, m_useRequests);
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
    public Text m_uiMemoryUsed;

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

                        case UIButton.EId.ABInit:
                            m_uiButtons[i].Button.onClick.AddListener(AB_Init);
                            break;

                        case UIButton.EId.ABReset:
                            m_uiButtons[i].Button.onClick.AddListener(AB_Reset);
                            break;

                        case UIButton.EId.MemoryCollect:
                            m_uiButtons[i].Button.onClick.AddListener(Ui_MemoryCollect);
                            break;

                        case UIButton.EId.UnloadSceneCubesAB:
                            m_uiButtons[i].Button.onClick.AddListener(SceneCubes_UnloadAB);
                            break;
                    }
                }                
            }
        }

        Ui_NeedsToUpdateMemoryUsed = true;
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

    private bool Ui_NeedsToUpdateMemoryUsed { get; set; }

    private void Ui_UpdateMemoryUsed()
    {
        if (m_uiMemoryUsed != null)
        {
            string text = "Memory used=" + Memory_GetUsedSize();
            if (!string.IsNullOrEmpty(m_memorySampleName))
            {
                text += " Sample diff = " + (m_memorySampleAtEnd - m_memorySampleAtBegin);
            }

            m_uiMemoryUsed.text = text;
        }
    }

    private void Ui_MemoryCollect()
    {
        Memory_Collect();
        Ui_NeedsToUpdateMemoryUsed = true; 
    }    
    #endregion

    #region ab
    public void AB_Init()
    {
        Logger logger = new ConsoleLogger("AssetBundles");

        Memory_BeginSample("AB_INIT");
        
        // Downloadables catalog        
        string catalogAsText = GetAddressablesFileText("downloadablesCatalog", true);
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
        AssetBundlesManager.Instance.Initialize(abCatalogASJSON, downloadablesConfig, downloadablesCatalogASJSON, true, tracker, logger);

        Memory_EndSample(true);
    }

    private string GetAddressablesFileText(string fileName, bool platformDependent)
    {
#if UNITY_EDITOR
        string path = "Assets/Editor/Addressables/generated/";
        if (platformDependent)
        {
            path += UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        }

        path += fileName;
        return File.ReadAllText(fileName);
#else
        string path = "Addressables/" + fileName;                        
        TextAsset targetFile = Resources.Load<TextAsset>(path);
        return (targetFile == null) ? null : targetFile.text;
#endif
    }

    public void AB_Reset()
    {
        Memory_BeginSample("AB_RESET");
        AssetBundlesManager.Instance.Reset();
        Memory_EndSample(true);
    }
    #endregion

    #region memory
    private string m_memorySampleName;
    private long m_memorySampleAtBegin;
    private long m_memorySampleAtEnd;

    private void Memory_Collect()
    {
        System.GC.Collect();        
    }

    private long Memory_GetUsedSize()
    {
        Memory_Collect();
        return Profiler.GetMonoUsedSizeLong();
    }    

    private void Memory_BeginSample(string name)
    {
        m_memorySampleName = name;
        m_memorySampleAtBegin = Memory_GetUsedSize();
        m_memorySampleAtEnd = -1;
    }

    private void Memory_EndSample(bool logResults)
    {
        m_memorySampleAtEnd = Memory_GetUsedSize();
        if (logResults)
        {
            Ui_NeedsToUpdateMemoryUsed = true;
            //Debug.Log("Memory sample " + memory_sampleName + " diff = " + (memory_sampleAtEnd - memory_sampleAtBegin));
        }
    }
    #endregion

    #region request
    private AssetBundlesOpRequest m_request;

    private void Request_OnDone(AssetBundlesOp.EResult result, object data)
    {
        Debug.Log("Request_OnDone " + result);
    }
    #endregion

    public void Update()
    {
        AssetBundlesManager.Instance.Update();

        if (Input.GetKeyDown(KeyCode.A))
        {
            AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled = !AssetBundlesManager.Instance.IsAutomaticDownloaderEnabled;
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            //Debug.Log(Memory_GetUsedSize());            
            //m_request = AssetBundlesManager.Instance.LoadAssetBundleAndDependencies(ASSET_CUBE_AB_NAME, Request_OnDone, true);                        

            Test_Download();            
        }

        if (m_request != null)
        {
            Debug.Log("request.progress = " + m_request.progress);

            if (m_request.isDone)
                m_request = null;
        }

        if (Ui_NeedsToUpdateMemoryUsed)
        {
            Ui_UpdateMemoryUsed();
            Ui_NeedsToUpdateMemoryUsed = false;
        }
    }

    void OnApplicationQuit()
    {
        AssetBundlesManager.Instance.Reset();
    }
    
    private void Test_Download()
    {
        Download("scene_cubes");
    }    

    private bool isBundleBaseInited = false;
    private string bundleBaseDownloadingURL = "http://10.44.4.69:7888/";

    private void Download(string assetBundleName)
    {
        if (!isBundleBaseInited)
        {
            SetDevelopmentAssetBundleServer();
        }

        WWW download = null;

        if (!bundleBaseDownloadingURL.EndsWith("/"))
        {
            bundleBaseDownloadingURL += "/";
        }

        string url = bundleBaseDownloadingURL + assetBundleName;
        
        download = new WWW(url);

        while (!download.isDone) ;

        Debug.Log(assetBundleName + " downloaded with error = " + download.error);                
    }

    /// <summary>
    /// Sets base downloading URL to a local development server URL.
    /// </summary>
    public void SetDevelopmentAssetBundleServer()
    {
        TextAsset urlFile = Resources.Load("AssetBundleServerURL") as TextAsset;
        string url = (urlFile != null) ? urlFile.text.Trim() : null;
        if (url == null || url.Length == 0)
        {
            Debug.LogError("Development Server URL could not be found.");
        }
        else
        {            
            SetSourceAssetBundleURL(url);
        }
    }

    public void SetSourceAssetBundleURL(string absolutePath)
    {
        if (!absolutePath.EndsWith("/"))
        {
            absolutePath += "/";
        }

        bundleBaseDownloadingURL = absolutePath;
    }        
}
