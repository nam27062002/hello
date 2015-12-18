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
		"Assets/Resources/Game/Levels/SC_Level_Test.unity",
		"Assets/Resources/Game/Levels/SC_Level_Empty.unity",
		"Assets/Resources/Game/Levels/SC_Level_HSEexp.unity"
	};

	const string m_bundleIdentifier = "com.ubisoft.hungrydragon";
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
		// PlayerSettings.bundleVersion = GameSettings.iOSVersion.ToString();
		PlayerSettings.iOS.buildNumber = GameSettings.internalVersion.ToString();

		// Build
		string dstPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));
		dstPath = System.IO.Path.Combine(dstPath, "xcode");
		BuildPipeline.BuildPlayer( m_scenes, dstPath, BuildTarget.iOS, BuildOptions.None); 

		// Restore 
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS, oldSymbols);
		// PlayerSettings.bundleVersion = oldVersion;
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
		// PlayerSettings.bundleVersion = GameSettings.androidVersion.ToString();
		PlayerSettings.Android.bundleVersionCode = GameSettings.androidVersionCode;

		string dstPath = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));
		string stagePath = System.IO.Path.Combine(dstPath, m_apkName + "_" + GameSettings.internalVersion.ToString() + ":" + GameSettings.androidVersionCode.ToString() + "aaaammdd" + ".apk");
		BuildPipeline.BuildPlayer(m_scenes, stagePath, BuildTarget.Android, BuildOptions.None);

		// Restore Player Settings
		PlayerSettings.bundleIdentifier = oldBundleIdentifier;
		PlayerSettings.SetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android, oldSymbols);
		// PlayerSettings.bundleVersion = oldVersion;
		PlayerSettings.Android.bundleVersionCode = oldVersionCode;
	}

	[MenuItem ("Build/Increase Internal Version Number")]
	private static void IncreaseInternalVersionNumber()
	{
		GameSettings.internalVersion.patch++;
		EditorUtility.SetDirty( GameSettings.instance);
		AssetDatabase.SaveAssets();
	}
	
	[MenuItem ("Build/Increase Android Version Code")]
	private static void IncreaseAndroidVersionCode()
	{
		GameSettings.androidVersionCode++;
		EditorUtility.SetDirty( GameSettings.instance);
		AssetDatabase.SaveAssets();
	}

	[MenuItem ("Build/Output Version")]
	private static void OutputVersion()
	{
		StreamWriter sw = File.CreateText("outputVersion.txt");
		sw.WriteLine( GameSettings.internalVersion.ToString() );
		sw.Close();
	}
}
