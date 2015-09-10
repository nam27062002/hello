using UnityEngine;
using System.Collections;

public class SingleSpawner : MonoBehaviour {


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private GameObject m_entityPrefab;
	[SerializeField] private float m_spawnTime;
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private float m_spawnTimer;

	private AreaBounds m_area;

	private GameObject m_entity;
	
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
		
		InstanceManager.pools.CreatePool(m_entityPrefab);
	
		m_area = GetComponent<Area>().bounds;

		m_spawnTimer = m_spawnTime;
		m_entity = null;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (m_spawnTimer > 0) {

			m_spawnTimer -= Time.deltaTime;
			if (m_spawnTimer <= 0) {

				m_spawnTimer = 0;
				m_entity = InstanceManager.pools.GetInstance(m_entityPrefab.name);
				SpawnBehaviour spawn = m_entity.GetComponent<SpawnBehaviour>();
				//spawn.Spawn(m_area);
			}
		} else if (m_entity == null || !m_entity.activeInHierarchy) {

			m_entity = null;
			m_spawnTimer = m_spawnTime;
		}
	}
}
