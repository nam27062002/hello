using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflammableDecoration : ISpawnable, IBroadcastListener {

	private enum State {
		Respawn = 0,
		Idle,
		Burning,
		Extinguish,
		Explode
	};

	[SerializeField] private float m_burningTime = 1f;
    [SerializeField] private ParticleData m_feedbackParticle = new ParticleData();
	// PF_FireHit
	[SerializeField] private bool m_feedbackParticleMatchDirection = false;
    [SerializeField] private ParticleData m_burnParticle = new ParticleData();
	//PF_FireProc
    [SerializeField] private ParticleData m_disintegrateParticle = new ParticleData();

	[SerializeField] private bool m_useAnimator = false;

    [SeparatorAttribute("Fire Nodes auto setup")]
    [SerializeField] private MonoBehaviour[] m_viewScripts = new MonoBehaviour[0];

    [SeparatorAttribute("Fire Nodes auto setup")]
	[SerializeField] private int m_boxelSize = 2;
	[SerializeField] private float m_hitRadius = 1.5f;


    //------
    public delegate void OnBurnDelegate();
    public OnBurnDelegate onBurn;
    //------

    [SerializeField] private List<FireNode> m_fireNodes;
    public List<FireNode> fireNodes { get { return m_fireNodes; } set { m_fireNodes = value; } }
    
    private Transform m_transform;

    private FireNodeSetup m_fireNodeSetup;

	private GameObject m_view;
	private GameObject m_viewBurned;

	private BoxCollider m_collider;
    
	private AutoSpawnBehaviour m_autoSpawner;
	private DestructibleDecoration m_destructibleBehaviour;
	protected DeviceOperatorSpawner[] m_operatorSpawner;
	protected DevicePassengersSpawner[] m_passengersSpawner;
	private Vector3 m_startPosition;

	private Renderer[] m_renderers;
	private Dictionary<int, List<Material>> m_originalMaterials = new Dictionary<int, List<Material>>();
	private Material m_ashMaterial;

    private Renderer[] m_viewBurnedRenderes;

	private Decoration m_entity;

	private ParticleHandler m_explosionProcHandler;


	private DeltaTimer m_timer = new DeltaTimer();
	private State m_state;
	private State m_nextState;
    private FireColorSetupManager.FireColorType m_extingishColor = FireColorSetupManager.FireColorType.RED;

	private bool m_initialized = false;

	private IEntity.Type m_burnSource = IEntity.Type.OTHER;

	public string sku { get { return m_entity.sku; } }

	private Decoration m_decoration;


	// Use this for initialization
	void Awake() {
        m_transform = transform;

        m_feedbackParticle.CreatePool();
		m_burnParticle.CreatePool();	
		m_disintegrateParticle.CreatePool();
		m_explosionProcHandler = ParticleManager.CreatePool("PF_FireExplosionProc");

		m_state = m_nextState = State.Idle;
		m_initialized = false;
        
        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);        
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
        if (m_fireNodes != null) {
            for (int i = 0; i < m_fireNodes.Count; i++) {
                m_fireNodes[i].Disable();
            }
        }
	}

    protected void OnDestroy() {
        ReturnAshMaterial();

        // Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER:
                OnLevelLoaded();
            break;
        }
    }

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
        ZoneManager.Zone zone = InstanceManager.zoneManager.GetZone(m_transform.position.z);

        if (zone == ZoneManager.Zone.None) {
            Destroy(this);
        } else {            
            m_view = m_transform.Find("view").gameObject;
            m_viewBurned = m_transform.Find("view_burned").gameObject;

            m_renderers = m_view.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < m_renderers.Length; i++) {
                Material[] materials = m_renderers[i].sharedMaterials;

                // Stores the materials of this renderer in a dictionary for direct access//
                int renderID = m_renderers[i].GetInstanceID();
                m_originalMaterials[renderID] = new List<Material>();
                m_originalMaterials[renderID].AddRange(materials);

                for (int m = 0; m < materials.Length; ++m) {
                    //TODO
                    //materials[m] = null;
                }

                m_renderers[i].sharedMaterials = materials;
            }

            m_viewBurnedRenderes = m_viewBurned.GetComponentsInChildren<Renderer>();

            m_entity = GetComponent<Decoration>();
            m_collider = GetComponent<BoxCollider>();
            m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
            m_operatorSpawner = GetComponents<DeviceOperatorSpawner>();
            m_passengersSpawner = GetComponents<DevicePassengersSpawner>();
            m_destructibleBehaviour = GetComponent<DestructibleDecoration>();


            for (int i = 0; i < m_fireNodes.Count; i++) {
                m_fireNodes[i].Init(this, m_entity, m_burnParticle, m_feedbackParticle, m_feedbackParticleMatchDirection, m_hitRadius);
                if (!gameObject.activeInHierarchy) {
                    m_fireNodes[i].Disable();
                }
            }
            m_startPosition = m_transform.position;

            m_initialized = true;
        }
	}

	public void SetupFireNodes() {
		if (m_fireNodeSetup == null) {
			m_fireNodeSetup = new FireNodeSetup();
		}

		m_fireNodeSetup.Init(transform);
		m_fireNodes = m_fireNodeSetup.Build(m_boxelSize);
	}

	override public void Spawn(ISpawner _spawner) {
		enabled = true;

		m_view.SetActive(true);
        for(int i = 0; i < m_viewScripts.Length; ++i) {
            m_viewScripts[i].enabled = true;
        }
        m_viewBurned.SetActive(false);

		for (int i = 0; i < m_fireNodes.Count; i++) {
			m_fireNodes[i].Reset();
		}

		m_transform.position = m_startPosition;
		ResetViewMaterials();

		m_state = m_nextState = State.Idle;
	}

	public bool IsBurning() {
		return m_state == State.Burning;
	}

	private void ChangeState() {
		switch (m_nextState) {
			case State.Idle: 
				break;

			case State.Burning:
				if (m_collider) m_collider.isTrigger = true;
				if (m_destructibleBehaviour != null) {
					m_destructibleBehaviour.enabled = false;
				}
				break;

			case State.Extinguish:
				BurnOperators();
                
                if (onBurn != null)
                    onBurn();

				m_timer.Start(m_burningTime * 1000);

				if (m_useAnimator) {
					m_view.SetActive(false);
				}

                for (int i = 0; i < m_viewScripts.Length; ++i) {
                    m_viewScripts[i].enabled = false;
                }

                m_viewBurned.SetActive(true);
                BurnedView();

				SwitchViewToDissolve();

				// Initialize some death info
				m_entity.onDieStatus.source = m_burnSource;
				m_entity.onDieStatus.reason = IEntity.DyingReason.BURNED;

				Messenger.Broadcast<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, m_transform, m_entity, m_entity.reward, KillType.BURN);

				break;

			case State.Explode:
				if (m_collider) m_collider.isTrigger = true;
				if (m_destructibleBehaviour != null) {
					m_destructibleBehaviour.enabled = false;
				}

				BurnOperators();
                if (onBurn != null)
                    onBurn();

				for (int i = 0; i < m_fireNodes.Count; ++i) {
					if (i % 2 == 0) {
						FireNode n = m_fireNodes[i];
						GameObject ex = m_explosionProcHandler.Spawn(null, n.position);
						if (ex != null) {
							ex.transform.localScale = GameConstants.Vector3.one * n.scale;
							ex.GetComponent<ExplosionProcController>().Explode(i * 0.015f, n.colorType); //delay
						}

						m_disintegrateParticle.Spawn(n.position + m_disintegrateParticle.offset);
					}
				}

				// Initialize some death info
				m_entity.onDieStatus.source = m_burnSource;
				m_entity.onDieStatus.reason = IEntity.DyingReason.BURNED;

                Messenger.Broadcast<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, m_transform, m_entity, m_entity.reward, KillType.BURN);


                m_timer.Start(250f);
				break;
		}

		m_state = m_nextState;
	}

	// Update is called once per frame
	override public void CustomUpdate() {		
		if (m_initialized) {
			if (m_state != m_nextState) {
				ChangeState();
			}

			switch (m_state) {
				case State.Burning: {
						bool allNodesBurning = true;
                        int max = m_fireNodes.Count;
						for (int i = 0; i < max && allNodesBurning; ++i) {
							allNodesBurning = allNodesBurning && m_fireNodes[i].IsBurning();
						}

						if (allNodesBurning || m_useAnimator) {
							m_nextState = State.Extinguish;
                            m_extingishColor = m_fireNodes[0].colorType;
						}
					} break;

				case State.Extinguish: {
                        // Advance dissolve!
                        if (!m_useAnimator && m_ashMaterial != null) {
                            m_ashMaterial.SetFloat("_BurnLevel", m_timer.GetDelta() * 3.0f);
                        }

                        if (m_timer.IsFinished()) {
                            bool extinguished = true;
                            int max = m_fireNodes.Count;
                            for (int i = 0; i < max; ++i) {
                                if (!m_fireNodes[i].IsExtinguished()) {
                                    if (!m_fireNodes[i].IsExtinguishing()) {
                                        m_fireNodes[i].Extinguish();
                                    }
                                    extinguished = false;
                                }
                            }

                            if (extinguished) {
                                Destroy();
                            }
                        }
                    }
                    break;

				case State.Explode: {
                        int max = m_fireNodes.Count;
                        for (int i = 0; i < max; ++i) {
                            m_fireNodes[i].Explode();
                        }
                        if (m_timer.IsFinished()) {
                            Destroy();
                        }
                    }
                    break;
			}
		}
	}

	public void LetsBurn(bool _explode, IEntity.Type _source, FireColorSetupManager.FireColorType _fireColorType) {
		if (m_state == State.Idle) {
			if (_explode) 	m_nextState = State.Explode;
			else 			m_nextState = State.Burning;

			m_burnSource = _source;
            m_extingishColor = _fireColorType;

            m_autoSpawner.RegisterDecoration();
		}
	}

	private void Destroy() {
		m_view.SetActive(false);
		m_viewBurned.SetActive(true);
        BurnedView();
		if (m_collider) m_collider.isTrigger = true;
		if (m_autoSpawner) m_autoSpawner.StartRespawn();
        ReturnAshMaterial();
		m_state = m_nextState = State.Respawn;
	}

	private void ResetViewMaterials() {
		for (int i = 0; i < m_renderers.Length; i++) {
			int renderID = m_renderers[i].GetInstanceID();
			Material[] materials = m_renderers[i].materials;
			for (int m = 0; m < materials.Length; ++m) {
				materials[m] = m_originalMaterials[renderID][m];
			}
			m_renderers[i].materials = materials;
		}
	}

	private void SwitchViewToDissolve() {
        if (m_ashMaterial == null)
            m_ashMaterial = FireColorSetupManager.instance.GetDecorationBurnMaterial( m_extingishColor );
		for (int i = 0; i < m_renderers.Length; i++) {
			int renderID = m_renderers[i].GetInstanceID();
			Material[] materials = m_renderers[i].materials;
			for (int m = 0; m < materials.Length; m++) {
				m_ashMaterial.SetTexture("_MainTex", materials[m].mainTexture);
				materials[m] = m_ashMaterial;
			}
			m_renderers[i].materials = materials;
		}
		m_ashMaterial.SetFloat("_BurnLevel", 0);
	}
    
    private void BurnedView()
    {
        Material burnedMaterial = FireColorSetupManager.instance.GetDecorationBurnedMaterial( m_extingishColor );
        int max = m_viewBurnedRenderes.Length;
        for (int i = 0; i < max; i++) {
            Material[] materials = m_viewBurnedRenderes[i].materials;
            for (int m = 0; m < materials.Length; m++) {
                materials[m] = burnedMaterial;
            }
            m_viewBurnedRenderes[i].materials = materials;
        }
    }

    private void BurnOperators() {
		for (int i = 0; i < m_passengersSpawner.Length; ++i) {
			m_passengersSpawner[i].PassengersBurn(m_burnSource);
		}

		for (int i = 0; i < m_operatorSpawner.Length; ++i) {
			if (!m_operatorSpawner[i].IsOperatorDead()) {
				m_operatorSpawner[i].OperatorBurn(m_burnSource);
			}
		}
	}


    private void ReturnAshMaterial()
    {
        if ( m_ashMaterial && FireColorSetupManager.instance)
        {
            FireColorSetupManager.instance.ReturnDecorationBurnMaterial(m_extingishColor, m_ashMaterial);
            m_ashMaterial = null;
        }
    }

    //----------------------------------------------------------------------------------------
    void OnDrawGizmosSelected() {
		if (m_fireNodes != null) {
			for (int i = 0; i < m_fireNodes.Count; i++) {
                m_fireNodes[i].OnDrawGizmosSelected(this);
			}
		}

		if (m_fireNodeSetup != null) {
			m_fireNodeSetup.OnDrawGizmosSelected();
		}
	}
}
