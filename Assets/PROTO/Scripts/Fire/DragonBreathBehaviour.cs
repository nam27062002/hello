using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private float m_furyDuration = 15f; //seconds
	public float furyDuration { get { return m_furyDuration; } }


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
			Messenger.Broadcast<bool>(GameEvents_OLD.FURY_RUSH_TOGGLED, false);
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn;
	}

	void Update() {

		if (m_isFuryOn) {

			float dt = Time.deltaTime / m_furyDuration;
			m_dragon.AddFury(-(dt * m_dragon.maxFury));

			if (m_dragon.fury <= 0) {

				m_isFuryOn = false;
				m_animator.SetBool("fire", false);
				Messenger.Broadcast<bool>(GameEvents_OLD.FURY_RUSH_TOGGLED, false);
			} else {
				
				Fire(Vector3.right);
				m_animator.SetBool("fire", true);
			}
		} else {

			if (m_dragon.fury >= m_dragon.maxFury) {

				m_isFuryOn = true;
				Messenger.Broadcast<bool>(GameEvents_OLD.FURY_RUSH_TOGGLED, true);
			}
		}
	}


	virtual protected void ExtendedStart() {}
	virtual public void Fire(Vector3 direction) {}
}
