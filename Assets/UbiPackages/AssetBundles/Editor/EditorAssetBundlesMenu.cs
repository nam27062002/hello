using UnityEditor;
using UnityEngine;

public class EditorAssetBundlesMenu : MonoBehaviour
{
    private const string ROOT_MENU = "Tech/AssetBundles/";
    private const string MENU_BROWSER = ROOT_MENU + "Browser";
    private const string MENU_LAUNCH_LOCAL_SERVER = ROOT_MENU + "Launch Local Server";
    private const string MENU_GENERATE_ASSETS_LUT_FROM_DOWNLOADABLES_CATALOG = ROOT_MENU + "Generate AssetsLUT from Downloadables";
    private const string MENU_GENERATE_DOWNLOADABLES_CATALOG_FROM_ASSETS_LUT = ROOT_MENU + "Generate Downloadables from AssetsLUT";

    [MenuItem(MENU_BROWSER)]
    static void ShowBrowser()
    {
        AssetBundleBrowser.AssetBundleBrowserMain.ShowWindow();        
    }
        
    [MenuItem(MENU_LAUNCH_LOCAL_SERVER)]
    public static void ToggleLocalAssetBundleServer()
    {
        AssetBundles.LaunchAssetBundleServer.SetRemoteAssetsFolderName(EditorAssetBundlesManager.DOWNLOADABLES_FOLDER);
        AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServer();
    }

    
    [MenuItem(MENU_LAUNCH_LOCAL_SERVER, true)]
    public static bool ToggleLocalAssetBundleServerValidate()
    {
        return AssetBundles.LaunchAssetBundleServer.ToggleLocalAssetBundleServerValidate(MENU_LAUNCH_LOCAL_SERVER);
    }

    [MenuItem(MENU_GENERATE_ASSETS_LUT_FROM_DOWNLOADABLES_CATALOG)]
    public static void GenerateAssetsLUTFromDownloadablesCatalog()
    {
        EditorAssetBundlesManager.GenerateAssetsLUTFromDownloadablesCatalog();        
    }

    [MenuItem(MENU_GENERATE_DOWNLOADABLES_CATALOG_FROM_ASSETS_LUT)]
    public static void GenerateDownloadablesCatalogFromAssetsLUT()
    {
        EditorAssetBundlesManager.GenerateDownloadablesCatalogFromAssetsLUT();
    }
}
