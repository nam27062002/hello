// LevelManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/01/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of levels.
/// </summary>
public class LevelManager : Singleton<LevelManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const string LEVEL_DATA_PATH = "Game/Levels/";

    //------------------------------------------------------------------//
    // PROPERTIES														//
    //------------------------------------------------------------------//
    private static Dictionary<string, HashSet<string>> m_scenesToInclude = new Dictionary<string, HashSet<string>>();
    public static void AddSceneToInclude(string _area, string _scene) { 
        if (!m_scenesToInclude.ContainsKey(_area)) { m_scenesToInclude[_area] = new HashSet<string>(); }
        m_scenesToInclude[_area].Add(_scene);
    }
    public static void RemoveSceneToInclude(string _area, string _scene) {
        m_scenesToInclude[_area].Remove(_scene);
    }

    private static Dictionary<string, HashSet<string>> m_scenesToExclude = new Dictionary<string, HashSet<string>>();
    public static void AddSceneToExclude(string _area, string _scene) {
        if (!m_scenesToExclude.ContainsKey(_area)) { m_scenesToExclude[_area] = new HashSet<string>(); }
        m_scenesToExclude[_area].Add(_scene);
    }
    public static void RemoveSceneToExclude(string _area, string _scene) {
        m_scenesToExclude[_area].Remove(_scene);
    }


    //-------------------------------------------------------------------
    private static string m_currentArea = "";
	public static string currentArea{
		get{ return m_currentArea; }
	}
	private static List<string> m_currentAreaScenes = new List<string>();

	// Shortcut to get the data of the currently selected level
	private static LevelData m_currentLevelData = null;
	public static LevelData currentLevelData {
		get { return m_currentLevelData; }
	}

	private static List<string> m_toSplitScenes = new List<string>();

    private static Dictionary<string, int> m_scenesLoaded = new Dictionary<string, int>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given a level, get its definition.
	/// </summary>
	/// <returns>The definition for the requested level. <c>null</c> if the given sku doesn't correspond to any known level.</returns>
	/// <param name="_sku">The sku of the level whose definition we want.</param>
	public static DefinitionNode GetLevelDef(string _sku) {
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEVELS, _sku);
	}

	/// <summary>
	/// Given a level, find and load its data object.
	/// </summary>
	/// <returns>The level data object for the requested level. <c>null</c> if the given sku doesn't correspond to any known level or the level data object doesn't exist.</returns>
	/// <param name="_sku">The sku of the level whose data we want.</param>
	public static LevelData GetLevelData(string _sku) {
		// Get the definition
		DefinitionNode def = GetLevelDef(_sku);

		// Load and initialize the level data
		// If the data object doesn't exist, content is wrong!!
		LevelData data = null;
		if(def != null) {
			data = Resources.Load<LevelData>(LEVEL_DATA_PATH + def.GetAsString("dataFile"));
			Debug.Assert(m_currentLevelData != null);
			data.Init(def);
		}
		return data;
	}

	/// <summary>
	/// Define a level as current and load its data.
	/// </summary>
	/// <param name="_sku">The level to be defined as current.</param>
	public static void SetCurrentLevel(string _sku) {
		// Ignore if already loaded
		if(m_currentLevelData != null && m_currentLevelData.def.sku == _sku) {
			return;
		}

		// Clear current data
		m_currentLevelData = null;

		if ( !string.IsNullOrEmpty(_sku) )
		{
			// Load new data
			m_currentLevelData = GetLevelData(_sku);

			m_toSplitScenes = m_currentLevelData.def.GetAsList<string>("split");
		}
		else
		{
			m_toSplitScenes.Clear();
		}

	}        

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!m_scenesLoaded.ContainsKey(scene.name))
        {
            m_scenesLoaded.Add(scene.name, 1);
        }

        Debug.Log("OnSceneLoaded: " + scene.name);        
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (m_scenesLoaded.ContainsKey(scene.name))
        {
            m_scenesLoaded.Remove(scene.name);
        }

        Debug.Log("OnSceneUnloaded: " + scene.name);        
    }

    public static LevelLoader LoadLevelForDragon(string _dragonSku) {
       return LoadLevel(m_currentLevelData.def.GetAsString(_dragonSku, "area1"));
    }

    public static LevelLoader LoadLevel(string _startingAreaName)
    {
        m_scenesLoaded.Clear();

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // Destroy any existing level in the game scene
        LevelEditor.Level[] activeLevels = Component.FindObjectsOfType<LevelEditor.Level>();
        for (int i = 0; i < activeLevels.Length; i++)
        {
            GameObject.DestroyImmediate(activeLevels[i].gameObject);
        }

        // Has the current level data been loaded?
        DebugUtils.SoftAssert(m_currentLevelData != null, "Current level has not been set!");
        DefinitionNode def = m_currentLevelData.def;

        LevelLoader returnValue = new LevelLoader(null, _startingAreaName);

        // Common Scenes
        List<string> commonScenes = def.GetAsList<string>("common");
        for (int i = 0; i < commonScenes.Count; i++)
        {
            // TODO: Check if is splitted to use different name
            string sceneName = GetRealSceneName(commonScenes[i]);
            returnValue.AddRealSceneNameToLoad(sceneName);            
        }

        if (FeatureSettingsManager.IsWIPScenesEnabled)
        {
            List<string> gameplayWip = def.GetAsList<string>("gameplayWip");
            for (int i = 0; i < gameplayWip.Count; i++)
            {
                // TODO: Check if is splitted to use different name
                string sceneName = GetRealSceneName(gameplayWip[i]);
                returnValue.AddRealSceneNameToLoad(sceneName);
            }
        }


        // Load area by dragon        
        m_currentArea = _startingAreaName;
        List<string> realSceneNamesPerArea = GetOnlyAreaScenesList(_startingAreaName);
        returnValue.AddRealSceneNameListToLoad(realSceneNamesPerArea);

        return returnValue;
    }

    public static void UnloadLevel()
    {
        m_scenesLoaded.Clear();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public static LevelLoader SwitchArea( string area )
    {                
        // Load additively all the scenes of the current level
        LevelLoader returnValue = new LevelLoader(m_currentArea, area);

        // Current area scenes need to be unloaded
        List<string> realSceneNamesPerArea = GetOnlyAreaScenesList(m_currentArea);

        if (realSceneNamesPerArea != null)
        {
            int count = realSceneNamesPerArea.Count;
            for (int i = 0; i < count; i++)
            {
                if (IsSceneLoaded(realSceneNamesPerArea[i]))
                {
                    returnValue.AddRealSceneNameToUnload(realSceneNamesPerArea[i]);
                }
            }
        }

        // Load area
        m_currentArea = area;
        realSceneNamesPerArea = GetOnlyAreaScenesList(area); 
        returnValue.AddRealSceneNameListToLoad(realSceneNamesPerArea);

        return returnValue;
    }   
    
    public static bool IsSceneLoaded(string sceneName)
    {
        return m_scenesLoaded.ContainsKey(sceneName);
    } 

	public static void DisableCurrentArea()
	{
        Scene s;
		for( int i = 0;i< m_currentAreaScenes.Count; i++ )
		{
            if (IsSceneLoaded(m_currentAreaScenes[i]))
            {
                s = SceneManager.GetSceneByName(m_currentAreaScenes[i]);
                if (s != null)
                {
                    GameObject[] gos = s.GetRootGameObjects();
                    for (int j = 0; j < gos.Length; ++j)
                        gos[j].SetActive(false);
                }
            }
		}
	}   

	/// <summary>
	/// Make sure the active scene is the one marked as "activeScene" in the current level definition.
	/// Current level must have been set via the <see cref="SetCurrentLevel"/> method.
	/// To be called typically after all the scenes in the level have been loaded.
	/// </summary>
	public static void SetArtSceneActive()
	{
		// [AOC] For some reason, the lightning settings of the Art scene makes the OSX Editor to crash
		// #if !UNITY_EDITOR_OSX
		Scene scene = SceneManager.GetSceneByName(m_currentLevelData.def.GetAsString(m_currentArea + "Active"));
		SceneManager.SetActiveScene(scene);
		// #endif
	}	        

	private static string GetRealSceneName( string sceneName )
	{
		if (m_toSplitScenes.Contains( sceneName ))
		{
			switch( FeatureSettingsManager.instance.LevelsLOD )
			{	
				default:
                case FeatureSettings.ELevel4Values.very_low:
                {
                    sceneName += "_verylow";
                }
                break;
                case FeatureSettings.ELevel4Values.low:
				{
					sceneName += "_low";
				}break;
				case FeatureSettings.ELevel4Values.mid:
				{
					sceneName += "_medium";
				}break;
				case FeatureSettings.ELevel4Values.high:
				{
					sceneName += "_high";
				}break;               
            }
		}
		return sceneName;
	}

    /// <summary>
    /// Returns a list with the name of the scenes belonging to the area passed as a parameter.
    /// </summary>    
    public static List<string> GetOnlyAreaScenesList(string area)
    {
        List<string> returnValue = new List<string>();

        HashSet<string> includeScenes = null; 
        if (m_scenesToInclude.ContainsKey(area)) { includeScenes = m_scenesToInclude[area]; }

        HashSet<string> excludeScenes = null;
        if (m_scenesToExclude.ContainsKey(area)) { excludeScenes = m_scenesToExclude[area]; }

        DefinitionNode def = m_currentLevelData.def;

        m_currentArea = area;
        m_currentAreaScenes = def.GetAsList<string>(m_currentArea);

        if (excludeScenes != null) {
            foreach (string scene in excludeScenes) {
                m_currentAreaScenes.Remove(scene);
            }
        }

        if (includeScenes != null) {
            m_currentAreaScenes.AddRange(includeScenes);
        }

        for (int i = 0; i < m_currentAreaScenes.Count && !string.IsNullOrEmpty(m_currentAreaScenes[i]); i++) {
            string sceneName = GetRealSceneName(m_currentAreaScenes[i]);
            returnValue.Add(sceneName);
        }

        return returnValue;
    }

    /// <summary>
    /// Returns a list with the name of the scenes common to all areas
    /// </summary>
    /// <returns></returns>
    public static List<string> GetCommonScenesList()
    {
        List<string> returnValue = new List<string>();

        DefinitionNode def = m_currentLevelData.def;

        // Common Scenes
        List<string> commonScenes = def.GetAsList<string>("common");
        for (int i = 0; i < commonScenes.Count; i++)
        {
            string sceneName = GetRealSceneName(commonScenes[i]);
            returnValue.Add(sceneName);
        }

		if (FeatureSettingsManager.IsWIPScenesEnabled)
		{
			List<string> gameplayWip = def.GetAsList<string>("gameplayWip");
			for (int i = 0; i < gameplayWip.Count; i++)
	        {
				string sceneName = GetRealSceneName(gameplayWip[i]);
	            returnValue.Add(sceneName);
	        }
		}

        return returnValue;
    }

    public static List<string> GetAllArenaScenesList(string area)
    {
        List<string> returnValue = GetCommonScenesList();
        List<string> onlyAreaScenes = GetOnlyAreaScenesList(area);

        int count = onlyAreaScenes.Count;
        for (int i = 0; i < count; i++)
        {
            returnValue.Add(onlyAreaScenes[i]);
        }

        return returnValue;
    }
}