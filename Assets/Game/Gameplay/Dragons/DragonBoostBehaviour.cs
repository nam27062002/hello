using UnityEngine;
using System.Collections;

public class DragonBoostBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonStats 	m_dragon;
	private DragonControl 	m_controls;
	private DragonHealthBehaviour m_healthBehaviour;

	private bool m_active;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
		m_dragon = GetComponent<DragonStats>();	
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

		if (activate && m_dragon.energy > m_dragon.energyMinRequired) {
			if (!m_active) {
				StartBoost();
			}
		} else if (m_active && (!activate || m_dragon.energy <= 0f)) {
			StopBoost();
		}

		if (m_active) {
			m_dragon.AddEnergy(-Time.deltaTime * m_dragon.energyDrainPerSecond);
		} else {
			m_dragon.AddEnergy(Time.deltaTime * m_dragon.energyRefillPerSecond);
		}
	}


	private void StartBoost() {
		m_active = true;
		if (m_healthBehaviour) m_healthBehaviour.enabled = false;
		m_dragon.SetSpeedMultiplier(m_dragon.boostMultiplier);
	}

	private void StopBoost() {
		m_active = false;
		if (m_healthBehaviour) m_healthBehaviour.enabled = true;
		m_dragon.SetSpeedMultiplier(1f);
	}
}
