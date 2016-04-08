using UnityEngine;
using System.Collections.Generic;

public class DragonHealthBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer m_dragon;
	private Animator m_animator;

	private GameSceneControllerBase m_gameController;

	// health drain
	private float m_healthDrainPerSecond;
	private float m_healthDrainAmpPerSecond;

	// Damage Multiplier for buffs
	private float m_damageMultiplier;

	// Curse
	private float m_curseTimer;
	private float m_curseDPS;

	// On session start modifiers
	private float m_sessionStartHealthDrainTime;
	private float m_sessionStartHealthDrainModifier;

	// Critical health modifiers
	private float m_healthCriticalLimit;
	private float m_criticalHealthModifier;
	private float m_starvingLimit;
	private float m_starvingHealthModifier;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------

	// Use this for initialization
	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_gameController = InstanceManager.GetSceneController<GameSceneControllerBase>();

		// Shark related values
		m_healthDrainPerSecond = m_dragon.data.def.GetAsFloat("healthDrain");
		m_healthDrainAmpPerSecond = m_dragon.data.def.GetAsFloat("healthDrainAmpPerSecond"); // 0.005
		m_sessionStartHealthDrainTime = m_dragon.data.def.GetAsFloat("sessionStartHealthDrainTime"); // 45
		m_sessionStartHealthDrainModifier = m_dragon.data.def.GetAsFloat("sessionStartHealthDrainModifier");// 0.5

		// Global setting values
		DefinitionNode settings = DefinitionsManager.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
		m_healthCriticalLimit = settings.GetAsFloat("healthCriticalThreshold");	// 0.08
		m_criticalHealthModifier = settings.GetAsFloat("healthCriticalModifier");	// 0.2
		m_starvingLimit = settings.GetAsFloat("healthWarningThreshold");	// 0.20
		m_starvingHealthModifier = settings.GetAsFloat("healthWarningModifier");	// 0.5

		// Curse initialization
		m_curseTimer = 0;
		m_curseDPS = 0;
	}
		
	// Update is called once per frame
	void Update() 
	{
		
		float drain = GetModifiedDamageForCurrentHealth( m_healthDrainPerSecond, true);
		m_dragon.AddLife(-drain * Time.deltaTime);

		if ( m_curseTimer > 0 )
		{
			m_curseTimer -= Time.deltaTime;
			float curse =  GetModifiedDamageForCurrentHealth( m_curseDPS );
			m_dragon.AddLife( -curse * Time.deltaTime );
		}
	}

	public bool IsAlive() {
		return m_dragon.IsAlive();
	}

	public bool IsCursed()
	{
		return m_curseTimer > 0;
	}

	public void ReceiveDamage(float _value, Transform _source = null, bool hitAnimation = true) 
	{
		if (enabled) 
		{
			if ( hitAnimation )
				m_animator.SetTrigger("damage");// receive damage?
			float damage = GetModifiedDamageForCurrentHealth( _value );
			m_dragon.AddLife(-damage);
			Messenger.Broadcast<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, _value, _source);
		}
	}

	public void Curse( float _damage, float _duration )
	{
		m_curseTimer = _duration;
		m_curseDPS = _damage;
		m_animator.SetTrigger("damage");// receive damage?
		Messenger.Broadcast(GameEvents.PLAYER_CURSED);
	}


	// before inflicting damage, push damage value through this method to get a possibly modified value
	// that is reduced when health is low
	protected virtual float GetModifiedDamageForCurrentHealth(float damage, bool includeHealthDrainAmp = false)
	{
        // Reduced health drain at session start
        if (m_sessionStartHealthDrainTime > 0.0f)
        {
            m_sessionStartHealthDrainTime -= Time.deltaTime;
            damage *= m_sessionStartHealthDrainModifier;
            //Debug.Log("Reducing health drain! (" + (int)m_sessionStartHealthDrainTime + ")");
        }

		// apply the buffs multiplier.
		damage += damage * m_damageMultiplier;

		//Health Drain Amplitude over time
		if (includeHealthDrainAmp)
		{
			damage = damage + (damage * (m_gameController.elapsedSeconds * m_healthDrainAmpPerSecond));
		}

		float healthFraction = m_dragon.healthFraction;

		if(healthFraction < m_healthCriticalLimit)
			return damage * m_criticalHealthModifier;

		if(healthFraction < m_starvingLimit)
			return damage * m_starvingHealthModifier;

		return damage;
	}


}
