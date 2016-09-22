using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflammableDecoration : Initializable {

	[CommentAttribute("Add an explosion effect when this object is burned out.")]
	[SerializeField] private string m_explosionParticle = "";



	private ZoneManager m_zoneManager;

	private GameObject m_view;
	private GameObject m_viewBurned;

	private BoxCollider m_collider;

	private FireNode[] m_fireNodes;
	private bool m_burned;
	private DeltaTimer m_timer = new DeltaTimer();

	private AutoSpawnBehaviour m_autoSpawner;
	private Vector3 m_startPosition;

	private Dictionary<Renderer, Material[]> m_originalMaterials = new Dictionary<Renderer, Material[]>();
	private Material m_ashMaterial;
	public string m_ashesAsset;
	private Entity m_entity;
	public string sku { get { return m_entity.sku; } }

	private bool m_shouldExplode;



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
		ZoneManager.ZoneEffect zEffect = m_zoneManager.GetFireEffectCode(transform.position, m_entity.sku);

		if (zEffect == ZoneManager.ZoneEffect.None) {
			if (m_collider) Destroy(m_collider);
			for (int i = 0; i < m_fireNodes.Length; i++) {
				Destroy(m_fireNodes[i].gameObject);
			}
			Destroy(m_viewBurned);
			Destroy(m_autoSpawner);
			Destroy(m_entity);
			Destroy(this);
		} else {
			m_view = transform.FindChild("view").gameObject;
			m_burned = false;
			m_shouldExplode = (zEffect == ZoneManager.ZoneEffect.L);

			int coins = (m_entity == null)? 0 : m_entity.reward.coins;
			int coinsPerNode = coins / m_fireNodes.Length;

			for (int i = 0; i < m_fireNodes.Length; i++) {
				m_fireNodes[i].Init(coinsPerNode, zEffect);
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
		m_view.SetActive(true);
		m_viewBurned.SetActive(false);

		transform.localScale = Vector3.one;
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

		if (m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning ) {	// if respawning we wait
			for (int i = 0; i < m_fireNodes.Length; i++) {
				m_fireNodes[i].Disable();
			}
			return;
		}

		if (m_burned) {
			// Advance dissolve!
			m_ashMaterial.SetFloat("_BurnLevel", m_timer.GetDelta() * 3.0f);

			if (m_timer.IsFinished()) {
				m_view.SetActive(false);
				m_autoSpawner.Respawn();
				if (m_collider) m_collider.enabled = false;
			}
		} else {
			m_burned = true;
			bool oneIsDamaged = false;
			// Vector3 breathDir = Vector3.zero;	-> usefull if you need to orientate the explosion particle
			for (int i = 0; i < m_fireNodes.Length; i++) 
			{
				m_burned = m_burned && m_fireNodes[i].IsBurned();
				if (m_fireNodes[i].IsDamaged())
				{
					oneIsDamaged = true;
					// breathDir += m_fireNodes[i].lastBreathHitDiretion;
				}
			}

			bool instantExplode = ShouldInstantExplode();
			if ((instantExplode && oneIsDamaged))
			{
				for (int i = 0; i < m_fireNodes.Length; i++) 
				{
					m_fireNodes[i].InstaBurnForReward();
				}
				if (m_explosionParticle != "" )
				{
					ParticleManager.Spawn(m_explosionParticle, transform.position + Vector3.back * 3f);
					// breathDir.Normalize();	-> if you need to recolocate for the explosion
					// destroyParticle.transform.rotation.SetLookRotation( breathDir );
				}
				m_autoSpawner.Respawn();
				m_view.SetActive(false);
				m_viewBurned.SetActive(true);
				if (m_collider) m_collider.enabled = false;
				// Switch material for the view to the dark and start dissolving

			}
			else if (m_burned)
			{
				for (int i = 0; i < m_fireNodes.Length; i++) {
					m_fireNodes[i].StartSmoke(2f);
				}

				// Crumble and dissolve time
				float seconds = m_fireNodes[0].burningTime + m_fireNodes[0].burningTime * 0.4f;
				m_timer.Start( seconds * 1000 );
				// m_view.SetActive(false);
				m_viewBurned.SetActive(true);
				SwitchViewToDissolve();
			}
		}
	}

	bool ShouldInstantExplode()
	{
		// Maybe check if a critial node was hit
		if ( m_shouldExplode )
			return true;

		DragonTier _dragonTier = InstanceManager.player.data.tier;
		if (InstanceManager.player.breathBehaviour.type == DragonBreathBehaviour.Type.Super)
			return true;

		return false;
	}

	void ResetViewMaterials()
	{
		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) 
		{
			renderers[i].materials = m_originalMaterials[ renderers[i] ];
		}
	}

	void SwitchViewToDissolve()
	{
		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) 
		{
			Material[] materials = renderers[i].materials;
			for (int m = 0; m < materials.Length; m++) 
			{
				m_ashMaterial.SetTexture("_MainTex", materials[m].mainTexture);
				materials[m] = m_ashMaterial;
			}
			renderers[i].materials = materials;
		}
		m_ashMaterial.SetFloat("_BurnLevel", 0);

		if (!string.IsNullOrEmpty(m_ashesAsset)) 
		{
			GameObject particle = ParticleManager.Spawn(m_ashesAsset, m_view.transform.position, "Ashes");
			if (particle) 
			{
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
