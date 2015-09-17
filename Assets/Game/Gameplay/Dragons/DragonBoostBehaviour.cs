using UnityEngine;
using System.Collections;

public class DragonBoostBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer 	m_dragon;
	private DragonControl 	m_controls;

	private bool m_active;
	private bool m_ready;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
		m_dragon = GetComponent<DragonPlayer>();	
		m_controls = GetComponent<DragonControl>();

		m_active = false;
		m_ready = true;
	}

	void OnDisable() {
		StopBoost();
	}
	
	// Update is called once per frame
	void Update () {
		bool activate = Input.GetKey(KeyCode.X) || m_controls.action;

		if (activate) {
			if (m_ready) {
				m_ready = false;
				if (m_dragon.energy > GameSettings.energyRequiredToBoost) {
					StartBoost();
				}
			}
		} else {
			m_ready = true;
			if (m_active) {
				StopBoost();
			}
		}

		if (m_active) {
			m_dragon.AddEnergy(-Time.deltaTime * m_dragon.data.energyDrainPerSecond);
			if (m_dragon.energy <= 0f) {
				StopBoost();
			}
		} else {
			m_dragon.AddEnergy(Time.deltaTime * m_dragon.data.energyRefillPerSecond);
		}
	}


	private void StartBoost() {
		m_active = true;
		m_dragon.SetSpeedMultiplier(m_dragon.data.boost.value);
	}

	private void StopBoost() {
		m_active = false;
		m_dragon.SetSpeedMultiplier(1f);
	}
}
