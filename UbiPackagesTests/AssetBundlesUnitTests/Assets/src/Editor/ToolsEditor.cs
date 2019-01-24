using UnityEditor;
using UnityEngine;

// Output the build size or a failure depending on BuildPlayer.

public class BuildPlayerExample : MonoBehaviour
{
	[MenuItem("Build/Build iOS")]
	public static void MyBuild()
	{
		string[] sceneNames = null;
		EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
		if (scenes != null) 
		{			
			int count = scenes.Length;
			sceneNames = new string[count];
			for (int i = 0; i < count; i++) 
			{
				sceneNames [i] = scenes[i].path;
			}
		}

		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
		buildPlayerOptions.scenes = sceneNames;
		buildPlayerOptions.locationPathName = "iOSBuild_ABManifest";
		buildPlayerOptions.target = BuildTarget.iOS;
		buildPlayerOptions.options = BuildOptions.None;
		buildPlayerOptions.assetBundleManifestPath = "AssetBundles/iOS/iOS.manifest";

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