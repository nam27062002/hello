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
		"Assets/Game/Scenes/Levels/Collision/CO_PlayTest_01.unity",
		"Assets/Game/Scenes/Levels/Spawners/SP_PlayTest_01.unity",

		"Assets/Game/Scenes/Levels/Art/ART_Medieval.unity",
		"Assets/Game/Scenes/Levels/Collision/CO_Medieval.unity",
		"Assets/Game/Scenes/Levels/Spawners/SP_Medieval.unity",

		"Assets/Game/Scenes/Levels/Art/ART_HS_Map.unity",
		"Assets/Game/Scenes/Levels/Collision/CO_HS_Map.unity",
		"Assets/Game/Scenes/Levels/Spawners/SP_HS_Map.unity",

		"Assets/Tools/LevelEditor/SC_LevelEditor.unity",
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
		string oldVersion = PlayerSettings.bundleVersion;
		string oldBuildNumber = PlayerSettings.iOS.buildNumber;

		// Generate project		
		PlayerSettings.bundleIdentifier = m_bundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, m_iOSSymbols);
		PlayerSettings.iOS.buildNumber = GameSettings.internalVersion;

		// Build
		string dstPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));
		dstPath = System.IO.Path.Combine(dstPath, "xcode");
		BuildPipeline.BuildPlayer( m_scenes, dstPath, BuildTarget.iOS, BuildOptions.None); 

		// Restore 
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, oldSymbols);
		PlayerSettings.iOS.buildNumber = oldBuildNumber;
	}
	
	[MenuItem ("Build/Android")]
	static void GenerateAPK()
	{
		// Save Player Settings
		string oldBundleIdentifier = PlayerSettings.bundleIdentifier;
		string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android);
		string oldVersion = PlayerSettings.bundleVersion;
		int oldVersionCode = PlayerSettings.Android.bundleVersionCode;

		// Build
		PlayerSettings.bundleIdentifier = m_bundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, m_AndroidSymbols);
		PlayerSettings.Android.bundleVersionCode = GameSettings.androidVersionCode;

		string dstPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));
		string date = System.DateTime.Now.ToString("yyyy-M-d");
		string stagePath = System.IO.Path.Combine(dstPath, m_apkName + "_" + GameSettings.internalVersion + ":" + GameSettings.androidVersionCode.ToString() + "_" + date + ".apk");
		BuildPipeline.BuildPlayer(m_scenes, stagePath, BuildTarget.Android, BuildOptions.None);

		// Restore Player Settings
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, oldSymbols);
		PlayerSettings.Android.bundleVersionCode = oldVersionCode;
	}

	[MenuItem ("Build/Increase Internal Version Number")]
	private static void IncreaseInternalVersionNumber()
	{
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null)
		{
			settingsInstance.IncreaseAllMinorVersionNumber();
			CaletySettings.UpdatePlayerSettings( ref settingsInstance );
		}
	}
	
	[MenuItem ("Build/Increase Android Version Code")]
	private static void IncreaseAndroidVersionCode()
	{
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null)
		{
			int value = int.Parse(settingsInstance.m_strVersionAndroidGplayCode);
			value++;
			settingsInstance.m_strVersionAndroidGplayCode = value.ToString();
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
