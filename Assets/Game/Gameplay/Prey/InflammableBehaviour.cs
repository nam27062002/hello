using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PreyStats))]
public class InflammableBehaviour : Initializable {
	
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private bool m_destroyOnBurn = false;
	[SerializeField] private float m_checkFireTime = 0.25f;
	[SerializeField] private float m_maxHealth = 100f;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private PreyStats m_prey;
	private DragonBreathBehaviour m_breath;
	
	private float m_health;
	private float m_timer;
	private CircleArea2D m_circleArea;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start() {
		m_prey = GetComponent<PreyStats>();
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();

		m_timer = m_checkFireTime;

		m_circleArea = GetComponent<CircleArea2D>();
	}

	void OnEnable() {

	}

	void OnDisable() {

	}
	
	public override void Initialize() {
		m_health = m_maxHealth;
	}

	void Update() {

		m_timer -= Time.deltaTime;
		if (m_timer <= 0) 
		{
			if ( m_circleArea != null )
			{
				if ( m_breath.Overlaps( m_circleArea ) )
					Burn(m_breath.damage);
			}
			else if (m_breath.IsInsideArea(transform.position)) {
				Burn(m_breath.damage);
			}
			m_timer = m_checkFireTime;
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

				// Particles
				ParticleManager.Spawn("SmokePuff", transform.position);

				// deactivate
				if (m_destroyOnBurn) {
					DestroyObject(gameObject);
				} else {
					gameObject.SetActive(false);
				}
			}
		}
	}

	protected virtual void OnBurn() {}
}
