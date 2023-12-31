﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for implementing related to HD stuff, for example to split the scene in the categories defined in the budget:
/// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/Dragon+Memory+Budget+Draft
/// </summary>
public class HDMemoryProfiler : MemoryProfiler
{
    public HDMemoryProfiler()
    {
        SizeStrategy = AbstractMemorySample.ESizeStrategy.DeviceHalf;

        // Supported category sets are setup
        CategorySet_Setup();
    }

    private void PrepareSample(bool clearAnalysis)
    {
        Clear(clearAnalysis);
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    /// <summary>
    /// Takes a sample of the scenes currently loaded. No categories are allowed.
    /// </summary>
    /// <returns></returns>
    public override AbstractMemorySample Scene_TakeASample(bool reuseAnalysis)
    {
        GameObject go;
        string key = CATEGORY_SET_GAME_KEY_EVERYTHING;

        // Loops through all root game objects and classify them
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (s.isLoaded                 
                /*&& s.name == "SC_Game"*/
                )
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {                    
                    go = allGameObjects[j];
                    Scene_AddGO(key, go);                    
                }
            }
        }

        // Searches for singletons
        GameObject singletons = GameObject.Find("Singletons");
        if (singletons != null)
        {
            UnityEngine.SceneManagement.Scene scene = singletons.scene;
            if (scene != null)
            {
                GameObject[] gos = scene.GetRootGameObjects();
                if (gos != null)
                {
                    int count = gos.Length;
                    for (int i = 0; i < count; i++)
                    {
                        Scene_AddGO(key, gos[i]);
                    }
                }
            }            
        }

        // Lightmaps are added manually
        Scene_AddLightmaps(key);        

