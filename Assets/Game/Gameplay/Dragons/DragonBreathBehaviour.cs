using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer m_dragon;
	private DragonHealthBehaviour m_healthBehaviour;
	private DragonEatBehaviour m_eatBehaviour;
	private Animator m_animator;

	private bool m_isFuryOn;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {

		m_dragon = GetComponent<DragonPlayer>();
		m_healthBehaviour = GetComponent<DragonHealthBehaviour>();
		m_eatBehaviour = GetComponent<DragonEatBehaviour>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_isFuryOn = false;

		ExtendedStart();
	}
	
	void OnDisable() {

		if (m_isFuryOn) {
			m_isFuryOn = false;
			m_animator.SetBool("fire", false);// Stop fury rush (if active)
			if (m_healthBehaviour) m_healthBehaviour.enabled = true;
			if (m_eatBehaviour) m_eatBehaviour.enabled = true;
			Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn;
	}

	void Update() {

		if (m_isFuryOn) {

			float dt = Time.deltaTime / m_dragon.data.furyDuration;
			m_dragon.AddFury(-(dt * m_dragon.data.maxFury));

			if (m_dragon.fury <= 0) {

				m_isFuryOn = false;
				m_dragon.StopFury();
				m_animator.SetBool("fire", false);
				if (m_healthBehaviour) m_healthBehaviour.enabled = true;
				if (m_eatBehaviour) m_eatBehaviour.enabled = true;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
			} else {
				
				Fire(15f);
				m_animator.SetBool("fire", true);
			}
		} else {

			if (m_dragon.fury >= m_dragon.data.maxFury) {

				m_isFuryOn = true;				
				m_dragon.StartFury();
				if (m_healthBehaviour) m_healthBehaviour.enabled = false;
				if (m_eatBehaviour) m_eatBehaviour.enabled = false;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
			}
		}

		ExtendedUpdate();
	}


	virtual protected void ExtendedStart() {}
	virtual protected void ExtendedUpdate() {}
	virtual protected void Fire(float _magnitude) {}
}
