using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField]private float m_damage = 25f;
	public float damage { get { return m_damage; } }
	
	protected Rect m_bounds2D;
	public Rect bounds2D { get { return m_bounds2D; } }

	private DragonPlayer m_dragon;
	private DragonEatBehaviour 		m_eatBehaviour;
	private DragonHealthBehaviour 	m_healthBehaviour;
	private DragonAttackBehaviour 	m_attackBehaviour;
	private Animator m_animator;

	protected bool m_isFuryOn;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {

		m_dragon = GetComponent<DragonPlayer>();
		m_eatBehaviour = GetComponent<DragonEatBehaviour>();
		m_healthBehaviour = GetComponent<DragonHealthBehaviour>();
		m_attackBehaviour = GetComponent<DragonAttackBehaviour>();		
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_isFuryOn = false;

		m_bounds2D = new Rect();

		ExtendedStart();
	}
	
	void OnDisable() {

		if (m_isFuryOn) {
			m_isFuryOn = false;
			m_animator.SetBool("breath", false);// Stop fury rush (if active)
			if (m_healthBehaviour) m_healthBehaviour.enabled = true;
			if (m_eatBehaviour) m_eatBehaviour.enabled = true;
			if (m_attackBehaviour) m_attackBehaviour.enabled = true;
			Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn;
	}

	void Update() {
		// Cheat for infinite fire
		bool cheating = (Debug.isDebugBuild && DebugSettings.infiniteFire);
		if(cheating) {
			m_dragon.AddFury(m_dragon.data.maxFury - m_dragon.fury);	// Set to max fury
		}

		if (m_isFuryOn) {

			// Don't decrease fury if cheating
			if(!cheating) {
				float dt = Time.deltaTime / m_dragon.data.furyDuration;
				m_dragon.AddFury(-(dt * m_dragon.data.maxFury));
			}

			if (m_dragon.fury <= 0) {

				m_isFuryOn = false;
				m_dragon.StopFury();
				m_animator.SetBool("breath", false);
				if (m_healthBehaviour) m_healthBehaviour.enabled = true;
				if (m_eatBehaviour) m_eatBehaviour.enabled = true;
				if (m_attackBehaviour) m_attackBehaviour.enabled = true;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
			} else {
				
				Fire();
				m_animator.SetBool("breath", true);
			}
		} else {

			if (m_dragon.fury >= m_dragon.data.maxFury) {

				m_isFuryOn = true;				
				m_dragon.StartFury();
				if (m_healthBehaviour) m_healthBehaviour.enabled = false;
				if (m_eatBehaviour) m_eatBehaviour.enabled = false;
				if (m_attackBehaviour) m_attackBehaviour.enabled = false;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
			}
		}

		ExtendedUpdate();
	}

	virtual public bool IsInsideArea(Vector2 _point) { return false; }
	virtual public bool Overlaps( CircleArea2D _circle) { return false; }
	virtual protected void ExtendedStart() {}
	virtual protected void ExtendedUpdate() {}
	virtual protected void Fire() {}
}
