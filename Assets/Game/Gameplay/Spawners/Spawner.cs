using UnityEngine;
using System.Collections;

public class Spawner : AbstractSpawner {
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

	public enum SpawnPointSeparation {
		Sphere = 0,
		Line
	}

	private enum EState {
		Init = 0,
		Respawning,
		Create_Instances,        
		Activating_Instances,
		Alive
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
	[SerializeField] public DragonTier m_minTier = DragonTier.TIER_0;

	[Tooltip("Spawners may not be present on every run (percentage).")]
	[SerializeField][Range(0f, 100f)] public float m_activationChance = 100f;

	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] public SpawnCondition[] m_activationTriggers;
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers;
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}

	[Separator("Respawn")]
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);
	[SerializeField] private SpawnPointSeparation m_homePosMethod = SpawnPointSeparation.Sphere;
	[SerializeField] private Range m_homePosDistance = new Range(1f, 2f);
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected AreaBounds m_area;
	public override AreaBounds area { 
		get { 
			if (m_guideFunction != null) {
				return m_guideFunction.GetBounds();
			} else {
				return m_area;
			}
		} 
	}
	
	protected EntityGroupController m_groupController;
	
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

	private EState m_state = EState.Init;
    private EState State {
        get { return m_state; }
        set { m_state = value; }
    }

    /// <summary>
    /// Watch used to meassure time time spent on spawning entities. It's defined static to save memory.
    /// </summary>
    private static System.Diagnostics.Stopwatch sm_watch;

    private int m_entitiesActivated;
    private int m_rail = 0;

    //-----------------------------------------------
    // Methods
    //-----------------------------------------------
    // Use this for initialization
    protected override void StartExtended() {
		float rnd = Random.Range(0f, 100f);

		if (InstanceManager.player.data.tier >= m_minTier) {
			if (!string.IsNullOrEmpty(m_entityPrefabStr) && rnd <= m_activationChance) {

				if (m_quantity.max < m_quantity.min) {
					m_quantity.min = m_quantity.max;
				}

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

				gameObject.SetActive(false);

				return;
			}
		}

		// we are not goin to use this spawner, lets destroy it
		Destroy(gameObject);        
    }

	private void OnDestroy() {
		if (SpawnerManager.isInstanceCreated)
			SpawnerManager.instance.Unregister(this);
	}

	public override void Initialize() {
		m_respawnTime = -1;
		m_respawnCount = 0;

		m_entityAlive = 0;
		m_entitySpawned = 0;
		m_entitiesKilled = 0;

		m_allEntitiesKilledByPlayer = false;

		m_readyToBeDisabled = false;

		State = EState.Respawning;
	}

	public void ResetSpawnTimer()
	{
		m_respawnTime = -1;
	}    

    public override void ForceRemoveEntities() {
        for (int i = 0; i < m_entitySpawned; i++)
        {
            if (m_entities[i] != null)
            {
                RemoveEntity(m_entities[i], false);
            }
        }

        m_entityAlive = 0;
        m_entitiesActivated = 0;
        m_respawnTime = -1;
        m_allEntitiesKilledByPlayer = false;

        State = EState.Respawning;
    }

    // entities can remove themselves when are destroyed by the player or auto-disabled when are outside of camera range
    protected override bool RemoveEntityExtended(GameObject _entity, bool _killedByPlayer) {        
		bool found = false;
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] == _entity) {				
				m_entities[i] = null;
				if (_killedByPlayer) {
					m_entitiesKilled++;
				}
				m_entityAlive--;

                if (ProfilerSettingsManager.ENABLED) {
                    RemoveFromTotalLogicUnits(1);
                }
                         
                found = true;
			}
		}

		if (found) {
			// all the entities are dead
			if (m_entityAlive == 0 && m_entitiesActivated == m_entitySpawned) {
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

				State = EState.Respawning;

				if (m_readyToBeDisabled) {
					SpawnerManager.instance.Unregister(this);
					Destroy(gameObject);
				}
			}
		}

        return found;		
	}

    protected override void ReturnEntityToPool(GameObject _entity) {
        PoolManager.ReturnInstance(m_entityPrefabStr, _entity);
    }

    public override bool CanRespawn() {		
		// Ignore all logic for always active spawners
		if (State == EState.Respawning) {
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
					Destroy(gameObject);
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

		// If already ready to be disabled, no need for further checks
		if(m_readyToBeDisabled) return false;

		if(State != EState.Respawning) return false;

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
    
	public override bool Respawn() {

		if (State == EState.Respawning) {
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

			State = EState.Create_Instances;

			return false;
		}

		if (State == EState.Create_Instances) {
            if (sm_watch == null) {
                sm_watch = new System.Diagnostics.Stopwatch();
                sm_watch.Start();
            }

            long start = sm_watch.ElapsedMilliseconds;                       
			for (uint i = m_entityAlive; i < m_entitySpawned; i++) {			
				m_entities[i] = PoolManager.GetInstance(m_entityPrefabStr, false);
				m_entityAlive++;

                if (ProfilerSettingsManager.ENABLED) {
                    AddToTotalLogicUnits(1);
                }

                if (sm_watch.ElapsedMilliseconds - start >= SpawnerManager.SPAWNING_MAX_TIME) {
					break;
				}
			}

			if (m_entityAlive == m_entitySpawned) {
                m_entitiesActivated = 0;
                State = EState.Activating_Instances;
			}

			return false;
		}
        
        if (State == EState.Activating_Instances)
        {
            ActivateEntities();

            if (m_entitiesActivated == m_entitySpawned)
            {
                // Disable this spawner after a number of spawns
                if (m_allEntitiesKilledByPlayer && m_maxSpawns > 0)
                {
                    m_respawnCount++;
                    if (m_respawnCount == m_maxSpawns)
                    {
                        gameObject.SetActive(false);
                        SpawnerManager.instance.Unregister(this);
                    }
                }

                State = EState.Alive;                
            }
            return false;
        }

        return State == EState.Alive;
	}

    private void ActivateEntities() {
        long start = sm_watch.ElapsedMilliseconds;
        while (m_entitiesActivated < m_entitySpawned) {
            GameObject spawning = m_entities[m_entitiesActivated];
            if (!spawning.activeSelf) {
                spawning.SetActive(true);
            }           

            Vector3 pos = transform.position;
            if (m_guideFunction != null) {
                m_guideFunction.ResetTime();
            }

            if (m_entitiesActivated > 0) {
                pos += RandomStartDisplacement(m_entitiesActivated); // don't let multiple entities spawn on the same point
            }

            spawning.transform.position = pos;
            spawning.transform.localScale = Vector3.one * m_scale.GetRandom();

            m_entitiesActivated++;

            Entity entity = spawning.GetComponent<Entity>();
            if (entity != null) {
                entity.Spawn(this); // lets spawn Entity component first
            }

            AI.Machine machine = spawning.GetComponent<AI.Machine>();
            if (machine != null) {
                machine.Spawn(this);
                if (m_groupController)
                    machine.EnterGroup(ref m_groupController.flock);
            }

            AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
            if (pilot != null) {
                pilot.SetRail(m_rail, (int)m_rails);
                m_rail = (m_rail + 1) % (int)m_rails;
                pilot.guideFunction = m_guideFunction;
                pilot.Spawn(this);
            }

            ISpawnable[] components = spawning.GetComponents<ISpawnable>();
            foreach (ISpawnable component in components) {
                if (component != entity && component != pilot && component != machine) {
                    component.Spawn(this);
                }
            }

            if (sm_watch.ElapsedMilliseconds - start >= SpawnerManager.SPAWNING_MAX_TIME) {
                break;
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

	public override void DrawStateGizmos() {
		switch (State) {
			case EState.Init:					Gizmos.color = Color.grey; 		break;
			case EState.Respawning: 				Gizmos.color = Color.yellow; 	break;
			case EState.Create_Instances: 		Gizmos.color = Color.red; 		break;            
			case EState.Activating_Instances: 	Gizmos.color = Color.blue; 		break;
			case EState.Alive:					Gizmos.color = Color.green; 	break;
		}
		Gizmos.DrawWireSphere(transform.position, 0.25f * GizmosExt.GetGizmoSize(transform.position));
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

		DrawStateGizmos();
	}



	protected Vector3 RandomStartDisplacement(int _index)	{
		Vector3 v = Vector3.zero;

		float d = m_homePosDistance.distance;
		int s = (_index % 2 == 0)? -1 : 1;
		_index = Mathf.FloorToInt((_index * s) / 2f) + s;

		switch (m_homePosMethod) {
			case SpawnPointSeparation.Sphere:
				v = Random.onUnitSphere * _index * (m_homePosDistance.min + Random.Range(-d * 0.5f, d * 0.5f));
				break;

			case SpawnPointSeparation.Line:
				v = Vector3.right * _index * (m_homePosDistance.min + Random.Range(-d * 0.5f, d * 0.5f));
				break;
		}

		v.z = 0f;
		return v;
	}

    #region profiler
    private static float sm_totalLogicUnits = 0f;    
    public static float totalLogicUnitsSpawned
    {
        get
        {
            return sm_totalLogicUnits;
        }       
    }

    private static int sm_totalEntities = 0;
    public static int totalEntities
    {
        get
        {
            return sm_totalEntities;
        }
    }

    private void AddToTotalLogicUnits(int amount)
    {
        float logicUnitsCoef = 1f;
        ProfilerSettings settings = ProfilerSettingsManager.SettingsCached;
        if (settings != null)
        {            
            logicUnitsCoef = settings.GetLogicUnits(m_entityPrefabStr);                        
        }

        sm_totalEntities += amount;
        sm_totalLogicUnits += logicUnitsCoef * amount;
        if (sm_totalLogicUnits < 0f)
        {
            sm_totalLogicUnits = 0f;
        }
    }

    private void RemoveFromTotalLogicUnits(int amount)
    {
        AddToTotalLogicUnits(-amount);        
    }

    public static void ResetTotalLogicUnitsSpawned()
    {
        sm_totalLogicUnits = 0f;
        sm_totalEntities = 0;
    }
    #endregion
}
