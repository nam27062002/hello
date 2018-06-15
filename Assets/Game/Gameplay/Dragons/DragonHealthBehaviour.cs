using UnityEngine;
using System.Collections.Generic;

public class DragonHealthBehaviour : MonoBehaviour {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	// DOT == Damage Over Time
	private class DOT {
		public float dps = 0f;
		public DamageType type = DamageType.NONE;
		public float timer = 0f;
        public Transform source = null;
	}

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer m_dragon;
	private DragonAnimationEvents m_animator;

	private GameSceneControllerBase m_gameController;

	// health drain
	//TONI START
	private float m_revivesCounter;
	private float m_reviveBonusDurationMax;
	private float m_reviveBonusValueIni = 0.05f;
	private float m_reviveBonusValueFin;
	private float m_reviveBonusTime;
	public float reviveBonusTime
	{
		get{ return m_reviveBonusTime; }
	}
	public float damageHUD;
	//TONI END
	private float m_healthDrainPerSecond;
	private float m_healthDrainAmpPerSecond;
    // health drain in space
    private float m_healthDrainPerSecondInSpace;

    // Damage Multiplier for buffs
    private float m_damageMultiplier;

	// Damage over time
	private List<DOT> m_dots = new List<DOT>();

	// On session start modifiers
	private float m_sessionStartHealthDrainTime;
	private float m_sessionStartHealthDrainModifier;



	// Power ups modifiers
	private float m_drainReduceModifier = 0;
	private Dictionary<DamageType, float> m_damageReductions;
	private Dictionary<string, float> m_damageOriginReductions;

	private Dictionary<string, float> m_eatingHpBoosts = new Dictionary<string, float>();
	private float m_globalEatingHpBoost = 0;

	public float m_damageAnimationThreshold = 0;	// to check if hit animation can play when Recieve Damage
	public float m_dotAnimationThreshold = 0;	// To check if hit animation can play when ReceiveDamageOverTime

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Awake()
	{
		DamageTypeComparer comparer = new DamageTypeComparer();
		m_damageReductions = new Dictionary<DamageType, float>(comparer);

		m_damageOriginReductions = new Dictionary<string, float>();

		m_dragon = GetComponent<DragonPlayer>();
	}

	// Use this for initialization
	void Start() {
		
		m_animator = transform.Find("view").GetComponent<DragonAnimationEvents>();

		m_gameController = InstanceManager.gameSceneControllerBase;

		// Shark related values
		m_healthDrainPerSecond = m_dragon.data.def.GetAsFloat("healthDrain");
		m_healthDrainAmpPerSecond = m_dragon.data.def.GetAsFloat("healthDrainAmpPerSecond"); // 0.005
		m_sessionStartHealthDrainTime = m_dragon.data.def.GetAsFloat("sessionStartHealthDrainTime"); // 45
		m_sessionStartHealthDrainModifier = m_dragon.data.def.GetAsFloat("sessionStartHealthDrainModifier");// 0.5
        m_healthDrainPerSecondInSpace = m_dragon.data.def.GetAsFloat("healthDrainSpacePlus");

		m_damageAnimationThreshold = m_dragon.data.def.GetAsFloat("damageAnimationThreshold", 0);
		m_dotAnimationThreshold = m_dragon.data.def.GetAsFloat("dotAnimationThreshold", 0);

        m_damageMultiplier = 0;
		//TONI START
		m_reviveBonusTime = 0;
		m_revivesCounter = 0;
		m_reviveBonusValueFin = 1.1f;
		//TONI END
	}
		
	// Update is called once per frame
	void Update() 
	{
		// Apply health drain
		float drain = GetModifiedDamageForCurrentHealth( m_healthDrainPerSecond, true);

		// Check power ups 
		drain = drain - drain * m_drainReduceModifier / 100.0f;

		m_dragon.AddLife(-drain * Time.deltaTime, DamageType.DRAIN, null);

		// Apply damage over time if not changing area
		if ( !m_dragon.changingArea )
		{
			// Reverse iterating since we will be removing them from the list when expired
			for(int i = m_dots.Count - 1; i >= 0; i--) {
				// Apply damage
				float damage = GetModifiedDamageForCurrentHealth(m_dots[i].dps);
				ReceiveDamage(damage * Time.deltaTime, m_dots[i].type, m_dots[i].source, false);		// No hit animation!

				// Update timer and check for dot finish
				m_dots[i].timer -= Time.deltaTime;
				if(m_dots[i].timer <= 0) {
					m_dots.RemoveAt(i);
				}
			}
		}

		#if DEBUG
			if ( Input.GetKeyDown( KeyCode.M) )
				m_dragon.AddLife( -m_dragon.health, DamageType.NONE, null );
		#endif
	}

