using UnityEngine;
using System.Collections;

public class DragonBoostBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer 	m_dragon;
	private DragonControl 	m_controls;
	private DragonHealthBehaviour m_healthBehaviour;

	private bool m_active;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
		m_dragon = GetComponent<DragonPlayer>();	
		m_controls = GetComponent<DragonControl>();
		m_healthBehaviour = GetComponent<DragonHealthBehaviour>();

		m_active = false;
	}

	void OnDisable() {
		StopBoost();
	}
	
	// Update is called once per frame
	void Update () {
		bool activate = Input.GetKey(KeyCode.X) || m_controls.action;

		if (activate && m_dragon.energy > GameSettings.energyRequiredToBoost) {
			if (!m_active) {
				StartBoost();
			}
		} else if (m_active && (!activate || m_dragon.energy <= 0f)) {
			StopBoost();
		}

<<<<<<< HEAD
		if (activate || m_active) {
			m_dragon.AddEnergy(-Time.deltaTime * m_dragon.data.energyDrainPerSecond);
=======
		if (m_active) {
			m_dragon.AddEnergy(-Time.deltaTime * m_dragon.energyDrainPerSecond);
>>>>>>> Bird_IA
		} else {
			m_dragon.AddEnergy(Time.deltaTime * m_dragon.data.energyRefillPerSecond);
		}
	}


	private void StartBoost() {
		m_active = true;
<<<<<<< HEAD
		m_healthBehaviour.enabled = false;
		m_dragon.SetSpeedMultiplier(m_dragon.data.boost.value);
=======
		if (m_healthBehaviour) m_healthBehaviour.enabled = false;
		m_dragon.SetSpeedMultiplier(m_dragon.boostMultiplier);
>>>>>>> Bird_IA
	}

	private void StopBoost() {
		m_active = false;
		if (m_healthBehaviour) m_healthBehaviour.enabled = true;
		m_dragon.SetSpeedMultiplier(1f);
	}
}
