using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalSpawner : Spawner 
{

	[System.Serializable]
	public class SeasonalConfig{
		public string m_season;

		[EntitySeasonalPrefabListAttribute]
		public string[] m_spawners;
	}

	public List<SeasonalConfig> m_spawnConfigs = new List<SeasonalConfig>();


	protected override void OnStart() {
		bool season = false;
		for( int i = 0; i<m_spawnConfigs.Count; ++i ){
			if ( m_spawnConfigs[i].m_season.Equals( SeasonManager.activeSeason ) )
			{
                int count = m_spawnConfigs[i].m_spawners.Length;
                m_entityPrefabList.Resize(count);

                for (int j = 0; j < count; ++j) {
                    m_entityPrefabList[j].name = m_spawnConfigs[i].m_spawners[j];
                }
				season = true;
			}
		}
		if (season){
			base.OnStart();
		}else{
			Destroy( gameObject );
		}
	}
}
