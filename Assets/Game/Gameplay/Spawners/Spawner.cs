using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;

public class Spawner : AbstractSpawner {

	private static float FLOCK_BONUS_MULTIPLIER = 0.1f;

	[System.Serializable]
	public class EntityPrefab {
		[EntityPrefabListAttribute]
		public string name = "";
		public float chance = 100;

		public EntityPrefab() {
			name = "";
			chance = 100;
		}
	}

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

	[System.Serializable]
	public class SpawnKillCondition {
		[EntityCategoryListAttribute]
		public string category;

		[NumericRange(0f)]	// Force positive value
		public float value = 0f;
	}

	[System.Serializable]
	public class SkuKillCondition {		
		[EntitySkuList]
		public string sku;

		[NumericRange(0f)]	// Force positive value
		public float value = 0f;
	}

	public enum SpawnPointSeparation {
		Sphere = 0,
		Line
	}

	public enum EntityGoldMode {
		Normal = 0,
		Gold,
		ReRoll
	}


	//-----------------------------------------------
	// Class members and methods
	//-----------------------------------------------
	private static Dictionary<string, float> sm_overrideSpawnFrequency = new Dictionary<string, float>(); // entities that must be spawned more often
	public static void AddSpawnFrequency(string _prefabName, float _percentage) {
		sm_overrideSpawnFrequency[_prefabName] = _percentage;
	}

