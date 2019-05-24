using UnityEditor;
using UnityEngine;

// Output the build size or a failure depending on BuildPlayer.

public class EditorBuildMenu : MonoBehaviour
{
    private const string BUILD_MENU = "Tech/Build";
    private const string BUILD_MENU_BUILD_PLAYER = BUILD_MENU + "/Build Player";
    private const string BUILD_MENU_BUILD_ADDRESSABLES_AND_PLAYER = BUILD_MENU + "/Build Addressables and Player";
    
    private static void InternalBuildPlayer()
    {
		Debug.Log("Building player...");
        string[] sceneNames = null;
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        if (scenes != null)
        {
            int count = scenes.Length;
            sceneNames = new string[count];
            for (int i = 0; i < count; i++)
            {
                sceneNames[i] = scenes[i].path;
            }
        }

        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
        string buildTargetAsString = buildTarget.ToString();
        string path = EditorUtility.SaveFilePanel("Build " + buildTargetAsString, "Builds", "", "");

        if (buildTarget == BuildTarget.Android)
        {
            string extension = ".apk";
            if (!path.EndsWith(extension))
            {
                path += extension;
            }
        }

        /*
        PlayerSettings.Android.keystoreName = "AndroidKeys/releaseKey.keystore";
        PlayerSettings.Android.keystorePass = "android";
        PlayerSettings.Android.keyaliasName = "androidreleasekey";
        PlayerSettings.Android.keyaliasPass = "android";
        */

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = sceneNames;
        buildPlayerOptions.locationPathName = path;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        buildPlayerOptions.assetBundleManifestPath = "AssetBundles/" + buildTargetAsString + "/" + buildTargetAsString + ".manifest";

        BuildPipeline.BuildPlayer(buildPlayerOptions);        
        /*
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded)
		{
			Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
		}

		if (summary.result == BuildResult.Failed)
		{
			Debug.Log("Build failed");
		}
		*/        
    }

    [MenuItem(BUILD_MENU_BUILD_PLAYER)]
    private static void BuildPlayer()
    {
		InternalBuildPlayer();
        AssetDatabase.Refresh();
        OnDone(BUILD_MENU_BUILD_PLAYER);
    }

    [MenuItem(BUILD_MENU_BUILD_ADDRESSABLES_AND_PLAYER)]
    public static void BuildAddressablesAndPlayer()
    {
		Debug.Log("Building addressables...");
        EditorAddressablesMenu.BuildForTargetPlatform();
        InternalBuildPlayer();
        OnDone(BUILD_MENU_BUILD_ADDRESSABLES_AND_PLAYER);
    }

    private static void OnDone(string taskName)
    {
        AssetDatabase.Refresh();
        Debug.Log(taskName + " done.");
    }    
}

public class BuildPreProcessor : UnityEditor.Build.IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        EditorAddressablesMenu.OnPreBuild(target);        
    }

    public class BuildPostProcessor : UnityEditor.Build.IPostprocessBuild
    {
        public int callbackOrder { get { return 0; } }

        public void OnPostprocessBuild(BuildTarget target, string path)
        {
            EditorAddressablesMenu.OnPostBuild();
        }
    }
}
