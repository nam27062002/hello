using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflammableDecoration : Initializable {

	[SerializeField] private float m_burningTime;
	[SerializeField] private ParticleData m_feedbackParticle;
	// PF_FireHit
	[SerializeField] private bool m_feedbackParticleMatchDirection = false;
	[SerializeField] private ParticleData m_burnParticle;
	//PF_FireProc
	[SerializeField] private ParticleData m_disintegrateParticle;
	[SerializeField] private string m_ashesAsset;

	[SeparatorAttribute("Fire Nodes auto setup")]
	[SerializeField] private int m_boxelSize = 2;
	[SerializeField] private float m_hitRadius = 1.5f;


	// private ZoneManager m_zoneManager;
	// private ZoneManager.ZoneEffect m_zoneEffect;
	private FireNodeSetup m_fireNodeSetup;

	private GameObject m_view;
	private GameObject m_viewBurned;

	private BoxCollider m_collider;

	private FireNode[] m_fireNodes;
	private bool m_isBurning;
	private bool m_burned;
	private DeltaTimer m_timer = new DeltaTimer();

	private AutoSpawnBehaviour m_autoSpawner;
	private DestructibleDecoration m_destructibleBehaviour;
	protected DeviceOperatorSpawner m_operatorSpawner;
	private Vector3 m_startPosition;

	private Dictionary<Renderer, Material[]> m_originalMaterials = new Dictionary<Renderer, Material[]>();
	private Material m_ashMaterial;

	private Decoration m_entity;

	public string sku { get { return m_entity.sku; } }

	// Use this for initialization
	void Awake() {
		m_fireNodes = transform.GetComponentsInChildren<FireNode>(true);

		ParticleManager.CreatePool("SmokeParticle", "");
		ParticleManager.CreatePool(m_feedbackParticle);
		ParticleManager.CreatePool(m_burnParticle);
		ParticleManager.CreatePool(m_disintegrateParticle);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(GameEvents.GAME_AREA_ENTER, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(GameEvents.GAME_AREA_ENTER, OnLevelLoaded);
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {		
		m_entity = GetComponent<Decoration>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponent<DeviceOperatorSpawner>();
		m_viewBurned = transform.FindChild("view_burned").gameObject;
		m_collider = GetComponent<BoxCollider>();

		/*
		m_zoneManager = GameObjectExt.FindComponent<ZoneManager>(true);
		if (m_zoneManager != null)
			m_zoneEffect = m_zoneManager.GetFireEffectCode(m_entity, InstanceManager.player.data.tier);
		else{
			m_zoneEffect = ZoneManager.ZoneEffect.None;
			Debug.LogWarning("No Zone Manager");
		}
		*/

		/*
		if (m_zoneEffect == ZoneManager.ZoneEffect.None) {
			if (m_collider) Destroy(m_collider);
			for (int i = 0; i < m_fireNodes.Length; i++) {
				Destroy(m_fireNodes[i].gameObject);
			}
			if (m_viewBurned) Destroy(m_viewBurned);
			Destroy(m_autoSpawner);
			Destroy(m_entity);
			Destroy(this);
		} else {
		*/
		m_destructibleBehaviour = GetComponent<DestructibleDecoration>();
		m_view = transform.FindChild("view").gameObject;
		m_burned = false;
		m_isBurning = false;

		for (int i = 0; i < m_fireNodes.Length; i++) {
			m_fireNodes[i].Init(m_entity, m_burnParticle, m_feedbackParticle, m_feedbackParticleMatchDirection, m_hitRadius);
		}
		m_startPosition = transform.position;

		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			m_originalMaterials[renderers[i]] = renderers[i].materials;
		}
		m_ashMaterial = new Material(Resources.Load("Game/Materials/RedBurnToAshes") as Material);
		m_ashMaterial.renderQueue = 3000;// Force transparent
		// }
	}

	public override void Initialize() {
		enabled = true;

		m_view.SetActive(true);
		m_viewBurned.SetActive(false);

		m_burned = false;
		m_isBurning = false;

		for (int i = 0; i < m_fireNodes.Length; i++) {
			m_fireNodes[i].Reset();
		}

		enabled = true;

		transform.position = m_startPosition;
		ResetViewMaterials();
	}

	public bool IsBurning() {
		return m_isBurning;
	}

	// Update is called once per frame
	void Update() {
		if (m_autoSpawner == null)
			return;

		if (m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning) {	// if respawning we wait
			for (int i = 0; i < m_fireNodes.Length; i++) {
				m_fireNodes[i].Disable();
			}
			return;
		}

		if (m_burned) {
			// Advance dissolve!
			m_ashMaterial.SetFloat("_BurnLevel", m_timer.GetDelta() * 3.0f);

			if (m_timer.GetDelta() > 0.75f) {
				for (int i = 0; i < m_fireNodes.Length; i++) {
					m_fireNodes[i].Extinguish();
				}
			}

			if (m_timer.IsFinished()) {
				m_view.SetActive(false);
				m_autoSpawner.StartRespawn();
				if (m_collider)
					m_collider.enabled = false;
			}
		} else {
			m_isBurning = false;
			m_burned = true;
			bool reachedByFire = false;
			DragonTier lastBurnTier = DragonTier.COUNT;

			for (int i = 0; i < m_fireNodes.Length && !reachedByFire; i++) {
				FireNode node = m_fireNodes[i];
				m_isBurning = m_isBurning || node.IsBurning();
				m_burned = m_burned && node.IsBurning();
				reachedByFire = node.IsExtinguishing();
				lastBurnTier = node.lastBurnTier;
			}

			if (reachedByFire) {
				for (int i = 0; i < m_fireNodes.Length; i++) {
					m_fireNodes[i].Burn(Vector2.zero, false, lastBurnTier);
				}

				if (m_operatorSpawner != null && !m_operatorSpawner.IsOperatorDead()) {
					m_operatorSpawner.OperatorBurn();
				}
				
				ZoneManager.ZoneEffect effect = InstanceManager.zoneManager.GetFireEffectCode(m_entity, lastBurnTier);
				if (effect == ZoneManager.ZoneEffect.L) {
					if (m_disintegrateParticle.IsValid()) {
						ParticleManager.Spawn(m_disintegrateParticle, transform.position + m_disintegrateParticle.offset);
					}
				}
				
				m_autoSpawner.StartRespawn();
				m_view.SetActive(false);
				m_viewBurned.SetActive(true);
				if (m_collider) m_collider.enabled = false;

				// [AOC] Notify game!
				Reward reward = new Reward();	// [AOC] TODO!! Should decorations have a reward?
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, transform, reward);
			} else {
				if (m_isBurning) {
					if (m_destructibleBehaviour != null) {
						m_destructibleBehaviour.enabled = false;
					}
				}

				if (m_burned) {
					// Crumble and dissolve time
					float seconds = m_burningTime;
					m_timer.Start(seconds * 1000);

					if (m_operatorSpawner != null && !m_operatorSpawner.IsOperatorDead()) {
						m_operatorSpawner.OperatorBurn();
					}

					m_viewBurned.SetActive(true);
					SwitchViewToDissolve();
				}
			}
		}
	}

	private void ResetViewMaterials() {
		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			if (m_originalMaterials.ContainsKey(renderers[i])) {
				renderers[i].materials = m_originalMaterials[renderers[i]];
			}
		}
	}

	private void SwitchViewToDissolve() {
		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			if (m_originalMaterials.ContainsKey(renderers[i])) {
				Material[] materials = renderers[i].materials;
				for (int m = 0; m < materials.Length; m++) {
					m_ashMaterial.SetTexture("_MainTex", materials[m].mainTexture);
					materials[m] = m_ashMaterial;
				}
				renderers[i].materials = materials;
			}
		}
		m_ashMaterial.SetFloat("_BurnLevel", 0);

		if (!string.IsNullOrEmpty(m_ashesAsset)) {
			GameObject particle = ParticleManager.Spawn(m_ashesAsset, m_view.transform.position, "Ashes");
			if (particle != null) {
				particle.transform.rotation = m_view.transform.rotation;
				particle.transform.localScale = m_view.transform.localScale;
			}
		}
	}

	public void SetupFireNodes() {
		if (m_fireNodeSetup == null) {
			m_fireNodeSetup = new FireNodeSetup();
		}

		m_fireNodeSetup.Init(transform);
		m_fireNodeSetup.Build(m_boxelSize);
	}

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
