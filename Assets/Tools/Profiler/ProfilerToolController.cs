using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

public class ProfilerToolController : MonoBehaviour
{    
    public GameObject m_spawner;

	// Use this for initialization
	void Awake ()
    {
        Spawner spawner = m_spawner.GetComponent<Spawner>();
        if (spawner != null)
        {
            if (!ProfilerSettingsManager.IsLoaded)
            {
                ProfilerSettingsManager.Load(null);
            }

            spawner.m_quantity.min = ProfilerSettingsManager.Spawner_NumEntities;
            spawner.m_quantity.max = ProfilerSettingsManager.Spawner_NumEntities;
            string prefabName = ProfilerSettingsManager.Spawner_Prefab;
            if (!string.IsNullOrEmpty(prefabName))
            {
                spawner.m_entityPrefabStr = prefabName;
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

    void OnLevelLoaded()
    {
        Spawner spawner = m_spawner.GetComponent<Spawner>();
        if (spawner != null)
        {            
            if (spawner.CanRespawn())
            {
                while (!spawner.Respawn()) ;
            }
        }
    }    
}
