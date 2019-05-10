using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpawnerConditions))]
public class SpawnerStar : AbstractSpawner {
	[SerializeField] private float m_rectExtraSize = 5f;
	[Separator("Entity")]
	[EntityJunkPrefabListAttribute]
	[SerializeField] private string m_entityPrefab = "";
    public string entityPrefab { get { return m_entityPrefab; } }

    [DelayedAttribute]
    [SerializeField] private uint m_quantity = 5;

	[Separator("Coin Bonus")]
	[SerializeField] private int m_coinsRewardFlock = 0;

	[Separator("Respawn")]
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);




    private PoolHandler m_poolHandler;

	private float m_respawnTime;
	private SpawnerConditions m_respawnConditions; 
	private GameSceneControllerBase m_gameSceneController;

	[SerializeField][HideInInspector] private List<Vector3> m_points = new List<Vector3>();
	public List<Vector3> points { get { return m_points; } set { m_points = value; } }

	private bool[] m_pointsAlive;
	private int[] m_pointToEntityIndex;


	private AreaBounds m_area;
	public override AreaBounds area {
		get {
			if (m_area == null) {				
				m_area = new RectAreaBounds(m_rect.center, m_rect.size);
			} else {
				m_area.UpdateBounds(m_rect.center, m_rect.size);
			}
			return m_area;
		}
	}

	protected override void OnStart() {
		m_respawnConditions = GetComponent<SpawnerConditions>();

		if (m_respawnConditions.IsAvailable()) {
			m_pointsAlive = new bool[m_quantity];
			m_pointToEntityIndex = new int[m_quantity];

			UpdateBounds();
			RegisterInSpawnerManager();

			gameObject.SetActive(false);
			return;
		}

		// we are not goin to use this spawner, lets destroy it
		Destroy(gameObject); 
	}

	protected override uint GetMaxEntities() {
		return m_quantity;
	}

	protected override void OnInitialize() {
		m_respawnTime = -1;

		for (int i = 0; i < m_quantity; ++i) {
			m_pointsAlive[i] = true;
			m_pointToEntityIndex[i] = -1;
		}

		m_poolHandler = PoolManager.RequestPool(m_entityPrefab, m_entities.Length);

		// Get external references
		// Spawners are only used in the game and level editor scenes, so we can be sure that game scene controller will be present
		m_gameSceneController = InstanceManager.gameSceneControllerBase;
	}

    public override List<string> GetPrefabList() {
        List<string> list = new List<string>();
        list.Add(m_entityPrefab);
        return list;
    }

    protected override bool CanRespawnExtended() {
		if (m_respawnConditions.IsReadyToSpawn(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
			// If we don't have any entity alive, proceed
			if (EntitiesAlive == 0) {
				// Respawn on cooldown?
				if (m_gameSceneController.elapsedSeconds > m_respawnTime || DebugSettings.ignoreSpawnTime) {
					// Everything ok! Spawn!
					return true;
				}
			}
		}
		// If we can't spawn and we're ready to be disabled, wait untill all entities are dead to do it
		else if (m_respawnConditions.IsReadyToBeDisabled(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
			if (EntitiesAlive == 0) {
				UnregisterFromSpawnerManager();
				Destroy(gameObject);
			}
		}
		return false;
	}

	protected override uint GetEntitiesAmountToRespawn() {
		return (EntitiesKilled == EntitiesToSpawn) ? m_quantity : EntitiesToSpawn - EntitiesKilled;
	}

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandler;
	}

	protected override string GetPrefabNameToSpawn(uint index) {
		return m_entityPrefab;
	}

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {		
		int point = 0;
		for (int i = 0; i < m_quantity; ++i) {
			if (m_pointsAlive[i]) {
				if (m_pointToEntityIndex[i] == -1) {
					m_pointToEntityIndex[i] = (int)index;
					point = i;
					break;
				}
			}
		}
		spawning.transform.position = transform.position + m_points[point]; // set position
	}

	protected override void OnRemoveEntity(IEntity _entity, int index, bool _killedByPlayer) {
		if (_killedByPlayer) {
			for (int i = 0; i < m_quantity; ++i) {
				if (m_pointToEntityIndex[i] == index) {
					m_pointsAlive[i] = false;
					m_pointToEntityIndex[i] = -1;
					break;
				}
			}
		}
	}

	protected override void OnAllEntitiesRemoved(IEntity _lastEntity, bool _allKilledByPlayer) {
		//
		if (_allKilledByPlayer) {
			// clear indexes
			for (int i = 0; i < m_quantity; ++i) {
				m_pointsAlive[i] = true;
				m_pointToEntityIndex[i] = -1;
			}

			// check if player has destroyed all the flock
			if (m_coinsRewardFlock > 0 && _lastEntity != null) {
				Reward reward = new Reward();
				reward.coins = m_coinsRewardFlock;
				Messenger.Broadcast<Transform, Reward>(MessengerEvents.STAR_COMBO, _lastEntity.transform, reward);
			}
			// Program the next spawn time
			m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();
		} else {
			for (int i = 0; i < m_quantity; ++i) {
				m_pointToEntityIndex[i] = -1;
			}
			ResetSpawnTimer(); // instant respawn, because player didn't kill all the entities
		}
	}    

	protected override void OnForceRemoveEntities() {
		ResetSpawnTimer();
	}

	// this spawner will kill its entities if it is outside camera disable area
	public override bool MustCheckCameraBounds() {
		return true;
	}

	public override void DrawStateGizmos() {
		switch (State) {
			case EState.Init: 					Gizmos.color = Color.grey; break;
			case EState.Respawning: 			Gizmos.color = Color.yellow; break;
			case EState.Create_Instances: 		Gizmos.color = Color.red; break;
			case EState.Activating_Instances: 	Gizmos.color = Color.blue; break;
			case EState.Alive: 					Gizmos.color = Color.green; break;
		}
		Gizmos.DrawWireSphere(transform.position, 0.25f * GizmosExt.GetGizmoSize(transform.position));
	}

	//-----------------------------------------------

	public void ResetSpawnTimer() {
		m_respawnTime = -1;
	}                

	public void UpdateBounds() {
		m_rect.min = Vector3.zero;
		m_rect.max = Vector3.zero;
		for (int i = 0; i < m_points.Count; ++i) {			
			m_rect.min = Vector3.Min(m_rect.min, m_points[i]);
			m_rect.max = Vector3.Max(m_rect.max, m_points[i]);
		}

		m_rect.size += Vector2.one * m_rectExtraSize;
		m_rect.center = m_rect.center + (Vector2)transform.position - Vector2.one * m_rectExtraSize * 0.5f;

	}

	//-----------------------------------------------

	void OnDrawGizmos() {
		// Draw spawn area
		area.DrawGizmo();

		// Draw icon! - only in editor!
		#if UNITY_EDITOR
		// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
		for (int i = 0; i < m_points.Count; ++i) {
			Gizmos.DrawIcon(transform.position + m_points[i], IEntity.ENTITY_PREFABS_PATH + m_entityPrefab, true);
		}
		#endif

		// orientation
		Gizmos.color = Colors.lime;
		Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Vector3.forward * 5f);

		DrawStateGizmos();
	}
}