        return base.Scene_TakeASample(reuseAnalysis);        
    }

    private void Scene_AddLightmaps(string key)
    {        
        LightmapData[] lightmaps = LightmapSettings.lightmaps;
        if (lightmaps != null)
        {
            int count = lightmaps.Length;
            for (int i = 0; i < count; i++)
            {
                Scene_AddObject(key, lightmaps[i].lightmapColor);
                Scene_AddObject(key, lightmaps[i].lightmapDir);
            }
        }
    }

    /// <summary>
    /// Takes a sample of the game. This method can be called only when playing a game.
    /// </summary>
    /// <returns></returns>
    public override AbstractMemorySample Scene_TakeAGameSample(bool reuseAnalysis)
    {
        return Scene_TakeAGameSampleInternal(reuseAnalysis, null);
    }

    /// <summary>
    /// Takes a sample of the game classifying the game objects according to the category set which name is passed as parameter. 
    /// This method can be called only when playing a game.
    /// </summary>
    /// <returns></returns>
    public override AbstractMemorySample Scene_TakeAGameSampleWithCategories(bool reuseAnalysis, string categorySetName)
    {
        return Scene_TakeAGameSampleInternal(reuseAnalysis, categorySetName);
    }

    private AbstractMemorySample Scene_TakeAGameSampleInternal(bool reuseAnalysis, string categorySetName)
    {
        PrepareSample(!reuseAnalysis);

        GameObject go;
        string key = CATEGORY_SET_KEY_PLAYER;
        bool calculateKey = true;
        if (categorySetName == CATEGORY_SET_GAME_KEY_EVERYTHING)
        {
            key = CATEGORY_SET_GAME_KEY_EVERYTHING;
            calculateKey = false;
        }

        List<GameObject> gosAlreadyProcessed = new List<GameObject>();
        if (InstanceManager.instance != null)
        {
            DragonPlayer player = InstanceManager.player;
            if (player != null)
            {
                go = player.gameObject;
                Scene_AddGO(key, go);
                gosAlreadyProcessed.Add(go);
            }
        }        

        // We need to ignore this game object because it loads the current dragon for the loading screen. We want to ignore this assets in the "Hud" category
        GO_BanGOByName("PF_LevelLoadingSplash");

        // Loops through all root game objects and classify them
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (s.isLoaded /*&& s.name == "SC_Game"*/)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    go = allGameObjects[j];

                    if (calculateKey)
                    {
                        key = go.scene.name;
                        if (go.GetComponent<Pet>() != null)
                        {
                            key = CATEGORY_SET_KEY_PLAYER;
                        }
                        else
                        {
                            key = CategorySet_GetKeyFromSceneName(categorySetName, go.scene.name);
                        }
                    }

                    if (key == null)
                    {
                        Debug.LogError("Key can't be empty");
                    }
                    else
                    {
                        // Makes sure that go hasn't already been processed
                        if (!gosAlreadyProcessed.Contains(go))
                        {
                            Scene_AddGO(key, go);
                        }
                    }
                }
            }
        }       

        // Singletons
        GameObject singletons = GameObject.Find("Singletons");
        if (singletons != null)
        {
            // Loops through all game objects except the one called "Singletons" in DontDestroy scene to add them to the profiler.
            // "Singletons" game object will be added manually because their children might need to be assigned to different keys
            UnityEngine.SceneManagement.Scene scene = singletons.scene;
            if (scene != null)
            {
                GameObject[] gos = scene.GetRootGameObjects();
                if (gos != null)
                {
                    int count = gos.Length;

                    if (calculateKey)
                    {
                        key = CATEGORY_SET_GAME_KEY_HUD;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        if (gos[i] != singletons)
                        {
                            Scene_AddGO(key, gos[i]);
                        }
                    }
                }
            }

            if (calculateKey)
            {
                // Most particles are used by npcs so they are assigned to LEVEL NPCS category
                GameObject particleManagerGO = (ParticleManager.instance == null) ? null : ParticleManager.instance.gameObject;
                if (particleManagerGO != null)
                {
                    Scene_AddGO(CATEGORY_SET_KEY_LEVEL_NPCS, particleManagerGO);
                }                

                // Most game objects in pool manager are npcs so they are assigned to LEVEL NPCS category
                GameObject poolManagerGO = (PoolManager.instance == null) ? null : PoolManager.instance.gameObject;
                if (poolManagerGO != null)
                {
                    Scene_AddGO(CATEGORY_SET_KEY_LEVEL_NPCS, poolManagerGO);
                }

                // Loops through the children of the singletons parent because they need to be assigned to different keys
                Transform t = singletons.transform;
                int count = t.childCount;                
                for (int i = 0; i < count; i++)
                {
                    go = t.GetChild(i).gameObject;
                    if (go != particleManagerGO && go != poolManagerGO)
                    {
                        Scene_AddGO(CATEGORY_SET_GAME_KEY_HUD, go);
                    }
                }
            }
            else
            {
                // We loop through them because 
                Scene_AddGO(key, singletons);
            }            
        }       


        // Lightmaps have to be added manually since there's no references in any game object
        if (calculateKey)
        {
            // They are assigned to the art level category
            key = CATEGORY_SET_KEY_LEVEL_ART;
        }
        Scene_AddLightmaps(key);

        // Takes the sample
        AbstractMemorySample sample;
        if (categorySetName != null)
        {
            sample = base.Scene_TakeAGameSampleWithCategories(reuseAnalysis, categorySetName);
        }
        else
        {
            sample = base.Scene_TakeAGameSample(reuseAnalysis);
        }
       
        return sample;                
    }

    public static string GAME_TYPE_GROUPS_TEXTURES = "Textures";
    public static string GAME_TYPE_GROUPS_ANIMATIONS = "Animations";
    public static string GAME_TYPE_GROUPS_PARTICLES = "Particles";
    public static string GAME_TYPE_GROUPS_MESHES = "Meshes";
    public static string GAME_TYPE_GROUPS_AUDIO = "Audio";

    private Dictionary<string, List<string>> mGameTypeGroups;
    public Dictionary<string, List<string>> GameTypeGroups
    {
        get
        {
            if (mGameTypeGroups == null)
            {
                Dictionary<string, List<string>> typeGroups = new Dictionary<string, List<string>>();

                // Textures
                string typeGroupName = "Textures";
                List<string> types = new List<string>() { "Texture2D", "Sprite" };
                typeGroups.Add(typeGroupName, types);

                // Animations
                typeGroupName = "Animations";
                types = new List<string>() { "AnimationClip", "Avatar", "Animator" };
                typeGroups.Add(typeGroupName, types);

                // Particles
                typeGroupName = "Particles";
                types = new List<string>() { "ParticleSystem", "ParticleSystemRenderer" };
                typeGroups.Add(typeGroupName, types);

                // Meshes
                typeGroupName = "Meshes";
                types = new List<string>() { "Mesh" };
                typeGroups.Add(typeGroupName, types);

                // Meshes
                typeGroupName = "Audio";
                types = new List<string>() { "AudioClip", "AudioController" };
                typeGroups.Add(typeGroupName, types);

                mGameTypeGroups = typeGroups;
            }

            return mGameTypeGroups;
        }
    }

    #region category_set    
    public const string CATEGORY_SET_NAME_GAME = "Game";
    public const string CATEGORY_SET_NAME_GAME_1_LEVEL = "Game_1_Level";
    public const string CATEGORY_SET_NAME_EVERYTHING = "Everything";

    public const string CATEGORY_SET_KEY_PLAYER = "Player";
    public const string CATEGORY_SET_GAME_KEY_HUD = "Hud";
    public const string CATEGORY_SET_KEY_LEVEL = "Level";
    public const string CATEGORY_SET_KEY_LEVEL_ART = "LevelArt";
    public const string CATEGORY_SET_KEY_LEVEL_DESIGN = "LevelDesign";
    public const string CATEGORY_SET_KEY_LEVEL_NPCS = "NPCS";
    public const string CATEGORY_SET_KEY_LEVEL_AUDIO = "Audio";
    public const string CATEGORY_SET_GAME_KEY_EVERYTHING = "Everything";

    private void CategorySet_Setup()
    {
        // Game
        CategorySet set = new CategorySet();
        set.Name = CATEGORY_SET_NAME_GAME;

        set.AddCategoryInMb(CATEGORY_SET_KEY_PLAYER, 15f, 20f, 30f);
        set.AddCategoryInMb(CATEGORY_SET_GAME_KEY_HUD, 15f, 15f, 15f);
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL_ART, 30f, 45f, 115f);
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL_DESIGN, 15f, 15f, 15f);
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL_NPCS, 37.5f, 40f, 50f);
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL_AUDIO, 6f, 6f, 6f);

        CategorySet_AddToCatalog(set);

        // Game one category for level
        set = new CategorySet();
        set.Name = CATEGORY_SET_NAME_GAME_1_LEVEL;

        set.AddCategoryInMb(CATEGORY_SET_KEY_PLAYER, 15f, 20f, 30f);
        set.AddCategoryInMb(CATEGORY_SET_GAME_KEY_HUD, 15f, 15f, 15f);
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL, 45f, 60f, 130f);        
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL_NPCS, 37.5f, 40f, 50f);
        set.AddCategoryInMb(CATEGORY_SET_KEY_LEVEL_AUDIO, 6f, 6f, 6f);

        CategorySet_AddToCatalog(set);

        // Game
        set = new CategorySet();
        set.Name = CATEGORY_SET_NAME_EVERYTHING;

        set.AddCategoryInMb(CATEGORY_SET_GAME_KEY_EVERYTHING, 123.5f, 141f, 230f);        

        CategorySet_AddToCatalog(set);
    }

    private string CategorySet_GetKeyFromSceneName(string categoryname, string sceneName)
    {
        string returnValue = null;
        switch (categoryname)
        {
            case CATEGORY_SET_NAME_GAME:
                returnValue = CategorySetGame_GetKeyFromSceneName(sceneName);
                break;

            case CATEGORY_SET_NAME_GAME_1_LEVEL:
                returnValue = CategorySetGame1Level_GetKeyFromSceneName(sceneName);
                break;

            default:
                returnValue = CATEGORY_SET_GAME_KEY_EVERYTHING;
                break;
        }

        return returnValue;
    }

    private string CategorySetGame_GetKeyFromSceneName(string sceneName)
    {
        string returnValue = null;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Invalid scene name");
        }
        else
        {
            if (sceneName == "SC_Game")
            {
                returnValue = CATEGORY_SET_GAME_KEY_HUD;
            }
            else
            {
                string[] tokens = sceneName.Split('_');
                string key = tokens[0].ToUpper();
                if (tokens.Length > 1)
                {
                    switch (key)
                    {
                        case "ART":
                            returnValue = CATEGORY_SET_KEY_LEVEL_ART;
                            break;

                        case "CO":
                            returnValue = CATEGORY_SET_KEY_LEVEL_DESIGN;                            
                            break;

                        case "SO":
                            returnValue = CATEGORY_SET_KEY_LEVEL_AUDIO;
                            break;

                        case "SP":
                            returnValue = CATEGORY_SET_KEY_LEVEL_NPCS;
                            break;
                    }
                }
            }
        }

        if (returnValue == null)
        {
            Debug.LogError("Key can't be empty");
            returnValue = CATEGORY_SET_GAME_KEY_HUD;
        }

        return returnValue;
    }

    private string CategorySetGame1Level_GetKeyFromSceneName(string sceneName)
    {
        string returnValue = null;
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Invalid scene name");
        }
        else
        {
            if (sceneName == "SC_Game")
            {
                returnValue = CATEGORY_SET_GAME_KEY_HUD;
            }
            else
            {
                string[] tokens = sceneName.Split('_');
                string key = tokens[0].ToUpper();
                if (tokens.Length > 1)
                {
                    switch (key)
                    {
                        case "CO":
                        case "ART":
                            returnValue = CATEGORY_SET_KEY_LEVEL;
                            break;                      

                        case "SO":
                            returnValue = CATEGORY_SET_KEY_LEVEL_AUDIO;
                            break;

                        case "SP":
                            returnValue = CATEGORY_SET_KEY_LEVEL_NPCS;
                            break;
                    }
                }
            }
        }

        if (returnValue == null)
        {
            Debug.LogError("Key can't be empty");
            returnValue = CATEGORY_SET_GAME_KEY_HUD;
        }

        return returnValue;
    }
    #endregion
}
