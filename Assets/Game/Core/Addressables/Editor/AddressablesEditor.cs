using System.IO;
using UnityEditor;
using UnityEngine;

public class AddressablesEditor : MonoBehaviour
{
    private const string ADDRESSABLES_MENU = "Addressables";
    private const string ADDRESSABLES_AB_MENU = ADDRESSABLES_MENU + "/" + "Asset Bundles";
    private const string ADDRESSABLES_AB_MENU_BUILD = ADDRESSABLES_AB_MENU + "/" + " Build";
    private const string ADDRESSABLES_BUILD_MENU = ADDRESSABLES_MENU + "/" + "Build";
    private const string ADDRESSABLES_BUILD_MENU_CLEAR = ADDRESSABLES_BUILD_MENU + "/" + "Clear";
    private const string ADDRESSABLES_BUILD_MENU_PREPARE = ADDRESSABLES_BUILD_MENU + "/" + "Prepare";

    private static string STREAMING_ASSETS_ROOT_PATH = "Assets/StreamingAssets";
    private static string REMOTE_ASSETS_ROOT_PATH = "Assets/RemoteAssets";
    private static string LOCAL_ASSET_BUNDLES_PATH = FileEditorTools.PathCombine(STREAMING_ASSETS_ROOT_PATH, "AssetBundles");
    private static string REMOTE_ASSET_BUNDLES_PATH = FileEditorTools.PathCombine(REMOTE_ASSETS_ROOT_PATH, "AssetBundles");

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

        AssetDatabase.Refresh();

        Debug.Log("Build ready");
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
