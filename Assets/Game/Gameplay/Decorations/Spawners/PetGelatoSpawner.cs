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


	[SerializeField] private float m_searchRange;

	//-------------------------------------------------------------------		
	[System.Serializable]
	public class EntityPrefab {
		[EntityPrefabListAttribute]
		public string spawnPrefab = "";
	}

	[SerializeField] private List<EntityPrefab> m_selectedPrefabs;
	
	
	private string[] m_prefabNames;
	private PoolHandler[] m_poolHandlers;


	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;

	private Vector3[] m_positions = new Vector3[50];
	private int[] m_tierToPoolHandler = new int[50];
	private uint m_entitiesToSpawn = 0;

	private List<IEntity> m_entitiesAlive;
	private List<int> m_entitiesAliveIndex;



    void Awake() {
		// Register change area events
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

	override protected void OnDestroy() {
		base.OnDestroy();
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);

		if (ApplicationManager.IsAlive) {
			ForceRemoveEntities();
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
			case BroadcastEventType.GAME_AREA_ENTER:
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
				PreparePools();
				ForceReset();
            }break;
        }
    }

	void PreparePools()	{
		List<string> listValidPrefab = new List<string>();
		List<PoolHandler> listValidHandlers = new List<PoolHandler>();

		for (int i = 0; i< m_selectedPrefabs.Count; i++) {
			string prefab = m_selectedPrefabs[i].spawnPrefab;
			PoolHandler handle = PoolManager.GetHandler(prefab);
			if (handle != null) {
				listValidPrefab.Add(m_selectedPrefabs[i].spawnPrefab);
				listValidHandlers.Add(handle);
			}
		}		
		
		m_prefabNames = listValidPrefab.ToArray();
		m_poolHandlers = listValidHandlers.ToArray();		
	}

    protected override void OnStart() {
        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;
        UseSpawnManagerTree = false;

		m_entitiesAlive = new List<IEntity>();
		m_entitiesAliveIndex = new List<int>();

        RegisterInSpawnerManager();
		PreparePools();
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
		if (State == EState.Respawning) {
			// find npcs....
			m_entitiesToSpawn = 0;

			m_numCheckEntities = EntityManager.instance.GetOverlapingEntities(transform.position, m_searchRange, m_checkEntities);
			for (int e = 0; e < m_numCheckEntities; e++) {
				Entity entity = m_checkEntities[e];
				int tierIndex = (int)entity.edibleFromTier;
				
				if (tierIndex < m_prefabNames.Length) {					
					m_positions[m_entitiesToSpawn] = entity.machine.position;
					m_tierToPoolHandler[m_entitiesToSpawn] = tierIndex;

					m_entitiesToSpawn++;
					entity.Disable(true);					
				}
			}

			bool succes = Respawn();
			if (succes) {
				for (int i = 0; i < m_entitiesToSpawn; ++i) {
					m_entities[i] = null;
				}
				EntitiesAlive = 0;
				EntitiesKilled = 0;
				EntitiesToSpawn = 0;
				State = EState.Respawning;
			}
		}
	}

	public new void ForceRemoveEntities() {
		foreach (IEntity e in m_entitiesAlive) {
			RemoveEntity(e, false);
		}
	}

	public new void RemoveEntity(IEntity _entity, bool _killedByPlayer) {
        int index = -1;
        for (int i = 0; i < m_entitiesAlive.Count && index == -1; i++) {
            if (m_entitiesAlive[i] != null && m_entitiesAlive[i] == _entity) {
                index = i;                                                
            }
        }

        if (index > -1) {   
			PoolHandler handler = m_poolHandlers[m_entitiesAliveIndex[index]];
            if (ProfilerSettingsManager.ENABLED) {               
				SpawnerManager.RemoveFromTotalLogicUnits(1, m_prefabNames[m_entitiesAliveIndex[index]]);
            }

            // Unregisters the entity
            UnregisterFromEntityManager(_entity);
            // Returns the entity to the pool
			ReturnEntityToPool(handler, _entity.gameObject);

			m_entitiesAlive.RemoveAt(index);
			m_entitiesAliveIndex.RemoveAt(index);
        }
    }

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
	   	Transform groundSensor = spawning.transform.Find("groundSensor");
        Transform t = spawning.transform;
        
		t.position = m_positions[index];
		t.localScale = GameConstants.Vector3.one;
		t.rotation = GameConstants.Quaternion.identity;		

		if (groundSensor != null) {
			t.position -= groundSensor.localPosition;
		}
		t.localScale = Vector3.one;

		m_entitiesAlive.Add(spawning);
		m_entitiesAliveIndex.Add(m_tierToPoolHandler[index]);
    }

    //-------------------------------------------------------------------
}
