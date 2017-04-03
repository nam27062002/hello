using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public class ProfilerToolController : MonoBehaviour
{    
    public GameObject m_spawnersRoot;
    private Spawner[] m_spawners;

    private int m_spawnerNumEntities;
    private string m_spawnerPrefab;

    // Use this for initialization
    void Awake ()
    {
        m_spawners = m_spawnersRoot.GetComponentsInChildren<Spawner>();        
        if (m_spawners != null)
        {
            if (!ProfilerSettingsManager.IsLoaded)
            {
                ProfilerSettingsManager.Load(null);
            }

            Setup();
        }	    
	}

    private void Setup()
    {
        if (m_spawners != null)
        {
            m_spawnerPrefab = ProfilerSettingsManager.Spawner_Prefab;
            m_spawnerNumEntities = ProfilerSettingsManager.Spawner_NumEntities;
            if (!string.IsNullOrEmpty(m_spawnerPrefab))
            {                
                if (PoolManager.ContainsPool(m_spawnerPrefab))
                {
                    PoolManager.ResizePool(m_spawnerPrefab, m_spawnerNumEntities);
                }
                else
                {
                    PoolManager.CreatePool(m_spawnerPrefab, "Game/Entities/NewEntites/", m_spawnerNumEntities);
                }

                Spawner spawner;
                int count = m_spawners.Length;
                for (int i = 0; i < count; i++)
                {
                    spawner = m_spawners[i];
                    spawner.m_quantity.min = m_spawnerNumEntities;
                    spawner.m_quantity.max = m_spawnerNumEntities;
                    if (!string.IsNullOrEmpty(m_spawnerPrefab))
                    {
                        spawner.m_entityPrefabList[0].name = m_spawnerPrefab;
                        spawner.m_entityPrefabList[0].chance = 100;                        
                    }

                    spawner.Initialize();
                }
            }            
        }            
    }

    private void OnEnable()
    {
        // Subscribe to external events
        Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);        
    }

    /// <summary>
    /// Component disabled.
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribe from external events
        Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);        
    }

    void Update()
    {
        InstanceManager.player.dragonEatBehaviour.eatingEntitiesEnabled = false;

        if (
            m_spawnerNumEntities != ProfilerSettingsManager.Spawner_NumEntities
            || m_spawnerPrefab != ProfilerSettingsManager.Spawner_Prefab
            || Input.GetKeyDown(KeyCode.A)
           )
        {            
            SpawnerManager.instance.ForceRespawn();
            Setup();
        }
    }

    void OnLevelLoaded()
    {
		
        /*Spawner spawner = m_spawner.GetComponent<Spawner>();
        if (spawner != null)
        {            
            if (spawner.CanRespawn())
            {
                while (!spawner.Respawn()) ;
            }
        }*/
    }    
}
