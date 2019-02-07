using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class BasicAddressablesTestController : MonoBehaviour
{
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
        string addressablesPath = Path.Combine(Application.streamingAssetsPath, "Addressables");
        string assetBundlesPath = Path.Combine(addressablesPath, "AssetBundles");
        string addressablesCatalogPath = Path.Combine(addressablesPath, "addressablesCatalog.json");

        string catalogAsText;
        if (Application.platform == RuntimePlatform.Android) //Need to extract file from apk first
        {
            WWW reader = new WWW(addressablesCatalogPath);
            while (!reader.isDone) { }

            catalogAsText = reader.text;
        }
        else
        {
            catalogAsText = File.ReadAllText(addressablesCatalogPath);
        }
        
        JSONNode catalogASJSON = JSON.Parse(catalogAsText);        
        m_addressablesManager.Initialize(catalogASJSON, assetBundlesPath, logger);        
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

            ELoadResourceMode mode = GetLoadResourceModeFromDropdown(m_uiSceneCubeDropdown);
            AddressablesOp op;
            switch (mode)
            {
                case ELoadResourceMode.Sync:
                    op = m_addressablesManager.LoadDependenciesAsync(SCENE_CUBES_SCENE_NAME);
                    op.OnDone = Scene_OnDependenciesLoaded;
                    break;

                case ELoadResourceMode.Async:
                    op = m_addressablesManager.LoadSceneAsync(SCENE_CUBES_SCENE_NAME, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    op.OnDone = SceneCubes_OnDoneByOp;
                    break;
            }                                    
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

    #region ui
    public List<UIButton> m_uiButtons;    
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
                    }
                }
            }
        }
        
        Ui_SetEnabled(true);
        Ui_SetOperationResultEmpty();        
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
