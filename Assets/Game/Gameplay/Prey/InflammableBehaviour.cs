using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Entity))]
public class InflammableBehaviour : Initializable {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	enum State {
		Idle = 0,
		Burned,
		Ashes
	};
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private bool m_destroyOnBurn = false;
	[SerializeField] private float m_checkFireTime = 0.25f;
	[SerializeField] private float m_maxHealth = 100f;

	[SeparatorAttribute]
	[SerializeField] private string m_ashesAsset;
	[SerializeField] private float m_dissolveTime = 1.5f;

	[SeparatorAttribute]
	[Header("Explosion")]
	[SerializeField] private GameObject m_explosionPrefab = null;
	[SerializeField] private Range m_scaleRange = new Range(1f, 5f);
	[SerializeField] private Range m_rotationRange = new Range(0f, 360f);
	[SerializeField] private bool m_shake = false;
	[SerializeField] private bool m_slowMotion = false;

	[SeparatorAttribute]
	[Header("Burn Settings")]
	[SerializeField] private float m_burnSize = 4;
	[SerializeField] private float m_burnDuration = 0.75f;
	[SerializeField] private bool m_orientedBurn = true;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Entity m_entity;

	public string sku
	{
		get{ return m_entity.sku; }
	}
	
	private float m_health;
	private float m_timer;

	private List<Material[]> m_ashMaterials = new List<Material[]>();
	private Renderer[] m_renderers;


	private State m_state;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start() {
		if (m_explosionPrefab != null) {
			PoolManager.CreatePool(m_explosionPrefab, 5, false);
		}

		m_entity = GetComponent<Entity>();

		m_timer = m_checkFireTime;

		// Renderers And Materials
		m_renderers = GetComponentsInChildren<Renderer>();
		if ( m_renderers.Length > 0 )
		{
			for( int i = 0;i<m_renderers.Length; i++ )
			{
				Renderer renderer = m_renderers[i];
				Material[] materials = new Material[ renderer.materials.Length];
				for( int j = 0; j<renderer.materials.Length; j++ )
				{
					string shaderName = renderer.materials[j].shader.name;
					if ( shaderName.EndsWith("Additive") )
					{
						// We will set to null and hide it at the beggining 
						materials[j] = null;
					}
					else if ( shaderName.EndsWith("Bird") )
					{
						// We ignore mask because its used for masking the diffuse texture
						Material newMat = Resources.Load ("Game/Assets/Materials/BurnToAshes") as Material;	
						newMat.renderQueue = 3000;
						materials[j] = newMat;
					}
					else
					{
						Material newMat = Resources.Load ("Game/Assets/Materials/BurnToAshes") as Material;
						newMat.SetTexture("_AlphaMask", m_renderers[i].material.mainTexture );
						newMat.renderQueue = 3000;
						materials[j] = newMat;
					}
				}
				m_ashMaterials.Add(materials);
			}
		}
		m_dissolveTime = 10;
		m_state = State.Idle;
	}
		
	public override void Initialize() {
		m_health = m_maxHealth;
		m_state = State.Idle;
	}

