using UnityEngine;
using System.Collections;

public class WindTrailManagement : MonoBehaviour 
{
	Pool m_trailPool;
	GameObject m_trailPrefab;
	bool m_canSpawnTrails;
	float timeToSpawn = 0;
	Vector2 m_spawnSize;

	// Use this for initialization
	void Start () 
	{
		m_trailPrefab = Resources.Load("Game/Assets/FX/WindTrail") as GameObject;
		m_trailPool = new Pool(m_trailPrefab, m_trailPrefab.name, null, null, 10, true, true, true);	// [AOC] Create new container if given container is the Pool Manager.
		m_canSpawnTrails = true;
		m_spawnSize = new Vector2( 10, 10);
		timeToSpawn = Random.Range( 1.0f, 10.0f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (m_canSpawnTrails)
		{
			timeToSpawn -= Time.deltaTime;
			if ( timeToSpawn < 0 )
			{
				SpawnTrail();
				timeToSpawn = Random.Range( 1.0f, 10.0f);
			}
		}
	}

	void SpawnTrail()
	{
		GameObject newTrail = m_trailPool.Get(true);
		// newTrail.transform.parent = null;
		newTrail.transform.position = transform.position + new Vector3( Random.Range( -m_spawnSize.x, m_spawnSize.x), Random.Range(-m_spawnSize.y, m_spawnSize.y), 0 );
		WindTrail wt = newTrail.GetComponent<WindTrail>();
		if ( wt != null )
			wt.SetOriginPool( m_trailPool );
	}
}
