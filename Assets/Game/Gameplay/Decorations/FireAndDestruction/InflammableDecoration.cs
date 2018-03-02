using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflammableDecoration : MonoBehaviour, ISpawnable {

	private enum State {
		Respawn = 0,
		Idle,
		Burning,
		Extinguish,
		Explode
	};

	[SerializeField] private float m_burningTime;
	[SerializeField] private ParticleData m_feedbackParticle;
	// PF_FireHit
	[SerializeField] private bool m_feedbackParticleMatchDirection = false;
	[SerializeField] private ParticleData m_burnParticle;
	//PF_FireProc
	[SerializeField] private ParticleData m_disintegrateParticle;

	[SeparatorAttribute("Fire Nodes auto setup")]
	[SerializeField] private int m_boxelSize = 2;
	[SerializeField] private float m_hitRadius = 1.5f;


	private FireNodeSetup m_fireNodeSetup;

	private GameObject m_view;
	private GameObject m_viewBurned;

	private BoxCollider m_collider;

	private FireNode[] m_fireNodes;

	private AutoSpawnBehaviour m_autoSpawner;
	private DestructibleDecoration m_destructibleBehaviour;
	protected DeviceOperatorSpawner m_operatorSpawner;
	protected DevicePassengersSpawner m_passengersSpawner;
	private Vector3 m_startPosition;

	private Renderer[] m_renderers;
	private Dictionary<int, List<Material>> m_originalMaterials = new Dictionary<int, List<Material>>();
	private Material m_ashMaterial;

	private Decoration m_entity;

	private ParticleHandler m_explosionProcHandler;


	private DeltaTimer m_timer = new DeltaTimer();
	private State m_state;
	private State m_nextState;

	private bool m_initialized = false;

	private IEntity.Type m_burnSource = IEntity.Type.OTHER;

	public string sku { get { return m_entity.sku; } }

	// Use this for initialization
	void Awake() {
		m_fireNodes = transform.GetComponentsInChildren<FireNode>(true);

		m_feedbackParticle.CreatePool();
		m_burnParticle.CreatePool();
	
		m_disintegrateParticle.CreatePool();

		m_explosionProcHandler = ParticleManager.CreatePool("PF_FireExplosionProc");

		m_view = transform.Find("view").gameObject;
		m_viewBurned = transform.Find("view_burned").gameObject;

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
		m_ashMaterial = new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material);
		m_ashMaterial.renderQueue = 3000;// Force transparent

		m_state = m_nextState = State.Idle;

		m_initialized = false;
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, OnLevelLoaded);
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {		
		m_entity = GetComponent<Decoration>();
		m_collider = GetComponent<BoxCollider>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponent<DeviceOperatorSpawner>();
		m_passengersSpawner = GetComponent<DevicePassengersSpawner>();
		m_destructibleBehaviour = GetComponent<DestructibleDecoration>();


		for (int i = 0; i < m_fireNodes.Length; i++) {
			m_fireNodes[i].Init(this, m_entity, m_burnParticle, m_feedbackParticle, m_feedbackParticleMatchDirection, m_hitRadius);
		}
		m_startPosition = transform.position;

		m_initialized = true;
	}

	public void SetupFireNodes() {
		if (m_fireNodeSetup == null) {
			m_fireNodeSetup = new FireNodeSetup();
		}

		m_fireNodeSetup.Init(transform);
		m_fireNodeSetup.Build(m_boxelSize);
	}

	public void Spawn(ISpawner _spawner) {
		enabled = true;

		m_view.SetActive(true);
		m_viewBurned.SetActive(false);

		for (int i = 0; i < m_fireNodes.Length; i++) {
			m_fireNodes[i].Reset();
		}

		transform.position = m_startPosition;
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
				if (m_operatorSpawner != null && !m_operatorSpawner.IsOperatorDead()) {
					m_operatorSpawner.OperatorBurn(m_burnSource);
				}

				if (m_passengersSpawner != null) {
					m_passengersSpawner.PassengersBurn(m_burnSource);
				}

				m_timer.Start(m_burningTime * 1000);

				m_viewBurned.SetActive(true);

				SwitchViewToDissolve();

				// Initialize some death info
				m_entity.onDieStatus.source = m_burnSource;

				Messenger.Broadcast<Transform, Reward>(MessengerEvents.ENTITY_BURNED, transform, m_entity.reward);

				break;

			case State.Explode:
				if (m_operatorSpawner != null && !m_operatorSpawner.IsOperatorDead()) {
					m_operatorSpawner.OperatorBurn(m_burnSource);
				}

				if (m_passengersSpawner != null) {
					m_passengersSpawner.PassengersBurn(m_burnSource);
				}

				//m_disintegrateParticle.Spawn(transform.position + m_disintegrateParticle.offset);
				for (int i = 0; i < m_fireNodes.Length; ++i) {
					if (i % 2 == 0) {
						FireNode n = m_fireNodes[i];
						GameObject ex = m_explosionProcHandler.Spawn(null, n.transform.position);
						if (ex != null) {
							ex.transform.localScale = n.transform.localScale * 1.0f;
							ex.GetComponent<ExplosionProcController>().Explode(i * 0.015f); //delay
						}
					}
				}

				// Initialize some death info
				m_entity.onDieStatus.source = m_burnSource;

				Messenger.Broadcast<Transform, Reward>(MessengerEvents.ENTITY_BURNED, transform, m_entity.reward);

				m_timer.Start(250f);
				break;
		}

		m_state = m_nextState;
	}

	// Update is called once per frame
	public void CustomUpdate() {		
		if (m_initialized) {
			if (m_state != m_nextState) {
				ChangeState();
			}

			switch (m_state) {
				case State.Burning: {
						bool allNodesBurning = true;
						for (int i = 0; i < m_fireNodes.Length; ++i) {
							allNodesBurning = allNodesBurning && m_fireNodes[i].IsBurning();
						}

						if (allNodesBurning) {
							m_nextState = State.Extinguish;
						}
					} break;

				case State.Extinguish:
					// Advance dissolve!
					m_ashMaterial.SetFloat("_BurnLevel", m_timer.GetDelta() * 3.0f);

					if (m_timer.IsFinished()) {
						bool extinguished = true;
						for (int i = 0; i < m_fireNodes.Length; ++i) {
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
					break;

				case State.Explode:
					for (int i = 0; i < m_fireNodes.Length; ++i) {
						m_fireNodes[i].Explode();
					}
					if (m_timer.IsFinished()) {
						Destroy();
					}
					break;
			}
		}
	}

	public void LetsBurn(bool _explode, IEntity.Type _source) {
		if (m_state == State.Idle) {
			if (_explode) 	m_nextState = State.Explode;
			else 			m_nextState = State.Burning;

			m_burnSource = _source;
		}
	}

	private void Destroy() {
		m_view.SetActive(false);
		m_viewBurned.SetActive(true);
		if (m_collider) m_collider.isTrigger = true;
		if (m_autoSpawner) m_autoSpawner.StartRespawn();

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


	//----------------------------------------------------------------------------------------
	void OnDrawGizmosSelected() {
		if (m_fireNodes != null) {
			Gizmos.color = Color.magenta;
			for (int i = 0; i < m_fireNodes.Length; i++) {
				Gizmos.DrawSphere(m_fireNodes[i].transform.position, 0.25f);
			}
		}

		if (m_fireNodeSetup != null) {
			m_fireNodeSetup.OnDrawGizmosSelected();
		}
	}
}