	//TONI START
	public void SetReviveBonusTime()
	{
		// Revive bonus last longer as times goes on (but from 2 min, is fixed to 60)
		if (m_gameController.elapsedSeconds > 120)	m_reviveBonusTime = 60;
		else m_reviveBonusTime = m_gameController.elapsedSeconds / 2;
		m_reviveBonusDurationMax = m_reviveBonusTime;
		// For each revive, bonus min. is 10% better to a max. of 60%
		m_revivesCounter++;
		if (m_reviveBonusValueFin > 0.6f)
			m_reviveBonusValueFin -= 0.1f;
	}
	//TONI END

	public void AddDrainReduceModifier( float value )
	{
		m_drainReduceModifier += value;
	}

	public bool IsAlive() {
		return m_dragon.IsAlive();
	}

	public bool HasDOT() {
		return m_dots.Count > 0;
	}

	public bool HasDOT(DamageType _type) {
		int count = m_dots.Count;
		bool ret = false;
		for( int i = 0; i<count && !ret; ++i )
		{
			if ( m_dots[i].type == _type )
			{
				ret = true;
			}
		}
		return ret;
	}

	/// <summary>
	/// Inflict instant damage to the dragon.
	/// </summary>
	/// <param name="_amount">The total amount of damage to be applied. Will be modified based on dragon's current health percentage.</param>
	/// <param name="_type">Type of damage to be applied.</param> 
	/// <param name="_source">The source of the damage, optional.</param> 
	/// <param name="_hitAnimation">Whether to trigger the hit animation or not. If the damage is not bigger than hit animation threshold, animation will no play</param>
	/// <param name="_damageOrigin">Damage origin identifier.Example: entity sku</param>	
	public void ReceiveDamage(float _amount, DamageType _type, Transform _source = null, bool _hitAnimation = true, string _damageOrigin = "", Entity _entity = null) {
		if(enabled) {
			if ( m_dragon.IsInvulnerable() )
				return;
			if ( m_dragon.HasShield( _type ) )
			{
				m_dragon.LoseShield( _type, _source );
				return;
			}

			// power ups
			if ( m_damageReductions.ContainsKey( _type ) )
			{
				_amount = _amount - _amount * m_damageReductions[ _type ] / 100.0f;
			}

			if ( !string.IsNullOrEmpty(_damageOrigin) && m_damageOriginReductions.ContainsKey( _damageOrigin ) )
			{
				_amount = _amount - _amount * m_damageOriginReductions[ _damageOrigin ] / 100.0f;
			}

			// Play animation?
			if(_hitAnimation && _amount >= m_damageAnimationThreshold) {
				PlayHitAnimation(_type);
			}

			// Apply damage
			float damage = GetModifiedDamageForCurrentHealth(_amount);
			m_dragon.AddLife(-damage, _type, _source);

			// Notify game
			Messenger.Broadcast<float, DamageType, Transform>(MessengerEvents.PLAYER_DAMAGE_RECEIVED, _amount, _type, _source);

			if (_entity != null && _entity.hasToShowTierNeeded(m_dragon.data.tier)) {
				DragonTier tier = _entity.edibleFromTier;
				if ( _entity.canBeGrabbed && _entity.grabFromTier < tier )
					tier = _entity.grabFromTier;
				if ( _entity.canBeLatchedOn && _entity.latchFromTier < tier )
					tier = _entity.latchFromTier;
				Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, tier, _entity.sku);
			}
		}
	}

	/// <summary>
	/// Start receiving a DOT.
	/// </summary>
	/// <param name="_dps">Damage per second to be applied.</param>
	/// <param name="_duration">Total duration.</param>
	/// <param name="_type">Type of damage to be applied. If a DOT of a different type is being applied, type will be override.</param> 
	/// <param name="_reset">Whether to override current DOT or accumulate it.</param>
	public void ReceiveDamageOverTime(float _dps, float _duration, DamageType _type, Transform _source = null, bool _reset = true, string _damageOrigin = "", Entity _entity = null) {

		if ( m_dragon.IsInvulnerable() )
			return;
		// Check shields
		if ( m_dragon.HasShieldActive( _type ) )
		{
			return;
		}
		else if ( m_dragon.HasShield( _type ) )
		{
			m_dragon.LoseShield( _type ,_source);
			return;
		}

		// Check damage Reduction
		if ( m_damageReductions.ContainsKey( _type ) )
		{
			_dps = _dps - _dps * m_damageReductions[ _type ] / 100.0f;
		}

		if (  !string.IsNullOrEmpty( _damageOrigin ) && m_damageOriginReductions.ContainsKey( _damageOrigin ) )
		{
			_dps = _dps - _dps * m_damageOriginReductions[ _damageOrigin ] / 100.0f;
		}

		// Clear current dots?
		if(_reset) {
			m_dots.Clear();
		}

		// Store new dot's values, dot will be applied during the update call
		DOT newDot = new DOT();
		newDot.dps = _dps;
		newDot.timer = _duration;
		newDot.type = _type;
        newDot.source = _source;
        m_dots.Add(newDot);

		// Do feedback animation
		if ( _dps >= m_dotAnimationThreshold ){
			PlayHitAnimation( _type );
		}

		if (_entity != null && _entity.hasToShowTierNeeded(m_dragon.data.tier)) {
			DragonTier tier = _entity.edibleFromTier;
			if ( _entity.canBeGrabbed && _entity.grabFromTier < tier )
				tier = _entity.grabFromTier;
			if ( _entity.canBeLatchedOn && _entity.latchFromTier < tier )
				tier = _entity.latchFromTier;
			Messenger.Broadcast<DragonTier, string>(MessengerEvents.BIGGER_DRAGON_NEEDED, tier, _entity.sku);
		}
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
        }

		// apply the buffs multiplier.
		damage += damage * m_damageMultiplier;

		//Health Drain Amplitude over time
		if (includeHealthDrainAmp)
		{
			damage = damage + (damage * (m_gameController.elapsedSeconds * m_healthDrainAmpPerSecond));

            //Add Space Drain 
            if (m_dragon.dragonMotion.IsInSpace())
            {
                damage = damage + m_healthDrainPerSecondInSpace;
            }
        }

		// Apply health modifier
		// float healthFraction = m_dragon.healthFraction;
		if(m_dragon.currentHealthModifier != null) {
			damage *= m_dragon.currentHealthModifier.modifier;
		}
		//TONI START
		if (m_reviveBonusTime > 0.0f) 
		{
			m_reviveBonusTime -= Time.deltaTime;
			damage *= m_reviveBonusValueFin - ((m_reviveBonusTime / m_reviveBonusDurationMax) * (m_reviveBonusValueFin - m_reviveBonusValueIni));
			damageHUD = 100 - (100 * (m_reviveBonusValueFin - ((m_reviveBonusTime / m_reviveBonusDurationMax) * (m_reviveBonusValueFin - m_reviveBonusValueIni)))); //REMOVE THIS, JUST TO CHECK FORMULA WORKS
		}
		//TONI END
		return damage;
	}

	private void PlayHitAnimation( DamageType _type ) {
		if ( m_animator != null )
		{
			m_animator.PlayHitAnimation( _type );
		}
	}

	void OnEnable() {
		// PlayHitAnimation();
	}

	public void AddDamageReduction( DamageType type, float percentage )
	{
		if (m_damageReductions.ContainsKey(type))
		{
			m_damageReductions[type] += percentage;
		}
		else
		{
			m_damageReductions.Add(type, percentage);
		}
	}

	public void AddDamageReduction( string origin, float percentage)
	{
		if (m_damageOriginReductions.ContainsKey(origin))
		{
			m_damageOriginReductions[origin] += percentage;
		}
		else
		{
			m_damageOriginReductions.Add(origin, percentage);
		}
	}

	/*
	*	Adds eating hp boost from entitySky entities
	*/
	public void AddEatingHpBoost( string entitySku, float value )
	{
		if ( m_eatingHpBoosts.ContainsKey(entitySku) )
		{
			m_eatingHpBoosts[entitySku] += value;
		}
		else
		{
			m_eatingHpBoosts.Add( entitySku, value);
		}
	}

	/**
	*	Adds eating boost to all eating
	*/
	public void AddEatingHpBoost( float value )
	{
		m_globalEatingHpBoost += value;
	}
	/**
	*	Boost applied to rewarded hp used when eating or burning entities
	*/
	public float GetBoostedHp( string origin, float reward )
	{
		float rewardHealth = reward + (reward * m_globalEatingHpBoost) / 100.0f;

		// Check if origin is in power up and give proper boost
		if ( !string.IsNullOrEmpty( origin ) && m_eatingHpBoosts.ContainsKey( origin ) )
		{
			rewardHealth += (reward * m_eatingHpBoosts[origin]) / 100.0f;
		}

		return rewardHealth;
	}

}
