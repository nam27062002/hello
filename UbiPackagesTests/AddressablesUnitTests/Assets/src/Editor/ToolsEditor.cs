using UnityEditor;
using UnityEngine;

// Output the build size or a failure depending on BuildPlayer.

public class BuildPlayerExample : MonoBehaviour
{
    [MenuItem("Build/Build")]
    public static void Build()
    {
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

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = sceneNames;
        buildPlayerOptions.locationPathName = path;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        buildPlayerOptions.assetBundleManifestPath = "AssetBundles/" + buildTargetAsString + "/" + buildTargetAsString + ".manifest";

        string result = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log("Build DONE with result " + result);
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
}