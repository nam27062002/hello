using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpawnerConditions))]
public class SpawnerWagon : MonoBehaviour, ISpawner {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	public enum State {
		Idle = 0,
		Respawning
	};

	[System.Serializable]
	public class EntityPrefab {
		[EntityWagonPrefabListAttribute]
		public string name = "";
		public float chance = 100;

		public EntityPrefab() {
			name = "";
			chance = 100;
		}
	}


	//-----------------------------------------------
	// Serialized Attributes
	//-----------------------------------------------
	[Separator("Rail")]
	[SerializeField] private string m_railName;

	[Separator("Entity")]
	[SerializeField] public EntityPrefab[] m_entityPrefabList = new EntityPrefab[1];

	[Separator("Respawn")]
	[SerializeField] public Range m_spawnTime = new Range(1f, 5f);


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private BSpline.BezierSpline m_railSpline;

	private PoolHandler[] m_poolHandlers;

	private List<IEntity> m_wagonList;
	private List<int> m_poolHandlerIndex;

	private State m_state;

	private float m_respawnTime;
	private SpawnerConditions m_respawnConditions; 

	private GameCamera m_camera;
	private GameSceneControllerBase m_gameSceneController;

	[SerializeField] protected Rect m_rect = new Rect(Vector2.zero, Vector2.one * 2f);
	public Rect boundingRect { get { return m_rect; } }

	private AreaBounds m_area;
	public AreaBounds area {
		get {
			if (m_area == null) m_area = new RectAreaBounds(m_rect.center, m_rect.size);
			else 				m_area.UpdateBounds(m_rect.center, m_rect.size);
			return m_area;
		}
	}

	public IGuideFunction guideFunction { get { return null; } }
	public Quaternion rotation { get { return Quaternion.identity; } }
	public Vector3 homePosition { get { return transform.position; } }


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		m_respawnConditions = GetComponent<SpawnerConditions>();

