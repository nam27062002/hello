using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflammableDecoration : Initializable {

	[SerializeField] private float m_burningTime;
	[CommentAttribute("Add an explosion effect when this object is burned out.")]
	[SerializeField] private string m_explosionParticle = "";
	[SerializeField] private string m_ashesAsset;


	private ZoneManager m_zoneManager;
	private ZoneManager.ZoneEffect m_zoneEffect;

	private GameObject m_view;
	private GameObject m_viewBurned;

	private BoxCollider m_collider;

	private FireNode[] m_fireNodes;
	private bool m_burned;
	private DeltaTimer m_timer = new DeltaTimer();

	private AutoSpawnBehaviour m_autoSpawner;
	private DestructibleDecoration m_destructibleBehaviour;
	private Vector3 m_startPosition;

	private Dictionary<Renderer, Material[]> m_originalMaterials = new Dictionary<Renderer, Material[]>();
	private Material m_ashMaterial;

	private Entity m_entity;
	public string sku { get { return m_entity.sku; } }



	// Use this for initialization
	void Start() {		
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

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		m_entity = GetComponent<Entity>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_viewBurned = transform.FindChild("view_burned").gameObject;
		m_fireNodes = transform.GetComponentsInChildren<FireNode>(true);
		m_collider = GetComponent<BoxCollider>();

		m_zoneManager = GameObjectExt.FindComponent<ZoneManager>(true);
		if ( m_zoneManager != null )
			m_zoneEffect = m_zoneManager.GetFireEffectCode(transform.position, m_entity.sku);
		else{
			m_zoneEffect = ZoneManager.ZoneEffect.None;
			Debug.LogWarning("No Zone Manager");
		}

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
			m_destructibleBehaviour = GetComponent<DestructibleDecoration>();
			m_view = transform.FindChild("view").gameObject;
			m_burned = false;

			int coins = (m_entity == null)? 0 : m_entity.reward.coins;
			int coinsPerNode = coins / m_fireNodes.Length;

			for (int i = 0; i < m_fireNodes.Length; i++) {
				m_fireNodes[i].Init(coinsPerNode, m_zoneEffect);
			}
			m_startPosition = transform.position;

			Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renderers.Length; i++) {
				m_originalMaterials[ renderers[i] ] = renderers[i].materials;
			}
			m_ashMaterial = new Material(Resources.Load ("Game/Assets/Materials/RedBurnToAshes") as Material);
			m_ashMaterial.renderQueue = 3000;// Force transparent
		}
	}

	public override void Initialize() {
		enabled = true;

		m_view.SetActive(true);
		m_viewBurned.SetActive(false);

		m_burned = false;

		for (int i = 0; i < m_fireNodes.Length; i++) {
			m_fireNodes[i].Reset();
		}

		enabled = true;

		transform.position = m_startPosition;
		ResetViewMaterials();
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

		switch (m_zoneEffect) {
			case ZoneManager.ZoneEffect.S:
				// only feedback from nodes
				break;

			case ZoneManager.ZoneEffect.M: {
					// burning 
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
							if (m_collider) m_collider.enabled = false;
						}
					} else {
						bool isBurning = false;
						m_burned = true;
						for (int i = 0; i < m_fireNodes.Length; i++) {
							isBurning = isBurning || m_fireNodes[i].IsBurning();
							m_burned = m_burned && m_fireNodes[i].IsBurning();
						}

						if (isBurning) {
							if (m_destructibleBehaviour != null) {
								m_destructibleBehaviour.enabled = false;
							}
						}

						if (m_burned) {
							// Crumble and dissolve time
							float seconds = m_burningTime;
							m_timer.Start(seconds * 1000);

							m_viewBurned.SetActive(true);
							SwitchViewToDissolve();
						}
					}
				} break;

			case ZoneManager.ZoneEffect.L: {
					// lets explode!
					bool reachedByFire = false;
					for (int i = 0; i < m_fireNodes.Length && !reachedByFire; i++) {
						reachedByFire = m_fireNodes[i].IsExtinguishing();
					}

					if (reachedByFire) {
						for (int i = 0; i < m_fireNodes.Length; i++) {
							m_fireNodes[i].Burn(Vector2.zero, false);
						}

						if (m_explosionParticle != "") {
							ParticleManager.Spawn(m_explosionParticle, transform.position + Vector3.back * 3f);
						}

						m_autoSpawner.StartRespawn();
						m_view.SetActive(false);
						m_viewBurned.SetActive(true);
						if (m_collider) m_collider.enabled = false;
					}
				} break;
		}
	}

	void ResetViewMaterials() {
		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].materials = m_originalMaterials[ renderers[i] ];
		}
	}

	void SwitchViewToDissolve() {
		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			Material[] materials = renderers[i].materials;
			for (int m = 0; m < materials.Length; m++) 
			{
				m_ashMaterial.SetTexture("_MainTex", materials[m].mainTexture);
				materials[m] = m_ashMaterial;
			}
			renderers[i].materials = materials;
		}
		m_ashMaterial.SetFloat("_BurnLevel", 0);

		if (!string.IsNullOrEmpty(m_ashesAsset)) {
			GameObject particle = ParticleManager.Spawn(m_ashesAsset, m_view.transform.position, "Ashes");
			if (particle) {
				particle.transform.rotation = m_view.transform.rotation;
				particle.transform.localScale = m_view.transform.localScale;
			}
		}
	}

	void OnDrawGizmosSelected() {
		if (m_fireNodes != null) {
			Gizmos.color = Color.magenta;
			for (int i = 0; i < m_fireNodes.Length; i++) {
				Gizmos.DrawSphere(m_fireNodes[i].transform.position, 0.25f);
			}
		}
	}
}
