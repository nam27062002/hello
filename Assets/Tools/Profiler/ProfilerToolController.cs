using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public class ProfilerToolController : MonoBehaviour
{    
    public GameObject m_spawnersRoot;

	// Use this for initialization
	void Awake ()
    {
        Spawner[] spawners = m_spawnersRoot.GetComponentsInChildren<Spawner>();        
        if (spawners != null)
        {
            if (!ProfilerSettingsManager.IsLoaded)
            {
                ProfilerSettingsManager.Load(null);
            }

            Spawner spawner;
            int count = spawners.Length;
            for (int i = 0; i < count; i++)
            {
                spawner = spawners[i];
                spawner.m_quantity.min = ProfilerSettingsManager.Spawner_NumEntities;
                spawner.m_quantity.max = ProfilerSettingsManager.Spawner_NumEntities;
                string prefabName = ProfilerSettingsManager.Spawner_Prefab;
                if (!string.IsNullOrEmpty(prefabName)) 
				{
					spawner.m_entityPrefabList[0].name = prefabName;
					spawner.m_entityPrefabList[0].chance = 100;
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

	void Update() {
		InstanceManager.player.dragonEatBehaviour.eatingEntitiesEnabled = false;
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