	void Update() {
		if (m_state != State.Idle) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				switch (m_state) {
					case State.Burned:
					{
						// Particles
						if (m_ashesAsset.Length > 0) {
							Renderer renderer = GetComponentInChildren<Renderer>();	
							GameObject particle = ParticleManager.Spawn(m_ashesAsset, renderer.transform.position, "Ashes/");
							if (particle) {
								particle.transform.rotation = renderer.transform.rotation;
								particle.transform.localScale = renderer.transform.localScale;
							}
						}

						m_state = State.Ashes;
						// Deactivate collider if needed
						Collider c = GetComponent<Collider>();
						if (c != null)
							c.enabled = false;
						m_timer = m_dissolveTime;
					}break;
					case State.Ashes:
					{
						if (m_destroyOnBurn) {
							DestroyObject(gameObject);
						} else {
							Collider c = GetComponent<Collider>();
							if (c != null)
								c.enabled = true;
							gameObject.SetActive(false);
						}
					}break;
				}
			} 
			else if (m_state == State.Ashes) 
			{
				float ashLevel = Mathf.Min(1, Mathf.Max(0, 1 - (m_timer / m_dissolveTime)));
				for( int i = 0; i<m_ashMaterials.Count; i++ )
				{
					Material[] mats = m_ashMaterials[i];
					for( int j = 0; j<mats.Length; j++ )
					{
						mats[j].SetFloat("_AshLevel", ashLevel);	
					}
				}

			}
		}
	}

	public void Burn(float _damage, Transform _from) {

		if (m_state == State.Idle) {

			//spawn fire hit particle!	
			ParticleManager.Spawn("PF_FireHit", transform.position + Vector3.back * 2);

			m_health -= _damage;

			if (m_health <= 0) {
				EntityManager.instance.Unregister(m_entity);
				
				// Let heirs do their magic
				OnBurn();

				// Get the reward to be given from the entity
				Reward reward = m_entity.GetOnKillReward(true);

				// Dispatch global event
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, this.transform, reward);

				// Material
				for (int i = 0; i < m_renderers.Length; i++) //  Force each renderer to have only one material!!!
				{
					/*
					Material[] materials = renderers[i].materials;
					for (int m = 0; m < materials.Length; m++) {
						materials[m] = m_ashMaterial;
					}
					renderers[i].materials = materials;
					*/
					if ( m_ashMaterials[i] != null )
					{
						m_renderers[i].materials = m_ashMaterials[i];
					}
					else
					{
						m_renderers[i].enabled = false;
					}

				}

				// Deactivate edible
				EdibleBehaviour edible = GetComponent<EdibleBehaviour>();
				if (edible != null) {
					edible.enabled = false;
				}

				// Disable colliders if they give us problems
				if ( GetComponent<MineBehaviour>() != null || GetComponent<CurseAttackBehaviour>() != null )
				{
					Collider c = GetComponent<Collider>();
					if (c != null)
						c.enabled = false;
				}

				PreyMotion motion = GetComponent<PreyMotion>();
				if (motion != null) 
				{
					motion.StartBurning();
					// motion.enabled = false;
				}

				// m_ashMaterial.SetFloat("_AshLevel", 0);

				m_state = State.Burned;
				m_timer = 0.5f; //secs

				// Add burned particle!
				GameObject burnParticle = PoolManager.GetInstance("BurnParticle");
				if (burnParticle != null) 
				{
					burnParticle.transform.position = transform.position + Vector3.back * 2;
					if ( m_orientedBurn )
					{
						Vector3 dir =  burnParticle.transform.position - _from.position;

						Quaternion q = burnParticle.transform.rotation;
						q.SetLookRotation( Vector3.forward, dir );
						burnParticle.transform.rotation = q;
					}
					else
					{
						burnParticle.transform.rotation = Quaternion.identity;
					}

					BurnParticle bp = burnParticle.GetComponent<BurnParticle>();
					bp.Activate( m_burnSize, m_burnDuration);
				}

				if (m_explosionPrefab != null) {
					GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);
					if (explosion != null) {
						Vector3 pos = transform.position;
						pos.z = -10;
						explosion.transform.position = pos;

						// Random scale within range
						explosion.transform.localScale = Vector3.one * m_scaleRange.GetRandom();

						// Random rotation within range
						explosion.transform.Rotate(0, 0, m_rotationRange.GetRandom());

						if (m_shake) {							
							GameCameraController camera = Camera.main.GetComponent<GameCameraController>();
							camera.Shake();
						}

						if (m_slowMotion) {
							SlowMotionController camera = Camera.main.GetComponent<SlowMotionController>();
							camera.StartSlowMotion();
						}
					}
				}
			}
		}
	}

	protected virtual void OnBurn() {}

}
