using UnityEngine;
using System.Collections;

public class AutoSpawnBehaviour : MonoBehaviour, ISpawner {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	public enum State {
		Idle = 0,
		Respawning
	};

	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private float m_spawnTime;


	private State m_state;
	public State state
	{
		get
		{
			return m_state;
		}
	}

	private float m_respawnTime;
	private SpawnerConditions m_spawnConditions;

	private Bounds m_bounds; // view bounds
	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }
	public Quaternion rotation { get { return Quaternion.identity; } }

	private bool m_disableAtFirstUpdate;

	// Scene referemces
	private GameSceneControllerBase m_gameSceneController = null;

	public AreaBounds area{ get {return null;} }
	public IGuideFunction guideFunction{ get {return null;} }

	private GameCamera m_newCamera;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		m_spawnConditions = GetComponent<SpawnerConditions>();

		if (m_spawnConditions == null || m_spawnConditions.IsAvailable()) {
			SpawnerManager.instance.Register(this, true);

			m_newCamera = Camera.main.GetComponent<GameCamera>();
			m_gameSceneController = InstanceManager.GetSceneController<GameSceneControllerBase>();

			GameObject view = transform.FindChild("view").gameObject;
			m_bounds = view.GetComponentInChildren<Renderer>().bounds;

			Vector2 position = (Vector2)m_bounds.min;
			Vector2 size = (Vector2)m_bounds.size;
			Vector2 extraSize = size * (transform.position.z * 4f) / 100f; // we have to increase the size due to z depth

			m_rect = new Rect(position - extraSize * 0.5f, size + extraSize);

			m_disableAtFirstUpdate = false;

			return;
		}

		// we are not goin to use this spawner, lets destroy it
		Destroy(gameObject);        
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

    void OnDestroy() {
        if (SpawnerManager.instance != null) {
            SpawnerManager.instance.Unregister(this, true);
        }
    }

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		m_disableAtFirstUpdate = m_spawnConditions != null && !m_spawnConditions.IsReadyToSpawn(0f, 0f);
		m_state = (m_disableAtFirstUpdate)? State.Respawning : State.Idle;
	}

	public void Initialize() {
		m_state = State.Respawning;
	}    

	void Update() {
		if (m_disableAtFirstUpdate) {
			gameObject.SetActive(false);
			m_state = State.Respawning;
			m_disableAtFirstUpdate = false;
		}
	}

    public void Clear() {
        ForceRemoveEntities();
        gameObject.SetActive(false);
    }

    public void ForceRemoveEntities() {}

	public void StartRespawn() {
		// Program the next spawn time
		m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime;
		m_state = State.Respawning;
	}        

    public bool CanRespawn() {
		if (m_state == State.Respawning) {
			if (m_spawnConditions != null && m_spawnConditions.IsReadyToBeDisabled(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
				if (!m_newCamera.IsInsideActivationMinArea(m_bounds)) {
					Destroy(gameObject);
				}
			} else if (m_spawnConditions == null || m_spawnConditions.IsReadyToSpawn(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
				if (m_gameSceneController.elapsedSeconds > m_respawnTime) {
					bool isInsideActivationArea = m_newCamera.IsInsideActivationArea(m_bounds);
					//bool isInsideActivationArea = m_newCamera.IsInsideActivationArea(transform.position);	
					if (isInsideActivationArea) {
						return true;
					}
				}
			}
		}

		return false;
	}

	public bool Respawn() {
		Spawn();
		return true;
	}
		
	private void Spawn() {
		gameObject.SetActive(true);

		Initializable[] components = GetComponents<Initializable>();
		foreach (Initializable component in components) {
			component.Initialize();
		}
		m_state = State.Idle;
	}

	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {}

	public virtual bool SpawnersCheckCurrents(){ return false; }

	public void DrawStateGizmos() {}


	#region save_spawner_state
	public virtual void AssignSpawnerID(int id){}
	public virtual int GetSpawnerID(){return -1;}
	public virtual AbstractSpawnerData Save(){return null;}
	public virtual void Save( ref AbstractSpawnerData _data){}
	public virtual void Load(AbstractSpawnerData _data){}
	#endregion
}