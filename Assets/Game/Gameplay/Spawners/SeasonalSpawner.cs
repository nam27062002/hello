using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonalSpawner : Spawner 
{

	[System.Serializable]
	public class SeasonalConfig{
		public string m_season;

		[EntitySeasonalPrefabListAttribute]
		public string m_spawner;
	}

	public List<SeasonalConfig> m_spawnConfigs = new List<SeasonalConfig>();


	protected override void OnStart() {
		bool season = false;
		for( int i = 0; i<m_spawnConfigs.Count; ++i ){
			if ( m_spawnConfigs[i].m_season.Equals( SeasonManager.activeSeason ) )
			{
				m_entityPrefabList[0].name = m_spawnConfigs[i].m_spawner;
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
