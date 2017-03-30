using SimpleJSON;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProfilerSettingsManager
{
#if PRODUCTION
    public const bool ENABLED = false;
#else
    public const bool ENABLED = true;
#endif             

    private const string PROFILER_SETTINGS_FILE_NAME = "profilerSettings";

    public static ProfilerSettings SettingsCached { get; set; }
    public static ProfilerSettings SettingsResources { get; set; }    

    public static bool IsLoaded { get; set; }
    
    public static void Load(List<string> spawnerNames)
    {
        if (!IsLoaded)
        {
            Debug.Log("ProfilerSettingsManager.Load");           

            // Loads settings from Resources
            LoadFromResources(spawnerNames);

            // Loads settings from cache
           // LoadFromCache(spawnerNames);

            // If there's no settings in cache then it's created out of settings in resources
            if (SettingsCached == null && SettingsResources != null)
            {
                SettingsCached = new ProfilerSettings();
                SettingsCached.Load(SettingsResources.ToJSON());
                SettingsCached.isDirty = true;
            }

            if (SettingsCached.isDirty)
            {
                SaveToCache();
            }

            Spawner_NumEntities = 10;

            if (spawnerNames != null && spawnerNames.Count > 0)
            {
                Spawner_Prefab = spawnerNames[0];
            }

            IsLoaded = true;
        }        
    }

    private static string GetResourcesFileNameFullPath()
    {
        return Application.dataPath + "/Resources/Profiler/" + PROFILER_SETTINGS_FILE_NAME + ".json";
    }

    private static void LoadFromResources(List<string> spawnerNames)
    {
        SettingsResources = new ProfilerSettings();                
        TextAsset textAsset = (TextAsset)Resources.Load("Profiler/" + PROFILER_SETTINGS_FILE_NAME, typeof(TextAsset)); ;
        if (textAsset == null)
        {
            Debug.LogError("Could not load text asset " + PROFILER_SETTINGS_FILE_NAME);
        }
        else
        {
            Debug.Log("From Resources = " + textAsset.text);
            JSONNode data = JSONNode.Parse(textAsset.text);            
            SettingsResources.Load(data);            
        }

        SettingsResources.ParseSpawners(spawnerNames);
        if (SettingsResources.isDirty)
        {
            SaveToResources();
        }
    }

    public static void SaveToResources()
    {
        if (SettingsResources != null)
        {
            File.WriteAllText(GetResourcesFileNameFullPath(), SettingsResources.ToJSON().ToString());
            SettingsResources.isDirty = false;
        }
    }

    public static void SaveJSONToResources(JSONNode data)
    {
        if (SettingsResources == null)
        {
            SettingsResources = new ProfilerSettings();
        }

        SettingsResources.Load(data);
        SaveToResources();        
    }

    public static void ResetSettingsResources()
    {
        if (SettingsResources != null)
        {
            SettingsResources.ResetSpawnersToDefault();
            SaveToResources();
        }
    }

    private static string GetCacheFileNameFullPath()
    {
        return Application.persistentDataPath + "/" + PROFILER_SETTINGS_FILE_NAME + ".json";
    }

    private static void LoadFromCache(List<string> spawnerNames)
    {                
        string strCachePath = GetCacheFileNameFullPath();

        // Checks if the file is cached 
        if (File.Exists(strCachePath))
        {
            StreamReader kReader = new StreamReader(strCachePath);
            string strFileContent = kReader.ReadToEnd();
            JSONNode data = JSONNode.Parse(strFileContent);
            SettingsCached = new ProfilerSettings();
            SettingsCached.Load(data);
            kReader.Close();
            SettingsCached.ParseSpawners(spawnerNames);
        }
    }

    public static void SaveToCache()
    {
        if (SettingsCached != null)
        {
            JSONNode data = SettingsCached.ToJSON();
            if (data != null)
            {
                string strCachePath = GetCacheFileNameFullPath();
                File.WriteAllText(strCachePath, data.ToString());
                SettingsCached.isDirty = false;
            }            
        }
    }

    public static void SaveJSONToCache(JSONNode data)
    {
        if (SettingsCached == null)
        {
            SettingsCached = new ProfilerSettings();
        }

        SettingsCached.Load(data);
        SaveToCache();        
    }

    public static void ResetCachedToResources()
    {
        JSONNode data = SettingsResources.ToJSON();
        SaveJSONToCache(data);
    }

    public static int Spawner_NumEntities { get; set; }
    public static string Spawner_Prefab { get; set; }
}
