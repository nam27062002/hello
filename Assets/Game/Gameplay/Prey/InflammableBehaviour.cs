using UnityEngine;
using System.Collections;

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
	private Entity m_prey;
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
		if (m_explosionPrefab != null) {
			PoolManager.CreatePool(m_explosionPrefab, 5, false);
		}

		m_prey = GetComponent<Entity>();
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
							Burn(m_breath.damage, m_breath.transform);
					}
					else if (m_breath.IsInsideArea(transform.position)) {
						Burn(m_breath.damage, m_breath.transform);
					}
					break;

				case State.Burned:
					// Particles
					if (m_ashesAsset.Length > 0) {
						Renderer renderer = GetComponentInChildren<Renderer>();	
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

	public void Burn(float _damage, Transform _from) {

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
				Renderer[] renderers = GetComponentsInChildren<Renderer>();
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
				if (motion != null) 
				{
					motion.StartBurning();
					// motion.enabled = false;
				}

				m_ashMaterial.SetFloat("_AshLevel", 0);

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
