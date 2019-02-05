using System.IO;
using UnityEditor;
using UnityEngine;

public class AddressablesEditor : MonoBehaviour
{
    private const string STREAMING_ASSETS_ROOT_PATH = "Assets/StreamingAssets";
    private const string REMOTE_ASSETS_ROOT_PATH = "Assets/RemoteAssets";
    private static string LOCAL_ASSET_BUNDLES_PATH = FileEditorTools.PathCombine(STREAMING_ASSETS_ROOT_PATH, "AssetBundles");
    private static string REMOTE_ASSET_BUNDLES_PATH = FileEditorTools.PathCombine(REMOTE_ASSETS_ROOT_PATH, "AssetBundles");

    private const string ADDRESSABLES_MENU = "Addressables";
    private const string ADDRESSABLES_AB_MENU = ADDRESSABLES_MENU + "/" + "Asset Bundles";
    private const string ADDRESSABLES_AB_MENU_BUILD = ADDRESSABLES_AB_MENU + "/" + " Build";
    private const string ADDRESSABLES_BUILD_MENU = ADDRESSABLES_MENU + "/" + "Build";
    private const string ADDRESSABLES_BUILD_MENU_CLEAR = ADDRESSABLES_BUILD_MENU + "/" + "1.Clear";    
    private const string ADDRESSABLES_BUILD_MENU_GENERATE_CATALOG = ADDRESSABLES_BUILD_MENU + "/" + "2.Generate catalog";
    private const string ADDRESSABLES_BUILD_MENU_PREPARE = ADDRESSABLES_BUILD_MENU + "/" + "3.Generate Asset Bundles";

    private const string ADDRESSSABLES_CATALOG_FILENAME = "addressablesCatalog.json";
    private const string ADDRESSABLES_EDITOR_CATALOG_PATH = "Assets/src/Editor/editor_" + ADDRESSSABLES_CATALOG_FILENAME;

	private const string ADDRESSABLES_ENGINE_CATALOG_FOLDER_PARENT = "Assets";
	private const string ADDRESSABLES_ENGINE_CATALOG_FOLDER_NAME = "StreamingAssets";
	private static string ADDRESSABLES_ENGINE_CATALOG_FOLDER_PATH = FileEditorTools.PathCombine(ADDRESSABLES_ENGINE_CATALOG_FOLDER_PARENT, ADDRESSABLES_ENGINE_CATALOG_FOLDER_NAME);
	private static string ADDRESSABLES_ENGINE_CATALOG_PATH = ADDRESSABLES_ENGINE_CATALOG_FOLDER_PATH + "/" + ADDRESSSABLES_CATALOG_FILENAME;    

    [MenuItem(ADDRESSABLES_AB_MENU_BUILD)]
    static void BuildAssetBundles()
    {
        AssetBundlesEditorTools.BuildAssetBundles();        
    }
    
    [MenuItem(ADDRESSABLES_BUILD_MENU_PREPARE)]
    static void PrepareBuild()
    {
        Debug.Log("Preparing build...");

        // Assumes asset bundles have already been built

        ClearBuildAssets();

        AssetBundlesEditorTools.CopyAssetBundles(STREAMING_ASSETS_ROOT_PATH);
        //CopyAssetBundles(REMOTE_ASSETS_ROOT_PATH);
        
        AddressablesEditorTools.Build(ADDRESSABLES_EDITOR_CATALOG_PATH, ADDRESSABLES_ENGINE_CATALOG_PATH, AddressablesTypes.EProviderMode.AsCatalog);

        AssetDatabase.Refresh();

        Debug.Log("Build ready");
    }

    [MenuItem(ADDRESSABLES_BUILD_MENU_GENERATE_CATALOG)]
    static void GenerateCatalog()
    {
		string path = ADDRESSABLES_ENGINE_CATALOG_FOLDER_PATH;
		if (!AssetDatabase.IsValidFolder(path))
		{
			AssetDatabase.CreateFolder(ADDRESSABLES_ENGINE_CATALOG_FOLDER_PARENT, ADDRESSABLES_ENGINE_CATALOG_FOLDER_NAME);
		}

        AddressablesEditorTools.Build(ADDRESSABLES_EDITOR_CATALOG_PATH, ADDRESSABLES_ENGINE_CATALOG_PATH, AddressablesTypes.EProviderMode.AsCatalog);

		AssetDatabase.Refresh();
    }

    [MenuItem(ADDRESSABLES_BUILD_MENU_CLEAR)]
    static void ClearBuild()
    {
        ClearBuildAssets();
        AssetDatabase.Refresh();
    }

    static void ClearBuildAssets()
    {
        //DeleteFileOrDirectory(LOCAL_ASSET_BUNDLES_PATH);
        //DeleteFileOrDirectory(REMOTE_ASSET_BUNDLES_PATH);        
        FileEditorTools.DeleteFileOrDirectory(STREAMING_ASSETS_ROOT_PATH);
        //DeleteFileOrDirectory(REMOTE_ASSETS_ROOT_PATH);        
    }
}
