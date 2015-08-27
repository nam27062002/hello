using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonStats m_dragon;
	private Animator m_animator;

	private bool m_isFuryOn;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {

		m_dragon = GetComponent<DragonStats>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_isFuryOn = false;

		ExtendedStart();
	}
	
	void OnDisable() {

		if (m_isFuryOn) {
			m_isFuryOn = false;
			m_animator.SetBool("fire", false);// Stop fury rush (if active)
			Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn;
	}

	void Update() {

		if (m_isFuryOn) {

			float dt = Time.deltaTime / m_dragon.furyDuration;
			m_dragon.AddFury(-(dt * m_dragon.maxFury));

			if (m_dragon.fury <= 0) {

				m_isFuryOn = false;
				m_dragon.FinishFury();
				m_animator.SetBool("fire", false);
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
			} else {
				
				Fire(500f);
				m_animator.SetBool("fire", true);
			}
		} else {

			if (m_dragon.fury >= m_dragon.maxFury) {

				m_isFuryOn = true;				
				m_dragon.ActivateFury();
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
			}
		}

		ExtendedUpdate();
	}


	virtual protected void ExtendedStart() {}
	virtual protected void ExtendedUpdate() {}
	virtual protected void Fire(float _magnitude) {}
}
