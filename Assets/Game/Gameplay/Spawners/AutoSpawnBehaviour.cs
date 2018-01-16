﻿using UnityEngine;
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
	[SeparatorAttribute("Spawner")]
	[SerializeField] private float m_spawnTime;

	[SeparatorAttribute("Ground")]
	[SerializeField] private Collider[] m_ground;


	private State m_state;
	public State state { get { return m_state; } }

	private Decoration m_decoration;

	private float m_respawnTime;
	private SpawnerConditions m_spawnConditions;

	private Bounds m_bounds; // view bounds

	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }
	public Quaternion rotation { get { return Quaternion.identity; } }
	public Vector3 homePosition { get { return transform.position; } }

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

			m_decoration = GetComponent<Decoration>();
			if (m_decoration != null) {
				EntityManager.instance.RegisterDecoration(m_decoration);
			}

			m_newCamera = Camera.main.GetComponent<GameCamera>();
			m_gameSceneController = InstanceManager.gameSceneControllerBase;


			m_bounds = new Bounds();
			GameObject view = transform.Find("view").gameObject;
			Renderer[] renderers = view.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renderers.Length; ++i) {
				m_bounds.Encapsulate(renderers[i].bounds);
			}

			Vector2 position = (Vector2)m_bounds.min;
			Vector2 size = (Vector2)m_bounds.size;
			Vector2 extraSize = size * (transform.position.z * 4f) / 100f; // we have to increase the size due to z depth

			m_rect = new Rect(position - extraSize * 0.5f, size + extraSize);

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
		Messenger.AddListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

    void OnDestroy() {
		if (SpawnerManager.isInstanceCreated)
            SpawnerManager.instance.Unregister(this, false);
	
		if (m_decoration != null) {
			EntityManager.instance.UnregisterDecoration(m_decoration);
		}
    }

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		bool disable = m_spawnConditions != null && !m_spawnConditions.IsReadyToSpawn(0f, 0f);
		if (disable) {
			m_state = State.Respawning;
			gameObject.SetActive(false);
		} else {
			m_state = State.Idle;
		}

	}

	public void Initialize() {
		m_state = State.Idle;
	}

    public void Clear() {
        ForceRemoveEntities();
        gameObject.SetActive(false);
    }

    public void ForceRemoveEntities() {}
    public void ForceReset() {}

    public void StartRespawn() {		
		// Program the next spawn time
		m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime;

		for (int i = 0; i < m_ground.Length; ++i) {
			m_ground[i].isTrigger = true;
		}

		m_state = State.Respawning;
	}        

	public bool IsRespawing() {
		return (m_state == State.Respawning);
	}

	// this spawner will kill its entities if it is outside camera disable area
	public bool MustCheckCameraBounds() 	{ return false; }
	public bool IsRespawingPeriodically() 	{ return false; }

    public bool CanRespawn() {
		if (m_state == State.Respawning) {
			if (m_spawnConditions != null && m_spawnConditions.IsReadyToBeDisabled(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
				if (!m_newCamera.IsInsideActivationMinArea(m_bounds)) {
					Destroy(gameObject);
				}
			} else if (m_spawnConditions == null || m_spawnConditions.IsReadyToSpawn(m_gameSceneController.elapsedSeconds, RewardManager.xp)) {
				if (m_gameSceneController.elapsedSeconds > m_respawnTime) {
					bool isInsideActivationArea = m_newCamera.IsInsideCameraFrustrum(m_bounds);
					if (!isInsideActivationArea) {
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

		ISpawnable[] components = GetComponents<ISpawnable>();
		foreach (ISpawnable component in components) {
			component.Spawn(this);
		}

		for (int i = 0; i < m_ground.Length; ++i) {
			m_ground[i].isTrigger = false;
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