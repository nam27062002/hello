using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// This class is resposible for managing all related to Addressables menu stuff. This class just deals with the view, the actual stuff is done by <c>AddressablesEditorManager</c>
/// </summary>
public class AddressablesEditorMenu : MonoBehaviour
{    
    private const string ADDRESSABLES_MENU = "Addressables";    
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU = ADDRESSABLES_MENU + "/" + "Build by Steps";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_CLEAR = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "1. Clear";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_CUSTOMIZE_EDTOR_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "2. Customize editor catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_PLAYER_CATALOG = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "3. Generate player catalog";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "4. Generate Asset Bundles";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "5. Process Asset Bundles";
    private const string ADDRESSABLES_BUILD_BY_STEPS_MENU_DISTRIBUTE_ASSET_BUNDLES = ADDRESSABLES_BUILD_BY_STEPS_MENU + "/" + "6. Distribute Asset Bundles";

    private const string ADDRESSABLES_BUILD_MENU = ADDRESSABLES_MENU + "/" + "Build";
    
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
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_CLEAR)]
    static void ClearBuild()
    {
        Manager.ClearBuild();
        AssetDatabase.Refresh();
    }    

    // 2.Customize Editor Catalog
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_CUSTOMIZE_EDTOR_CATALOG)]
    static void CustomizeEditorCatalog()
    {
        Manager.CustomizeEditorCatalog();
        AssetDatabase.Refresh();
    }    

    // 3.Generate Player Catalog
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_PLAYER_CATALOG)]
    static void GeneratePlayerCatalog()
    {
        Manager.GeneratePlayerCatalog();
        AssetDatabase.Refresh();
    }
    
    // 4. Generate Asset Bundles
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_GENERATE_ASSET_BUNDLES)]
    static void BuildAssetBundles()
    {
        Manager.BuildAssetBundles();
        AssetDatabase.Refresh();
    }

    // 5. Process Asset Bundles
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_PROCESS_ASSET_BUNDLES)]
    static void ProcessAssetBundles()
    {
        Manager.ProcessAssetBundles();
        AssetDatabase.Refresh();
    }    

    // 6. Distribute Asset Bundles
    [MenuItem(ADDRESSABLES_BUILD_BY_STEPS_MENU_DISTRIBUTE_ASSET_BUNDLES)]
    static void DistributeAssetBundles()
    {
        Manager.DistributeAssetBundles();
        AssetDatabase.Refresh();                        
    }    

    [MenuItem(ADDRESSABLES_BUILD_MENU)]
    public static void Build()
    {
        Manager.Build();
        AssetDatabase.Refresh();
    }    
}
