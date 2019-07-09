using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;

public class PetGelatoSpawner : AbstractSpawner, IBroadcastListener  {	

	private enum LookAtVector {
		Right = 0,
		Left,
		Forward,
		Back
	};


	[SerializeField] private float m_searchCooldown;

	//-------------------------------------------------------------------		
	[System.Serializable]
	public class EntityPrefab {
		[EntityPrefabListAttribute]
		public string spawnPrefab = "";
	}

	[SerializeField] private string[] m_selectedPrefabs = new string[(int)DragonTier.COUNT];
	[SerializeField] private ParticleData m_onSpawnEffect;
	
	private string[] m_prefabNames;
	private PoolHandler[] m_poolHandlers;
	
	private uint m_gelatosToSpawn = 0;
	private int[] m_gelatoTypesToSpawn;

	private Entity[] m_checkEntities = new Entity[50];
	private DefinitionNode[] m_gelatoDefinitions = new DefinitionNode[50];
	private Vector3[] m_gelatoPositions = new Vector3[50];
	private Transform[] m_gelatoLockedInCage = new Transform[50];	
	private bool[] m_gelatoGolden = new bool[50];
	private int[] m_indexToGelato = new int[50];
	
	
	private List<IEntity> m_spawnedGelatos;
	private List<int> m_spawnedGelatosIndex;


	private DragonTier m_tierCheck;

	private float m_timer;


    void Awake() {
		// Register change area events
		Broadcaster.AddListener(BroadcastEventType.POOL_MANAGER_READY, this);

		m_spawnedGelatos = new List<IEntity>();
		m_spawnedGelatosIndex = new List<int>();

		m_timer = m_searchCooldown;
    }

	override protected void OnDestroy() {
		base.OnDestroy();
		Broadcaster.RemoveListener(BroadcastEventType.POOL_MANAGER_READY, this);

		if (ApplicationManager.IsAlive) {
			ForceRemoveEntities();
		}
	}

