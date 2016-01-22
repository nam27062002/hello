using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Header("Entity")]
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area.")]
	[SerializeField] public GameObject m_entityPrefab;
	[SerializeField] public RangeInt m_quantity = new RangeInt(1, 1);
	[SerializeField] public Range	 m_scale = new Range(1f, 1f);

	[Header("Activation")]
	[SerializeField] private float m_enableTime;
	[SerializeField] private float m_disableTime;

	[Header("Respawn")]
	[SerializeField] private Range m_spawnTime = new Range(60f, 120f);
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected AreaBounds m_area;

	protected FlockController m_flockController;

	private uint m_entityAlive;
	private uint m_entitySpawned;
	private uint m_entitiesKilled; // we'll use this to give rewards if the dragon destroys a full flock
	protected GameObject[] m_entities; // list of alive entities

	private float m_enableTimer;
	private float m_disableTimer;
	private float m_respawnTimer;
	private uint m_respawnCount;

	private GameCameraController m_camera;
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Start () {		
		PoolManager.CreatePool(m_entityPrefab);
		m_entities = new GameObject[m_quantity.max];

		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();

		m_area = GetArea();


		m_flockController = GetComponent<FlockController>();
		if (m_flockController) {
			// this spawner has a flock controller! let's setup it
			m_flockController.Init(m_quantity.max);
		}
	}

	protected virtual void OnEnable() {
		m_enableTimer = m_enableTime;
		m_disableTimer = m_disableTime;

		m_respawnTimer = 0;
		m_respawnCount = 0;

		m_entityAlive = 0;
		m_entitySpawned = 0;
		m_entitiesKilled = 0;
	}

	// entities can remove themselves when are destroyed by the player or auto-disabled when are outside of camera range
	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] == _entity) {
				m_entities[i] = null;
				if (_killedByPlayer) {
					m_entitiesKilled++;
				}
				m_entityAlive--;
			}
		}

		// all the entities are 
		if (m_entityAlive == 0 && m_entitiesKilled >= 3) {
			// check if player has destroyed all the flock
			if (m_entitiesKilled == m_entitySpawned) {
				// TODO: give flock reward! rise event
			}
		}
	}

	// Update is called once per frame
	void Update () {		
		// A spawner can have a delay time before it can spawn things
		if (m_enableTimer > 0) {
			m_enableTimer -= Time.deltaTime;
			if (m_enableTimer <= 0) {
				m_enableTimer = 0;
			}
		} else {
			// re-spawn logic
			if (m_entityAlive == 0) {
				if (m_respawnTimer > 0) {
					m_respawnTimer -= Time.deltaTime;
					if (m_respawnTimer <= 0) {
						m_respawnTimer = 0;
					}
				} else {
					if (m_camera.IsInsideActivationArea(transform.position)) {
						Spawn();
						m_respawnTimer = m_spawnTime.GetRandom();
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

	void Spawn() {
		m_entitySpawned = (uint)m_quantity.GetRandom();
		for (int i = 0; i < m_entitySpawned; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_entityPrefab.name);
			m_entityAlive++;
		}
		m_entitiesKilled = 0;

		ExtendedSpawn();

		if (m_flockController) {
			for (int i = 0; i < m_entities.Length; i++) {
				if (m_entities[i] != null) {
					PreyMotion motion = m_entities[i].GetComponent<PreyMotion>();
					if (motion != null) {
						motion.AttachFlock(m_flockController);
					}
				}
			}
		}

		for (int i = 0; i < m_entitySpawned; i++) {			
			SpawnBehaviour spawn = m_entities[i].GetComponent<SpawnBehaviour>();
			Vector3 pos = transform.position;
			if (i > 0) pos += Random.onUnitSphere * 2f; // don't let multiple entities spawn on the same point

			spawn.Spawn(this, i, pos, m_area);
			spawn.transform.localScale = Vector3.one * m_scale.GetRandom();
		}

		// Disable this spawner after a number of spawns
		if (m_maxSpawns > 0) {
			m_respawnCount++;			
			if (m_respawnCount == m_maxSpawns) {
				gameObject.SetActive(false);
			}
		}
	}

	protected virtual void ExtendedSpawn() {}

	protected virtual AreaBounds GetArea() {
		Area area = GetComponent<Area>();
		if (area != null) {
			return area.bounds;
		} else {
			// spawner for static objects with a fixed position
			return new CircleAreaBounds(transform.position, 0);
		}
	}

	void OnDrawGizmos() {
		GetArea().DrawGizmo();
	}
}
