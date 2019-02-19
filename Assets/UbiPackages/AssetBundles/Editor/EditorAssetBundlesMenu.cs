using UnityEditor;
using UnityEngine;

public class EditorAssetBundlesMenu : MonoBehaviour
{
    private const string ROOT_MENU = "Tech/AssetBundles/";
    private const string MENU_BROWSER = ROOT_MENU + "Browser";
    private const string MENU_LAUNCH_LOCAL_SERVER = ROOT_MENU + "Launch Local Server";
    
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
}
