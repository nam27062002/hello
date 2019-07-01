using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class Builder : MonoBehaviour, UnityEditor.Build.IPreprocessBuild
{
	const string m_bundleIdentifier = "com.ubisoft.hungrydragon.dev";
	const string m_iOSSymbols = "";

	// const string m_apkName = "hd";
	const string m_AndroidSymbols = "";

    /// <summary>
    /// When <c>true</c> the symbols defined by player settings are overriden by the ones that defined in <c>m_iOSSymbols</c> for iOs and <c>m_AndroidSymbols</c> for Android
    /// </summary>
    const bool OVERRIDE_SYMBOLS = false;

	public int callbackOrder { get { return 0; } }
	public void OnPreprocessBuild(BuildTarget target, string path) {
		Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + target + " at path " + path);
		if (target == BuildTarget.iOS) {
			// We make sure bundleVersiont is set to game settings version so xcode files generated manually have the right version number
			PlayerSettings.bundleVersion = GameSettings.internalVersion.ToString();
		}
	}

	static void PrepareAddressablesMode()
	{
		AddressablesManager.Mode = GetAddressablesMode();

		// SetMode() shouldn't be called again because it was called in a previous step so the editor will have time to leave the assets as they need to be
		EditorAddressablesMenu.NeedsToSetModeOnPreBuild = false;
	}

	//[MenuItem ("Build/IOs")]
	static void GenerateXcode()
	{
		PrepareAddressablesMode();

		// Save Player Settings
		string oldBundleIdentifier = PlayerSettings.applicationIdentifier;
		string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.iOS);

		// Generate project
		PlayerSettings.applicationIdentifier = m_bundleIdentifier;
        if (OVERRIDE_SYMBOLS)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, m_iOSSymbols);
        }

		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
		UpdateCaletySettings();
		string oldBundleVersion = PlayerSettings.bundleVersion;
		PlayerSettings.bundleVersion = GameSettings.internalVersion.ToString();

		// Figure out output file
		string outputDir = GetArg("-outputDir");
		if(string.IsNullOrEmpty(outputDir)) {
			outputDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));	// Default output dir is the project's folder
		}
		string stagePath = System.IO.Path.Combine(outputDir, "xcode");	// Should be something like ouputDir/xcode

		// Some feedback
		UnityEngine.Debug.Log("Generating XCode project at path: " + stagePath);

		// Do the build!
		BuildPlayer( stagePath, BuildTarget.iOS);

		// Restore
		PlayerSettings.applicationIdentifier = oldBundleIdentifier;
        if (OVERRIDE_SYMBOLS)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, oldSymbols);
        }
		PlayerSettings.bundleVersion = oldBundleVersion;
	}

    private static void BuildPlayer(string locationPathName, BuildTarget buildTarget)
    {
        string[] levels = GetBuildingScenes();
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels;
        buildPlayerOptions.locationPathName = locationPathName;
        buildPlayerOptions.target = buildTarget;
        buildPlayerOptions.options = BuildOptions.None;

        string buildTargetAsString = buildTarget.ToString();
        string assetBundlesManifestPath = "AssetBundles/" + buildTargetAsString + "/" + buildTargetAsString + ".manifest";
        if (File.Exists(assetBundlesManifestPath))
        {
            buildPlayerOptions.assetBundleManifestPath = assetBundlesManifestPath;
        }

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
		
	//[MenuItem ("Build/Android")]
	static void GenerateAPK()
	{		
		PrepareAddressablesMode();

		// Save Player Settings
		string oldBundleIdentifier = PlayerSettings.applicationIdentifier;
		string oldSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup( BuildTargetGroup.Android);

		// Build
		PlayerSettings.applicationIdentifier = m_bundleIdentifier;
        if (OVERRIDE_SYMBOLS)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, m_AndroidSymbols);
        }

		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
		UpdateCaletySettings();

		// Figure out output file
		string outputDir = GetArg("-outputDir");
		if(string.IsNullOrEmpty(outputDir)) {
			outputDir = Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"));	// Default output dir is the project's folder
		}

		bool obbValue = false;
		string generateObbs = GetArg("-obb");
		if ( generateObbs != null )
		{
			bool.TryParse( generateObbs, out obbValue);
		}
		PlayerSettings.Android.useAPKExpansionFiles = obbValue;

		#if UNITY_ANDROID
        bool aabValue = false;
        string generateAAB = GetArg("-aab");
        if (generateAAB != null)
        {
            bool.TryParse(generateAAB, out aabValue);
        }
        EditorUserBuildSettings.buildAppBundle = aabValue;
		#endif

		string date = System.DateTime.Now.ToString("yyyyMMdd");
		string code = GetArg("-code");
		string finalApkName = code + "_" + GameSettings.internalVersion + "_" + date + "_b" + PlayerSettings.Android.bundleVersionCode + "_" +
			GetEnvironmentString() + "_" + AddressablesManager.ModeToKey(AddressablesManager.EffectiveMode) + (aabValue ? ".aab": ".apk");

		string stagePath = System.IO.Path.Combine(outputDir, finalApkName);	// Should be something like ouputDir/hd_2.4.3_20160826_b12421.apk

		// Some feedback
		if (aabValue) {
			UnityEngine.Debug.Log ("Generating Android AAB at path: " + stagePath);
		} else {
			UnityEngine.Debug.Log ("Generating Android APK at path: " + stagePath);
		}

		// string keyPath = Application.dataPath;
		// keyPath = keyPath.Substring( 0, keyPath.Length - "Assets".Length);
		// keyPath += "AndroidKeys/releaseKey.keystore";
		// PlayerSettings.Android.keystoreName = keyPath;
		PlayerSettings.Android.keystoreName = "AndroidKeys/releaseKey.keystore";

		PlayerSettings.Android.keystorePass = "android";
		PlayerSettings.Android.keyaliasName = "androidreleasekey";
		PlayerSettings.Android.keyaliasPass = "android";
        
        // Do the build!
        BuildPlayer(stagePath, BuildTarget.Android);

        // Restore Player Settings
        PlayerSettings.applicationIdentifier = oldBundleIdentifier;
        if (OVERRIDE_SYMBOLS)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, oldSymbols);
        }
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

	private static void PrintArgs() {		
		var args = System.Environment.GetCommandLineArgs();
		string msg = "Args: count = " + args.Length + " args: ";
		for (int i = 0; i < args.Length; i++) {
			if (i > 0)
				msg += ", ";
			
			msg += args[i];
		}

		UnityEngine.Debug.Log(msg);
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
	/// Expected args: -version version_number
	/// Used by the development team, QC, etc. to identify each build internally.
	/// Format X.Y.Z where:
	/// - X: Development Stage [1..4] (1 - Preproduction, 2 - Production, 3 - Soft Launch, 4 - Worldwide Launch)
	/// - Y: Sprint Number [1..N]
	/// - Z: Build Number [1..N] within the sprint, increased by 1 for each new build
	/// </summary>
	private static void SetInternalVersion() {
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

	//[MenuItem ("Build/Increase Minor Version Number")]
	private static void IncreaseMinorVersionNumber()
	{
		// Increase game settings internal version
		GameSettings.internalVersion.patch++;

		// Save assets
		EditorUtility.SetDirty(GameSettings.instance);
		AssetDatabase.SaveAssets();
	}

	/// <summary>
	/// Manually sets the public version in a target platform(s).
	/// Expected arguments: [-ios ios_version_name] [-ggp ggp_version_name] [-amz amz_version_name]
	/// Public version number displayed in the stores, used by the players to identify different application updates
	/// Format X.Y where:
	/// - X: Arbitrary, only changed on major game change (usually never)
	/// - 0 during Preproduction/Production
	/// - [1..N] at Soft/WW Launch
	/// - Y: Increased for every push to the app store [1..N]
	/// </summary>
	private static void SetPublicVersion() {
		// Aux vars
		bool dirty = false;

		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null){
			// Try iOS
			string iosVersion = GetArg("-ios");
			if(!string.IsNullOrEmpty(iosVersion)) {
				settingsInstance.m_strVersionIOS = iosVersion;
				dirty = true;
			}

			// Try Google Play
			string ggpVersion = GetArg("-ggp");
			if(!string.IsNullOrEmpty(ggpVersion)) {
				settingsInstance.m_strVersionAndroidGplay = ggpVersion;
				dirty = true;
			}

			// Try Amazon
			string amzVersion = GetArg("-amz");
			if(!string.IsNullOrEmpty(amzVersion)) {
				settingsInstance.m_strVersionAndroidAmazon = amzVersion;
				dirty = true;
			}

			// Save assets
			if(dirty) {
				EditorUtility.SetDirty( settingsInstance );
				AssetDatabase.SaveAssets();
			}
		}
	}

	/// <summary>
	/// Manually sets the version codes in a target platform(s).
	/// Expected arguments: [-ios ios_version_code] [-ggp ggp_version_code] [-amz amz_version_code]
	/// version code number displayed for the stores
	/// </summary>
	private static void SetVersionCode() {
		// Aux vars
		bool dirty = false;

		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null){
			// Try iOS
			string iosVersion = GetArg("-ios");
			if(!string.IsNullOrEmpty(iosVersion)) {
				settingsInstance.m_strVersionIOSCode = iosVersion;
				dirty = true;
			}

			// Try Google Play
			string ggpVersion = GetArg("-ggp");
			if(!string.IsNullOrEmpty(ggpVersion)) {
				settingsInstance.m_strVersionAndroidGplayCode = ggpVersion;
				dirty = true;
			}

			// Try Amazon
			string amzVersion = GetArg("-amz");
			if(!string.IsNullOrEmpty(amzVersion)) {
				settingsInstance.m_strVersionAndroidAmazonCode = amzVersion;
				dirty = true;
			}

			// Save assets
			if(dirty) {
				EditorUtility.SetDirty( settingsInstance );
				AssetDatabase.SaveAssets();
			}
		}
	}

	//[MenuItem ("Build/Increase Version Codes")]
	private static void IncreaseVersionCodes()
	{
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null)
		{
			settingsInstance.m_strVersionIOSCode = IncreaseVersionCode( settingsInstance.m_strVersionIOSCode );
			settingsInstance.m_strVersionAndroidGplayCode = IncreaseVersionCode( settingsInstance.m_strVersionAndroidGplayCode );
			settingsInstance.m_strVersionAndroidAmazonCode = IncreaseVersionCode( settingsInstance.m_strVersionAndroidAmazonCode );
			CaletySettings.UpdatePlayerSettings( ref settingsInstance );
			EditorUtility.SetDirty( settingsInstance );
			AssetDatabase.SaveAssets();
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
			// Public version number displayed in the app store. Should be 1.0 at the first Soft Launch release.
			CaletySettings.UpdatePlayerSettings( ref settingsInstance );
		}
	}

	//[MenuItem ("Build/Output Version")]
	private static void OutputVersion() {
		StreamWriter sw = File.CreateText("outputVersion.txt");
		sw.WriteLine( GameSettings.internalVersion );
		sw.Close();
	}

	private static void OutputAndroidBuildVersion(){
		StreamWriter sw2 = File.CreateText("androidBuildVersion.txt");
		sw2.WriteLine( PlayerSettings.Android.bundleVersionCode );
		sw2.Close();
	}

	// [MenuItem ("Build/Output Bundle Identifier")]
	private static void OutputBundleIdentifier(){
		StreamWriter sw2 = File.CreateText("bundleIdentifier.txt");
		sw2.WriteLine( PlayerSettings.GetApplicationIdentifier( EditorUserBuildSettings.selectedBuildTargetGroup ) );
		sw2.Close();
	}

	// [MenuItem ("Build/Output Environment")]
	private static void OutputEnvironment(){
		StreamWriter sw2 = File.CreateText("environment.txt");
		sw2.WriteLine( GetEnvironmentString() );
		sw2.Close();
	}

	private static string GetEnvironmentString()
	{
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		string environment = "unknown";
		if(settingsInstance != null)
		{
			switch( settingsInstance.m_iBuildEnvironmentSelected )
			{
				case (int)CaletyConstants.eBuildEnvironments.BUILD_DEV: environment = "dev"; break;
				case (int)CaletyConstants.eBuildEnvironments.BUILD_INTEGRATION: environment = "integration"; break;
				case (int)CaletyConstants.eBuildEnvironments.BUILD_LOCAL: environment = "local"; break;
				case (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION: environment = "production"; break;
				case (int)CaletyConstants.eBuildEnvironments.BUILD_STAGE: environment = "stage"; break;
				case (int)CaletyConstants.eBuildEnvironments.BUILD_STAGE_QC: environment = "stage_qc"; break;
			}
		}
		return environment;
	}

	private static void SetEnvironment()
	{
		CaletySettings settingsInstance = (CaletySettings)Resources.Load("CaletySettings");
		if(settingsInstance != null){
			CaletyModularSettings currentModularSettings = CaletyModularSettingsEditor.GetCaletyModularSettings ();
			string environment = GetArg("-env");
			CaletySettings clonedSettings = settingsInstance.Clone ();
			switch( environment )
			{
				case "dev": settingsInstance.m_iBuildEnvironmentSelected = (int)CaletyConstants.eBuildEnvironments.BUILD_DEV;break;
				case "integration": settingsInstance.m_iBuildEnvironmentSelected = (int)CaletyConstants.eBuildEnvironments.BUILD_INTEGRATION;break;
				case "local": settingsInstance.m_iBuildEnvironmentSelected = (int)CaletyConstants.eBuildEnvironments.BUILD_LOCAL;break;
				case "production": settingsInstance.m_iBuildEnvironmentSelected = (int)CaletyConstants.eBuildEnvironments.BUILD_PRODUCTION;break;
				case "stage": settingsInstance.m_iBuildEnvironmentSelected = (int)CaletyConstants.eBuildEnvironments.BUILD_STAGE;break;
				case "stage_qc": settingsInstance.m_iBuildEnvironmentSelected = (int)CaletyConstants.eBuildEnvironments.BUILD_STAGE_QC;break;
			}
			CaletySettings.UpdatePlayerSettings (ref settingsInstance);
			EditorUtility.SetDirty( settingsInstance );
			AssetDatabase.SaveAssets();
			// if (clonedSettings.CheckAndroidManifestUpdateNeeded (ref settingsInstance))
			{
				NetworkManager.DestroyInstance ();
				GameContext.DestroyInstance ();
				// Update Gradle
				CaletySettingsEditor.UpdateGradleConfig(currentModularSettings);
				// Generate Manifest
				CaletySettingsEditor.UpdateManifest( settingsInstance, currentModularSettings );
			}
		}			
	}				

	private static AddressablesManager.EMode GetAddressablesMode()
	{
		AddressablesManager.EMode returnValue = AddressablesManager.EffectiveMode;

		string addressablesModeStr = GetArg("-addressablesMode");											  
		if (!string.IsNullOrEmpty(addressablesModeStr)) 
		{						
			returnValue = AddressablesManager.KeyToMode(addressablesModeStr);
		}

		return returnValue;
	}

	private static void SetAddressablesMode()
	{
		// Platform assetsLUT file needs to be moved to Resources. We need to do it here because otherwise this file is not loaded on time 
		// when building from the script
		EditorAddressablesMenu.CopyPlatformAssetsLUTToResources(EditorUserBuildSettings.activeBuildTarget);

		EditorAddressablesMenu.SetMode(GetAddressablesMode());	
		UnityEngine.Debug.Log ("Addressables mode: " + AddressablesManager.Mode);
	}

	// This action will be used to make custom project stuff. Like generating lightmaps or splitting scenes
	public static void CustomAction()
	{
		UnityEngine.Debug.Log("Custom Action");
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


	private static readonly string ASSETS_DIR = "Assets/Game/Scenes/Levels";
	// [MenuItem ("Build/Bake Occlusion")]
	public static void BakeOcclusion()
	{
		// Load proper scenes
		ContentManager.InitContent(true);
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, "level_0");
		List<string> levels = def.GetAsList<string>("common");

		int areaIndex = 1;
		List<string> areaList = LevelManager.GetOnlyAreaScenesList("area"+areaIndex);
		while(areaList.Count > 0 && !string.IsNullOrEmpty(areaList[0]) )
		{
			for(int i = 0; i<areaList.Count; i++)
			{
				if(!string.IsNullOrEmpty( areaList[i] ))
				{
					levels.Add(areaList[i]);
				}
			}
			areaIndex++;
			areaList = LevelManager.GetOnlyAreaScenesList("area"+areaIndex);
		}

		for( int i = 0; i<levels.Count; i++ )
		{
			string path = ASSETS_DIR;
			string levelName = levels[i];
			string lower = levelName.ToLower();
			if ( lower.StartsWith("so_") )
			{
				path += "/Sound/" + levelName;
			}
			else if ( lower.StartsWith("sp_") )
			{
				path += "/Spawners/" + levelName;
			}
			else if ( lower.StartsWith("co_") )
			{
				path += "/Collision/" + levelName;
			}
			else if ( lower.StartsWith("art_") )
			{
				path += "/Art/" + levelName;
			}
			path += ".unity";
			if ( i == 0 )
			{
				EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
			}
			else
			{
				EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
			}
		}

		// Set active scene
		string _currentArea = "area1";
		Scene scene = SceneManager.GetSceneByName(def.GetAsString(_currentArea + "Active"));
		SceneManager.SetActiveScene(scene);

		// Bake Occlusion
		UnityEditor.StaticOcclusionCulling.Compute();

			// Wait bake occlusion to end
		while( UnityEditor.StaticOcclusionCulling.isRunning )
		{
			System.Threading.Thread.Sleep(1);
		}

		// Save Data ?
	}

    public static string GetLevelPath(string levelName)
    {
        string path = ASSETS_DIR;        
        string lower = levelName.ToLower();
        if (lower.StartsWith("so_"))
        {
            path += "/Sound/" + levelName;
        }
        else if (lower.StartsWith("sp_"))
        {
            path += "/Spawners/" + levelName;
        }
        else if (lower.StartsWith("co_"))
        {
            path += "/Collision/" + levelName;
        }
        else if (lower.StartsWith("art_"))
        {
            path += "/Art/" + levelName;
        }
        path += ".unity";

        return path;
    }

    [PostProcessBuild(1090)]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if ( target == BuildTarget.iOS )
        {   
            // Load Info.plist
            string file = System.IO.Path.Combine(path, "Info.plist");
            if (!File.Exists(file))
            {
                return;
            }
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(file);
            XmlNode dict = xmlDocument.SelectSingleNode("plist/dict");

            if (dict != null)
            {
                // Add hungrydragon to CFBundleURLTypes/CFBundleURLSchemes so other applications can ask if we are installed
                CaletyPostprocessor.PListItem urlTypes = CaletyPostprocessor.GetPlistItem( dict, "CFBundleURLTypes");
                XmlElement urlTypesArray = null;
                if (urlTypes != null)
                {
                    urlTypesArray = (XmlElement)urlTypes.itemValueNode;
                }
                else
                {
                    urlTypesArray = xmlDocument.CreateElement("array");
                    dict.AppendChild(urlTypesArray);
                }

                    // dict
                XmlElement keyURLTypeDict = xmlDocument.CreateElement("dict");

                    // Add key to dict
                XmlElement keyURLSchemes = xmlDocument.CreateElement("key");
                keyURLSchemes.InnerText = "CFBundleURLSchemes";
                keyURLTypeDict.AppendChild(keyURLSchemes);


                    // Array inside dict
                XmlElement keyURLSchemesArray = xmlDocument.CreateElement("array");
                        // Add hungry dragon to array
                XmlElement keyURLScheme = xmlDocument.CreateElement("string");
                keyURLScheme.InnerText = "hungrydragon";
                keyURLSchemesArray.AppendChild(keyURLScheme);
                    
                    // Add array after the key on dict
                keyURLTypeDict.AppendChild(keyURLSchemesArray);
                urlTypesArray.AppendChild(keyURLTypeDict);


                // Add hungrysharkevolution and hungrysharkworld to LSApplicationQueriesSchemes so we can ask for them
                CaletyPostprocessor.PListItem applicationQueriesSchemes = CaletyPostprocessor.GetPlistItem(dict, "LSApplicationQueriesSchemes");
                XmlElement applicationQueriesSchemesArray = null;
                if (applicationQueriesSchemes != null)
                {
                    applicationQueriesSchemesArray = (XmlElement)applicationQueriesSchemes.itemValueNode;
                }
                else
                {
                    applicationQueriesSchemesArray = xmlDocument.CreateElement("array");
                    dict.AppendChild(applicationQueriesSchemesArray);
                }

                if(applicationQueriesSchemesArray != null)
                {
                    XmlElement hungrysharkevolution = xmlDocument.CreateElement("string");
                    hungrysharkevolution.InnerText = "hungrysharkevolution";
                    applicationQueriesSchemesArray.AppendChild(hungrysharkevolution);

                    XmlElement hungrysharkworld = xmlDocument.CreateElement("string");
                    hungrysharkworld.InnerText = "hungrysharkworld";
                    applicationQueriesSchemesArray.AppendChild(hungrysharkworld);
                }


            }

            // Save file
            xmlDocument.Save(file);
            // Remove extra gargabe added by the XmlDocument save
            CaletyPostprocessor.UpdateStringInFile(file, "dtd\"[]>", "dtd\">");

        }
        else if  (target == BuildTarget.Android)
		{
			//GenerateAdaptiveAPK (path);
		}

    }

    /*[MenuItem("Hungry Dragon/Build/Generate Adaptive APK")]
    public static void GenerateAdaptiveApkFromMenu()
    {
        string path = ValidatePath(Application.dataPath + "/../HD.apk");
        GenerateAdaptiveAPK(path);
    }
    */

    public static void GenerateAdaptiveAPK(string path)
    {
        string error = null;

        UnityEngine.Debug.Log("------------------------------------------------------");
        UnityEngine.Debug.Log("Processing adaptive icons...");

        if (File.Exists(path))
        {
            string apkFileName = null;
            string[] tokens = path.Split('/');
            if (tokens.Length == 0)
            {
                tokens = path.Split('\\');
            }

            if (tokens == null || tokens.Length == 0)
            {
                error = "Error when parsing " + path + " name";
            }
            else
            {
                apkFileName = tokens[tokens.Length - 1];
            }

            if (!string.IsNullOrEmpty(apkFileName))
            {
                string rootPath = ValidatePath(Application.dataPath + "/..");
                string tempDirectoryPath = ValidatePath(rootPath + "/TempAPK");

                // Deletes the directory if already exists
                if (Directory.Exists(tempDirectoryPath))
                {
                    UnityEngine.Debug.Log("Deleting temp directory " + tempDirectoryPath + " ...");
                    try
                    {
                        Directory.Delete(tempDirectoryPath, true);
                    }
                    catch (Exception)
                    {
                        UnityEngine.Debug.Log("Error deleting directory " + tempDirectoryPath);
                    }
                }

                string decompressFolder = ValidatePath(tempDirectoryPath + "/DecompressFolder");

                // Decompress apk
                DecompressApk(rootPath, path, decompressFolder);

                // Makes sure the temp directory was created
                if (Directory.Exists(decompressFolder))
                {
                    string destPath = ValidatePath(decompressFolder + "/res");

                    // Delete old icons
                    UnityEngine.Debug.Log("Deleting old icons...");
					DeleteOldIcon(destPath, "drawable-ldpi");
					DeleteOldIcon(destPath, "drawable-mdpi");
					DeleteOldIcon(destPath, "drawable-hdpi");
					DeleteOldIcon(destPath, "drawable-xhdpi");
					DeleteOldIcon(destPath, "drawable-xxhdpi");
					DeleteOldIcon(destPath, "drawable-xxxhdpi");

                    // Copy icons
                    string sourcePath = ValidatePath(Application.dataPath + "/Game/UI/Icon/Android/AdaptiveIcons");
                    UnityEngine.Debug.Log("Copying adaptive icons from " + sourcePath + " to " + destPath + "...");
                    CopyFilesRecursively(new DirectoryInfo(sourcePath), new DirectoryInfo(destPath));

                    // Change manifest
                    UnityEngine.Debug.Log("Changing icons in Manifest ...");
                    SetMipmapIconsInManifest(decompressFolder);

                    // Compress apk again
                    string resultPath = ValidatePath(tempDirectoryPath + "/" + apkFileName);
                    UnityEngine.Debug.Log("Compressing the APK file with adaptive icons ...");
                    CompressApk(rootPath, decompressFolder, resultPath);

                    // Makes sure that the file was created
                    if (File.Exists(resultPath))
                    {
                        // Aligns the apk
                        UnityEngine.Debug.Log("Aligning the APK file with adaptive icons ...");

                        string apkFileNameNoExt = apkFileName;
                        tokens = apkFileName.Split('.');
                        if (tokens != null && tokens.Length > 0)
                        {
                            apkFileNameNoExt = tokens[0];
                        }

                        string apkAlignedFileName = apkFileNameNoExt + "_aligned" + ".apk";
                        AlignApk(tempDirectoryPath, apkFileName, apkAlignedFileName);

                        string apkAlignedPath = ValidatePath(tempDirectoryPath + "/" + apkAlignedFileName);
                        if (File.Exists(apkAlignedPath))
                        {
                            // Signs the apk
                            UnityEngine.Debug.Log("Signing the APK file with adaptive icons ...");

                            try
                            {
                                SignApk(tempDirectoryPath, apkAlignedFileName);
                            }
                            catch (Exception e)
                            {
                                error = "Error when signing: " + e.ToString();
                            }

                            if (error == null)
                            {
                                UnityEngine.Debug.Log("Coping the APK file with adaptive icons to the original location ...");

                                // Exchange the original APK for this final one
                                try
                                {
                                    File.Delete(path);
                                }
                                catch (Exception e)
                                {
                                    error = "Error when deleting the original APK: " + e.ToString();
                                }

                                if (error == null)
                                {
                                    try
                                    {
                                        File.Copy(apkAlignedPath, path);
                                    }
                                    catch (Exception e)
                                    {
                                        error = "Error when copying the final APK to the original location: " + e.ToString();
                                    }
                                }

                                // We're done so Temp directory is deleted
                                if (error == null)
                                {
                                    try
                                    {
                                        Directory.Delete(tempDirectoryPath, true);
                                    }
                                    catch (Exception e)
                                    {
                                        error = "Error when deleting the temporal directory: " + e.ToString();
                                    }
                                }
                            }
                        }
                        else
                        {
                            error = "Error when aligning the APK file";
                        }
                    }
                    else
                    {
                        error = "Error when compressing the APK file";
                    }
                }
                else
                {
                    error = "Error when decompressing the original APK file";
                }
            }
        }
        else
        {
            error = "No original APK file found at " + path + ": Unity probably failed when building the APK";
        }

        if (error == null)
        {
            UnityEngine.Debug.Log("Adaptive icons process finished SUCCESSFULLY");
        }
        else
        {
            UnityEngine.Debug.Log("Adaptive icons process FAILED: " + error);
        }

        UnityEngine.Debug.Log("------------------------------------------------------");
    }

	private static void DeleteOldIcon(string path, string folder)
	{
		try
		{
			File.Delete(ValidatePath(path + "/" + folder + "/app_icon.png"));
		}
		catch (Exception e)
		{
			UnityEngine.Debug.Log("No old icon found: " + e.ToString());
		}
	}

    private static void DecompressApk(string rootPath, string apkPath, string destPath)
    {
        ApkTool(rootPath, "d " + apkPath + " -o " + destPath);
    }

    private static void CompressApk(string rootPath, string sourcePath, string destPath)
    {
        ApkTool(rootPath, "b " + sourcePath + " -o " + destPath);
    }

    private static bool sm_apkToolUsed = false;

    private static void InitApkTool(string rootPath)
    {
        ProcessStartInfo start = new ProcessStartInfo();
		start.FileName = "chmod";
		start.Arguments = " +x " + ValidatePath(rootPath + "/Tools/apktool/apktool.*");
        //start.UseShellExecute = false;
        start.RedirectStandardOutput = false;
        using (Process process = Process.Start(start))
        {
            while (!process.HasExited)
            {
            }
        }

        sm_apkToolUsed = true;
    }

    private static void ApkTool(string rootPath, string arguments)
    {
#if UNITY_EDITOR_OSX
        if (!sm_apkToolUsed)
        {
            InitApkTool(rootPath);
        }
#endif

        ProcessStartInfo start = new ProcessStartInfo();
		start.WorkingDirectory = ValidatePath(rootPath + "/Tools/apktool/");
        string fileName;
#if UNITY_EDITOR_OSX
		fileName = "sh";
		arguments = " apktool.sh " + arguments;
#else
        fileName = ValidatePath(rootPath + "/Tools/apktool/apktool.bat");
#endif

        start.FileName = ValidatePath(fileName);
        start.Arguments = arguments;
        //start.UseShellExecute = false;
        start.RedirectStandardOutput = false;
        using (Process process = Process.Start(start))
        {
            while (!process.HasExited)
            {
            }
        }
    }

    private static string GetApkToolsPath()
    {
        return ValidatePath(EditorPrefs.GetString("AndroidSdkRoot") + "/build-tools/26.0.2");
    }

    private static void AlignApk(string workingDirectoryPath, string sourceApkName, string outputApkName)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.WorkingDirectory = workingDirectoryPath;
        start.FileName = ValidatePath(GetApkToolsPath() + "/zipalign");
        start.Arguments = "-f -v 4 " + sourceApkName + " " + outputApkName;
        //start.UseShellExecute = false;
        start.RedirectStandardOutput = false;
        using (Process process = Process.Start(start))
        {
            while (!process.HasExited)
            {
            }
        }
    }

    private static void SignApk(string workingDirectoryPath, string apkName)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.WorkingDirectory = workingDirectoryPath;
        start.FileName = ValidatePath(GetApkToolsPath() + "/apksigner");
        start.Arguments = "sign --ks " + "../AndroidKeys/releaseKey.keystore" + " -ks-key-alias androidreleasekey --ks-pass pass:android " + apkName;
        //start.UseShellExecute = false;
        start.RedirectStandardOutput = false;
        using (Process process = Process.Start(start))
        {
            while (!process.HasExited)
            {
            }
        }
    }

    private static string ValidatePath(string path)
    {
        return path;
    }

    public static void SetMipmapIconsInManifest(string workingDirectoryPath)
    {
        // Patch android manifest to remove debuggable property
        // the gradle build file will automatically turn debuggable to true/false
        // lint will throw errors on a release build if the debuggable property is hard coded.
        string manifestPath = ValidatePath(workingDirectoryPath + "/AndroidManifest.xml");
        string manifestContent = File.ReadAllText(manifestPath);
        manifestContent = manifestContent.Replace("android:icon=\"@drawable/app_icon\"", "android:icon=\"@mipmap/ic_launcher\" android_roundIcon=\"@mipmap/ic_launcher_round\"");
        File.WriteAllText(manifestPath, manifestContent);
    }

    //File Utility Functions
    public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
        {
            string newPath = ValidatePath(target.FullName + "/" + dir.Name);
            if (!Directory.Exists(newPath))
            {
                target.CreateSubdirectory(dir.Name);
            }
            CopyFilesRecursively(dir, new DirectoryInfo(newPath));
        }
        foreach (FileInfo file in source.GetFiles())
        {
            //never copy meta files
            if (!file.Name.Contains(".meta") &&
                !file.Name.Contains(".DS_Store"))
            {
                try
                {
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Copying file " + file.FullName + " to " + target.FullName + " failed with exception: " + e);
                }
            }
        }
    }
}
