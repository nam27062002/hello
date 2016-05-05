using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour, ISpawner {


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Header("Entity")]
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area.")]
	[SerializeField] public GameObject m_entityPrefab;
	[SerializeField] public RangeInt m_quantity = new RangeInt(1, 1);
	[SerializeField] public Range	 m_scale = new Range(1f, 1f);
	[CommentAttribute("Amount of points obtained after killing the whole flock. Points are multiplied by the amount of entities spawned.")]
	[SerializeField] private int m_flockBonus = 0;

	[Header("Activation")]
	[SerializeField] private bool m_alwaysActive = false;
	[SerializeField] private bool m_activeOnStart = false;
	[SerializeField] private float m_enableTime;
	[SerializeField] private float m_disableTime;

	[Header("Respawn")]
	[SerializeField] private Range m_spawnTime = new Range(60f, 120f);
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected AreaBounds m_area;
	protected EntityGroupController m_groupController;

	private uint m_entityAlive;
	private uint m_entitySpawned;
	private uint m_entitiesKilled; // we'll use this to give rewards if the dragon destroys a full flock
	private bool m_allEntitiesKilledByPlayer;
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
	protected virtual void Start() {
		m_entities = new GameObject[m_quantity.max];

		PoolManager.CreatePool(m_entityPrefab, Mathf.Max(15, m_entities.Length), true);

		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();

		m_area = GetArea();

		m_groupController = GetComponent<EntityGroupController>();
		if (m_groupController) {
			m_groupController.Init(m_quantity.max);
		}

		SpawnerManager.instance.Register(this);

		gameObject.SetActive(false);
	}

	public void Initialize() {
		m_enableTimer = m_enableTime;
		m_disableTimer = m_disableTime;

		m_respawnTimer = 0;
		m_respawnCount = 0;

		m_entityAlive = 0;
		m_entitySpawned = 0;
		m_entitiesKilled = 0;

		m_allEntitiesKilledByPlayer = false;

		if (m_activeOnStart) {
			Spawn();
		}
	}

	public void ResetSpawnTimer()
	{
		m_respawnTimer = 0;
	}

	public void ForceRemoveEntities() {
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] != null) {
				PoolManager.ReturnInstance(m_entities[i]);
				m_entities[i] = null;
			}
		}

		m_entityAlive = 0;
		m_respawnTimer = 0;
		m_allEntitiesKilledByPlayer = false;
	}

	// entities can remove themselves when are destroyed by the player or auto-disabled when are outside of camera range
	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] == _entity) 
			{
				PoolManager.ReturnInstance( m_entities[i] );
				m_entities[i] = null;
				if (_killedByPlayer) {
					m_entitiesKilled++;
				}
				m_entityAlive--;
			}
		}

		// all the entities are dead
		if (m_entityAlive == 0) {
			m_allEntitiesKilledByPlayer = m_entitiesKilled == m_entitySpawned;

			if (m_allEntitiesKilledByPlayer) {
				// check if player has destroyed all the flock
				if (m_flockBonus > 0) {
					Reward reward = new Reward();
					reward.score = (int)(m_flockBonus * m_entitiesKilled);
					Messenger.Broadcast<Transform, Reward>(GameEvents.FLOCK_EATEN, _entity.transform, reward);
				}
			} else {
				m_respawnTimer = 0; // instant respawn, because player didn't kill all the entities
			}
		}
	}
		
	public void UpdateTimers() {		
		if (m_alwaysActive) {
			if (m_entityAlive == 0) {
				Spawn();
			}
		} else {
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
						if (m_camera != null && m_camera.IsInsideActivationArea(transform.position)) {
							Spawn();
							m_respawnTimer = m_spawnTime.GetRandom();
						}
					}
				}
				
				// Check if we have to disable this spawner after few seconds
				if (m_disableTimer > 0) {
					m_disableTimer -= Time.deltaTime;
					if (m_disableTimer <= 0) {
						gameObject.SetActive(false);
						SpawnerManager.instance.Unregister(this);
					}
				}
			}
		}
	}

	public void UpdateLogic() {
		ExtendedUpdateLogic();
	}

	public void Respawn() {

	}

	private void Spawn() {
		m_entitySpawned = (uint)m_quantity.GetRandom();
		for (int i = 0; i < m_entitySpawned; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_entityPrefab.name);
			m_entityAlive++;
		}
		m_entitiesKilled = 0;

		ExtendedSpawn();

		if (m_groupController) {
			for (int i = 0; i < m_entities.Length; i++) {
				if (m_entities[i] != null) {
					EntityGroupBehaviour groupBehaviour = m_entities[i].GetComponent<EntityGroupBehaviour>();
					if (groupBehaviour != null) {
						groupBehaviour.AttachGroup(m_groupController);
					}
				}
			}
		}

		for (int i = 0; i < m_entitySpawned; i++) {			
			SpawnBehaviour spawn = m_entities[i].GetComponent<SpawnBehaviour>();
			Vector3 pos = transform.position;
			if (i > 0) 
			{
				pos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
			}

			spawn.Spawn(this, i, pos, m_area);
			spawn.transform.localScale = Vector3.one * m_scale.GetRandom();
		}

		// Disable this spawner after a number of spawns
		if (m_allEntitiesKilledByPlayer && m_maxSpawns > 0) {
			m_respawnCount++;
			if (m_respawnCount == m_maxSpawns) {
				gameObject.SetActive(false);
				SpawnerManager.instance.Unregister(this);
			}
		}

		m_allEntitiesKilledByPlayer = false;
	}

	protected virtual void ExtendedSpawn() {}
	protected virtual void ExtendedUpdateLogic() {}

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

	virtual protected Vector3 RandomStartDisplacement()
	{
		return Random.onUnitSphere * 2f;
	}
}
