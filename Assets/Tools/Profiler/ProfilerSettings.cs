using SimpleJSON;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// This class is responsible for storing profiler settings
/// </summary>
public class ProfilerSettings
{    
    public class SpawnerData
    {
        public const string ATT_PREFAB_NAME = "prefab";
        public const string ATT_LOGIC_UNITS_NAME = "logicUnits";

        public const float DEFAULT_LOGIC_UNITS = 1f;

        public static string GetPrefabName(JSONNode data)
        {
            return (data != null) ? data[ATT_PREFAB_NAME] : null;
        }

        public static float GetLogicUnits(JSONNode data)
        {
            return (data != null) ? data[ATT_LOGIC_UNITS_NAME].AsFloat : 0f;
        }

        public SpawnerData(string prefabName, float logicUnits)
        {
            this.prefabName = prefabName;
            this.logicUnits = logicUnits;
        }

        public string prefabName { get; set; }
        public float logicUnits { get; set; }

        public JSONNode ToJSON()
        {
            JSONNode returnValue = new JSONClass();
            returnValue.Add(ATT_PREFAB_NAME, prefabName);
            returnValue.Add(ATT_LOGIC_UNITS_NAME, logicUnits.ToString());
            return returnValue;
        }

        public void ResetToDefault()
        {
            logicUnits = DEFAULT_LOGIC_UNITS;
        }
    }
    
    public Dictionary<string, SpawnerData> SpawnerDatas { get; set; }

    private void AddSpawnerData(string prefabName, float logicUnits)
    {
        if (SpawnerDatas == null)
        {
            SpawnerDatas = new Dictionary<string, SpawnerData>();
        }

        if (SpawnerDatas.ContainsKey(prefabName))
        {           
            SpawnerDatas[prefabName].logicUnits = logicUnits;            
        }
        else
        {
            SpawnerDatas.Add(prefabName, new SpawnerData(prefabName, logicUnits));
        }
    }

    private void AddSpawnerData(SpawnerData spawnerData)
    {
        if (spawnerData != null)
        {
            AddSpawnerData(spawnerData.prefabName, spawnerData.logicUnits);
        }
    }

    public void Reset()
    {
        if (SpawnerDatas != null)
        {
            SpawnerDatas.Clear();
        }
    }

    public void Load(JSONNode data)
    {
        Reset();

        if (data != null)
        {
            // Spawners
            if (data.ContainsKey("spawners"))
            {
                SimpleJSON.JSONArray spawners = data["spawners"] as SimpleJSON.JSONArray;                
                if (spawners != null)
                {
                    int count = spawners.Count;                   
                    for (int i = 0; i < count; i++)
                    {
                        AddSpawnerData(SpawnerData.GetPrefabName(spawners[i]), SpawnerData.GetLogicUnits(spawners[i]));
                    }
                }
            }
        }

        isDirty = false;
    }

    public JSONNode ToJSON()
    {
        // Create new object, initialize and return it
        SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

        SimpleJSON.JSONArray spawners = new SimpleJSON.JSONArray();
        if (SpawnerDatas != null)
        {
            foreach (KeyValuePair<string, SpawnerData> pair in SpawnerDatas)
            {
                spawners.Add(pair.Value.ToJSON());
            }
        }

        data.Add("spawners", spawners);

        return data;
    }

    public void ParseSpawners(List<string> spawnerNames)
    {        
        if (spawnerNames != null)
        {           
            // Deletes obsolete spawners
            if (SpawnerDatas != null && SpawnerDatas.Count > 0)
            {
                foreach (KeyValuePair<string, SpawnerData> pair in SpawnerDatas)
                {
                    if (!spawnerNames.Contains(pair.Key))
                    {
                        SpawnerDatas.Remove(pair.Key);
                        isDirty = true;                   
                    }
                }
            }

            // Adds new spawners
            int i;
            int count = spawnerNames.Count;
            string prefabName;
            for (i = 0; i < count; i++)
            {
                prefabName = spawnerNames[i];
                if (SpawnerDatas == null || !SpawnerDatas.ContainsKey(prefabName))
                {
                    // By default 1 logic unit is assigned
                    AddSpawnerData(prefabName, SpawnerData.DEFAULT_LOGIC_UNITS);
                    isDirty = true;
                }
            }            
        }        
    }

    public bool isDirty { get; set; }

    public void ResetSpawnersToDefault()
    {
        if (SpawnerDatas != null)
        {
            foreach (KeyValuePair<string, SpawnerData> pair in SpawnerDatas)
            {
                pair.Value.ResetToDefault();
            }

            isDirty = true;
        }
    }

    public float GetLogicUnits(string prefabName)
    {
        float returnValue = 0f;
        if (SpawnerDatas != null && SpawnerDatas.ContainsKey(prefabName))
        {
            returnValue = SpawnerDatas[prefabName].logicUnits;
        }

        return returnValue;
    }

    public float SetLogicUnits(string prefabName, float value)
    {
        float returnValue = 0f;
        if (SpawnerDatas != null && SpawnerDatas.ContainsKey(prefabName))
        {
            if (SpawnerDatas[prefabName].logicUnits != value)
            {
                SpawnerDatas[prefabName].logicUnits = value;
                isDirty = true;
            }
        }

        return returnValue;
    }
}
