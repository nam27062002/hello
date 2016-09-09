using UnityEngine;
using System.Collections;

public class SpawnerBg : MonoBehaviour, ISpawner {
	[System.Serializable]
	public class SpawnCondition {
		public enum Type {
			XP,
			TIME
		}

		public Type type = Type.XP;

		[NumericRange(0f)]	// Force positive value
		public float value = 0f;
	}

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Separator("Entity")]
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area.")]
	[SerializeField] public GameObject m_entityPrefab;

	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area.")]
	[EntityPrefabListAttribute]
	[SerializeField] public string m_entityPrefabStr = "";
	private string m_entityPrefabPath;

	[SerializeField] public RangeInt m_quantity = new RangeInt(1, 1);
	[SerializeField] public Range	 m_scale = new Range(1f, 1f);
	[SerializeField] private uint	m_rails = 1;
	[CommentAttribute("Amount of points obtained after killing the whole flock. Points are multiplied by the amount of entities spawned.")]
	[SerializeField] private int m_flockBonus = 0;

	[Separator("Activation")]
	[Tooltip("For the spawners that must spawn even when the dragon is not near (i.e. the spawners around the start area)")]
	[SerializeField] private bool m_activeOnStart = false;

	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] private SpawnCondition[] m_activationTriggers;
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers;
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}

	[Separator("Respawn")]
	[SerializeField] private Range m_spawnTime = new Range(40f, 45f);
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected AreaBounds m_area;
	public AreaBounds area { 
		get { 
			if (m_guideFunction != null) {
				return m_guideFunction.GetBounds();
			} else {
				return m_area;
			}
		} 
	}

	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	protected EntityGroupController m_groupController;
	protected IGuideFunction m_guideFunction;
	public IGuideFunction guideFunction { get { return m_guideFunction; } }

	private uint m_entityAlive;
	private uint m_entitySpawned;
	private uint m_entitiesKilled; // we'll use this to give rewards if the dragon destroys a full flock
	private bool m_allEntitiesKilledByPlayer;
	protected GameObject[] m_entities; // list of alive entities

	private float m_respawnTimer;
	private uint m_respawnCount;

	private bool m_readyToBeDisabled;

	private GameCamera m_newCamera;

	// Level editing stuff
	private bool m_showSpawnerInEditor = true;
	public bool showSpawnerInEditor {
		get { return m_showSpawnerInEditor; }
		set { m_showSpawnerInEditor = value; }
	}
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Start() {
		m_rect = new Rect((Vector2)transform.position, Vector2.zero);
		m_entities = new GameObject[m_quantity.max];

		if (m_rails == 0) m_rails = 1;

		m_entityPrefabPath = IEntity.ENTITY_PREFABS_PATH + m_entityPrefabStr;
		PoolManager.CreatePool(m_entityPrefabStr, m_entityPrefabPath, Mathf.Max(15, m_entities.Length), true);

		m_newCamera = Camera.main.GetComponent<GameCamera>();

		m_area = GetArea();

		m_groupController = GetComponent<EntityGroupController>();
		if (m_groupController) {
			m_groupController.Init(m_quantity.max);
		}

		m_guideFunction = GetComponent<IGuideFunction>();

		SpawnerManager.instance.Register(this);

		gameObject.SetActive(false);
	}

	private void OnDestroy() {
		SpawnerManager.instance.Unregister(this);
	}

	public void Initialize() {

		m_respawnTimer = 0;
		m_respawnCount = 0;

		m_entityAlive = 0;
		m_entitySpawned = 0;
		m_entitiesKilled = 0;

		m_allEntitiesKilledByPlayer = false;

		m_readyToBeDisabled = false;

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
				PoolManager.ReturnInstance(m_entityPrefabStr, m_entities[i]);
				m_entities[i] = null;
			}
		}

		m_entityAlive = 0;
		m_respawnTimer = 0;
		m_allEntitiesKilledByPlayer = false;
	}

	// entities can remove themselves when are destroyed by the player or auto-disabled when are outside of camera range
	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		bool found = false;
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] == _entity) 
			{
				PoolManager.ReturnInstance( m_entityPrefabStr, m_entities[i] );
				m_entities[i] = null;
				if (_killedByPlayer) {
					m_entitiesKilled++;
				}
				m_entityAlive--;
				found = true;
			}
		}

		if (found) {
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

				if (m_readyToBeDisabled) {
					SpawnerManager.instance.Unregister(this);
				}
			}
		}
	}
		
	public void CheckRespawn() {		
		
		// If we don't have any entity alive, proceed
		if(m_entityAlive == 0) {
			// Respawn on cooldown?

			// Check activation area
			if(m_newCamera != null && m_newCamera.IsInsideBackgroundActivationArea(transform.position)) {
				Spawn();
			}
			

		}


	}

	public void UpdateLogic() {
		ExtendedUpdateLogic();
	}

	public void Respawn() {

	}

	private void Spawn() {
		if (m_entitiesKilled == m_entitySpawned) {
			m_entitySpawned = (uint)m_quantity.GetRandom();
		} else {
			// If player didn't killed all the spawned entities we'll re spawn only the remaining alive.
			// Also, this respawn will be instant.
			m_entitySpawned = m_entitySpawned - m_entitiesKilled;
		}

		for (int i = 0; i < m_entitySpawned; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_entityPrefabStr);
			m_entityAlive++;
		}
		m_entitiesKilled = 0;

		ExtendedSpawn();

		uint rail = 0;
		for (int i = 0; i < m_entitySpawned; i++) {			
			AI.AIPilot pilot = m_entities[i].GetComponent<AI.AIPilot>();
			pilot.guideFunction = m_guideFunction;

			Vector3 pos = transform.position;
			if (m_guideFunction != null) {
				m_guideFunction.ResetTime();
			}

			if (i > 0) {
				pos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
			}

			pilot.transform.position = pos;
			pilot.transform.localScale = Vector3.one * m_scale.GetRandom();

			pilot.Spawn(this);

			ISpawnable[] components = pilot.GetComponents<ISpawnable>();
			foreach (ISpawnable component in components) {
				if (component != pilot ) {
					component.Spawn(this);
				}
			}

			AI.IMachine machine = pilot.GetComponent<AI.IMachine>();
			machine.SetRail(rail, m_rails);
			rail = (rail + 1) % m_rails;

			if (m_groupController) {	
				machine.EnterGroup(ref m_groupController.flock);
			}
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

		m_respawnTimer = m_spawnTime.GetRandom();
	}

	protected virtual void ExtendedSpawn() {}
	protected virtual void ExtendedUpdateLogic() {}

	protected virtual AreaBounds GetArea() {
		Area area = GetComponent<Area>();
		if (area != null) {
			return area.bounds;
		} else {
			// spawner for static objects with a fixed position
			return new CircleAreaBounds(transform.position, 1f);
		}
	}

	void OnDrawGizmos() {
		// Only if editor allows it
		if(showSpawnerInEditor) {
			// Draw spawn area
			GetArea().DrawGizmo();

			// Draw icon! - only in editor!
			#if UNITY_EDITOR
				// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
				Gizmos.DrawIcon(transform.position, IEntity.ENTITY_PREFABS_PATH + this.m_entityPrefabStr, true);
			#endif
		}
	}

	virtual protected Vector3 RandomStartDisplacement()
	{
		return Random.onUnitSphere * 2f;
	}
}
