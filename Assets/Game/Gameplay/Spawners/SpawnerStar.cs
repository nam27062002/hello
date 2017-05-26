using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpawnerConditions))]
public class SpawnerStar : AbstractSpawner {
	private static float GROUP_BONUS_MULTIPLIER = 0.1f;

	[Separator("Entity")]
	[EntityJunkPrefabListAttribute]
	[SerializeField] private string m_entityPrefab = "";
	[SerializeField] private uint m_quantity = 5;


	[Separator("Coin Bonus")]
	[SerializeField] private int m_coinsReward = 1;

	[Separator("Respawn")]
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);

	private float m_respawnTime;
	private SpawnerConditions m_respawnConditions; 
	private GameSceneControllerBase m_gameSceneController;
	private float m_groupBonus = 0;

	[SerializeField][HideInInspector] private Vector3[] m_points = new Vector3[0];
	public Vector3[] points { get { return m_points; } set { m_points = value; } }


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
			RegisterInSpawnerManager();
			SpawnerAreaManager.instance.Register(this);

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

		PoolManager.RequestPool(m_entityPrefab, IEntity.EntityPrefabsPath, m_entities.Length);

		// Get external references
		// Spawners are only used in the game and level editor scenes, so we can be sure that game scene controller will be present
		m_gameSceneController = InstanceManager.gameSceneControllerBase;
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
		return m_quantity;
	}

	protected override string GetPrefabNameToSpawn(uint index) {		
		return m_entityPrefab;
	}

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {		
		spawning.transform.position = transform.position + m_points[index]; // set position
	}
		
	protected override void OnAllEntitiesRespawned() {		
		m_groupBonus = m_coinsReward * EntitiesToSpawn * GROUP_BONUS_MULTIPLIER;
	}

	protected override void OnAllEntitiesRemoved(GameObject _lastEntity, bool _allKilledByPlayer) {
		if (_allKilledByPlayer) {
			// check if player has destroyed all the flock
			if (m_groupBonus > 0) {
				Reward reward = new Reward();
				reward.coins = (int)(m_groupBonus * EntitiesKilled);
				Messenger.Broadcast<Transform, Reward>(GameEvents.FLOCK_EATEN, _lastEntity.transform, reward);
			}
			// Program the next spawn time
			m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();
		} else {
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

	public override bool SpawnersCheckCurrents() { return false; }

	public void UpdateBounds() {
		m_rect.min = Vector3.zero;
		m_rect.max = Vector3.zero;
		for (int i = 0; i < m_points.Length; ++i) {			
			m_rect.min = Vector3.Min(m_rect.min, m_points[i]);
			m_rect.max = Vector3.Max(m_rect.max, m_points[i]);
		}
		m_rect.center = m_rect.center + (Vector2)transform.position - m_rect.size * 0.125f;
		m_rect.size = m_rect.size * 1.25f;
	}

	//-----------------------------------------------

	void OnDrawGizmos() {
		// Draw spawn area
		area.DrawGizmo();

		// Draw icon! - only in editor!
		#if UNITY_EDITOR
		// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
		for (int i = 0; i < m_points.Length; ++i) {
			Gizmos.DrawIcon(transform.position + m_points[i], IEntity.ENTITY_PREFABS_PATH + m_entityPrefab, true);
		}
		#endif

		// orientation
		Gizmos.color = Colors.lime;
		Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Vector3.forward * 5f);

		DrawStateGizmos();
	}
}
