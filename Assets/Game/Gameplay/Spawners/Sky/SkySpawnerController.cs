using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkySpawnerController : MonoBehaviour {
	
	struct SpawnedGroup {
		public Vector3 		position;
		public GameObject 	group;
	}


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	
	public Object[] m_spawnables;
	public float    m_spawnDistance;
	public float	m_spawnTime;
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------

	private AreaBounds m_area;
	private float m_timer;

	private DragonMotion m_player;

	List<SpawnedGroup> m_spawnedList = new List<SpawnedGroup>();
	

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------

	// Use this for initialization
	void Start () {
	
		m_player = GameObject.Find ("Player").GetComponent<DragonMotion>();
		m_timer = 0;

		m_area = GetComponent<RectArea2D>().bounds;
	}
	
	// Update is called once per frame
	void Update() {
	
		// Update this spawner only if player is inside
		Vector3 playerPos = m_player.transform.position;

		if (playerPos.x > m_area.bounds.min.x && playerPos.x < m_area.bounds.max.x 
		&&  playerPos.y > m_area.bounds.min.y && playerPos.y < m_area.bounds.max.y) {

			m_timer -= Time.deltaTime;

			if (m_timer < 0f) {
				Vector3 dir = m_player.GetDirection();
				Vector3 spawnPos = playerPos + dir * m_spawnDistance;

				if(CanCreateGroupAt(spawnPos)) {
					CreateGroup(spawnPos);
					m_timer = m_spawnTime;
				} else {
					m_timer = m_spawnTime * 0.25f;
				}
			}
		}
	}

	bool CanCreateGroupAt(Vector3 pos) {

		if (m_area.bounds.Contains(pos)) {

			float spawnDistanceSqr = m_spawnDistance * m_spawnDistance;
			for (int i = 0; i < m_spawnedList.Count; i++) {
				float distSqr = (m_spawnedList[i].position - pos).sqrMagnitude;
				if (distSqr < spawnDistanceSqr)
					return false;
			}
			return true;
		}

		return false;
	}

	void CreateGroup(Vector3 position){

		SpawnedGroup sgroup = new SpawnedGroup();
		sgroup.position = position;

		GameObject spawnable = (GameObject)Object.Instantiate(m_spawnables[Random.Range(0, m_spawnables.Length)]);
		spawnable.transform.position = position;
		spawnable.transform.parent = this.transform;

		sgroup.group = spawnable;
		m_spawnedList.Add(sgroup);
	}
}
