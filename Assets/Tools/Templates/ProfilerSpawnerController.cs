using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfilerSpawnerController : MonoBehaviour
{
    [SerializeField]
    public Dropdown m_numEntities;

    [SerializeField]
    public Dropdown m_prefabToSpawn;
    private List<string> m_prefabNames;

    private const int MAX_NUM_ENTITIES = 200;
    private const int NUM_ENTITIES_PERIOD = 10;

    private Dictionary<string, GameObject> m_prefabs;

    [SerializeField]
    private Button m_spawnButton;

    private List<GameObject> m_gameObjects;

    void Awake()
    {
        ContentManager.InitContent();

        if (!ProfilerSettingsManager.IsLoaded)
        {
            ProfilerSettingsManager.Load(null);
        }

        if (m_spawnButton != null)
        {
            m_spawnButton.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        List<Dropdown.OptionData> options = null;

        // Options for the num entities drop down are created
        if (m_numEntities != null)
        {
            m_numEntities.ClearOptions();

            Dropdown.OptionData optionData;
            options = new List<Dropdown.OptionData>();

            if (NUM_ENTITIES_PERIOD >= 10)
            {
                for (int i = 1; i < 10; i++)
                {
                    optionData = new Dropdown.OptionData();
                    optionData.text = i + "";
                    options.Add(optionData);
                }
            }

            for (int i = NUM_ENTITIES_PERIOD; i <= MAX_NUM_ENTITIES; i += NUM_ENTITIES_PERIOD)
            {
                optionData = new Dropdown.OptionData();
                optionData.text = i + "";
                options.Add(optionData);
            }

            m_numEntities.AddOptions(options);

            int value = (ProfilerSettingsManager.Spawner_NumEntities / NUM_ENTITIES_PERIOD) - 1;
            if (value >= MAX_NUM_ENTITIES / NUM_ENTITIES_PERIOD)
            {
                value = MAX_NUM_ENTITIES / NUM_ENTITIES_PERIOD;
            }

            m_numEntities.value = value;
            options = null;

            SetNumEntities(1);
        }

        // Options for the prefab drop down have to be created        
        if (m_prefabToSpawn != null)
        {
            m_prefabToSpawn.ClearOptions();
            options = new List<Dropdown.OptionData>();
        }

        ProfilerSettings settings = ProfilerSettingsManager.SettingsCached;
        if (settings != null)
        {
            Dictionary<string, ProfilerSettings.SpawnerData> spawnerDatas = settings.SpawnerDatas;
            if (spawnerDatas != null)
            {
                m_prefabNames = new List<string>();

                Dropdown.OptionData optionData;
                foreach (KeyValuePair<string, ProfilerSettings.SpawnerData> pair in spawnerDatas)
                {
                    m_prefabNames.Add(pair.Key);

                    if (options != null)
                    {
                        optionData = new Dropdown.OptionData();
                        optionData.text = pair.Key;
                        options.Add(optionData);
                    }                   
                }
            }
        }

        if (m_prefabToSpawn != null)
        {
            m_prefabToSpawn.AddOptions(options);
        }

        if (m_prefabNames != null)
        {
            string prefabToSpawn = ProfilerSettingsManager.Spawner_Prefab;

            // If there's no prefab to spawn assigned yet then the first of the list should be used
            if (string.IsNullOrEmpty(prefabToSpawn))
            {
                if (m_prefabNames.Count > 0)
                {
                    SetPrefabToSpawn(0);
                }
            }
            else
            {
                int index = m_prefabNames.IndexOf(ProfilerSettingsManager.Spawner_Prefab);
                if (index > -1)
                {
                    m_prefabToSpawn.value = index;
                }
            }
        }       
    }

    public void Update()
    {
        if (m_spawnButton != null && ContentManager.ready)
        {
            m_spawnButton.gameObject.SetActive(true);
        }
    }

    public void SetNumEntities(int optionId)
    {
        if (optionId < 10)
        {
            ProfilerSettingsManager.Spawner_NumEntities = optionId + 1;
        }
        else
        {
            ProfilerSettingsManager.Spawner_NumEntities = (optionId - 8) * NUM_ENTITIES_PERIOD;
        }
    }

    public void SetPrefabToSpawn(int optionId)
    {
        if (m_prefabNames != null && optionId > -1 && optionId < m_prefabNames.Count)
        {
            ProfilerSettingsManager.Spawner_Prefab = m_prefabNames[optionId];
        }
    }

    public void Spawn()
    {
        if (ProfilerSettingsManager.Spawner_NumEntities > 0 && !string.IsNullOrEmpty(ProfilerSettingsManager.Spawner_Prefab))
        {
            Object prefab = GetPrefab(ProfilerSettingsManager.Spawner_Prefab);
            if (prefab == null)
            {
                Debug.LogError("prefab " + ProfilerSettingsManager.Spawner_Prefab + " not found");
            }
            else
            {
                if (m_gameObjects == null)
                {
                    m_gameObjects = new List<GameObject>();
                }

                for (int i = 0; i < ProfilerSettingsManager.Spawner_NumEntities; i++)
                {
                    Object o = Instantiate(prefab);
                    if (o != null)
                    {
                        GameObject go = o as GameObject;
                        go.transform.position = new Vector3(0f + i * 1f, 0f, 0f);
                        m_gameObjects.Add(go);

                        /*
                        IEntity entity = go.GetComponent<IEntity>();
                        if (entity != null)
                        {                            
                            entity.Spawn(null); // lets spawn Entity component first
                           //OnEntitySpawned(entity, EntitiesSpawned, startPosition);
                        }

                        IViewControl view = go.GetComponent<IViewControl>();
                        if (view != null)
                        {
                            view.Spawn(null);
                        }

                        AI.IMachine machine = go.GetComponent<AI.IMachine>();
                        if (machine != null)
                        {
                            machine.Spawn(null);
                            //OnMachineSpawned(machine);
                        }

                        AI.AIPilot pilot = go.GetComponent<AI.AIPilot>();
                        if (pilot != null)
                        {
                            //OnPilotSpawned(pilot);
                            pilot.Spawn(null);
                        }

                        ISpawnable[] components = go.GetComponents<ISpawnable>();
                        foreach (ISpawnable component in components)
                        {
                            if (component != entity && component != pilot && component != machine && component != view)
                            {
                                component.Spawn(null);
                            }
                        }*/
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Can't spawn " + ProfilerSettingsManager.Spawner_NumEntities + " units of prefab " + ProfilerSettingsManager.Spawner_Prefab);
        }
    }   

    private GameObject GetPrefab(string path)
    {
        GameObject returnValue = null;

        if (m_prefabs == null)
        {
            m_prefabs = new Dictionary<string, GameObject>();
        }

        if (m_prefabs.ContainsKey(path))
        {
            returnValue = m_prefabs[path];
        }
        else
        {
            returnValue = Resources.Load(IEntity.ENTITY_PREFABS_PATH + path) as GameObject;
            m_prefabs.Add(path, returnValue);
        }


        return returnValue;
    }

    public void CleanUp()
    {
        if (m_gameObjects != null)
        {
            for (int i = 0; i < m_gameObjects.Count; i++)
            {
                Destroy(m_gameObjects[i]);
            }

            m_gameObjects.Clear();
            System.GC.Collect();
        }
    }
}