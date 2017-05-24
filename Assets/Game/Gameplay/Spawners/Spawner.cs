using UnityEngine;
using System.Collections;
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

	public enum SpawnPointSeparation {
		Sphere = 0,
		Line
	}	

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Separator("Entity")]	
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area. The spawner can create instances of multiple prefabs, spawning mixed groups (mix entities = true) or random groups each spawn (mix entities = false).")]
	[SerializeField] private bool m_mixEntities = false;
	[SerializeField] public EntityPrefab[] m_entityPrefabList = new EntityPrefab[1];

	[SerializeField] public RangeInt 	m_quantity = new RangeInt(1, 1);
	[SerializeField] private Range 		m_speedFactorRange = new Range(1f, 1f);
	[SerializeField] public Range	 	m_scale = new Range(1f, 1f);
	[SerializeField] private uint		m_rails = 1;
	[SerializeField] private bool		m_hasGroupBonus = false;

	[Separator("Activation")]
	[SerializeField] private DragonTier m_minTier = DragonTier.TIER_0;
	[SerializeField] private DragonTier m_maxTier = DragonTier.TIER_4;
	[SerializeField] private bool	    m_checkMaxTier = false;

	[Tooltip("Spawners may not be present on every run (percentage).")]
	[SerializeField][Range(0f, 100f)] public float m_activationChance = 100f;

	[Tooltip("Start spawning when any of the activation conditions is triggered.\nIf empty, the spawner will be activated at the start of the game.")]
	[SerializeField] public SpawnCondition[] m_activationTriggers;
	public SpawnCondition[] activationTriggers { get { return m_activationTriggers; }}

	[SerializeField] public SpawnKillCondition[] m_activationKillTriggers;
	public SpawnKillCondition[] activationKillTriggers { get { return m_activationKillTriggers; } }

	[Tooltip("Stop spawning when any of the deactivation conditions is triggered.\nLeave empty for infinite spawning.")]
	[SerializeField] private SpawnCondition[] m_deactivationTriggers;
	public SpawnCondition[] deactivationTriggers { get { return m_deactivationTriggers; }}

	[Separator("Respawn")]
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);
	[Tooltip("NPCs which always give Premium Currency as reward have a, content based, change to not respawn reduced over kills.")]
	[SerializeField] private bool m_isPremiumCurrencyNPC = false;
	[SerializeField] private SpawnPointSeparation m_homePosMethod = SpawnPointSeparation.Sphere;
	[SerializeField] private Range m_homePosDistance = new Range(1f, 2f);

	[SerializeField] private int m_maxSpawns;

	[SerializeField] private bool m_checkCurrents = false;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private int m_prefabIndex;

	private string[] m_entitySku;
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
		float rnd = Random.Range(0f, 100f);
		DragonTier playerTier = InstanceManager.player.data.tier;

		bool enabledByTier = playerTier >= m_minTier;
		if (enabledByTier && m_checkMaxTier) {
			enabledByTier = (playerTier <= m_maxTier);
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
				for (int i = 0; i < m_entitySku.Length; i++) {
					m_entitySku[i] = "";
				}

				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
				m_pcProbCoefA = def.GetAsFloat("flyingPigsProbaCoefA", 1f);
				m_pcProbCoefB = def.GetAsFloat("flyingPigsProbaCoefB", 1f);

				if (m_activationKillTriggers == null) {
					m_activationKillTriggers = new SpawnKillCondition[0];
				}

				if (m_quantity.max < m_quantity.min) {
					m_quantity.min = m_quantity.max;
				}                			

				// adjust probabilities
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

					RegisterInSpawnerManager();
					SpawnerAreaManager.instance.Register(this);

					gameObject.SetActive(false);

					return;
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

		for (int i = 0; i < m_entityPrefabList.Length; i++) {
			PoolManager.RequestPool(m_entityPrefabList[i].name, IEntity.EntityPrefabsPath, m_entities.Length);
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

						string key = m_entitySku[m_prefabIndex];
						if (RewardManager.killCount.ContainsKey(key)) {
							eaten = RewardManager.killCount[key];
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

	protected override string GetPrefabNameToSpawn(uint index) {
		if (m_mixEntities) {
			m_prefabIndex = GetPrefabIndex();
		}

		return m_entityPrefabList[m_prefabIndex].name;
	}

	private int GetPrefabIndex() {
		int i = 0;
		float rand = Random.Range(0f, 100f);
		float prob = 0;

		for (i = 0; i < m_entityPrefabList.Length - 1; i++) {
			prob += m_entityPrefabList[i].chance;

			if (rand <= prob) {
				break;
			} 

			rand -= prob;
		}

		return i;
	}

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
		m_entitySku[index] = spawning.sku;

		if (index > 0) {
			originPos += RandomStartDisplacement((int)index); // don't let multiple entities spawn on the same point
		}

		spawning.transform.position = originPos;
		spawning.transform.localScale = Vector3.one * m_scale.GetRandom();
	}

	protected override void OnMachineSpawned(IMachine machine) {
		if (m_groupController) {				
			machine.EnterGroup(ref m_groupController.flock);
			machine.position = transform.position + m_groupController.flock.GetOffset(machine, 2f);
		}
	}

	protected override void OnPilotSpawned(Pilot pilot) {
		pilot.speedFactor = m_speedFactorRange.GetRandom();
		pilot.SetRail(m_rail, (int)m_rails);
		m_rail = (m_rail + 1) % (int)m_rails;
		pilot.guideFunction = m_guideFunction;
	}   

	protected override void OnAllEntitiesRespawned() {
		if (m_hasGroupBonus) {
			m_groupBonus = m_entities[0].score * EntitiesToSpawn * FLOCK_BONUS_MULTIPLIER;
		} else {
			m_groupBonus = 0f;
		}
	}

	protected override void OnAllEntitiesRemoved(GameObject _lastEntity, bool _allKilledByPlayer) {
		if (_allKilledByPlayer) {
			// check if player has destroyed all the flock
			if (m_groupBonus > 0) {
				Reward reward = new Reward();
				reward.score = (int)(m_groupBonus * EntitiesKilled);
				Messenger.Broadcast<Transform, Reward>(GameEvents.FLOCK_EATEN, _lastEntity.transform, reward);
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
			string cat = m_activationKillTriggers[i].category;

			if (RewardManager.categoryKillCount.ContainsKey(cat)) {
				startConditionsOk |= RewardManager.categoryKillCount[cat] >= m_activationKillTriggers[i].value;
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

		// If we've reached either of the end conditions, mark the spawner as ready to disable
		// Only during actual gameplay, not while using the level editor simulator!
		if(!endConditionsOk && Application.isPlaying) {
			m_readyToBeDisabled = true;
		}

		return endConditionsOk;
	}    

	public override bool SpawnersCheckCurrents(){ return m_checkCurrents; }

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
			if (this.m_entityPrefabList != null && this.m_entityPrefabList.Length > 0) {
				Gizmos.DrawIcon(transform.position, IEntity.ENTITY_PREFABS_PATH + this.m_entityPrefabList[0].name, true);
			}
			#endif

			// orientation
			Gizmos.color = Colors.lime;
			Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Vector3.forward * 5f);
		}

		DrawStateGizmos();
	}

	protected Vector3 RandomStartDisplacement(int _index)	{
		Vector3 v = Vector3.zero;
		float dAngle = (2f * Mathf.PI) / EntitiesToSpawn;
		float distance = m_homePosDistance.distance;
		float randomDistance = Random.Range(distance * 0.5f, distance);

		switch (m_homePosMethod) {
			case SpawnPointSeparation.Sphere:
				v.x = randomDistance * 0.5f * Mathf.Cos(dAngle * _index);
				v.y = randomDistance * 0.5f * Mathf.Sin(dAngle * _index);
				break;

			case SpawnPointSeparation.Line:
				v = Vector3.right * m_homePosDistance.GetRandom();
				break;
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
