using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour, ISpawner {
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
	[EnumListAttribute(typeof(DragonTier))]
	[SerializeField] private DragonTier m_minTier = DragonTier.TIER_0;

	[Tooltip("Spawners may not be present on every run (percentage).")]
	[SerializeField][Range(0f, 100f)] private float m_activationChance = 100f;

	[Tooltip("Meant for background spawners, will ignore respawn settings and activation triggers.")]
	[SerializeField] private bool m_alwaysActive = false;
	public bool alwaysActive { get { return m_alwaysActive; }}

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

	private float m_respawnTime;
	private uint m_respawnCount;

	private bool m_readyToBeDisabled;

	// Scene referemces
	private GameSceneControllerBase m_gameSceneController = null;

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
		float rnd = Random.Range(0f, 100f);

		if (InstanceManager.player.data.tier >= m_minTier) {
			if (!string.IsNullOrEmpty(m_entityPrefabStr) && rnd <= m_activationChance) {
				m_entities = new GameObject[m_quantity.max];

				if (m_rails == 0) m_rails = 1;

				m_entityPrefabPath = IEntity.ENTITY_PREFABS_PATH + m_entityPrefabStr;

				// TODO[MALH]: Get path relative to quality version
				PoolManager.CreatePool(m_entityPrefabStr, m_entityPrefabPath, Mathf.Max(15, m_entities.Length), true);

				// Get external references
				// Spawners are only used in the game and level editor scenes, so we can be sure that game scene controller will be present
				m_gameSceneController = InstanceManager.GetSceneController<GameSceneControllerBase>();

				m_area = GetArea();

				m_groupController = GetComponent<EntityGroupController>();
				if (m_groupController) {
					m_groupController.Init(m_quantity.max);
				}

				m_rect = new Rect((Vector2)transform.position, Vector2.zero);

				m_guideFunction = GetComponent<IGuideFunction>();

				SpawnerManager.instance.Register(this);
				SpawnerAreaManager.instance.Register(this);
			}
		}

		gameObject.SetActive(false);
	}

	private void OnDestroy() {
		if ( SpawnerManager.instance != null )
			SpawnerManager.instance.Unregister(this);
	}

	public void Initialize() {
		m_respawnTime = -1;
		m_respawnCount = 0;

		m_entityAlive = 0;
		m_entitySpawned = 0;
		m_entitiesKilled = 0;

		m_allEntitiesKilledByPlayer = false;

		m_readyToBeDisabled = false;
	}

	public void ResetSpawnTimer()
	{
		m_respawnTime = -1;
	}

	public void ForceRemoveEntities() {
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] != null) {
				PoolManager.ReturnInstance(m_entityPrefabStr, m_entities[i]);
				m_entities[i] = null;
			}
		}

		m_entityAlive = 0;
		m_respawnTime = -1;
		m_allEntitiesKilledByPlayer = false;
	}

	// entities can remove themselves when are destroyed by the player or auto-disabled when are outside of camera range
	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		bool found = false;
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] == _entity) 
			{
				PoolManager.ReturnInstance(m_entityPrefabStr, m_entities[i]);
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

					// Program the next spawn time
					m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();
				} else {
					m_respawnTime = -1; // instant respawn, because player didn't kill all the entities
				}

				if (m_readyToBeDisabled) {
					SpawnerManager.instance.Unregister(this);
				}
			}
		}
	}
		
	public bool CanRespawn() {		
		// Ignore all logic for always active spawners
		if (m_alwaysActive) {
			if (m_entityAlive == 0) {
				return true;
			}
		}

		// Rest of the spawners
		else {
			// If we can spawn, do it
			if(CanSpawn(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
				// If we don't have any entity alive, proceed
				if(m_entityAlive == 0) {
					// Respawn on cooldown?
					if(m_gameSceneController.elapsedSeconds > m_respawnTime) {
						// Everything ok! Spawn!
						return true;
					}
				}
			}

			// If we can't spawn and we're ready to be disabled, wait untill all entities are dead to do it
			else if(m_readyToBeDisabled) {
				if(m_entityAlive == 0) {
					SpawnerManager.instance.Unregister(this);
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Check all the required conditions (time, xp) to determine whether this spawner can spawn or not.
	/// Doesn't check respawn timer nor activation area, only time and xp constraints.
	/// </summary>
	/// <returns>Whether this spawner can spawn or not.</returns>
	/// </returns><param name="_time">Elapsed game time.</param>
	/// </returns><param name="_xp">Earned xp.</param>
	public bool CanSpawn(float _time, float _xp) {
		// If always active, we're done!
		if(m_alwaysActive) return true;

		// If already ready to be disabled, no need for further checks
		if(m_readyToBeDisabled) return false;

		// Check start conditions
		bool startConditionsOk = (m_activationTriggers.Length == 0);	// If there are no activation triggers defined, the spawner will be considered ready
		for(int i = 0; i < m_activationTriggers.Length; i++) {
			// Is this condition satisfied?
			switch(m_activationTriggers[i].type) {
				case SpawnCondition.Type.XP: {
					startConditionsOk |= (_xp >= m_activationTriggers[i].value);	// We've earned enough xp
				} break;

				case SpawnCondition.Type.TIME: {
					startConditionsOk |= (_time >= m_activationTriggers[i].value);	// We've reached the activation time
				} break;
			}

			// If one of the conditions has already triggered, no need to keep checking
			// [AOC] This would be useful if we had a lot of conditions to check, but it will usually be just one and we would be adding an extra instruction for nothing, so let's keep it commented for now
			// if(startConditionsOk) break;
		}

		// If start conditions aren't met, we can't spawn, no need to check anything else
		if(!startConditionsOk) {
			return false;
		}

		// Check end conditions
		bool endConditionsOk = true;
		for(int i = 0; i < m_deactivationTriggers.Length; i++) {
			// Is this condition satisfied?
			switch(m_deactivationTriggers[i].type) {
				case SpawnCondition.Type.XP: {
					endConditionsOk &= (_xp < m_deactivationTriggers[i].value);		// We haven't yet reached the xp limit
				} break;

				case SpawnCondition.Type.TIME: {
					endConditionsOk &= (_time < m_deactivationTriggers[i].value);	// We haven't yet reached the time limit
				} break;
			}

			// If one of the conditions has already triggered, no need to keep checking
			// [AOC] This would be useful if we had a lot of conditions to check, but it will usually be just one and we would be adding an extra instruction for nothing, so let's keep it commented for now
			// if(!endConditionsOk) break;
		}

		// If we've reached either of the end conditions, mark the spawner as ready to disable
		// Only during actual gameplay, not while using the level editor simulator!
		if(!endConditionsOk && Application.isPlaying) {
			m_readyToBeDisabled = true;
		}

		return endConditionsOk;
	}

	public bool Respawn() {
		if (m_entityAlive == 0) {
			if (m_entitiesKilled == m_entitySpawned) {
				m_entitySpawned = (uint)m_quantity.GetRandom();
			} else {
				// If player didn't killed all the spawned entities we'll re spawn only the remaining alive.
				// Also, this respawn will be instant.
				m_entitySpawned = m_entitySpawned - m_entitiesKilled;
			}
			m_entitiesKilled = 0;
			m_allEntitiesKilledByPlayer = false;
			m_respawnTime = -1;
		}

		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		watch.Start();
		for (uint i = m_entityAlive; i < m_entitySpawned; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_entityPrefabStr, false);
			m_entityAlive++;

			if (watch.ElapsedMilliseconds >= 9f) {
				break;
			}
		}

		if (m_entityAlive == m_entitySpawned) {
			Spawn();
			return true;
		}

		return false;
	}

	private void Spawn() {
		uint rail = 0;
		for (int i = 0; i < m_entitySpawned; i++) {
			GameObject spawning = m_entities[i];

			Vector3 pos = transform.position;
			if (m_guideFunction != null) {
				m_guideFunction.ResetTime();
			}

			if (i > 0) {
				pos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
			}

			spawning.transform.position = pos;
			spawning.transform.localScale = Vector3.one * m_scale.GetRandom();

			Entity entity = spawning.GetComponent<Entity>();
			if (entity != null) {
				entity.Spawn(this); // lets spawn Entity component first
			}

			AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
			if (pilot != null) {
				pilot.guideFunction = m_guideFunction;
				pilot.Spawn(this);
			}

			ISpawnable[] components = spawning.GetComponents<ISpawnable>();
			foreach (ISpawnable component in components) {
				if (component != entity && component != pilot) {
					component.Spawn(this);
				}
			}

			AI.Machine machine = spawning.GetComponent<AI.Machine>();
			if (machine != null) {
				machine.SetRail(rail, m_rails);
				rail = (rail + 1) % m_rails;

				if (m_groupController) {	
					machine.EnterGroup(ref m_groupController.flock);
				}
			}

			spawning.SetActive(true);
		}

		// Disable this spawner after a number of spawns
		if (m_allEntitiesKilledByPlayer && m_maxSpawns > 0) {
			m_respawnCount++;
			if (m_respawnCount == m_maxSpawns) {
				gameObject.SetActive(false);
				SpawnerManager.instance.Unregister(this);
			}
		}
	}

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

	virtual protected Vector3 RandomStartDisplacement()	{
		return Random.onUnitSphere * 2f;
	}
}