	private void Update() {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0f) {
			Spawn();
		}
	}

    public override List<string> GetPrefabList() {
        return null;
    }
    //-------------------------------------------------------------------

    //-------------------------------------------------------------------
    // AbstractSpawner implementation
    //-------------------------------------------------------------------	

    private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
    public override AreaBounds area { get { return m_areaBounds; } set { m_areaBounds = value; } }

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
			case BroadcastEventType.POOL_MANAGER_READY:
            {
				PreparePools();
				ForceReset();
            }break;
        }
    }

	void PreparePools()	{
		List<string> listValidPrefab = new List<string>();
		List<PoolHandler> listValidHandlers = new List<PoolHandler>();

		for (int i = 0; i< m_selectedPrefabs.Length; i++) {
			string prefab = m_selectedPrefabs[i];
			PoolHandler handle = PoolManager.RequestPool(prefab, 1);
			if (handle != null) {
				listValidPrefab.Add(prefab);
				listValidHandlers.Add(handle);
			}
		}		
		
		m_prefabNames = listValidPrefab.ToArray();
		m_poolHandlers = listValidHandlers.ToArray();
		m_gelatoTypesToSpawn = new int[m_poolHandlers.Length];

		m_onSpawnEffect.CreatePool();
	}

    protected override void OnStart() {
        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;
        UseSpawnManagerTree = false;

		m_tierCheck = InstanceManager.player.data.tier;
    }

    protected override uint GetMaxEntities() {
        return 255;
    }

    protected override uint GetEntitiesAmountToRespawn() {        
        return m_gelatosToSpawn;
    }        

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandlers[m_indexToGelato[index]];
	}

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_prefabNames[m_indexToGelato[index]];
    }    


	public void Spawn() {
		m_timer = 0f;
		if (State == EState.Respawning) {
			m_gelatosToSpawn = 0;

			int entityCount = EntityManager.instance.GetOnScreenEntities(m_checkEntities);

			for (int i = 0; i < m_gelatoTypesToSpawn.Length; ++i) {
				m_gelatoTypesToSpawn[i] = 0;
			}

			for (int i = 0; i < entityCount; ++i) {
				// Check if it can be eaten by the player
				Entity entity = m_checkEntities[i];

				if (!entity.HasTag(IEntity.Tag.Collectible)
				&& (entity.IsEdible(m_tierCheck) || entity.CanBeHolded(m_tierCheck))) {
					int tierIndex = (int)entity.edibleFromTier;
					
					if (tierIndex < m_prefabNames.Length
					&&  m_gelatoTypesToSpawn[tierIndex] < m_poolHandlers[tierIndex].pool.NumFreeObjects()) {

						m_gelatoDefinitions[m_gelatosToSpawn] = entity.def;

						if (entity.circleArea != null) {
							m_gelatoPositions[m_gelatosToSpawn] = entity.circleArea.center;
						} else {
							m_gelatoPositions[m_gelatosToSpawn] = entity.machine.position;
						}

						if (entity.machine.GetSignal(Signals.Type.LockedInCage)) {
							m_gelatoLockedInCage[m_gelatosToSpawn] = entity.machine.transform.parent;
						} else {
							m_gelatoLockedInCage[m_gelatosToSpawn] = null;
						}

						m_gelatoGolden[m_gelatosToSpawn] = entity.isGolden;

						m_indexToGelato[m_gelatosToSpawn] = tierIndex;
						
						m_onSpawnEffect.Spawn(m_gelatoPositions[m_gelatosToSpawn]);

						entity.Disable(true);

						m_gelatosToSpawn++;
						m_gelatoTypesToSpawn[tierIndex]++;
					}
				}				
			}

			if (m_gelatosToSpawn > 0) {
				Respawn();
 				
				for (int i = 0; i < EntitiesAlive; i++) {
            		m_entities[i] = null;
				}

				EntitiesToSpawn = 0;
				EntitiesAlive = 0;
				EntitiesKilled = 0;
				EntitiesAllKilledByPlayer = false;

				State = EState.Respawning;
				m_timer = m_searchCooldown;				
			} else {
				m_timer *= 0.25f;
			}
		}
	}

	public override void ForceRemoveEntities() {
		foreach (IEntity entity in m_spawnedGelatos) {	
			RemoveEntity(entity, false);
		}
		OnForceRemoveEntities();
	}

	public override void RemoveEntity(IEntity _entity, bool _killedByPlayer) {
		int index = m_spawnedGelatos.IndexOf(_entity);

		if (index >= 0) {
			int tierIndex = m_spawnedGelatosIndex[index];

			if (ProfilerSettingsManager.ENABLED) {               
				SpawnerManager.RemoveFromTotalLogicUnits(1, m_prefabNames[tierIndex]);
            }

            // Unregisters the entity            
            UnregisterFromEntityManager(_entity);

            // Returns the entity to the pool
			ReturnEntityToPool(m_poolHandlers[tierIndex], _entity.gameObject);

			OnRemoveEntity(_entity, index, _killedByPlayer);

            if (m_spawnedGelatos.Count == 0) {
				OnAllEntitiesRemoved(_entity, false);
			}

			m_spawnedGelatos.RemoveAt(index);
			m_spawnedGelatosIndex.RemoveAt(index);
		}
	}

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {	   	
        Gelato gelato = spawning as Gelato;
		gelato.SetSku(m_gelatoDefinitions[index].sku);

		Transform t = spawning.transform;
        
		t.position = m_gelatoPositions[index];
		t.localScale = GameConstants.Vector3.one;
		t.rotation = GameConstants.Quaternion.identity;

		t.localScale = Vector3.one;

		if (m_gelatoGolden[index]) {
			spawning.SetGolden(Spawner.EntityGoldMode.Gold);
		}

		m_spawnedGelatos.Add(spawning);
		m_spawnedGelatosIndex.Add(m_indexToGelato[(int)index]);
    }

	protected override void OnMachineSpawned(IMachine machine, uint index) {
		if (m_gelatoLockedInCage[index] != null) {
			machine.EnterDevice(true);
			machine.transform.SetParent(m_gelatoLockedInCage[index], true);
			m_gelatoLockedInCage[index] = null;
		}
	}

	protected override void OnAllEntitiesRemoved(IEntity _lastEntity, bool _allKilledByPlayer) {
		State = EState.Respawning;
	}

    //-------------------------------------------------------------------
}
