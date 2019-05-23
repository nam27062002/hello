using System;
using System.IO;
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
    private const string ADDRESSABLES_COPY_LOCAL_ASSET_BUNDLES_TO_PLAYER = ADDRESSABLES_MENU + "/" + "Copy Local Asset Bundles To Player";

    private const string ADDRESSABLES_MODE = ADDRESSABLES_MENU + "/Mode";
    private const string ADDRESSABLES_EDITOR_MODE = ADDRESSABLES_MODE + "/Editor";
    private const string ADDRESSABLES_CATALOG_MODE = ADDRESSABLES_MODE + "/Catalog";
    private const string ADDRESSABLES_ALL_IN_LOCAL_AB_MODE = ADDRESSABLES_MODE + "/All In Local Asset Bundles";
    private const string ADDRESSABLES_ALL_IN_RESOURCES_MODE = ADDRESSABLES_MODE + "/All In Resources";

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
    public static void ProcessAssetBundles()
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
    
    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }    

    public static void SetMode(AddressablesManager.EMode value)
    {
        if (Application.isPlaying)
        {
            Debug.LogError("This option can't be toggled when the player is running");
            return;
        }

        // Generated assets need to be moved to their original location again
        Manager.MoveGeneratedResourcesToOriginalUbication();        

        AddressablesManager.Mode = value;

        if (AddressablesManager.Mode_NeedsAssetBundles())
        {
            Manager.CopyLocalAndRemoteAssetBundlesToSource(EditorUserBuildSettings.activeBuildTarget);
        }

        Manager.GeneratePlayerCatalog();
        Manager.GenerateAssetBundlesCatalog();        

        if (AddressablesManager.Mode == AddressablesManager.EMode.Editor || AddressablesManager.Mode == AddressablesManager.EMode.AllInResources)
        {
            DeleteLocalAssetBundlesInPlayerDestination();
        }
        else if (AddressablesManager.Mode_NeedsAssetBundles())
        {         
            Manager.ProcessAssetBundles(EditorUserBuildSettings.activeBuildTarget, true);
        }

		AssetDatabase.Refresh ();
    }

    [MenuItem(ADDRESSABLES_EDITOR_MODE)]
    public static void Mode_SetEditor()
    {
        SetMode(AddressablesManager.EMode.Editor);        
    }

    [MenuItem(ADDRESSABLES_EDITOR_MODE, true)]
    public static bool Mode_SetEditorValidate()
    {
        Menu.SetChecked(ADDRESSABLES_EDITOR_MODE, AddressablesManager.Mode == AddressablesManager.EMode.Editor);
        return true;
    }

    [MenuItem(ADDRESSABLES_CATALOG_MODE)]
    public static void Mode_SetCatalog()
    {
        SetMode(AddressablesManager.EMode.Catalog);        
    }

    [MenuItem(ADDRESSABLES_CATALOG_MODE, true)]
    public static bool Mode_SetCatalogValidate()
    {
        Menu.SetChecked(ADDRESSABLES_CATALOG_MODE, AddressablesManager.Mode == AddressablesManager.EMode.Catalog);
        return true;
    }

    [MenuItem(ADDRESSABLES_ALL_IN_LOCAL_AB_MODE)]
    public static void Mode_SetAllInLocalAB()
    {
        SetMode(AddressablesManager.EMode.AllInLocalAssetBundles);        
    }

    [MenuItem(ADDRESSABLES_ALL_IN_LOCAL_AB_MODE, true)]
    public static bool Mode_SetAllInLocalABValidate()
    {
        Menu.SetChecked(ADDRESSABLES_ALL_IN_LOCAL_AB_MODE, AddressablesManager.Mode == AddressablesManager.EMode.AllInLocalAssetBundles);
        return true;
    }

    [MenuItem(ADDRESSABLES_ALL_IN_RESOURCES_MODE)]
    public static void Mode_SetAllInResources()
    {
        SetMode(AddressablesManager.EMode.AllInResources);
    }

    [MenuItem(ADDRESSABLES_ALL_IN_RESOURCES_MODE, true)]
    public static bool Mode_SetAllInResourcesValidate()
    {
        Menu.SetChecked(ADDRESSABLES_ALL_IN_RESOURCES_MODE, AddressablesManager.Mode == AddressablesManager.EMode.AllInResources);
        return true;
    }

    private static AddressablesManager.EMode sm_modePreBuild;

    public static void OnPreBuild(BuildTarget target)
    {
        // Copy the platform assetsLUT to Resources
		if (Downloadables.Manager.USE_CRC_IN_URL) 
		{
			CopyPlatformAssetsLUTToResources (target);
		}

        sm_modePreBuild = AddressablesManager.Mode;

		Debug.Log("OnPrebuild Mode " + AddressablesManager.EffectiveMode);
		if (AddressablesManager.EffectiveMode != AddressablesManager.EMode.AllInResources)
		{
        	SetMode(AddressablesManager.EffectiveMode);     
		}
    }

    public static void CopyPlatformAssetsLUTToResources(BuildTarget target)
    {
        string directoryInResources = "Assets/Resources/AssetsLUT/";
        string assetsLUTInResources = directoryInResources + "assetsLUT.json";
        if (Directory.Exists(directoryInResources))
        {
            if (File.Exists(assetsLUTInResources))
            {
                File.Delete(assetsLUTInResources);
            }
        }
        else
        {
            // Makes sure that the destination folder exists        
            Directory.CreateDirectory(directoryInResources);
        }

        string assetsLUTSource = "AssetsLUT/assetsLUT_";
        switch (target)
        {
            case BuildTarget.Android:
                assetsLUTSource += "Android";
                break;

            case BuildTarget.iOS:
                assetsLUTSource += "iOS";
                break;
        }

        assetsLUTSource += ".json";

        if (File.Exists(assetsLUTSource))
        {
            File.Copy(assetsLUTSource, assetsLUTInResources);
        }
        else
        {
            Debug.LogWarning("No assetsLUT found for platform " + target.ToString() + ": " + assetsLUTSource);
        }

        AssetDatabase.Refresh();
    }

    public static void OnPostBuild()
    {
        if (AddressablesManager.Mode != sm_modePreBuild)
        {
            SetMode(sm_modePreBuild);
        }                        
    }

    public static void DeleteLocalAssetBundlesInPlayerDestination()
    {
        Manager.DeleteLocalAssetBundlesInPlayerDestination();
    }


}
