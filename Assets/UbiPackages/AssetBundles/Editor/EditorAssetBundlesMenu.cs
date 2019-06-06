using UnityEditor;
using UnityEngine;

public class EditorAssetBundlesMenu : MonoBehaviour
{
    private const string ROOT_MENU = "Tech/AssetBundles/";
    private const string MENU_BROWSER = ROOT_MENU + "Browser";
    private const string MENU_LAUNCH_LOCAL_SERVER = ROOT_MENU + "Launch Local Server";
    private const string MENU_BUILD_ASSET_BUNDLES = ROOT_MENU + "Build Asset Bundles";
    private const string MENU_GENERATE_ASSETS_LUT_FROM_DOWNLOADABLES_CATALOG = ROOT_MENU + "Generate AssetsLUT from Downloadables";
    private const string MENU_GENERATE_DOWNLOADABLES_CATALOG_FROM_ASSETS_LUT = ROOT_MENU + "Generate Downloadables from AssetsLUT";
    private const string MENU_CLEAR_DOWNLOADABLES_CACHE = ROOT_MENU + "Clear Downloadables Cache";



    [MenuItem(MENU_BROWSER, false, 1)]
    static void ShowBrowser()
    {
        AssetBundleBrowser.AssetBundleBrowserMain.ShowWindow();        
    }
        
    [MenuItem(MENU_LAUNCH_LOCAL_SERVER, false, 2)]
    public static void ToggleLocalAssetBundleServer()
    {
        AssetBundles.LaunchAssetBundleServer.SetRemoteAssetsFolderName(EditorAssetBundlesManager.DOWNLOADABLES_FOLDER);
        AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServer();
    }

    
    [MenuItem(MENU_LAUNCH_LOCAL_SERVER, true, 3)]
    public static bool ToggleLocalAssetBundleServerValidate()
    {
        return AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServerValidate(MENU_LAUNCH_LOCAL_SERVER);
    }
    
    [MenuItem(MENU_BUILD_ASSET_BUNDLES, false, 4)]
    public static void BuildAssetBundles()
    {
        EditorAssetBundlesManager.BuildAssetBundles(EditorUserBuildSettings.activeBuildTarget);
        OnDone(MENU_BUILD_ASSET_BUNDLES);
    }

    [MenuItem(MENU_GENERATE_ASSETS_LUT_FROM_DOWNLOADABLES_CATALOG, false, 4)]
    public static void GenerateAssetsLUTFromDownloadablesCatalog()
    {
        EditorAssetBundlesManager.GenerateAssetsLUTFromDownloadablesCatalog();        
    }

    [MenuItem(MENU_GENERATE_DOWNLOADABLES_CATALOG_FROM_ASSETS_LUT, false, 5)]
    public static void GenerateDownloadablesCatalogFromAssetsLUT() {
        EditorAssetBundlesManager.GenerateDownloadablesCatalogFromAssetsLUT();
    }

    [MenuItem(MENU_CLEAR_DOWNLOADABLES_CACHE)]
    static void ClearDownloadablesCache()
    {
        EditorAssetBundlesManager.ClearDownloadablesCache();
    }

    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }
}
