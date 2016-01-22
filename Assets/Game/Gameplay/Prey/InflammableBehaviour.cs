using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PreyStats))]
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

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private PreyStats m_prey;
	private DragonBreathBehaviour m_breath;
	
	private float m_health;
	private float m_timer;
	private CircleArea2D m_circleArea;

	private Material m_ashMaterial;

	private State m_state;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start() {
		m_prey = GetComponent<PreyStats>();
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();

		m_timer = m_checkFireTime;

		m_circleArea = GetComponent<CircleArea2D>();


		m_ashMaterial = new Material(Resources.Load ("Game/Assets/Materials/BurnToAshes") as Material);


		m_state = State.Idle;
	}
		
	public override void Initialize() {
		m_health = m_maxHealth;
		m_state = State.Idle;
	}

	void Update() {
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			switch (m_state) {
				case State.Idle:
					m_timer = m_checkFireTime;
					if ( m_circleArea != null ) {
						if ( m_breath.Overlaps( m_circleArea ) )
							Burn(m_breath.damage);
					}
					else if (m_breath.IsInsideArea(transform.position)) {
						Burn(m_breath.damage);
					}
					break;

				case State.Burned:
					// Particles
					if (m_ashesAsset.Length > 0) {
						SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();	
						GameObject particle = ParticleManager.Spawn("Ashes/" + m_ashesAsset, renderer.transform.position);
						particle.transform.rotation = renderer.transform.rotation;
						particle.transform.localScale = renderer.transform.localScale;
					}



					m_state = State.Ashes;
					m_timer = m_dissolveTime;
					break;

				case State.Ashes:
					if (m_destroyOnBurn) {
						DestroyObject(gameObject);
					} else {
						gameObject.SetActive(false);
					}
					break;
			}
		} else if (m_state == State.Ashes) {
			m_ashMaterial.SetFloat("_AshLevel", Mathf.Min(1, Mathf.Max(0, 1 - (m_timer / m_dissolveTime))));
		}
	}

	public void Burn(float _damage) {

		if (m_health > 0) {

			m_health -= _damage;

			if (m_health <= 0) {
				// Let heirs do their magic
				OnBurn();

				// Get the reward to be given from the prey stats
				Reward reward = m_prey.GetOnKillReward();

				// Dispatch global event
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, this.transform, reward);

				// Material
				SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
				for (int i = 0; i < renderers.Length; i++) {
					Material[] materials = renderers[i].materials;
					for (int m = 0; m < materials.Length; m++) {
						materials[m] = m_ashMaterial;
					}
					renderers[i].materials = materials;
				}

				// Deactivate edible
				EdibleBehaviour edible = GetComponent<EdibleBehaviour>();
				if (edible != null) {
					edible.enabled = false;
				}

				PreyMotion motion = GetComponent<PreyMotion>();
				if (motion != null) {
					motion.enabled = false;
				}

				m_ashMaterial.SetFloat("_AshLevel", 0);

				m_state = State.Burned;
				m_timer = 0.5f; //secs

				// Add burned particle!
				GameObject burnParticle = PoolManager.GetInstance("BurnParticle");
				if (burnParticle != null)
				{
					burnParticle.transform.position = transform.position + Vector3.back * 2;
					BurnParticle bp = burnParticle.GetComponent<BurnParticle>();
					bp.Activate( 2, 1.0f);
				}
			}
		}
	}

	protected virtual void OnBurn() {}
}
