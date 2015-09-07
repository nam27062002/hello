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

	private Bounds m_bounds;
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

		m_bounds = GetComponent<RectArea2D>().bounds;
		m_bounds.extents = new Vector3(m_bounds.extents.x, m_bounds.extents.y, 1000);

		Messenger.AddListener<GameObject>("SpawnOutOfRange", OnSpawnOutOfRange);
	}

	void OnDestroy(){
		Messenger.RemoveListener<GameObject>("SpawnOutOfRange", OnSpawnOutOfRange);
	}
	
	// Update is called once per frame
	void Update() {
	
		// Update this spawner only if player is inside
		Vector3 playerPos = m_player.transform.position;

		if (playerPos.x > m_bounds.min.x && playerPos.x < m_bounds.max.x 
		&&  playerPos.y > m_bounds.min.y && playerPos.y < m_bounds.max.y) {

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

		if (m_bounds.Contains(pos)) {

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

	public void OnSpawnOutOfRange(GameObject obj) {

		for (int i = 0; i < m_spawnedList.Count; i++){
			if (m_spawnedList[i].group == obj) {
				m_spawnedList.Remove(m_spawnedList[i]);
				DestroyObject(obj);
				break;
			}
		}
	}
}
