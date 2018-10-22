using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Xft;

public class DragonBoostBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer 	m_dragon;
	private DragonMotion 	m_motion;
	private DragonControlPlayer 	m_controls;
	private Animator 		m_animator;

	private bool m_active;
	private bool m_ready;

	private bool m_insideWater = false;

	// Cache content data
	private float m_energyDrain = 0f;	// energy per second

	// Refill
	private float m_energyRefill = 1f;	// energy per second
	private float m_energyRefillBase = 1f;	// energy per second
	private float m_energyRefillBonus = 0;	// energy per second
	private float m_energyRequiredToBoost = 0.2f;	// Percent of total energy
	public float energyRequiredToBoost
	{
		get { return m_energyRequiredToBoost; }
		set { m_energyRequiredToBoost = value; }
	}
    private float m_energyRestartThreshold = 1.0f;   // Percent of total energy required to restart
    
	private float m_boostMultiplier;
	public float boostMultiplier
	{
		get { return m_boostMultiplier; }
	}

	protected bool m_superSizeInfiniteBoost = false;
	public bool superSizeInfiniteBoost
	{
		get { return m_superSizeInfiniteBoost; }
		set { m_superSizeInfiniteBoost = value; }
	}

	protected bool m_petInfiniteBoost = false;
	public bool petInfiniteBoost
	{
		get { return m_petInfiniteBoost; }
		set { m_petInfiniteBoost = value; }
	}

	protected bool m_modInfiniteBoost = false;
	public bool modInfiniteBoost
	{
		get { return m_modInfiniteBoost; }
		set { m_modInfiniteBoost = value; }
	}
	
	protected bool m_alwaysDrain = false;
	public bool alwaysDrain
	{
		get { return m_alwaysDrain; }
		set { m_alwaysDrain = value; }
	}

	// Control Panel settings
	private bool m_CPAutoRestart = true;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
		m_dragon = GetComponent<DragonPlayer>();
		m_motion = GetComponent<DragonMotion>();
		m_controls = GetComponent<DragonControlPlayer>();
		m_animator = transform.Find("view").GetComponent<Animator>();

		m_active = false;
		m_ready = true;

		// Cache content data
		m_energyDrain = m_dragon.data.energyDrain;
		m_energyRefillBase = m_dragon.data.energyRefillRate;
		SetRefillBonus( m_energyRefillBonus );
        m_boostMultiplier = m_dragon.data.boostMultiplier;
        // m_energyRequiredToBoost = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings").GetAsFloat("energyRequiredToBoost");
        m_energyRequiredToBoost = m_dragon.data.energyRequiredToBoost;// DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings").GetAsFloat("energyRequiredToBoost");
		m_energyRequiredToBoost *= m_dragon.energyMax;

        m_energyRestartThreshold = m_dragon.data.energyRestartThreshold;
        m_energyRestartThreshold *= m_dragon.energyMax;
	}

	void Start() {
		// Init debug settings
		m_CPAutoRestart = Prefs.GetBoolPlayer(DebugSettings.BOOST_AUTO_RESTART, m_CPAutoRestart);

		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.CP_PREF_CHANGED, OnPrefChanged);
	}

	void OnEnable() {
		m_active = false;
		m_ready = true;
	}

	void OnDisable() {
		StopBoost();
	}

	void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.CP_PREF_CHANGED, OnPrefChanged);
	}

	// Update is called once per frame
	void Update () {
		bool activate = m_controls.action || (m_dragon.changingArea && m_motion.m_useBoostOnIntro);

#if UNITY_EDITOR
        activate = activate || Input.GetKey(KeyCode.X);
#endif

        //if (m_insideWater)
        //	activate = false;

        if (activate) {
			if (m_ready) {
				if (m_dragon.energy >= m_energyRequiredToBoost || m_dragon.changingArea) {
					m_ready = false;
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
			// Don't drain energy if cheat is enabled or dragon fury is on
			if(IsDraining()) {
				//DebugUtils.Log("<color=orange>draining " + m_dragon.energy + "</color>");
                if (m_insideWater)
                    m_dragon.AddEnergy(-Time.deltaTime * m_energyDrain * 5);
                else
                    m_dragon.AddEnergy(-Time.deltaTime * m_energyDrain);

				if (m_dragon.energy <= 0f) {
					StopBoost();
				}
			}
		} else if(m_dragon.energy < m_dragon.energyMax) {
			//DebugUtils.Log("<color=yellow>refilling " + m_dragon.energy + "</color>");
			m_dragon.AddEnergy(Time.deltaTime * m_energyRefill);
		}
        
        if(m_CPAutoRestart)
        {
            // Energy required to restart
            if (!m_active && m_dragon.energy >= m_energyRestartThreshold)
            {
                 m_ready = true;
            }
        }
            
	}

	private void StartBoost()
	{
		//DebugUtils.Log("<color=green>START " + m_dragon.energy + "/" + m_energyRequiredToBoost + "</color>");
		m_active = true;
		m_motion.boostSpeedMultiplier = m_boostMultiplier;
		// ActivateTrails();
		if (m_animator && m_animator.isInitialized)
		{
			m_animator.SetBool( GameConstants.Animator.BOOST , true);
		}
		Messenger.Broadcast<bool>(MessengerEvents.BOOST_TOGGLED, true);
	}

	public void StopBoost()
	{
		//DebugUtils.Log("<color=red>STOP</color>");
		m_active = false;
		m_motion.boostSpeedMultiplier = 1;
		// DeactivateTrails();
		if (m_animator && m_animator.isInitialized && !m_insideWater)
		{
			m_animator.SetBool( GameConstants.Animator.BOOST, false);
		}

		Messenger.Broadcast<bool>(MessengerEvents.BOOST_TOGGLED, false);
	}

	public bool IsDraining() {
		if (m_alwaysDrain)
			return true;
		// Don't drain energy if cheat is enabled or dragon fury is on, or super size, or pet infinite boost
		return !(DebugSettings.infiniteBoost || m_dragon.IsFuryOn() || m_superSizeInfiniteBoost || m_petInfiniteBoost || m_modInfiniteBoost || m_dragon.changingArea);
	}

	public bool IsBoostActive()
	{
		return m_active;
	}

	public void ResumeBoost() {
		m_ready = true;
	}

	public void AddRefillBonus( float value )
	{
		m_energyRefillBonus += value;
		SetRefillBonus( m_energyRefillBonus );
	}

	public void SetRefillBonus( float percentage )
	{
		m_energyRefillBonus = percentage;
		m_energyRefill = m_energyRefillBase + ( m_energyRefillBonus / 100.0f * m_energyRefillBase);
	}

	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") && !m_insideWater)
		{
			m_insideWater = true;
		}

	}

	void OnTriggerExit( Collider _other )
	{
		if ( _other.CompareTag("Water") && m_insideWater)
		{
			m_insideWater = false;
		}
	}

	/// <summary>
	/// A CP pref has been changed.
	/// </summary>
	/// <param name="_prefId">Preference identifier.</param>
	protected void OnPrefChanged(string _prefId) {
		// We only care about some prefs
		if(_prefId == DebugSettings.BOOST_AUTO_RESTART) {
			m_CPAutoRestart = Prefs.GetBoolPlayer(DebugSettings.BOOST_AUTO_RESTART, m_CPAutoRestart);
		}
	}
}
