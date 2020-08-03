using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalSpawner : Spawner {

    [System.Serializable]
    public class SeasonalConfig {
        [SeasonList]
        public string m_season;

        [EntitySeasonalPrefabListAttribute]
        public string[] m_spawners;
    }

    public List<SeasonalConfig> m_spawnConfigs = new List<SeasonalConfig>();


    protected override void OnStart() {
        bool season = false;
        for (int i = 0; i < m_spawnConfigs.Count; ++i) {
            if (m_spawnConfigs[i].m_season.Equals(SeasonManager.activeSeason)) {
                int count = m_spawnConfigs[i].m_spawners.Length;
                m_entityPrefabList = new EntityPrefab[count];

                for (int j = 0; j < count; ++j) {
                    m_entityPrefabList[j] = new EntityPrefab();
                    m_entityPrefabList[j].name = m_spawnConfigs[i].m_spawners[j];
                }
                season = true;
                break;
            }
        }
        if (season) {
            base.OnStart();
        } else {
            Destroy(gameObject);
        }
    }

    public override List<string> GetPrefabList() {
        List<string> list = new List<string>();
        for (int s = 0; s < m_spawnConfigs.Count; ++s) {
            for (int j = 0; j < m_spawnConfigs[s].m_spawners.Length; ++j) {
                list.Add(m_spawnConfigs[s].m_spawners[j]);
            }
        }
        return list;
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        
#if UNITY_EDITOR
        // Only if editor allows it
        if(showSpawnerInEditor) 
        {
            // Draw icon! - only in editor!
            
            // Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
            if (this.m_spawnConfigs != null && this.m_spawnConfigs.Count > 0) {
                Gizmos.DrawIcon(transform.position, IEntity.ENTITY_PREFABS_PATH + this.m_spawnConfigs[0].m_spawners[0], true);
            }
            
        }
#endif

    }
}
