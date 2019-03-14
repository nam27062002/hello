using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class is resposible for managing all related to Addressables menu stuff. This class just deals with the view, the actual stuff is done by <c>EditorAddressablesManager</c>
/// </summary>
public class EditorAddressablesMenu : MonoBehaviour
{
    private const string ADDRESSABLES_MENU = "Tech/Addressables";
    private const string ADDRESSABLES_BUILD_MENU = ADDRESSABLES_MENU + "/Build";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU = ADDRESSABLES_BUILD_MENU + "/" + "Build by Steps";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_CLEAR = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "1. Clear";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_CUSTOMIZE_EDTOR_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "2. Customize editor catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_PLAYER_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "3. Generate player catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "4. Generate Asset Bundles";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "5. Generate Asset Bundles Catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "6. Process Asset Bundles";    

    private const string ADDRESSABLES_BUILD_FOR_TARGET_PLATFORM = ADDRESSABLES_BUILD_MENU + "/" + "Build for target platform";
    private const string ADDRESSABLES_BUILD_FOR_BOTH_PLATFORMS = ADDRESSABLES_BUILD_MENU + "/" + "Build for iOS and Android";

    private const string ADDRESSABLES_COPY_LOCAL_ASSET_BUNDLES_TO_PLAYER = ADDRESSABLES_MENU + "/" + "Copy Local Asset Bundles To Player";

    private const string ADDRESSABLES_EDITOR_MODE = ADDRESSABLES_MENU + "/Editor mode";

    public static EditorAddressablesManager m_manager;
    public static EditorAddressablesManager Manager
    {
        get
        {
            if (m_manager == null)
            {
                object o = Activator.CreateInstance(Type.GetType("HDEditorAddressablesManager"));
                if (o != null && o is EditorAddressablesManager)
                {
                    m_manager = (EditorAddressablesManager)o;
                }
                else
                {
                    m_manager = new EditorAddressablesManager();
                }                
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
        //AddressablesCatalog editorCatalog = EditorAddressablesManager.GetCatalog(EditorAddressablesManager.ADDRESSABLES_EDITOR_CATALOG_PATH, true);

        Manager.ClearBuild(EditorUserBuildSettings.activeBuildTarget);
        EditorAssetBundlesManager.Clear();
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
    // Generates addressablesCatalog.json in Assets/Resources/Addressables folder
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
        Manager.BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES);
    }

    // 4. Generate Asset Bundles
    // Generates asset bundles manager in Assets/Resources/Addressables/AssetBundles folder
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES_CATALOG)]
    static void GenerateAssetBundlesCatalog()
    {
        Manager.GenerateAssetBundlesCatalog();
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES_CATALOG);
    }    

    // 5. Process Asset Bundles
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES)]
    static void ProcessAssetBundles()
    {
        Manager.ProcessAssetBundles(EditorUserBuildSettings.activeBuildTarget, true);
        OnDone(ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES);
    }        

    [MenuItem(ADDRESSABLES_BUILD_FOR_TARGET_PLATFORM)]
    public static void BuildForTargetPlatform()
    {
        Manager.BuildForTargetPlatform();        
        OnDone(ADDRESSABLES_BUILD_FOR_TARGET_PLATFORM);
    }

    [MenuItem(ADDRESSABLES_BUILD_FOR_BOTH_PLATFORMS)]
    public static void Build()
    {
        Manager.BuildForBothPlatforms();
        OnDone(ADDRESSABLES_BUILD_FOR_BOTH_PLATFORMS);
    }
    
    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }    

    [MenuItem(ADDRESSABLES_EDITOR_MODE)]
    public static void ToggleEditorMode()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("This option can't be toggled when the player is running");
            return;
        }

        AddressablesManager.EditorMode = !AddressablesManager.EditorMode;    
        
        // If no editor mode is enabled then local asset bundles need to be loaded to the player folder so they can be used
        if (AddressablesManager.EditorMode)
        {
            DeleteLocalAssetBundlesInPlayerDestination();
        }
        else
        {
            CopyLocalAssetBundlesToPlayerDestination(EditorUserBuildSettings.activeBuildTarget);
        }
    }

    [MenuItem(ADDRESSABLES_EDITOR_MODE, true)]
    public static bool ToggleEditorModeValidate()
    {        
        Menu.SetChecked(ADDRESSABLES_EDITOR_MODE, AddressablesManager.EditorMode);
        return true;
    }    
     
    public static void CopyLocalAssetBundlesToPlayerDestination(BuildTarget target)
    {                
        Manager.CopyLocalAssetBundlesToPlayerDestination(target);
    }

    public static void DeleteLocalAssetBundlesInPlayerDestination()
    {
        Manager.DeleteLocalAssetBundlesInPlayerDestination();
    }
}
