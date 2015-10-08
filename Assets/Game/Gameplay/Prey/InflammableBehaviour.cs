using UnityEngine;
using System.Collections;

public class InflammableBehaviour : Initializable {
	
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private bool m_destroyOnBurn = false;
	[SerializeField] private float m_checkFireTime = 0.25f;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private PreyStats m_prey;
	private DragonBreathBehaviour m_breath;

	private float m_timer;
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start() {
		m_prey = GetComponent<PreyStats>();
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();

		m_timer = m_checkFireTime;
	}

	void OnEnable() {

	}

	void OnDisable() {

	}
	
	public override void Initialize() {

	}

	void Update() {

		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			if (m_breath.IsInsideArea(transform.position)) {
				Burn(m_breath.damage);
			}
			m_timer = m_checkFireTime;
		}
	}

	public void Burn(float _damage) {

		if (m_prey.health > 0) {

			m_prey.AddLife(-_damage);

			if (m_prey.health <= 0) {

				OnBurn();

				//TODO: Drop money event?
				//

				// Particles
				InstanceManager.particles.Spaw("SmokePuff", transform.position);
								
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
