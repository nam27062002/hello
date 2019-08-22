// HungryDragonEditorMenu.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Setup of the Hungry Dragon menu entry in the Unity Editor menu, giving fast access to several tools.
/// </summary>
[InitializeOnLoad]	// To call the constructor upon Unity initialization
public class HungryDragonEditorMenu
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//
    private static readonly string DEFINITIONS_FOLDER = "Assets/Resources/Definitions/";
    private static readonly string SINGLETONS_FOLDER = "Assets/Resources/Singletons/";

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Constructor.
    /// </summary>
    static HungryDragonEditorMenu()
    {
        // Subscribe to the scene update call
        //SceneView.onSceneGUIDelegate += OnSceneGUI;

        // By default we want all shaders to behave as if HIGH key was enabled when editing
        Shader.EnableKeyword(FeatureSettingsManager.SHADERS_KEY_HIGH);
        // By default we want to force lightmap if exists
        Shader.EnableKeyword(GameConstants.Materials.Keyword.FORCE_LIGHTMAP);
    }

    /// <summary>
    /// Scene has been updated.
    /// </summary>
    /// <param name="_sceneview">Target scene view.</param>
    public static void OnSceneGUI(SceneView _sceneview)
    {
        /*Event e = Event.current;
		if(e != null && e.keyCode != KeyCode.None) {
			Debug.Log("Key pressed in editor: " + e.keyCode);
		}*/
    }

    //------------------------------------------------------------------------//
    // MENU SETUP															  //
    //------------------------------------------------------------------------//
    //--------------------------------------------------- CONTENT ----------------------------------------------------//
    [MenuItem("Hungry Dragon/Content/Game Settings", false, 0)]
    public static void ShowSettings() { OpenFile("GameSettings.asset", SINGLETONS_FOLDER); }

    [MenuItem("Hungry Dragon/Content/UI Constants", false, 0)]
    public static void ShowUIConstants() { OpenFile("UIConstants.asset", SINGLETONS_FOLDER); }

    [MenuItem("Hungry Dragon/Content/Reload Rules", false, 50)]
	public static void ReloadDefinitions() { ContentManager.InitContent(true, false); }

	//---------------------------------------------------- TOOLS -----------------------------------------------------//
	/// <summary>
	/// Custom toolbar for the project.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Hungry Dragon Toolbar", false, -100)]
	public static void HungryDragonToolbar() {
		HungryDragonEditorToolbar.ShowWindow();
	}

	/// <summary>
	/// Saves all assets to disk. Useful to make sure changes in scriptable object instances are stored.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Save Assets", false, 0)]
    public static void SaveAssets()
    {
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Delete all empty folders under the "Assets" directory.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Delete Empty Folders", false, 1)]
    public static void DoDeleteEmptyFolders()
    {
        DeleteEmptyFolders.DeleteFolders();
    }
    /*
	[MenuItem("Hungry Dragon/Tools/Fog Area Save Gradients", false, 1)]
    public static void FogAreaSave()
    {
		FogArea[] allObjects = UnityEngine.Object.FindObjectsOfType<FogArea>();
		foreach( FogArea go in allObjects)
		{
			if ( go.gameObject.scene.IsValid() )
			{
				go.m_attributes.SaveKeys();
				Undo.RecordObject(go, "marking");
				EditorUtility.SetDirty(go);
			}
		}

		FogManager[] allManagers = UnityEngine.Object.FindObjectsOfType<FogManager>();
		foreach( FogManager go in allManagers)
		{
			if ( go.gameObject.scene.IsValid() )
			{
				go.m_defaultAreaFog.SaveKeys();
				Undo.RecordObject(go, "marking");
				EditorUtility.SetDirty(go);
			}
		}
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

	[MenuItem("Hungry Dragon/Tools/Fog Area Restore Gradients", false, 1)]
    public static void FogAreaLoad()
    {
		FogArea[] allObjects = UnityEngine.Object.FindObjectsOfType<FogArea>();
		foreach( FogArea go in allObjects)
		{
			if ( go.gameObject.scene.IsValid() )
			{
				go.m_attributes.LoadKeys();
				Undo.RecordObject(go, "marking");
				EditorUtility.SetDirty(go);
			}
		}
		FogManager[] allManagers = UnityEngine.Object.FindObjectsOfType<FogManager>();
		foreach( FogManager go in allManagers)
		{
			if ( go.gameObject.scene.IsValid() )
			{
				go.m_defaultAreaFog.LoadKeys();
				Undo.RecordObject(go, "marking");
				EditorUtility.SetDirty(go);
			}
		}
		EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
    */

    /// <summary>
    /// Simple content viewer.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Rules Reader", false, 2)]
    public static void ShowRulesReader()
    {
        RulesReaderEditorWindow.ShowWindow();
    }

    /// <summary>
    /// Find missing references on scene.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Find Missing References", false, 3)]
    public static void FindMissingReferences()
    {
        FindMissingReferencesTool.FindMissingReferences(false);
    }

    /// <summary>
    /// Find missing references on scene.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Find Missing And NULL References", false, 4)]
    public static void FindMissingAndNullReferences()
    {
        FindMissingReferencesTool.FindMissingReferences(true);
    }

	/// <summary>
	/// Find TIDs in the project.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Find TIDs", false, 5)]
	public static void FindTids() {
		FindTidTool.ShowWindow();
	}

    /// <summary>
    /// Easily change time scale in runtime.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Time Scaler", false, 50)]
    public static void TimeScalerWindow()
    {
        TimeScaler.ShowWindow();
    }

	/// <summary>
    /// Preview of all Ease functions.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Ease Preview Tool", false, 51)]
    public static void EasePreviewToolWindow()
    {
        EasePreviewTool.ShowWindow();
    }

	/// <summary>
	/// Show extended transform data.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Transform View Tool", false, 52)]
	public static void TransformViewWindow()
	{
		TransformViewTool.ShowWindow();
	}

    /// <summary>
    /// Custom tools for the dragon selection menu.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Dragon Selection Menu Tools", false, 150)]
    public static void DragonMenuTools()
    {
        DragonMenuToolsEditorWindow.ShowWindow();
    }

	/// <summary>
	/// Capture tool for dragon disguises.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/UI Disguises Capture Tool", false, 200)]
	public static void OpenDisguisesCaptureTool() {
		OpenScene("Assets/Tools/UITools/CaptureTool/SC_DisguisesCaptureTool.unity", true);
	}

	/// <summary>
	/// Capture tool for dragon disguises.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/UI Pets Capture Tool", false, 201)]
	public static void OpenPetsCaptureTool() {
		OpenScene("Assets/Tools/UITools/CaptureTool/SC_PetsCaptureTool.unity", true);
	}

	/// <summary>
	/// Capture tool for dragon disguises.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/UI Spawners Capture Tool", false, 202)]
	public static void OpenSpawnersCaptureTool() {
		OpenScene("Assets/Tools/UITools/CaptureTool/SC_SpawnersCaptureTool.unity", true);
	}

	/// <summary>
	/// Capture tool for dragon disguises.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Live Event Icons Capture Tool", false, 203)]
	public static void OpenLiveEventIconsCaptureTool() {
		OpenScene("Assets/Tools/UITools/CaptureTool/SC_LiveEventIconsCaptureTool.unity", true);
	}

	/// <summary>
	/// Regenerate the icon for the selected entity prefab.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Generate Spawner Icons (selected entity prefabs)", false, 204)]
    public static void GenerateSpawnerIconsSelected()
    {
        // Show error message if nothing is selected
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No prefab is selected.", "Ok");
            return;
        }

        int removeString = "Assets/Resources/".Length;
        int removeExternsion = ".prefab".Length;
        // Pick selected prefab adn check that it's valid
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
			// Aux vars
			GameObject entityPrefab = Selection.gameObjects[i];

			// Show progress bar
			if(EditorUtility.DisplayCancelableProgressBar(
				"Generating Spawner Icons...",
				i + "/" + Selection.gameObjects.Length + ": " + entityPrefab.name,
				(float)i/(float)Selection.gameObjects.Length
			)) {
				// Cancel pressed, break loop!
				break;
			}

            // Check that prefab corresponds to an entity
            if (entityPrefab.GetComponent<Entity>() == null)
            {
                EditorUtility.DisplayDialog("Error", "Selected prefab " + entityPrefab.name + " doesn't have the Entity component.", "Skip It");
                continue;
            }

            string myPath = AssetDatabase.GetAssetPath(entityPrefab);
            if (!string.IsNullOrEmpty(myPath))
            {
                myPath = myPath.Substring(removeString);
                myPath = myPath.Substring(0, myPath.Length - removeExternsion);
                SpawnerIconGeneratorEditor.GenerateIcon(entityPrefab, Colors.transparentWhite, myPath + ".png");
            }

            // Generate icon for the selected prefab
            // SpawnerIconGeneratorEditor.GenerateIcon(entityPrefab, Colors.transparentWhite);
        }

		// Clear progress bar
		EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// Regenerate the icon for all the spawners in the scene.
    /// </summary>
    [MenuItem("Hungry Dragon/Tools/Generate Spawner Icons (all, takes a while)", false, 205)]
    public static void GenerateSpawnerIconsAll()
    {
        //SpawnerIconGeneratorEditor.GenerateSpawnerIconsInScene();
        SpawnerIconGeneratorEditor.GenerateSpawnerIconsInResources(Colors.transparentWhite);
    }

	/// <summary>
	/// Live Events Tools
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Tournament Leaderboard Generator", false, 251)]
	public static void TournamentLeaderboardGenerator() {
		TournamentLeaderboardGeneratorEditor.ShowWindow();
	}

	/// <summary>
	/// Preview of all Ease functions.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/League Leaderboard Generator", false, 252)]
	public static void LeagueLeaderboardGenerator() {
		LeagueLeaderboardGeneratorEditor.ShowWindow();
	}

	/// <summary>
	/// Preview of all Ease functions.
	/// </summary>
	[MenuItem("Hungry Dragon/Tools/Color Ramp Editor", false, 301)]
	public static void MenuColorRampEditor() {
		ColorRampEditor.OpenWindow();
	}

	//--------------------------------------------------- OTHERS -----------------------------------------------------//
	/// <summary>
	/// Add menu item to be able to open the level editor.
	/// </summary>
	[MenuItem("Hungry Dragon/Level Editor", false, 50)]
    public static void ShowLevelEditorWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        LevelEditor.LevelEditorWindow window = LevelEditor.LevelEditorWindow.instance;

        // Setup window
        window.titleContent = new GUIContent("Level Editor");
        window.minSize = new Vector2(330f, 350f);   // Min required width to properly fit all the content
                                                    //window.maxSize = new Vector2(window.minSize.x, window.minSize.y);
        window.position = new Rect(100f, 100f, 540f, Screen.currentResolution.height - 110f);

        // Make sure everything is initialized properly
        window.Init();
        window.CloseNonEditableScenes();

        // Show it
        window.ShowTab();
    }

    /// <summary>
    /// Add menu item to be open the persistence profiles editor.
    /// </summary>
    [MenuItem("Hungry Dragon/Persistence Profiles", false, 51)]
    public static void ShowPersistenceProfilesWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        PersistenceProfilesEditorWindow window = PersistenceProfilesEditorWindow.instance;

        // Setup window
        window.titleContent = new GUIContent("Persistence Profiles");
        window.minSize = new Vector2(PersistenceProfilesEditorWindow.SPACING + PersistenceProfilesEditorWindow.PROFILE_LIST_COLUMN_WIDTH + PersistenceProfilesEditorWindow.PROFILE_VIEW_COLUMN_WIDTH + PersistenceProfilesEditorWindow.SPACING, PersistenceProfilesEditorWindow.MIN_WINDOW_HEIGHT); // Fixed width, arbitrary minimum
        window.maxSize = new Vector2(window.minSize.x, float.PositiveInfinity);                     // Fixed width, limitless

        // Reset some vars
        window.m_newProfileName = "";

        // Show it
        window.Show();  // In this case we actually want the window to be closed when losing focus so the temp object created to display savegames is properly destroyed
    }

    /// <summary>
    /// Add menu item to generate the shaders properties file, which is used by the memory profiler to obtain the set of textures used by a game object
    /// </summary>
    [MenuItem("Hungry Dragon/Profiler/Generate shaders properties file", false, 51)]
    public static void Profiler_GenerateShadersPropertiesFile()
    {
        Debug.Log("Generating shaders properties file");

        //        EditorUtility.("Material keyword reset", "Obtaining Material list ...", "");

        Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
        List<string> properties;
        Material[] materialList;
        AssetFinder.FindAssetInContent<Material>(System.IO.Directory.GetCurrentDirectory() + "\\Assets", out materialList);

        Shader shader;
        int count = materialList.Length;
        int propertiesCount;
        string propertyName;
        for (int c = 0; c < count; c++)
        {
            shader = materialList[c].shader;

            if (!data.ContainsKey(shader.name))
            {
                properties = new List<string>();
                propertiesCount = UnityEditor.ShaderUtil.GetPropertyCount(shader);
                for (int i = 0; i < propertiesCount; i++)
                {
                    if (UnityEditor.ShaderUtil.GetPropertyType(shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, i);
                        properties.Add(propertyName);
                    }
                }

                data.Add(shader.name, properties);
            }
        }

        AssetMemoryProfiler.ShadersSettings_SaveToFile(data);

        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// Add menu item to be open the npcs settings manager editor.
    /// </summary>
    [MenuItem("Hungry Dragon/Profiler/NPCs Settings", false, 51)]
    public static void ShowNpcsSettingsManagerWindow()
    {
        // Show existing window instance. If one doesn't exist, make one.
        ProfilerEditorWindow window = ProfilerEditorWindow.instance;

        // Setup window
        window.titleContent = new GUIContent("NPCs Settings");
        window.minSize = new Vector2(ProfilerEditorWindow.SPACING + ProfilerEditorWindow.PROFILE_LIST_COLUMN_WIDTH + ProfilerEditorWindow.PROFILE_VIEW_COLUMN_WIDTH + ProfilerEditorWindow.SPACING, ProfilerEditorWindow.MIN_WINDOW_HEIGHT); // Fixed width, arbitrary minimum
        window.maxSize = new Vector2(window.minSize.x, float.PositiveInfinity);                     // Fixed width, limitless        

        // Show it
        window.Show();  // In this case we actually want the window to be closed when losing focus so the temp object created to display savegames is properly destroyed
    }

    //----------------------------------------------- SCENE SHORTCUTS -------------------------------------------------//
    [MenuItem("Hungry Dragon/Scenes/SC_Loading", false, 0)]
    public static void OpenScene1() { OpenScene("Assets/Game/Scenes/SC_Loading.unity", true); }

    [MenuItem("Hungry Dragon/Scenes/SC_Menu", false, 1)]
    public static void OpenScene2() { OpenScene("Assets/Game/Scenes/SC_Menu.unity", true); }

    [MenuItem("Hungry Dragon/Scenes/SC_Game", false, 2)]
    public static void OpenScene3() { OpenScene("Assets/Game/Scenes/SC_Game.unity", true); }

	[MenuItem("Hungry Dragon/Scenes/SC_Results", false, 3)]
	public static void OpenScene4() { OpenScene("Assets/Game/Scenes/SC_ResultsScreen.unity", false); }
    
	[MenuItem("Hungry Dragon/Scenes/SC_Popups", false, 51)]
    public static void OpenScene5() { OpenScene("Assets/Tests/SC_Popups.unity", false); }


    //------------------------------------------------------------------------//
    // CONTEXT MENU SETUP													  //
    // Use the "Assets/" prefix to show the option in the Project's view 	  //
    // context menu															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Regenerate the icon for the selected entity prefab.
    /// </summary>
    [MenuItem("Assets/Hungry Dragon/Generate Spawner Icons", false, 0)]
    public static void ContextGenerateSpawnerIcons(MenuCommand _command)
    {
        GenerateSpawnerIconsSelected();
    }

    //------------------------------------------------------------------------//
    // INTERNAL UTILS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Open and select a file (scriptable object, prefab).
    /// </summary>
    /// <param name="_fileName">The name of the file, including extension.</param>
    /// <param name="_folderPath">The path of folder containing such file, optional. From project's root and ending with a '/' (i.e. "Assets/Resources/MyFolder/").</param>
    public static void OpenFile(string _fileName, string _folderPath)
    {
        // Just find and select the scriptable object
        Object targetObj = AssetDatabase.LoadMainAssetAtPath(_folderPath + _fileName);
        EditorUtils.FocusObject(targetObj, true, false, true);
    }

    /// <summary>
    /// Open the scene with the given name.
    /// </summary>
    /// <param name="_sceneName">The path of the scene starting at project root and with extension (e.g. "Assets/MyScenesFolder/MyScene.unity").</param>
    /// <param name="_closeLevelEditor">Whether to force closing the level editor before opening the scene.</param>
    public static void OpenScene(string _scenePath, bool _closeLevelEditor)
    {
        // Ask to save current scenes first
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // If asked to close the level editor, do it now
        if (_closeLevelEditor)
        {
            LevelEditor.LevelEditorWindow.instance.Close();
        }

        // Just do it
        EditorSceneManager.OpenScene(_scenePath, OpenSceneMode.Single); // Will close all open scenes
    }

    #region memory_profiler
    private static string MEMORY_PROFILER_PATH = "profiler/develop/";

    private static HDMemoryProfiler smMemoryProfiler;

    private static HDMemoryProfiler MemoryProfiler_MP {
        get {
            if (smMemoryProfiler == null) {
                
                // The profiler depends on the shaders properties, that's why this list is generated the first time the profiler is needed
                Profiler_GenerateShadersPropertiesFile();
                smMemoryProfiler = new HDMemoryProfiler();
            }

            return smMemoryProfiler;
        }
    }
    
    private static MemorySample MemoryProfiler_SampleFromAll { get; set; }        

    private static void MemoryProfiler_Clear() {
        if (MemoryProfiler_SampleFromAll != null) {
            MemoryProfiler_SampleFromAll.Clear();
        }

        MemoryProfiler_MP.Clear(true);                        
    }   

    [MenuItem("Hungry Dragon/Profiler/Memory/Take a sample from game object", false, 51)]
    public static void MemoryProfiler_TakeAsampleFromGO() {
        MemoryProfiler_MP.Clear(true);

        GameObject go = GameObject.Find("PF_DragonBaby");
        if (go != null) {
            MemorySample.ESizeStrategy sizeStrategy = MemorySample.ESizeStrategy.Profiler;
            AbstractMemorySample sample = MemoryProfiler_MP.GO_TakeASample(go, null, sizeStrategy);

            //Dictionary<string, List<string>> typeGroups = null;
            Dictionary<string, List<string>> typeGroups = MemoryProfiler_MP.GameTypeGroups;
            string xml = sample.ToXML(null, null, typeGroups).OuterXml;           
            Debug.Log(xml);
            File.WriteAllText(MEMORY_PROFILER_PATH + "memorySampleFromGO", xml);
        }
    }   

    /*[MenuItem("Hungry Dragon/Profiler/Memory/Take a sample from all", false, 51)]
    public static void MemoryProfiler_TakeAsampleFromAll() {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        
        MemoryProfiler_SampleFromAll = MemoryProfiler_MP.All_TakeASample();
        string xml = MemoryProfiler_SampleFromAll.ToXML().OuterXml;
        Debug.Log(xml);
        File.WriteAllText("memorySampleFromAll", xml);        
    }*/

    [MenuItem("Hungry Dragon/Profiler/Memory/Take a sample from scene", false, 51)]
    public static void MemoryProfiler_TakeAsampleFromScene()
    {
        MemoryProfiler_Clear();

        AbstractMemorySample sample = MemoryProfiler_MP.Scene_TakeASample(false);
        
        string xml = sample.ToXML(null, null, MemoryProfiler_MP.GameTypeGroups).OuterXml;
        Debug.Log(xml);
        File.WriteAllText(MEMORY_PROFILER_PATH + "memorySampleFromScene", xml);
    }

    [MenuItem("Hungry Dragon/Profiler/Memory/Take a sample from game", false, 51)]
    public static void MemoryProfiler_TakeASampleFromGame()
    {
        MemoryProfiler_TakeASampleFromGameInternal(false);
    }

    [MenuItem("Hungry Dragon/Profiler/Memory/Take a sample from game with categories", false, 51)]
    public static void MemoryProfiler_TakeASampleFromGameWithCategories()
    {
        MemoryProfiler_TakeASampleFromGameInternal(true);
    }

    private static void MemoryProfiler_TakeASampleFromGameInternal(bool withCategories)
    {
        MemoryProfiler_Clear();               

        AbstractMemorySample sample;
        if (withCategories)
        {
            sample = MemoryProfiler_MP.Scene_TakeAGameSampleWithCategories(false, HDMemoryProfiler.CATEGORY_SET_NAME_GAME);
        }
        else
        {
            sample = MemoryProfiler_MP.Scene_TakeAGameSample(false);
        }
       
        string xml = sample.ToXML(null, null, MemoryProfiler_MP.GameTypeGroups).OuterXml;
        Debug.Log(xml);
        File.WriteAllText(MEMORY_PROFILER_PATH + "memorySampleFromGame", xml);
    }

    [MenuItem("Hungry Dragon/Profiler/Open Memory Profiler", false, 51)]
    public static void MemoryProfiler_OpenMemoryProfiler()
    {
        // The profiler depends on the shaders properties, that's why this list is generated the first time the profiler is needed
        Profiler_GenerateShadersPropertiesFile();
        MemoryProfilerEditorWindow.Init();
    }
    #endregion

    #region build_reporter
    [MenuItem("Hungry Dragon/BuildReporter/Extract Audio", false, 51)]
    public static void BuildReporter_ExtractAudio()
    {
        string path = "Assets/BuildReport/report.txt";
        string pathOutput = "Assets/BuildReport/audio_report.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);

        float size = 0f;
        int count = 0;
        string line;
        string[] tokens;
        List<string> content = new List<string>();
        while ((line = reader.ReadLine()) != null)
        {
            if (line.EndsWith(".wav"))
            {
                tokens = line.Trim().Split(' ');
                if (tokens.Length >= 3)
                {
                    float fileSize = float.Parse(tokens[0]);
                    switch (tokens[1].Trim())
                    {
                        case "kb":
                            size += fileSize;
                            break;

                        case "mb":
                            size += fileSize * 1024f;
                            break;
                    }

                    content.Add(tokens[0] + " " + tokens[1].Trim() + " " + tokens[3]);                    
                }

                count++;
                //Debug.Log(line);
            }
        }
                    
        reader.Close();

        content.Add("");
        string summary = "Total size: " + (size / 1024f) + " mb" + " files amount: " + count;
        content.Add(summary);                
        BuildReporter_WriteFile(pathOutput, content);

        Debug.Log(summary);

        /*int counter = 0;
        string line;

        // Read the file and display it line by line.  
        StreamReader file = new StreamReader(@"c:\test.txt");
        while ((line = file.ReadLine()) != null)
        {
            System.Console.WriteLine(line);
            counter++;
        }

        file.Close();
        System.Console.WriteLine("There were {0} lines.", counter);
        // Suspend the screen.  
        System.Console.ReadLine();
        */
    }

    private static void BuildReporter_WriteFile(string path, List<string> content)
    {        
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        int count = content.Count;
        for (int i = 0; i < count; i++)
        {
            writer.WriteLine(content[i]);
        }

        writer.Close();        
    }
    #endregion

    #region minimal_build
    [MenuItem("Hungry Dragon/MinimalBuild/Extract Spawners", false, 51)]
    private static void MinimalBuild_ExtractSpawners()
    {
        List<string> scenes = MinimalBuild_GetScenes("SP_");
        
        string pathOutput = "Assets/BuildReport/sp_report.txt";
        List<string> content = new List<string>();

        string path;
        int count = scenes.Count;
        StreamReader reader;
        string line;
        string[] tokens;
        string prefix = "PF_";
        for (int i = 0; i < count; i++)
        {
            path = Builder.GetLevelPath(scenes[i]);            
            reader = new StreamReader(path);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains(prefix))
                {
                    tokens = line.Split(' ');
                    for (int j = 0; j < tokens.Length; j++)
                    {
                        if (tokens[j].Contains(prefix))
                        {
                            if (!content.Contains(tokens[j]))
                            {
                                content.Add(tokens[j]);
                                Debug.Log(tokens[j]);
                            }

                            break;
                        }
                    }                    
                }
            }

            reader.Close();
        }

        BuildReporter_WriteFile(pathOutput, content);

        Debug.Log("Done!");
    }

    private static List<string> MinimalBuild_GetScenes(string prefix)
    {
        List<string> returnValue = new List<string>();

        // Load proper scenes        
        ContentManager.InitContent(true, false);
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, "level_0");
        List<string> levels = def.GetAsList<string>("common");                
        List<string> areaList = def.GetAsList<string>("area1");
        levels.AddRange(areaList);

        int count = levels.Count;
        for (int i = 0; i < count; i++)
        {
            if (levels[i].StartsWith(prefix))
            {
                returnValue.Add(levels[i]);
            }
        }

        return returnValue;
    }    
    #endregion
}