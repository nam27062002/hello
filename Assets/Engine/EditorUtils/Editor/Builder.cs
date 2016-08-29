using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class Builder : MonoBehaviour 
{	
	
	const string m_bundleIdentifier = "com.ubisoft.hungrydragon.dev";
	const string m_iOSSymbols = "";

	const string m_apkName = "hd";
	const string m_AndroidSymbols = "";

	[MenuItem ("Build/IOs")]
	static void GenerateXcode()
	{
		// Save Player Settings
		string oldBundleIdentifier = PlayerSettings.bundleIdentifier;
		string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS);

		// Generate project		
		PlayerSettings.bundleIdentifier = m_bundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, m_iOSSymbols);
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
		UpdateCaletySettings();

		// Figure out output file
		string outputDir = GetArg("outputDir");
		if(string.IsNullOrEmpty(outputDir)) {
			outputDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));	// Default output dir is the project's folder
		}
		string stagePath = System.IO.Path.Combine(outputDir, "xcode");	// Should be something like ouputDir/xcode

		// Do the build!
		BuildPipeline.BuildPlayer( GetBuildingScenes(), stagePath, BuildTarget.iOS, BuildOptions.None); 

		// Restore 
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, oldSymbols);
	}
	
	[MenuItem ("Build/Android")]
	static void GenerateAPK()
	{
		// Save Player Settings
		string oldBundleIdentifier = PlayerSettings.bundleIdentifier;
		string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android);

		// Build
		PlayerSettings.bundleIdentifier = m_bundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, m_AndroidSymbols);
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
		UpdateCaletySettings();

		// Figure out output file
		string outputDir = GetArg("outputDir");
		if(string.IsNullOrEmpty(outputDir)) {
			outputDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));	// Default output dir is the project's folder
		}
		string date = System.DateTime.Now.ToString("yyyyMMdd");
		string stagePath = System.IO.Path.Combine(outputDir, m_apkName + "_" + GameSettings.internalVersion + "_" + date + "_b" + PlayerSettings.Android.bundleVersionCode + ".apk");	// Should be something like ouputDir/hd_2.4.3_20160826_b12421.apk

		// Do the build!
		BuildPipeline.BuildPlayer(GetBuildingScenes(), stagePath, BuildTarget.Android, BuildOptions.None);

		// Restore Player Settings
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, oldSymbols);
	}

	/// <summary>
	/// Get an argument from the command line.
	/// </summary>
	/// <returns>The value of the requested argument, <c>null</c> if the requested argument was not passed through the command line.</returns>
	/// <param name="_argName">The name of the argument to be retrieved.</param>
	private static string GetArg(string _argName) {
		// From https://effectiveunity.com/articles/making-most-of-unitys-command-line.html
		var args = System.Environment.GetCommandLineArgs();
		for(int i = 0; i < args.Length; i++) {
			if(args[i] == _argName && args.Length > i + 1) {
				return args[i + 1];
			}
		}
		return null;
	}

	public static string[] GetBuildingScenes()
	{
		List<string> scenes = new List<string>();
		for( int i = 0; i< EditorBuildSettings.scenes.Length; i++)
		{
			if ( EditorBuildSettings.scenes[i].enabled )
			{
				scenes.Add(EditorBuildSettings.scenes[i].path);
			}
		}
		return scenes.ToArray();
	}

	[MenuItem ("Build/Increase Minor Version Number")]
	private static void IncreaseMinorVersionNumber()
	{
		Debug.Log("UNITY: IncreaseMinorVersionNumber()");
		GameSettings.internalVersion.patch++;
		EditorUtility.SetDirty(GameSettings.instance);
		AssetDatabase.SaveAssets();
	}
	
	[MenuItem ("Build/Increase Version Codes")]
	private static void IncreaseVersionCodes()
	{
		Debug.Log("UNITY: IncreaseVersionCode()");
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null)
		{
			settingsInstance.m_strVersionIOSCode = IncreaseVersionCode( settingsInstance.m_strVersionIOSCode );
			settingsInstance.m_strVersionAndroidGplayCode = IncreaseVersionCode( settingsInstance.m_strVersionAndroidGplayCode );
			settingsInstance.m_strVersionAndroidAmazonCode = IncreaseVersionCode( settingsInstance.m_strVersionAndroidAmazonCode );
			EditorUtility.SetDirty( settingsInstance );
			AssetDatabase.SaveAssets();
			CaletySettings.UpdatePlayerSettings( ref settingsInstance );
		}
	}

	private static string IncreaseVersionCode( string versionCode )
	{
		Debug.Log("UNITY: IncreaseVersionCode(" + versionCode + ")");
		int res;
		if (int.TryParse( versionCode, out res))
		{
			res++;
			return res.ToString();
		}
		return versionCode;
	}

	private static void UpdateCaletySettings()
	{
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null)
		{
			CaletySettings.UpdatePlayerSettings( ref settingsInstance );
		}
	}

	[MenuItem ("Build/Output Version")]
	private static void OutputVersion()
	{
		Debug.Log("UNITY: OutputVersion()");
		StreamWriter sw = File.CreateText("outputVersion.txt");
		sw.WriteLine( GameSettings.internalVersion );
		sw.Close();
	}
}
