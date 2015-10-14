using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Header("Entity")]
	[SerializeField] public GameObject m_entityPrefab;
	[SerializeField] protected RangeInt m_quantity;

	[Header("Activation")]
	[SerializeField] private float m_enableTime;
	[SerializeField] private float m_disableTime;
	[SerializeField] private float m_playerDistance;

	[Header("Respawn")]
	[SerializeField] private Range m_spawnTime;
	[SerializeField] private float m_TimeInc;
	[SerializeField] private float m_TimeIncTime;
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected GameObject[] m_entities;
	protected AreaBounds m_area;

	private float m_enableTimer;
	private float m_disableTimer;
	private float m_spawnTimer;
	private uint m_spawnCount;
	private uint m_entityAlive;

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Start () {
		
		PoolManager.CreatePool(m_entityPrefab);
		m_entities = new GameObject[m_quantity.max];

		Area area = GetComponent<Area>();
		if (area != null) {
			m_area = area.bounds;
		} else {
			// spawner for static objects with a fixed position
			m_area = new CircleAreaBounds(transform.position, 0);
		}
	}

	protected virtual void OnEnable() {

		m_enableTimer = m_enableTime;
		m_disableTimer = m_disableTime;

		m_spawnTimer = 0;
		m_spawnCount = 0;
		m_entityAlive = 0;
	}

	// entities can remove themselves when are destroyed or auto-disabled
	public void RemoveEntity(GameObject _entity) {

		for (int i = 0; i < m_entities.Length; i++) {			
			if (m_entities[i] == _entity) {
				m_entities[i] = null;
				m_entityAlive--;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
		bool playerNear = IsPlayerNear();

		// A spawner can have a delay time before it can spawn things
		if (m_enableTimer > 0) {
			m_enableTimer -= Time.deltaTime;
			if (m_enableTimer <= 0) {
				m_enableTimer = 0;
			}
		} else {
			// Modify spawn time over time
			// TODO: spawners can have more or less respawn time while we keep playing

			// Spawn logic
			if (m_entityAlive == 0) {
				if (m_spawnTimer > 0) {
					m_spawnTimer -= Time.deltaTime;
					if (m_spawnTimer <= 0) {
						m_spawnTimer = 0;
					}
				} else {
					if (playerNear && !IsPlayerInsideArea()) {
						Spawn();
						m_spawnTimer = m_spawnTime.GetRandom();
					}
				}
			}
			
			// Check if we have to disable this spawner after few seconds
			if (m_disableTimer > 0) {
				m_disableTimer -= Time.deltaTime;
				if (m_disableTimer <= 0) {
					enabled = false;
				}
			}
		}
	}

	bool IsPlayerNear() {
		Vector2 playerPos = InstanceManager.player.transform.position;
		float distX = m_area.bounds.extents.x + m_playerDistance;
		float distY = m_area.bounds.extents.y + m_playerDistance;

		if (playerPos.x < m_area.bounds.center.x - distX ||  playerPos.x > m_area.bounds.center.x + distX
		||  playerPos.y < m_area.bounds.center.y - distY ||  playerPos.y > m_area.bounds.center.y + distY) {

			// disable all alive entities
			if (m_entityAlive > 0) {
				for (int i = 0; i < m_entities.Length; i++) {				
					if (m_entities[i] != null) {
						m_entities[i].SetActive(false);
						m_entities[i] = null;
					}
				}
				m_spawnTimer = 0; // this spawner will be restarted when player is near again
				m_entityAlive = 0;
			}

			return false;
		}

		return true;
	}

	bool IsPlayerInsideArea() {
		DragonPlayer player = InstanceManager.player;
		return m_area.Contains(player.transform.position);
	}

	void Spawn() {
		int count = m_quantity.GetRandom();
		for (int i = 0; i < count; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_entityPrefab.name);
			m_entityAlive++;
		}

		ExtendedSpawn();

		for (int i = 0; i < count; i++) {			
			SpawnBehaviour spawn = m_entities[i].GetComponent<SpawnBehaviour>();
			spawn.Spawn(this, m_area);
		}

		// Disable this spawner after a number of spawns
		if (m_maxSpawns > 0) {
			m_spawnCount++;			
			if (m_spawnCount == m_maxSpawns) {
				gameObject.SetActive(false);
			}
		}
	}

	protected virtual void ExtendedSpawn() {}



	void OnDrawGizmos() {
		Area area = GetComponent<Area>();
		if (area != null) {
			area.bounds.DrawGizmo();
		} else {
			Gizmos.DrawSphere(transform.position, 100);
		}
	}
}
