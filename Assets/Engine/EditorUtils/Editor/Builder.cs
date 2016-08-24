using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Xml;

public class Builder : MonoBehaviour 
{	
	

	static string[] m_scenes  = 
	{
		"Assets/Game/Scenes/SC_Loading.unity",
		"Assets/Game/Scenes/SC_Menu.unity",
		"Assets/Game/Scenes/SC_Game.unity",

		"Assets/Game/Scenes/Levels/Art/ART_PlayTest_01.unity",
		// "Assets/Game/Scenes/Levels/Collision/CO_PlayTest_01.unity",
		"Assets/Game/Scenes/Levels/Spawners/SP_PlayTest_01.unity",

		"Assets/Game/Scenes/Levels/Spawners/SP_Spawners.unity",
		"Assets/Game/Scenes/Levels/Collision/CO_Spawners.unity",
		"Assets/Game/Scenes/Levels/Art/ART_Spawners.unity",


		"Assets/Game/Scenes/Levels/Art/ART_Medieval.unity",
		"Assets/Game/Scenes/Levels/Collision/CO_Medieval.unity",
		"Assets/Game/Scenes/Levels/Spawners/SP_Medieval.unity",

		"Assets/Game/Scenes/Levels/Art/ART_Carla.unity",
		"Assets/Game/Scenes/Levels/Collision/CO_Carla.unity",
		"Assets/Game/Scenes/Levels/Spawners/SP_Carla.unity",
		"Assets/Game/Scenes/Levels/Collision/CO_Carla_cut.unity",

	};

	const string m_bundleIdentifier = "com.ubisoft.hungrydragon.dev";
	const string m_iOSSymbols = "";

	const string m_apkName = "hd_";
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

		// Build
		string dstPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));
		dstPath = System.IO.Path.Combine(dstPath, "xcode");
		BuildPipeline.BuildPlayer( m_scenes, dstPath, BuildTarget.iOS, BuildOptions.None); 

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

		string dstPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));
		string date = System.DateTime.Now.ToString("yyyy-M-d");
		string stagePath = System.IO.Path.Combine(dstPath, m_apkName + "_" + GameSettings.internalVersion + ":" + PlayerSettings.Android.bundleVersionCode + "_" + date + ".apk");
		BuildPipeline.BuildPlayer(m_scenes, stagePath, BuildTarget.Android, BuildOptions.None);

		// Restore Player Settings
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, oldSymbols);
	}

	[MenuItem ("Build/Increase Minor Version Number")]
	private static void IncreaseMinorVersionNumber()
	{
		GameSettings.internalVersion.patch++;
		EditorUtility.SetDirty(GameSettings.instance);
		AssetDatabase.SaveAssets();
	}
	
	[MenuItem ("Build/Increase Version Codes")]
	private static void IncreaseVersionCodes()
	{
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
		StreamWriter sw = File.CreateText("outputVersion.txt");
		sw.WriteLine( GameSettings.internalVersion );
		sw.Close();
	}
}
