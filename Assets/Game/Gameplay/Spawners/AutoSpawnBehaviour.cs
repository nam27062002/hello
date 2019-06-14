using UnityEngine;
using System.Collections.Generic;

public class AutoSpawnBehaviour : MonoBehaviour, ISpawner, IBroadcastListener {
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
	[SerializeField] private int m_maxSpawns = 0;
	[SerializeField] private bool m_mustBedestroyed = true;

	[SeparatorAttribute("Ground")]
	[SerializeField] private Collider[] m_ground;


	private State m_state;
	public State state { get { return m_state; } }

	private Decoration m_decoration;

	private int m_respawnCount;
	private float m_respawnTime;
	private SpawnerConditions m_spawnConditions;

    private ISpawnable[] m_components;


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
        m_components = GetComponents<ISpawnable>();

        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);

        if (m_spawnConditions == null || m_spawnConditions.IsAvailable()) {

            ZoneManager.Zone zone = InstanceManager.zoneManager.GetZone(transform.position.z);
            if (zone == ZoneManager.Zone.None) {
                Destroy(this);
            } else {
                SpawnerManager.instance.Register(this, true);

                m_decoration = GetComponent<Decoration>();

                m_newCamera = Camera.main.GetComponent<GameCamera>();
                m_gameSceneController = InstanceManager.gameSceneControllerBase;

                GameObject view = transform.Find("view").gameObject;
                Renderer[] renderers = view.GetComponentsInChildren<Renderer>();

                if (renderers.Length > 0) {
                    m_bounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; ++i) {
                        m_bounds.Encapsulate(renderers[i].bounds);
                    }
                } else {
                    m_bounds = new Bounds(transform.position, GameConstants.Vector3.one);
                }

                Vector2 position = (Vector2)m_bounds.min;
                Vector2 size = (Vector2)m_bounds.size;
                Vector2 extraSize = size * (transform.position.z * 2f) / 100f; // we have to increase the size due to z depth

                m_rect = new Rect(position - extraSize * 0.5f, size + extraSize);

                m_respawnCount = 0;
            }
			return;
		}

		// we are not goin to use this spawner, lets destroy it
		Destroy(gameObject);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                OnLevelLoaded();
            }break;
        }
    }
    
    void OnDestroy() {
		if (ApplicationManager.IsAlive) {
			if (SpawnerManager.isInstanceCreated)
				SpawnerManager.instance.Unregister (this, true);

			if (m_decoration != null) {
				EntityManager.instance.UnregisterDecoration (m_decoration);
			}
		}

        // Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		bool disable = m_spawnConditions != null && !m_spawnConditions.IsReadyToSpawn(0f, 0f);
		if (disable) {
			m_respawnCount = 0;
			m_state = State.Respawning;
			gameObject.SetActive(false);
		} else {
			if (m_decoration != null) {
				EntityManager.instance.RegisterDecoration(m_decoration);
			}

			m_respawnCount = 1;
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

    public List<string> GetPrefabList() {
        return null;
    }

    public void ForceRemoveEntities() {}
    public void ForceReset() {}

	public void ForceGolden( IEntity entity ){
		// entity.SetGolden(Spawner.EntityGoldMode.Gold);
	}

    public void StartRespawn() {
		m_respawnCount++;

		if (m_maxSpawns > 0 && m_respawnCount > m_maxSpawns) {
			// we are not goin to use this spawner, lets destroy it
			if (m_mustBedestroyed) {
				Destroy(gameObject, 0.15f);
			}
		} else {
			// Program the next spawn time
			m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime;

			for (int i = 0; i < m_ground.Length; ++i) {
				m_ground[i].isTrigger = true;
			}

			m_state = State.Respawning;
		}
	}

	public bool IsRespawing() {
		return (m_state == State.Respawning);
	}

	// this spawner will kill its entities if it is outside camera disable area
	public bool MustCheckCameraBounds() 	{ return false; }
	public bool IsRespawingPeriodically() 	{ return false; }

    public bool CanRespawn() {
		if (m_spawnConditions != null 
		&&  m_spawnConditions.IsReadyToBeDisabled(m_gameSceneController.elapsedSeconds + m_gameSceneController.progressionOffsetSeconds,
		 										  RewardManager.xp + m_gameSceneController.progressionOffsetXP))
		{
			if (!m_newCamera.IsInsideActivationMinArea(m_bounds)) {
				Destroy(gameObject, 0.15f);
				return false;
			}
		}

		if (m_state == State.Respawning) {
			if (m_spawnConditions == null
			||  m_spawnConditions.IsReadyToSpawn(m_gameSceneController.elapsedSeconds + m_gameSceneController.progressionOffsetSeconds,
												 RewardManager.xp + m_gameSceneController.progressionOffsetXP))
			{
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
		if (m_respawnCount == 0) {
			if (m_decoration != null) {
				EntityManager.instance.RegisterDecoration(m_decoration);
			}
			gameObject.SetActive(true);
		}
		
		foreach (ISpawnable component in m_components) {
			component.Spawn(this);
		}

		for (int i = 0; i < m_ground.Length; ++i) {
			m_ground[i].isTrigger = false;
		}

		m_state = State.Idle;
	}

	public void RemoveEntity(IEntity _entity, bool _killedByPlayer) {}

	public void DrawStateGizmos() {}

    /*
    private void OnDrawGizmosSelected()
    {
        GameObject view = transform.Find("view").gameObject;
        Renderer[] renderers = view.GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers[0].bounds;
        for (int i = 0; i < renderers.Length; ++i)
        {
            Gizmos.color = Colors.gray;
            Gizmos.DrawWireCube(renderers[i].bounds.center, renderers[i].bounds.size);
            bounds.Encapsulate(renderers[i].bounds);
        }
        Gizmos.color = Colors.slateBlue;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }*/

    #region save_spawner_state
    public virtual void AssignSpawnerID(int id){}
	public virtual int GetSpawnerID(){return -1;}
	public virtual AbstractSpawnerData Save(){return null;}
	public virtual void Save( ref AbstractSpawnerData _data){}
	public virtual void Load(AbstractSpawnerData _data){}
	#endregion

	/// <summary>
	/// Callback to draw gizmos that are pickable and always drawn.
	/// </summary>
	private void OnDrawGizmosSelected() {
		Gizmos.DrawWireCube((Vector3)m_rect.center, (Vector3)m_rect.size);
	}
}