		if (!string.IsNullOrEmpty(m_railName) && m_respawnConditions.IsAvailable()) {	
			if (m_rect.size == Vector2.zero) {
				m_rect.size = Vector2.one * 2f;
			}
			m_rect.center = (Vector2)transform.position + m_rect.position;

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

				SpawnerManager.instance.Register(this, true);

				gameObject.SetActive(false);
				return;
			}

		}

		// we are not goin to use this spawner, lets destroy it
		Destroy(gameObject); 
	}

	void OnDestroy() {
		if (SpawnerManager.isInstanceCreated)
			SpawnerManager.instance.Unregister(this, true);
	}

	public void Initialize() {
		m_poolHandlers = new PoolHandler[m_entityPrefabList.Length];
		for (int i = 0; i < m_entityPrefabList.Length; i++) {
			m_poolHandlers[i] = PoolManager.RequestPool(m_entityPrefabList[i].name, 1);
		}
		m_wagonList = new List<IEntity>();
		m_poolHandlerIndex = new List<int>();

		// Get external references
		// Spawners are only used in the game and level editor scenes, so we can be sure that game scene controller will be present
		m_camera = Camera.main.GetComponent<GameCamera>();
		m_gameSceneController = InstanceManager.gameSceneControllerBase;

		//
		m_respawnTime = -1;
		m_state = State.Idle;
	}    

	public void Clear() {
		ForceRemoveEntities();
		gameObject.SetActive(false);
	}

    public List<string> GetPrefabList() {
        List<string> list = new List<string>();
        for (int j = 0; j < m_entityPrefabList.Length; ++j) {
            list.Add(m_entityPrefabList[j].name);
        }
        return list;
    }

    public bool IsRespawing() { return (m_state == State.Respawning); }

	// this spawner will kill its entities if it is outside camera disable area
	public bool MustCheckCameraBounds() 	{ return false; }
	public bool IsRespawingPeriodically() 	{ return true; }

	public bool CanRespawn() {
		if (m_state == State.Idle) {
			// lets find the spline
			if (m_railSpline == null) {
				m_railSpline = RailManager.GetRailByName(m_railName);
				if (m_railSpline == null) {
					Destroy(gameObject);
					return false;
				}
			}

			if (m_respawnConditions.IsReadyToBeDisabled(m_gameSceneController.elapsedSeconds + m_gameSceneController.progressionOffsetSeconds,
													    RewardManager.xp + m_gameSceneController.progressionOffsetXP))
			{
				if (!m_camera.IsInsideActivationMaxArea(area.bounds)) {
					Destroy(gameObject);
				}
			} else if (m_respawnConditions.IsReadyToSpawn(m_gameSceneController.elapsedSeconds + m_gameSceneController.progressionOffsetSeconds,
														  RewardManager.xp + m_gameSceneController.progressionOffsetXP))
			{
				bool isInsideActivationArea = m_camera.IsInsideActivationMinArea(area.bounds);
				if (isInsideActivationArea) {
					m_state = State.Respawning;
					return true;
				}
			}
		}

		return false;
	}

	public bool Respawn() {
		m_state = State.Respawning;

		if (m_gameSceneController.elapsedSeconds > m_respawnTime) {
			m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();
			Spawn();
		}

		// it'll be spawning wagons every few seconds.
		return false;
	}
		
	private void Spawn() {
		// drop a Wagon
		int index = GetPrefabIndex();

		PoolHandler handler = m_poolHandlers[index];
		GameObject spawning = handler.GetInstance(true);

		if (spawning != null) {
			Transform spawningTransform = spawning.transform;
			spawningTransform.rotation = Quaternion.identity;
			spawningTransform.localRotation = Quaternion.identity;
			spawningTransform.localScale = Vector3.one;

			IEntity entity = spawning.GetComponent<IEntity>();
			if (entity != null) {
				EntityManager.instance.RegisterEntity(entity as Entity);
				entity.Spawn(this); // lets spawn Entity component first
			}

			AI.MachineWagon machine = spawning.GetComponent<AI.MachineWagon>();
			if (machine != null) { 
				machine.SetRails(m_railSpline);
				machine.Spawn(this); 
			}

			AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
			if (pilot != null) {
				pilot.Spawn(this); 
			}

			ISpawnable[] components = spawning.GetComponents<ISpawnable>();
			foreach (ISpawnable component in components) {
				if (component != entity && component != pilot && component != machine) {
					component.Spawn(this);
				}
			}

			if (ProfilerSettingsManager.ENABLED) {
				SpawnerManager.AddToTotalLogicUnits(1, m_entityPrefabList[index].name);
			}

			m_wagonList.Add(entity);
			m_poolHandlerIndex.Add(index);
		}
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

	public void RemoveEntity(IEntity _entity, bool _killedByPlayer) {
		int index = -1;
		for (int i = 0; i < m_wagonList.Count && index == -1; ++i) {
			if (m_wagonList[i].gameObject == _entity) {
				index = i;                                                
			}
		}

		if (index > -1) {
			PoolHandler handler = m_poolHandlers[m_poolHandlerIndex[index]];
			if (ProfilerSettingsManager.ENABLED) {               
				SpawnerManager.RemoveFromTotalLogicUnits(1, m_entityPrefabList[m_poolHandlerIndex[index]].name);
			}

			// Unregisters the entity
			EntityManager.instance.UnregisterEntity(m_wagonList[index] as Entity);

			// Returns the entity to the pool
			handler.ReturnInstance(m_wagonList[index].gameObject);

			m_wagonList.RemoveAt(index);
			m_poolHandlerIndex.RemoveAt(index);
		}
	}

	public void ForceRemoveEntities() {
		m_state = State.Idle;
	}

	public void ForceReset() {
		ForceRemoveEntities();
		Initialize();
	}

	public void ForceGolden( IEntity entity ){
		// entity.SetGolden(Spawner.EntityGoldMode.Gold);
	}

	public void DrawStateGizmos() {}


	#region save_spawner_state
	public virtual void AssignSpawnerID(int id){}
	public virtual int GetSpawnerID(){return -1;}
	public virtual AbstractSpawnerData Save(){return null;}
	public virtual void Save( ref AbstractSpawnerData _data){}
	public virtual void Load(AbstractSpawnerData _data){}
	#endregion

	void OnDrawGizmos() {
		Gizmos.color = Colors.WithAlpha(Colors.paleGreen, 0.25f);
		Gizmos.DrawCube(transform.position + (Vector3)boundingRect.position, m_rect.size);

		Gizmos.color = Colors.paleGreen;
		Gizmos.DrawWireCube(transform.position + (Vector3)m_rect.position, m_rect.size);

		// Draw icon! - only in editor!
		#if UNITY_EDITOR
		// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
		if (this.m_entityPrefabList != null && this.m_entityPrefabList.Length > 0) {
			Gizmos.DrawIcon(transform.position, IEntity.ENTITY_PREFABS_PATH + this.m_entityPrefabList[0].name, true);
		}
		#endif

	}
}
