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
	private int[] m_entitiesToSpawnPerHandler;

	private Entity[] m_checkEntities = new Entity[50];

	private Vector3[] m_positions = new Vector3[50];
	private int[] m_tierToPoolHandler = new int[50];
	private uint m_entitiesToSpawn = 0;

	private DragonTier m_tierCheck;

	private float m_timer;


    void Awake() {
		// Register change area events
		Broadcaster.AddListener(BroadcastEventType.POOL_MANAGER_READY, this);

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
		m_entitiesToSpawnPerHandler = new int[m_poolHandlers.Length];

		m_onSpawnEffect.CreatePool();
	}

    protected override void OnStart() {
        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;
        UseSpawnManagerTree = false;

		m_tierCheck = InstanceManager.player.data.tier;

        RegisterInSpawnerManager();
    }

    protected override uint GetMaxEntities() {
        return 255;
    }

    protected override uint GetEntitiesAmountToRespawn() {        
        return m_entitiesToSpawn;
    }        

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandlers[m_tierToPoolHandler[index]];
	}

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_prefabNames[m_tierToPoolHandler[index]];
    }    


	public void Spawn() {
		m_timer = 0f;	

		if (State == EState.Respawning) {
			m_entitiesToSpawn = 0;

			int entityCount = EntityManager.instance.GetOnScreenEntities(m_checkEntities);

			for (int i = 0; i < m_entitiesToSpawnPerHandler.Length; ++i) {
				m_entitiesToSpawnPerHandler[i] = 0;
			}

			for (int i = 0; i < entityCount; ++i) {
				// Check if it can be eaten by the player
				Entity entity = m_checkEntities[i];

				if (!entity.HasTag(IEntity.Tag.Collectible)
				&& (entity.IsEdible(m_tierCheck) || entity.CanBeHolded(m_tierCheck))) {
					int tierIndex = (int)entity.edibleFromTier;
					
					if (tierIndex < m_prefabNames.Length
					&&  m_entitiesToSpawnPerHandler[tierIndex] < m_poolHandlers[tierIndex].pool.NumFreeObjects()) {
						if (entity.circleArea != null) {
							m_positions[m_entitiesToSpawn] = entity.circleArea.center;
						} else {
							m_positions[m_entitiesToSpawn] = entity.machine.position;
						}
						m_tierToPoolHandler[m_entitiesToSpawn] = tierIndex;
						
						m_onSpawnEffect.Spawn(m_positions[m_entitiesToSpawn]);
						entity.Disable(true);

						m_entitiesToSpawn++;
						m_entitiesToSpawnPerHandler[tierIndex]++;
					}
				}				
			}

			if (m_entitiesToSpawn > 0) {
				Respawn();
				m_timer = m_searchCooldown;				
			} else {
				m_timer *= 0.25f;
			}
		}
	}

	protected override void OnAllEntitiesRemoved(IEntity _lastEntity, bool _allKilledByPlayer) {
		State = EState.Respawning;
	}

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {	   	
        Transform t = spawning.transform;
        
		t.position = m_positions[index];
		t.localScale = GameConstants.Vector3.one;
		t.rotation = GameConstants.Quaternion.identity;

		t.localScale = Vector3.one;
    }

    //-------------------------------------------------------------------
}
