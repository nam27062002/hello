using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField]private float m_damage = 25f;
	public float damage { 
		get 
		{ 
			if ( m_isSuperFuryOn )
				return m_damage * 2;
			else
				return m_damage; 
		} 
	}
	
	protected Rect m_bounds2D;
	public Rect bounds2D { get { return m_bounds2D; } }

	protected Vector2 m_direction;
	public Vector2 direction { get { return m_direction; } }

	private DragonPlayer m_dragon;
	private DragonEatBehaviour 		m_eatBehaviour;
	private DragonHealthBehaviour 	m_healthBehaviour;
	private DragonAttackBehaviour 	m_attackBehaviour;
	private Animator m_animator;

	// Cache content values
	protected float m_furyMax = 1f;
	protected float m_furyDuration = 1f;

	protected bool m_isFuryOn;
	protected bool m_isSuperFuryOn;
	protected float m_actualLength;	// Set breath length. Used by the camera
	public float actualLength
	{
		get
		{
			return m_actualLength;
		}
	}

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
		m_isSuperFuryOn = false;
		m_bounds2D = new Rect();

		// Init content cache
		m_furyMax = m_dragon.data.def.GetAsFloat("furyMax");
		m_furyDuration = m_dragon.data.def.GetAsFloat("furyDuration");

		ExtendedStart();

		Messenger.AddListener<Transform,Reward>(GameEvents.ENTITY_BURNED, OnEntityBurned);
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_BURNED, OnEntityBurned);
	}
	
	void OnDisable() {

		if (m_isFuryOn || m_isSuperFuryOn) {
			m_isFuryOn = false;
			m_animator.SetBool("breath", false);// Stop fury rush (if active)
			if (m_healthBehaviour) m_healthBehaviour.enabled = true;
			if (m_eatBehaviour) m_eatBehaviour.enabled = true;
			if (m_attackBehaviour) m_attackBehaviour.enabled = true;
			Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn || m_isSuperFuryOn;
	}

	void Update() {
		// Cheat for infinite fire
		bool cheating = (Debug.isDebugBuild && (DebugSettings.infiniteFire || DebugSettings.infiniteSuperFire));
		if(cheating) {
			if ( DebugSettings.infiniteFire )
				m_dragon.AddFury(m_furyMax - m_dragon.fury);	// Set to max fury
			else if ( DebugSettings.infiniteSuperFire )
				m_dragon.AddSuperFury(m_furyMax - m_dragon.superFury);
		}

		if (m_isFuryOn || m_isSuperFuryOn) 
		{

			// Don't decrease fury if cheating
			if(!cheating) {
				float dt = Time.deltaTime / m_furyDuration;
				if ( m_isFuryOn )
					m_dragon.AddFury(-(dt * m_furyMax));
				else
					m_dragon.AddSuperFury(-(dt * m_furyMax));
			}

			if ((m_isFuryOn && m_dragon.fury <= 0) || (m_isSuperFuryOn && m_dragon.superFury <= 0)) 
			{
				EndBreath();
				if ( m_isFuryOn )
				{
					m_dragon.StopFury();
					m_dragon.AddSuperFury(m_furyMax * 0.2f);
				}
				else
				{
					m_dragon.StopSuperFury();
				}
				m_isFuryOn = false;
				m_isSuperFuryOn = false;
				m_animator.SetBool("breath", false);
				if (m_healthBehaviour) m_healthBehaviour.enabled = true;
				if (m_eatBehaviour) m_eatBehaviour.enabled = true;
				if (m_attackBehaviour) m_attackBehaviour.enabled = true;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, false);
			} else {				
				Breath();
				m_animator.SetBool("breath", true);
			}
		} else {

			if ( m_dragon.superFury >= m_furyMax )
			{
				m_isSuperFuryOn = true;
				BeginBreath();
				m_dragon.StartSuperFury();
				if (m_healthBehaviour) m_healthBehaviour.enabled = false;
				if (m_eatBehaviour) m_eatBehaviour.enabled = false;
				if (m_attackBehaviour) m_attackBehaviour.enabled = false;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
			}
			else if (m_dragon.fury >= m_furyMax) 
			{
				m_isFuryOn = true;
				BeginBreath();
				m_dragon.StartFury();
				if (m_healthBehaviour) m_healthBehaviour.enabled = false;
				if (m_eatBehaviour) m_eatBehaviour.enabled = false;
				if (m_attackBehaviour) m_attackBehaviour.enabled = false;
				Messenger.Broadcast<bool>(GameEvents.FURY_RUSH_TOGGLED, true);
			}
		}

		ExtendedUpdate();
	}


	protected virtual void OnEntityBurned(Transform t, Reward reward)
	{	
		m_dragon.AddLife( reward.health );
	}

	virtual public bool IsInsideArea(Vector2 _point) { return false; }
	virtual public bool Overlaps( CircleArea2D _circle) { return false; }
	virtual protected void ExtendedStart() {}
	virtual protected void ExtendedUpdate() {}

	virtual protected void BeginBreath() 
	{
		m_eatBehaviour.enabled = false;
	}
	virtual protected void Breath() {}
	virtual protected void EndBreath() 
	{
		m_eatBehaviour.enabled = true;
	}
}
