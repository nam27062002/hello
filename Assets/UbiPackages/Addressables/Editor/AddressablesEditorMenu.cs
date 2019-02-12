using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class is resposible for managing all related to Addressables menu stuff. This class just deals with the view, the actual stuff is done by <c>AddressablesEditorManager</c>
/// </summary>
public class AddressablesEditorMenu : MonoBehaviour
{
    private const string ADDRESSABLES_MENU = "Tech/Addressables";
    private const string ADDRESSABLES_BUILD_MENU = ADDRESSABLES_MENU + "/Build";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU = ADDRESSABLES_BUILD_MENU + "/" + "Build by Steps";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_CLEAR = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "1. Clear";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_CUSTOMIZE_EDTOR_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "2. Customize editor catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_PLAYER_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "3. Generate player catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "4. Generate Asset Bundles";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "5. Process Asset Bundles";    

    private const string ADDRESSABLES_BUILD_MENU_ALL = ADDRESSABLES_BUILD_MENU + "/" + "Build";

    private const string ADDRESSABLES_SIMULATOR_MODE = ADDRESSABLES_MENU + "/Simulator mode";

    public static AddressablesEditorManager m_manager;
    public static AddressablesEditorManager Manager
    {
        get
        {
            if (m_manager == null)
            {
                m_manager = new AddressablesEditorManager();
            }

            return m_manager;
        }

        set
        {
            m_manager = value;
        }
    }

    // 1.Clear
    // Deletes AssetBundles/<currentPlatformName> folder, Assets/Streaming_Assets/Addressables folder, Downloadables folder
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_CLEAR)]
    static void ClearBuild()
    {        
        Manager.ClearBuild();
        AssetBundlesEditorManager.Clear();
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_CLEAR);
    }    

    // 2.Customize Editor Catalog
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_CUSTOMIZE_EDTOR_CATALOG)]
    static void CustomizeEditorCatalog()
    {
        Manager.CustomizeEditorCatalog();
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_CUSTOMIZE_EDTOR_CATALOG);
    }    

    // 3.Generate Player Catalog
    // Generates addressablesCatalog.json in Assets/StreamingAssets/Addressables folder
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_PLAYER_CATALOG)]
    static void GeneratePlayerCatalog()
    {
        Manager.GeneratePlayerCatalog();
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_PLAYER_CATALOG);
    }
    
    // 4. Generate Asset Bundles
    // Generates asset bundles in AssetBundles/<currentPlatformName> folder
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES)]
    static void BuildAssetBundles()
    {
        Manager.BuildAssetBundles();
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES);
    }

    // 5. Process Asset Bundles
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES)]
    static void ProcessAssetBundles()
    {
        Manager.ProcessAssetBundles();
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES);
    }        

    [MenuItem(ADDRESSABLES_BUILD_MENU_ALL)]
    public static void Build()
    {
        Manager.Build();        
        OnDone(ADDRESSABLES_BUILD_MENU_ALL);
    }   
    
    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }    

    [MenuItem(ADDRESSABLES_SIMULATOR_MODE)]
    public static void ToggleSimulatorMode()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("This option can't be toggled when the player is running");
            return;
        }

        AddressablesManager.SimulationMode = !AddressablesManager.SimulationMode;        
    }

    [MenuItem(ADDRESSABLES_SIMULATOR_MODE, true)]
    public static bool ToggleSimulatorModeValidate()
    {        
        Menu.SetChecked(ADDRESSABLES_SIMULATOR_MODE, AddressablesManager.SimulationMode);
        return true;
    }    
}
