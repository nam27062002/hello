using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonBoostBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer 	m_dragon;
	private DragonMotion 	m_motion;
	private DragonControl 	m_controls;
	private Animator 		m_animator;

	private bool m_active;
	private bool m_ready;

	public List<GameObject> m_trails;
	private bool m_trailsActive = false;
	private bool m_insideWater = false;

	// Cache content data
	private float m_energyDrain = 0f;	// energy per second
	private float m_energyRefill = 1f;	// energy per second
	private float m_energyRequiredToBoost = 0.2f;	// Percent of total energy
	private float m_boostMultiplier;
	public float boostMultiplier
	{
		get { return m_boostMultiplier; }
	}


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
		m_dragon = GetComponent<DragonPlayer>();	
		m_motion = GetComponent<DragonMotion>();
		m_controls = GetComponent<DragonControl>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_active = false;
		m_ready = true;

		// Cache content data
		m_energyDrain = m_dragon.data.def.GetAsFloat("energyDrain");
		m_energyRefill = m_dragon.data.def.GetAsFloat("energyRefillRate");
		m_boostMultiplier = m_dragon.data.def.GetAsFloat("boostMultiplier");
		m_energyRequiredToBoost = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings").GetAsFloat("energyRequiredToBoost");
		m_energyRequiredToBoost *= m_dragon.data.def.GetAsFloat("energyMax");

		for( int i = 0; i<m_trails.Count; i++ )
		{
			TrailRenderer tr = m_trails[i].GetComponent<TrailRenderer>();
			tr.sortingLayerName = "player";
		}

		DeactivateTrails();
	}

	void OnEnable() {
		
		m_active = false;
		m_ready = true;
	}

	void OnDisable() {
		StopBoost();
	}
	
	// Update is called once per frame
	void Update () {
		bool activate = Input.GetKey(KeyCode.X) || m_controls.action;

		if (m_insideWater)
			activate = false;

		if (activate) {
			if (m_ready) {
				m_ready = false;
				if (m_dragon.energy > m_energyRequiredToBoost) {
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
			// Don't drain energy if cheat is enabled
			if(!UnityEngine.Debug.isDebugBuild || !DebugSettings.infiniteBoost) {
				m_dragon.AddEnergy(-Time.deltaTime * m_energyDrain);
				if (m_dragon.energy <= 0f) {
					StopBoost();
				}
			}
		} else {
			m_dragon.AddEnergy(Time.deltaTime * m_energyRefill);
		}
	}


	private void StartBoost() 
	{
		m_active = true;
		m_motion.targetSpeedMultiplier = m_boostMultiplier;
		// ActivateTrails();
		if (m_animator && m_animator.isInitialized)
		{
			m_animator.SetBool("boost", true);
			// speed up eating
			m_animator.SetFloat("eatingSpeed", m_boostMultiplier);
		}
		Messenger.Broadcast<bool>(GameEvents.BOOST_TOGGLED, true);
	}

	public void StopBoost() 
	{
		m_active = false;
		m_motion.targetSpeedMultiplier = 1;
		// DeactivateTrails();
		if (m_animator && m_animator.isInitialized && !m_insideWater)
		{
			m_animator.SetBool("boost", false);
			// set eating speed as normal
			m_animator.SetFloat("eatingSpeed", 1);
		}

		Messenger.Broadcast<bool>(GameEvents.BOOST_TOGGLED, false);
	}

	public bool IsBoostActive()
	{
		return m_active;
	}

	public void ResumeBoost() {
		m_ready = true;
	}

	public void ActivateTrails()
	{
		m_trailsActive = true;
		if (!m_insideWater)
		for( int i = 0; i<m_trails.Count; i++ )
		{
			m_trails[i].SetActive(true);
		}
	}

	public void DeactivateTrails()
	{
		m_trailsActive = false;
		for( int i = 0; i<m_trails.Count; i++ )
		{
			m_trails[i].SetActive(false);
		}
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.tag == "Water" )
		{
			m_insideWater = true;
			// if trails active then activate bubles
			if ( m_trailsActive )
			{
				DeactivateTrails();
				m_trailsActive = true;
			}
		}

	}

	void OnTriggerExit( Collider _other )
	{
		if ( _other.tag == "Water" )
		{
			m_insideWater = false;
			// if trails active
			if ( m_trailsActive )
			{
				ActivateTrails();
			}
		}

	}
}
