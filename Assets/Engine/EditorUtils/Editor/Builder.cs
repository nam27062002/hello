using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
		string outputDir = GetArg("-outputDir");
		if(string.IsNullOrEmpty(outputDir)) {
			outputDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));	// Default output dir is the project's folder
		}
		string stagePath = System.IO.Path.Combine(outputDir, "xcode");	// Should be something like ouputDir/xcode

		// Some feedback
		UnityEngine.Debug.Log("Generating XCode project at path: " + stagePath);

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
		string outputDir = GetArg("-outputDir");
		if(string.IsNullOrEmpty(outputDir)) {
			outputDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));	// Default output dir is the project's folder
		}
		string date = System.DateTime.Now.ToString("yyyyMMdd");
		string stagePath = System.IO.Path.Combine(outputDir, m_apkName + "_" + GameSettings.internalVersion + "_" + date + "_b" + PlayerSettings.Android.bundleVersionCode + ".apk");	// Should be something like ouputDir/hd_2.4.3_20160826_b12421.apk

		// Some feedback
		UnityEngine.Debug.Log("Generating Android APK at path: " + stagePath);

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

	/// <summary>
	/// Sets the version number via console argument.
	/// </summary>
	private static void SetVersionNumber() {
		// Get new version number from parameter
		string versionString = GetArg("-version");
		if(string.IsNullOrEmpty(versionString)) {
			PrintMessage("ERROR SetVersionNumber: no parameter -version could be found");
			EditorApplication.Exit(1);	// Error!
			return;
		}

		// Parse version number
		// Must have the format X.Y.Z - regular expressions come in handy!
		bool error = true;
		Match match = Regex.Match(versionString, @"([0-9]+).([0-9]+).([0-9]+)");
		if(match.Success) {
			// We should have 4 groups: the whole match, and each individual version number. If not, something went wrong, abort
			if(match.Groups.Count == 4) {
				// Parse the value of each group as an int (already validated by the regex, so it shouldn't throw any exception)
				GameSettings.internalVersion.major = int.Parse(match.Groups[1].Value);
				GameSettings.internalVersion.minor = int.Parse(match.Groups[2].Value);
				GameSettings.internalVersion.patch = int.Parse(match.Groups[3].Value);

				// Save assets
				EditorUtility.SetDirty(GameSettings.instance);
				AssetDatabase.SaveAssets();

				// Mark as success
				error = false;
			}
		}

		// If there was any error, show feedback and exit with error
		if(error) {
			PrintMessage("ERROR SetVersionNumber: parameter -version unrecognized format" +
				"\nMust be X.Y.Z where:" +
				"\n- X: Development Stage [1..4] (1 - Preproduction, 2 - Production, 3 - Soft Launch, 4 - Worldwide Launch)" +
				"\n- Y: Sprint Number [1..N]" +
				"\n- Z: Build Number [1..N] within the sprint, increased by 1 for each new build");
			EditorApplication.Exit(1);	// Error!
		}
	}

	[MenuItem ("Build/Increase Minor Version Number")]
	private static void IncreaseMinorVersionNumber()
	{
		// Increase game settings internal version
		GameSettings.internalVersion.patch++;
		EditorUtility.SetDirty(GameSettings.instance);

		// Save assets
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
			// Make sure version numbers match Game Settings
			// Public version number displayed in the app store. Should be 1.0 at the first Soft Launch release.
			// Build code (m_strVersionIOSCode, m_strVersionAndroidGplayCode, m_strVersionAndroidAmazonCode) is increased automatically for each build, don't change it manually
			settingsInstance.m_strVersionIOS = GameSettings.publicVersioniOS;
			settingsInstance.m_strVersionAndroidGplay = GameSettings.publicVersionGGP;
			settingsInstance.m_strVersionAndroidAmazon = GameSettings.publicVersionAmazon;

			CaletySettings.UpdatePlayerSettings( ref settingsInstance );
		}
	}

	[MenuItem ("Build/Output Version")]
	private static void OutputVersion() {
		StreamWriter sw = File.CreateText("outputVersion.txt");
		sw.WriteLine( GameSettings.internalVersion );
		sw.Close();
	}

	/// <summary>
	/// Start a process.
	/// </summary>
	/// <param name="_command">The process to be started.</param>
	/// <param name="_args">Parmeters line.</param>
	private static void RunProcess(string _command, string _args = "") {
		// Setup start info
		Process process = new Process();
		process.StartInfo.FileName = _command;
		process.StartInfo.Arguments = _args;

		process.StartInfo.RedirectStandardError = false;
		process.StartInfo.RedirectStandardOutput = false;
		process.StartInfo.RedirectStandardInput = false;
		process.StartInfo.UseShellExecute = true;

		// Run the process and wait until it finishes
		process.Start();
		process.WaitForExit();
	}

	/// <summary>
	/// Print a message to the output terminal. Attach BUILDER prefix to make it 
	/// easy to filter among Unity's default messages.
	/// Filtering can be done by attaching the following command after the Unity instruction:
	/// <c>| grep BUILDER</c>
	/// To be able to see the output in when launching in batch mode, the -logfile 
	/// parameter without any value must be used.
	/// </summary>
	private static void PrintMessage(string _msg) {
		// Add prefix before each line so it can be parsed using grep
		string prefix = "BUILDER> ";
		_msg = _msg.Replace("\n", "\n" + prefix);
		UnityEngine.Debug.Log(prefix + _msg);
	}
}
