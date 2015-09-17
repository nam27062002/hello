using UnityEngine;
using System.Collections;

public class InflammableBehaviour : Initializable {
	
	
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private bool m_destroyOnBurn = false;



	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private PreyStats m_prey;


	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start() {
		m_prey = GetComponent<PreyStats>();
	}

	void OnEnable() {

	}

	void OnDisable() {

	}
	
	public override void Initialize() {

	}

	// Update is called once per frame
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