	public static void RemoveSpawnFrequency(string _prefabName) {
		sm_overrideSpawnFrequency.Remove(_prefabName);
	}


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Separator("Entity")]	
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area. The spawner can create instances of multiple prefabs, spawning mixed groups (mix entities = true) or random groups each spawn (mix entities = false).")]
	[SerializeField] private bool m_mixEntities = false;
	[SerializeField] private bool m_forceHeterogeneityMix = false;
	[SerializeField] public EntityPrefab[] m_entityPrefabList = new EntityPrefab[1];

	[SerializeField] public RangeInt 	m_quantity = new RangeInt(1, 1);
	[SerializeField] private Range 		m_speedFactorRange = new Range(1f, 1f);
	[SerializeField] public Range	 	m_scale = new Range(1f, 1f);
	[SerializeField] private uint		m_rails = 1;
	[SerializeField] private bool		m_hasGroupBonus = false;

	[Separator("Activation")]
	[SerializeField] private bool 		m_eventOnly = false;
	[SerializeField] private DragonTier m_minTier = DragonTier.TIER_0;
	[SerializeField] private DragonTier m_maxTier = DragonTier.TIER_4;
	[SerializeField] private bool	    m_checkMaxTier = false;

	[Tooltip("Spawners may not be present on every run (percentage).")]
	[SerializeField][Range(0f, 100f)] public float m_activationChance = 100f;

	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] public SpawnCondition[] m_activationTriggers;
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[SerializeField] public SkuKillCondition[] m_activationKillTriggers;
	public SkuKillCondition[] activationKillTriggers { get { return m_activationKillTriggers; } }

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers;
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}

	[SerializeField] public SkuKillCondition[] m_deactivationKillTriggers = new SkuKillCondition[0];
	public SkuKillCondition[] deactivationKillTriggers { get { return m_deactivationKillTriggers; } }

	[Separator("Respawn")]
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);
	[Tooltip("NPCs which always give Premium Currency as reward have a, content based, change to not respawn reduced over kills.")]
	[SerializeField] private bool m_isPremiumCurrencyNPC = false;
	[SerializeField] private SpawnPointSeparation m_homePosMethod = SpawnPointSeparation.Sphere;
	[SerializeField] private Range m_homePosDistance = new Range(1f, 2f);
	[SerializeField] private float m_homePosLineRotation = 0f;

	[SerializeField] private int m_maxSpawns;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private int m_prefabIndex;

	private PoolHandler[] m_poolHandlers;
	private int[] m_poolHandlerIndex;
	private bool[] m_hasbeenSpawned;

	private string[] m_entitySku;
	private EntityGoldMode[] m_entityGoldMode;

	private float m_pcProbCoefA;
	private float m_pcProbCoefB;

	protected EntityGroupController m_groupController;		

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

	private int m_rail = 0;

	private float m_groupBonus = 0;

    //----------------------------------------------- 
    // Properties
    public bool HasGroupBonus
    {
        get { return m_hasGroupBonus; }
    }


    //-----------------------------------------------
    // AbstractSpawner implementation
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

	protected override void OnStart() {		
		bool enabledByEvents = true;

		if (m_eventOnly) {
			// enabledByEvents = GlobalEventManager.CanContribute() == GlobalEventManager.ErrorCode.NONE;
				// Maybe only check if joined?
			enabledByEvents = HDLiveDataManager.quest.IsRunning() && HDLiveDataManager.quest.isActive;
		}

		if (enabledByEvents) {
			float rnd = Random.Range(0f, 100f);
			DragonTier playerTier = InstanceManager.player.data.tier;

			bool enabledByTier = false;
			if (m_checkMaxTier) {
				enabledByTier = (playerTier >= m_minTier) && (playerTier <= m_maxTier);
			} else {
				enabledByTier = (playerTier >= m_minTier);
			}

			if (m_activationChance < 100f) {
				// check debug 
				if (DebugSettings.spawnChance0) {
					rnd = 100f;
				} else if (DebugSettings.spawnChance100) {
					rnd = 0f;
				}
			}

			if (InstanceManager.player != null && enabledByTier) {
				if (m_entityPrefabList != null && m_entityPrefabList.Length > 0 && rnd <= m_activationChance) {

					m_entitySku = new string[GetMaxEntities()];
					m_entityGoldMode = new EntityGoldMode[GetMaxEntities()];
					m_poolHandlerIndex = new int[GetMaxEntities()];

					for (int i = 0; i < m_entitySku.Length; i++) {
						m_entitySku[i] = "";
						m_entityGoldMode[i] = EntityGoldMode.ReRoll;
						m_poolHandlerIndex[i] = 0;
					}

					m_hasbeenSpawned = new bool[m_entityPrefabList.Length];
					for (int i = 0; i < m_hasbeenSpawned.Length; i++) {
						m_hasbeenSpawned[i] = false;
					}

					DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
					m_pcProbCoefA = def.GetAsFloat("flyingPigsProbaCoefA", 1f);
					m_pcProbCoefB = def.GetAsFloat("flyingPigsProbaCoefB", 1f);

					if (m_activationKillTriggers == null) {
						m_activationKillTriggers = new SkuKillCondition[0];
					}

					if (m_quantity.max < m_quantity.min) {
						m_quantity.min = m_quantity.max;
					}                			

					// adjust probabilities
					// and check if this spawner has an invasion enabled
					float probFactor = 0;
					for (int i = 0; i < m_entityPrefabList.Length; i++) {
						probFactor += m_entityPrefabList[i].chance;
					}

					if (probFactor > 0f) {
						probFactor = 100f / probFactor;
						for (int i = 0; i < m_entityPrefabList.Length; i++) {
							m_entityPrefabList[i].chance *= probFactor;
						}

						//sort probs
						for (int i = 0; i < m_entityPrefabList.Length; i++) {
							for (int j = 0; j < m_entityPrefabList.Length - i - 1; j++) {
								if (m_entityPrefabList[j].chance > m_entityPrefabList[j + 1].chance) {
									EntityPrefab temp = m_entityPrefabList[j];
									m_entityPrefabList[j] = m_entityPrefabList[j + 1];
									m_entityPrefabList[j + 1] = temp;
								}
							}
						}

						// clamp scale values
						if (m_scale.min < 0.95f) m_scale.min = 0.95f;
						if (m_scale.min > 1.05f) m_scale.min = 1.05f;

						if (m_scale.max > 1.05f) m_scale.max = 1.05f;
						if (m_scale.max < 0.95f) m_scale.max = 0.95f;

						bool hasOverrideSpawnFreq = false;
						float spawnFreqPercentage = 0f;

						Dictionary<string, float>.Enumerator it = sm_overrideSpawnFrequency.GetEnumerator();
						while (it.MoveNext() && !hasOverrideSpawnFreq) {
							for (int i = 0; i < m_entityPrefabList.Length; i++) {
								if (m_entityPrefabList[i].name.Contains(it.Current.Key)) {
									hasOverrideSpawnFreq = true;
									spawnFreqPercentage = it.Current.Value;
									break;
								}
							}
						}

						if (hasOverrideSpawnFreq) {
							m_spawnTime.min += m_spawnTime.min * spawnFreqPercentage / 100f;
							m_spawnTime.max += m_spawnTime.max * spawnFreqPercentage / 100f;

							for (int i = 0; i < m_activationTriggers.Length; ++i) {
								m_activationTriggers[i].value += m_activationTriggers[i].value * spawnFreqPercentage / 100f;
							}

							for (int i = 0; i < m_activationKillTriggers.Length; ++i) {
								float value = m_activationKillTriggers[i].value;
								value += value * spawnFreqPercentage / 100f;
								if (value < 1) {
									value = 1;
								}
								m_activationKillTriggers[i].value = value;
							}
						}

						RegisterInSpawnerManager();

						gameObject.SetActive(false);

						return;
					}
				}
			}
		}

		// we are not goin to use this spawner, lets destroy it
		Destroy(gameObject);        
	}

	protected override uint GetMaxEntities() {
		return (uint)m_quantity.max;
	}

	protected override void OnInitialize() {        
		m_respawnTime = -1;
		m_respawnCount = 0;		
		m_readyToBeDisabled = false;

		if (m_rails == 0) m_rails = 1;

		m_poolHandlers = new PoolHandler[m_entityPrefabList.Length];

		for (int i = 0; i < m_entityPrefabList.Length; i++) {
			m_poolHandlers[i] = PoolManager.RequestPool(m_entityPrefabList[i].name, m_entities.Length);
		}

		// Get external references
		// Spawners are only used in the game and level editor scenes, so we can be sure that game scene controller will be present
		m_gameSceneController = InstanceManager.gameSceneControllerBase;

		m_area = GetArea();

		m_groupController = GetComponent<EntityGroupController>();
		if (m_groupController) {
			m_groupController.Init(m_quantity.max);
		}

		m_guideFunction = GetComponent<IGuideFunction>();

		m_prefabIndex = GetPrefabIndex();
	}

    public override List<string> GetPrefabList() {
        List<string> list = new List<string>();
        for (int j = 0; j < m_entityPrefabList.Length; ++j) {
            list.Add(m_entityPrefabList[j].name);
        }
        return list;
    }

    protected override bool CanRespawnExtended() {
		if (m_maxSpawns > 0 && m_respawnCount >= m_maxSpawns)
		{
			m_readyToBeDisabled = true;
		}
		// If we can spawn, do it
		else if (CanSpawn(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
			// If we don't have any entity alive, proceed
			if (EntitiesAlive == 0) {
				// Respawn on cooldown?
				if (m_gameSceneController.elapsedSeconds > m_respawnTime || DebugSettings.ignoreSpawnTime) {
					if (m_isPremiumCurrencyNPC) {
						float eaten = 1f;

						string key = GetPrefabNameToSpawn((uint)m_prefabIndex);
                        if (RewardManager.npcPremiumCount.ContainsKey(key)) {
							eaten += RewardManager.npcPremiumCount[key];
						}

						float rnd = Random.Range(0f, 1f);
						float prob = m_pcProbCoefA / (m_pcProbCoefB * eaten);

						if (rnd > prob) {
							m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();
							return false;
						}
					}

					// Everything ok! Spawn!
					return true;
				}
			}
		}
		// If we can't spawn and we're ready to be disabled, wait untill all entities are dead to do it
		else if (m_readyToBeDisabled) {
			if (EntitiesAlive == 0) {
				UnregisterFromSpawnerManager();
				Destroy(gameObject);
			}
		}

		return false;
	}

	protected override uint GetEntitiesAmountToRespawn() {
		// If player didn't killed all the spawned entities we'll re spawn only the remaining alive.
		// Also, this respawn will be instant.
		return (EntitiesKilled == EntitiesToSpawn) ? (uint)m_quantity.GetRandom() : EntitiesToSpawn - EntitiesKilled;
	}    

	protected override void OnPrepareRespawning() {
		m_prefabIndex = GetPrefabIndex();
	}

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandlers[m_poolHandlerIndex[index]];
	}

	protected override string GetPrefabNameToSpawn(uint index) {
		if (m_mixEntities) {
			if (m_forceHeterogeneityMix) {
				m_prefabIndex = GetPrefabIndexHeterogeneity();
			} else {
				m_prefabIndex = GetPrefabIndex();
			}
		}

		m_poolHandlerIndex[index] = m_prefabIndex;

		return m_entityPrefabList[m_prefabIndex].name;
	}

	private int GetPrefabIndex() {
		int i = 0;
		float rand = Random.Range(0f, 100f);
		float prob = 0;

		for (i = 0; i < m_entityPrefabList.Length - 1; ++i) {
			prob += m_entityPrefabList[i].chance;

			if (rand <= prob) {
				break;
			}
		}

		return i;
	}

	private int GetPrefabIndexHeterogeneity() {
		int fallback = -1;
		float rand = Random.Range(0f, 100f);
		float prob = 0;

		for (int i = 0; i < m_entityPrefabList.Length; ++i) {
			prob += m_entityPrefabList[i].chance;

			if (!m_hasbeenSpawned[i]) {
				fallback = i;
				if (rand <= prob) {					
					m_hasbeenSpawned[i] = true;
					return i;
				}
			}
		}

		if (fallback < 0) {
			return GetPrefabIndex();
		} else {
			m_hasbeenSpawned[fallback] = true;
			return fallback;
		}
	}

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
		m_entitySku[index] = spawning.sku;
		originPos += RandomStartDisplacement((int)index); // don't let multiple entities spawn on the same point

		spawning.SetGolden(m_entityGoldMode[index]);
		m_entityGoldMode[index] = (spawning.isGolden)? EntityGoldMode.Gold : EntityGoldMode.Normal;

		spawning.transform.position = originPos;
		spawning.transform.localScale = Vector3.one * m_scale.GetRandom();
	}

	public override void ForceGolden( IEntity entity ){
		base.ForceGolden( entity );
		int l = m_entities.Length;
		for (int i = 0; i < l; ++i) {
            if (m_entities[i] == entity) {
				m_entityGoldMode[i] = (entity.isGolden) ? EntityGoldMode.Gold : EntityGoldMode.Normal;
				break;
            }
        }
    }

	protected override void OnMachineSpawned(IMachine machine, uint index) {
		if (m_groupController) {				
			machine.EnterGroup(ref m_groupController.flock);
		//	machine.position = transform.position + m_groupController.flock.GetOffset(machine, 2f);
		}
	}

	protected override void OnPilotSpawned(Pilot pilot) {
		pilot.speedFactor = m_speedFactorRange.GetRandom();
		pilot.SetRail(m_rail, (int)m_rails);
		m_rail = (m_rail + 1) % (int)m_rails;
		pilot.guideFunction = m_guideFunction;
	}   

	protected override void OnAllEntitiesRespawned() {
		m_groupBonus = 0f;
		if (m_hasGroupBonus) {
			if (m_entities[0] != null) {
				m_groupBonus = m_entities[0].score * EntitiesToSpawn * FLOCK_BONUS_MULTIPLIER;
			}
		}
    }

    protected override void OnRemoveEntity(IEntity _entity, int index, bool _killedByPlayer) {
        if (m_isPremiumCurrencyNPC && _killedByPlayer) {
            string key = GetPrefabNameToSpawn((uint)m_prefabIndex);
            if (RewardManager.npcPremiumCount.ContainsKey(key)) {
                RewardManager.npcPremiumCount[key]++;
            } else {
                RewardManager.npcPremiumCount.Add(key, 1);
            }
        }
    }
    	
	protected override void OnAllEntitiesRemoved(IEntity _lastEntity, bool _allKilledByPlayer) {
		if (_allKilledByPlayer) {
			// check if player has destroyed all the flock
			if (m_groupBonus > 0 && _lastEntity != null) {
				Reward reward = new Reward();
				reward.score = (int)(m_groupBonus * EntitiesKilled);
				Messenger.Broadcast<Transform, IEntity, Reward>(MessengerEvents.ENTITY_BURNED, _lastEntity.transform, _lastEntity, reward);
			}

			// Reroll the Golden chance
			for (int i = 0; i < GetMaxEntities(); ++i) {				
				m_entityGoldMode[i] = EntityGoldMode.ReRoll;
			}

			m_respawnCount++;
			if (m_maxSpawns > 0 && m_respawnCount >= m_maxSpawns) {
				m_readyToBeDisabled = true;
			} else {
				// Program the next spawn time
				m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();
			}
		} else {
			ResetSpawnTimer(); // instant respawn, because player didn't kill all the entities
		}

		for (int i = 0; i < m_hasbeenSpawned.Length; i++) {
			m_hasbeenSpawned[i] = false;
		}

		if (m_readyToBeDisabled) {
			UnregisterFromSpawnerManager();
			Destroy(gameObject);
		}
	}    

	protected override void OnForceRemoveEntities() {
		ResetSpawnTimer();
	}

	public override void DrawStateGizmos() {
		switch (State) {
			case EState.Init: Gizmos.color = Color.grey; break;
			case EState.Respawning: Gizmos.color = Color.yellow; break;
			case EState.Create_Instances: Gizmos.color = Color.red; break;
			case EState.Activating_Instances: Gizmos.color = Color.blue; break;
			case EState.Alive: Gizmos.color = Color.green; break;
		}
		Gizmos.DrawWireSphere(transform.position, 0.25f * GizmosExt.GetGizmoSize(transform.position));
	}

	//-----------------------------------------------    

	public void ResetSpawnTimer() {
		m_respawnTime = -1;
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
		bool startConditionsOk = (m_activationTriggers.Length == 0) && (m_activationKillTriggers.Length == 0);	// If there are no activation triggers defined, the spawner will be considered ready
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

		for (int i = 0; i < m_activationKillTriggers.Length; i++) {
			string sku = m_activationKillTriggers[i].sku;

			if (RewardManager.killCount.ContainsKey(sku)) {
				startConditionsOk |= RewardManager.killCount[sku] >= m_activationKillTriggers[i].value;
			}
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

		for (int i = 0; i < m_deactivationKillTriggers.Length; i++) {
			string sku = m_deactivationKillTriggers[i].sku;

			if (RewardManager.killCount.ContainsKey(sku)) {
				endConditionsOk &= RewardManager.killCount[sku] < m_deactivationKillTriggers[i].value;
			}
		}

		// If we've reached either of the end conditions, mark the spawner as ready to disable
		// Only during actual gameplay, not while using the level editor simulator!
		if(!endConditionsOk && Application.isPlaying) {
			m_readyToBeDisabled = true;
		}

		return endConditionsOk;
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

	public virtual void OnDrawGizmos() {
		Gizmos.color = Colors.paleGreen;
		Gizmos.DrawCube(transform.position + (Vector3)m_rect.position, m_rect.size);

		// Only if editor allows it
		if(showSpawnerInEditor) {
			// Draw spawn area
			GetArea().DrawGizmo();

			// Draw icon! - only in editor!
			#if UNITY_EDITOR
			// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
			if (this.m_entityPrefabList != null && this.m_entityPrefabList.Length > 0) {
				Gizmos.DrawIcon(transform.position, IEntity.ENTITY_PREFABS_PATH + this.m_entityPrefabList[0].name, true);
			}
			#endif

			// orientation
			Gizmos.color = Colors.lime;
			Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Vector3.forward * 5f);
		}

		if (Application.isPlaying) {
			DrawStateGizmos();
		}
	}


	void OnDrawGizmosSelected() {
		Gizmos.color = Colors.fuchsia;

		if (m_homePosMethod == SpawnPointSeparation.Sphere) {
			float angleOffset = (m_homePosLineRotation * Mathf.PI) / 180f;
			float distance = m_homePosDistance.distance;

			Gizmos.DrawWireSphere(transform.position, distance * 0.5f);
			Gizmos.DrawWireSphere(transform.position, distance);

			Gizmos.color = Colors.WithAlpha(Colors.slateBlue, 0.85f);
			for (int i = 0; i < m_quantity.max; ++i) {
				float angle = angleOffset + (i * (2f * Mathf.PI) / m_quantity.max);
				float d = distance * (0.5f + (0.25f * (i % 2)));

				Vector3 vs = transform.position;
				vs.x += d * Mathf.Cos(angle);
				vs.y += d * Mathf.Sin(angle);

				Vector3 ve = transform.position;
				ve.x += distance * Mathf.Cos(angle);
				ve.y += distance * Mathf.Sin(angle);

				Gizmos.DrawLine(vs, ve);
				Gizmos.DrawWireSphere(vs + (ve - vs) * 0.5f, 0.125f);
			}
		} else if (m_homePosMethod == SpawnPointSeparation.Line) {		
			Quaternion rot = Quaternion.AngleAxis(m_homePosLineRotation, Vector3.forward);	
			Vector3 start = rot * (Vector3.right * m_homePosDistance.min);
			Vector3 end = rot * (Vector3.right * m_homePosDistance.max);

			start += transform.position;
			end += transform.position;

			Gizmos.DrawLine(start, end);

			// preview entity positions
			float distance = m_homePosDistance.distance;
			float offset = distance / m_quantity.max;
			float rndArea = offset * 0.125f;

			Vector3 start_l1 = GameConstants.Vector3.zero;
			Vector3 end_l1 = GameConstants.Vector3.zero;
			Vector3 start_l2 = GameConstants.Vector3.zero;
			Vector3 end_l2 = GameConstants.Vector3.zero;

			Gizmos.color = Colors.WithAlpha(Colors.slateBlue, 0.85f);
			for (int i = 0; i < m_quantity.max; ++i) {
				start_l1 = rot * (Vector3.right * (m_homePosDistance.min + offset * i + rndArea) + Vector3.up * 0.125f) + transform.position;
				end_l1 = rot * (Vector3.right * (m_homePosDistance.min + offset * (i + 1) - rndArea) + Vector3.up * 0.125f) + transform.position;

				start_l2 = rot * (Vector3.right * (m_homePosDistance.min + offset * i + rndArea) + Vector3.down * 0.125f) + transform.position;
				end_l2 = rot * (Vector3.right * (m_homePosDistance.min + offset * (i + 1) - rndArea) + Vector3.down * 0.125f) + transform.position;

				Gizmos.DrawLine(start_l1, end_l1);
				Gizmos.DrawLine(start_l1, start_l2);
				Gizmos.DrawLine(start_l2, end_l2);
				Gizmos.DrawLine(end_l1, end_l2);
			}
		}
	}

	protected Vector3 RandomStartDisplacement(int _index)	{
		Vector3 v = GameConstants.Vector3.zero;

		float distance = m_homePosDistance.distance;

		if (m_homePosMethod == SpawnPointSeparation.Sphere) {
			float angleOffset = (m_homePosLineRotation * Mathf.PI) / 180f;
			float angle = angleOffset + (_index * (2f * Mathf.PI) / m_quantity.max);
			float randomDistance = Random.Range(distance * (0.5f + (0.25f * (_index % 2))), distance);

			v.x = randomDistance * Mathf.Cos(angle);
			v.y = randomDistance * Mathf.Sin(angle);
		} else if (m_homePosMethod == SpawnPointSeparation.Line) {
			float offset = distance / EntitiesToSpawn;

			float min = offset * _index;
			float max = offset * (_index + 1);
			float rndOffset = Random.Range(-offset * 0.875f * 0.5f, offset * 0.875f * 0.5f); //we use an smaller area so the entities won't appear too close to each other

			v = Quaternion.AngleAxis(m_homePosLineRotation, Vector3.forward) * Vector3.right * (m_homePosDistance.min + min + ((max - min) * 0.5f) + rndOffset);
		}

		v.z = 0f;
		return v;
	}    




	#region save_spawner_state
	override public AbstractSpawnerData Save()
	{
		AbstractSpawnerData data = new SpawnerData() as AbstractSpawnerData;
		Save( ref data );
		return data;
	}
	override public void Save( ref AbstractSpawnerData _data)
	{
		base.Save(ref _data);
		SpawnerData spData = _data as SpawnerData;
		if (spData != null)
		{
			spData.m_respawnCount = m_respawnCount;
			spData.m_respawnTime = m_respawnTime;
		}
	}
	override public void Load(AbstractSpawnerData _data)
	{
		base.Load(_data);
		SpawnerData spData = _data as SpawnerData;
		if (spData != null)
		{
			m_respawnCount = spData.m_respawnCount;
			m_respawnTime = spData.m_respawnTime;
		}

	}
	#endregion
}
