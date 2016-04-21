using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InflammableDecoration : Initializable {

	[CommentAttribute("Add an explosion effect when this object is burned out.")]
	[SerializeField] private string m_explosionParticle = "";

	private GameObject m_view;
	private GameObject m_viewBurned;

	private FireNode[] m_fireNodes;
	private bool m_burned;
	private DeltaTimer m_timer = new DeltaTimer();

	private AutoSpawnBehaviour m_autoSpawner;
	private Vector3 m_startPosition;

	private Dictionary<Renderer, Material[]>  m_originalMaterials = new Dictionary<Renderer, Material[]>();
	private Material m_ashMaterial;
	public string m_ashesAsset;
	private Entity m_entity;
	public string sku
	{
		get{ return m_entity.sku; }
	}

	private bool m_shouldExplode;

	// Use this for initialization
	IEnumerator Start()
	{
		while( !InstanceManager.GetSceneController<GameSceneControllerBase>().IsLevelLoaded())
		{
			yield return null;
		}

		m_entity = GetComponent<Entity>();
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_view = transform.FindChild("view").gameObject;
		m_viewBurned = transform.FindChild("view_burned").gameObject;
		m_fireNodes = transform.GetComponentsInChildren<FireNode>();
		m_burned = false;

		int coins = 0;

		if (GetComponent<Entity>() != null) 
		{
			coins = GetComponent<Entity>().reward.coins;
		}

		int coinsPerNode = coins / m_fireNodes.Length;

		DragonBreathBehaviour breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
		bool _canBeBurned = breath.CanBurn( this );
		m_shouldExplode = breath.ShouldExplode( this );
		for (int i = 0; i < m_fireNodes.Length - 1; i++) {
			m_fireNodes[i].Init(coinsPerNode, _canBeBurned);
		}

		m_fireNodes[m_fireNodes.Length - 1].Init(coins - (coinsPerNode * (m_fireNodes.Length - 1)), _canBeBurned);

		m_startPosition = transform.position;


		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) 
		{
			m_originalMaterials[ renderers[i] ] = renderers[i].materials;
		}
		m_ashMaterial = new Material(Resources.Load ("Game/Assets/Materials/BurnToAshes") as Material);
	}

	public override void Initialize() {
		m_view.SetActive(true);
		m_viewBurned.SetActive(false);

		transform.localScale = Vector3.one;
		m_burned = false;

		for (int i = 0; i < m_fireNodes.Length; i++) 
		{
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

		if ( m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning )	// if respawning we wait
			return;

		if (m_burned) 
		{
			// Advance dissolve!
			m_ashMaterial.SetFloat("_AshLevel", m_timer.GetDelta());

			if ( m_timer.Finished() )
			{
				m_view.SetActive(false);
				m_autoSpawner.Respawn();
			}
		} 
		else 
		{
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
				// Switch material for the view to the dark and start dissolving

			}
			else if (m_burned)
			{
				for (int i = 0; i < m_fireNodes.Length; i++) 
				{
					m_fireNodes[i].StartSmoke();
				}

				// Crumble and dissolve time
				m_timer.Start( 2f );
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
				m_ashMaterial.SetTexture("_AlphaMask", materials[m].mainTexture);
				materials[m] = m_ashMaterial;
			}
			renderers[i].materials = materials;
		}
		m_ashMaterial.SetFloat("_AshLevel", 0);

		if (!string.IsNullOrEmpty( m_ashesAsset)) 
		{
			GameObject particle = ParticleManager.Spawn(m_ashesAsset, m_view.transform.position, "Ashes/");
			if (particle) 
			{
				particle.transform.rotation = m_view.transform.rotation;
				particle.transform.localScale = m_view.transform.localScale;
			}
		}
	}
}
