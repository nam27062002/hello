using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private float MaxGoldRushCompletitionPercentageForConsecutiveRushes = 0.5f; // max 50%.
	private float AdditionalGoldRushCompletitionPercentageForConsecutiveRushes = 0.05f;


	protected Rect m_bounds2D;
	public Rect bounds2D { get { return m_bounds2D; } }

	protected Vector2 m_direction;
	public Vector2 direction { get { return m_direction; } }

	protected DragonPlayer m_dragon;
	private DragonHealthBehaviour 	m_healthBehaviour;
	private DragonAttackBehaviour 	m_attackBehaviour;
	protected Animator m_animator;

	// Cache content values

	protected float m_furyMax = 1f;
	public float furyMax
	{
		get{ return m_furyMax; }
	}
	protected float m_currentFury = 0;
	public float currentFury
	{
		get{ return m_currentFury; }
	}
	private float m_furyBase = 1f;
	public float furyBase {get{return m_furyBase;}}

	protected float m_furyBaseDuration = 1f;	// Sandard fury Base Duration
	protected float m_furyDurationBonus = 0;	// Power ups multiplier
	protected float m_furyDuration = 1f;	// Sandard fury Duration


	protected float m_currentFuryDuration;		// If fury Active, total time it lasts
	protected float m_currentRemainingFuryDuration;	// If fury Active remaining time 


	public enum Type
    {
        Standard,
        Mega,
        None
    };

	protected bool m_isFuryOn;
	protected bool m_isFuryPaused;
    protected Type m_type = Type.None;
    public Type type
    {
    	get { return m_type; }
    }

	protected float m_actualLength;	// Set breath length. Used by the camera
	public float actualLength
	{
		get
		{
			return m_actualLength;
		}
	}

	protected DragonTier m_tier;

	private int m_furyRushesCompleted;
	private float m_scoreToAddForNextFuryRushes;
	private float m_maxScoreToAddForNextFuryRushes;

	private float m_superFuryMax;
	protected float m_superFuryDurationModifier;
	protected float m_superFuryCoinsMultiplier;
	protected float m_superFuryDamageMultiplier;
	protected float m_superFuryLengthMultiplier;
	protected float m_fireRushMultiplier = 1;

	public string m_breathSound;
    private AudioObject m_breathSoundAO;
    public string m_superBreathSound;
    private AudioObject m_superBreathSoundAO;

	private float m_checkNodeFireTime = 0.25f;
	private float m_fireNodeTimer = 0;

	protected float m_lengthPowerUpMultiplier = 0;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------

	void Awake()
	{
		DefinitionNode settings = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
		m_superFuryMax = settings.GetAsFloat("superfuryMax", 8);
		m_superFuryDurationModifier = settings.GetAsFloat("superFuryDurationModifier", 1.2f);
		m_superFuryCoinsMultiplier = settings.GetAsFloat("superFuryCoinsMultiplier", 1.2f);
		m_superFuryDamageMultiplier = settings.GetAsFloat("superFuryDamageMultiplier", 1.2f);
		m_superFuryLengthMultiplier = settings.GetAsFloat("superFuryLengthMultiplier", 1.2f);

		DefinitionNode gameSettings = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		MaxGoldRushCompletitionPercentageForConsecutiveRushes = gameSettings.GetAsFloat("MaxGoldRushCompletitionPercentageForConsecutiveRushes");
		AdditionalGoldRushCompletitionPercentageForConsecutiveRushes = gameSettings.GetAsFloat("AdditionalGoldRushCompletitionPercentageForConsecutiveRushes");

		m_dragon = GetComponent<DragonPlayer>();
	}

	void Start() 
	{
		m_healthBehaviour = GetComponent<DragonHealthBehaviour>();
		m_attackBehaviour = GetComponent<DragonAttackBehaviour>();		
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_isFuryOn = false;
		m_bounds2D = new Rect();

		m_tier = m_dragon.data.tier;

		// Init content cache
		m_furyBase = m_dragon.data.def.GetAsFloat("furyMax");
		m_furyMax = m_furyBase;
		m_currentFury = 0;

		if (!UsersManager.currentUser.furyUsed) {
			m_currentFury = m_furyMax * 0.5f;
		}

		// Get the level
		AddDurationBonus(0);
		m_fireRushMultiplier = m_dragon.data.def.GetAsFloat("furyScoreMultiplier", 2);

		m_furyRushesCompleted = 0;
		m_scoreToAddForNextFuryRushes = (int)(AdditionalGoldRushCompletitionPercentageForConsecutiveRushes * (float)m_furyMax);
		m_maxScoreToAddForNextFuryRushes = (int)(MaxGoldRushCompletitionPercentageForConsecutiveRushes * (float)m_furyMax);

		ExtendedStart();

		Messenger.AddListener<Transform,Reward>(GameEvents.ENTITY_BURNED, OnEntityBurned);
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() 
	{
	}

	public void SetFuryMax( float max )
	{
		m_furyMax = max;
	}

	public void SetDurationBonus( float furyBonus )
	{
		m_furyBaseDuration = m_dragon.data.def.GetAsFloat("furyBaseDuration");
		m_furyDurationBonus = furyBonus;
		m_furyDuration = m_furyBaseDuration + (m_furyDurationBonus / 100.0f * m_furyBaseDuration);
	}

	public void AddDurationBonus( float furyBonus )
	{
		m_furyDurationBonus += furyBonus;
		SetDurationBonus( m_furyDurationBonus );
	}

	void OnDestroy()
	{
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_BURNED, OnEntityBurned);
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}
	
	void OnDisable() {
		if ( ApplicationManager.IsAlive )
		{
			if (m_isFuryOn) 
			{
				m_isFuryOn = false;
				m_animator.SetBool("breath", false);// Stop fury rush (if active)
				if (m_healthBehaviour) m_healthBehaviour.enabled = true;
				if (m_attackBehaviour) m_attackBehaviour.enabled = true;
				Messenger.Broadcast<bool, Type>(GameEvents.FURY_RUSH_TOGGLED, false, Type.None);
			}
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn;
	}

	protected virtual void Update() {
		// Cheat for infinite fire
		bool cheating = ((DebugSettings.infiniteFire || DebugSettings.infiniteSuperFire));


		if (m_isFuryOn) 
		{
			if ( !m_isFuryPaused )
			{
				// Don't decrease fury if cheating
				if(!cheating && !m_dragon.changingArea)
				{
					m_currentRemainingFuryDuration -= Time.deltaTime;
				}

				switch( m_type )
				{
					case Type.Standard:	{
							m_currentFury = m_currentRemainingFuryDuration / m_currentFuryDuration * m_furyMax;
							if (UsersManager.currentUser.superFuryProgression + 1 == m_superFuryMax) {
								if (m_currentRemainingFuryDuration <= 0.25f) {
									MegaFireUp();
								}
							}
						}
						break;
					case Type.Mega:
					{
							
					}
					break;
				}
				
				// With fury on boost is infinite
				m_dragon.AddEnergy(m_dragon.energyMax);




				if (m_currentRemainingFuryDuration <= 0)
				{
					EndFury();
					m_animator.SetBool("breath", false);
				} 
				else
				{
					Breath();
					m_animator.SetBool("breath", true);
				}
			}
		} else {

			if (cheating)
			{
				if (DebugSettings.infiniteFire)
					AddFury(m_furyMax - m_currentFury);	// Set to max fury
				else if (DebugSettings.infiniteSuperFire)
					UsersManager.currentUser.superFuryProgression = (int)m_superFuryMax;
			}

			if ( !m_dragon.dragonEatBehaviour.IsEating())
			{
				if (UsersManager.currentUser.superFuryProgression >= m_superFuryMax)
				{
					BeginFury( Type.Mega );

				}
				else if (m_currentFury >= m_furyMax)
				{
					BeginFury( Type.Standard );
				}
			}
		}

		ExtendedUpdate();
	}


	protected virtual void OnEntityBurned(Transform t, Reward reward)
	{	
		float healthReward = m_healthBehaviour.GetBoostedHp(reward.origin, reward.health);
		m_dragon.AddLife( healthReward, DamageType.NONE, t );
		m_dragon.AddEnergy(reward.energy);
	}

	protected virtual void OnRewardApplied( Reward _reward, Transform t)
	{
		AddFury( _reward.score );
	}

	virtual public bool IsInsideArea(Vector2 _point) { return false; }
	virtual public bool Overlaps( CircleAreaBounds _circle) { return false; }
	virtual protected void ExtendedStart() {}
	virtual protected void ExtendedUpdate() {}
	virtual public void RecalculateSize(){}

	virtual protected void BeginFury( Type _type ) 
	{
		RecalculateSize();
		m_type = _type;
		m_isFuryOn = true;

		UsersManager.currentUser.furyUsed = true;

		switch( m_type )
		{
			case Type.Mega:
			{
				// Set super gold rush progress
				m_currentFuryDuration = m_currentRemainingFuryDuration = m_furyDuration * m_superFuryDurationModifier;

				// Set coins multiplier for burn
				RewardManager.burnCoinsMultiplier = m_superFuryCoinsMultiplier;

				if ( !string.IsNullOrEmpty(m_superBreathSound) )
					m_superBreathSoundAO = AudioController.Play( m_superBreathSound, transform);

			}break;
			case Type.Standard:
			{
				m_currentFuryDuration = m_currentRemainingFuryDuration = m_furyDuration;

				// Set coins multiplier for burn
				RewardManager.burnCoinsMultiplier = 1;

				if ( !string.IsNullOrEmpty(m_breathSound) )
					m_breathSoundAO = AudioController.Play( m_breathSound, transform);
			}break;
		}

		RewardManager.currentFireRushMultiplier = m_fireRushMultiplier;

		if (m_healthBehaviour) m_healthBehaviour.enabled = false;
		if (m_attackBehaviour) m_attackBehaviour.enabled = false;

		Messenger.Broadcast<bool, Type>(GameEvents.FURY_RUSH_TOGGLED, true, m_type);
	}
	virtual protected void Breath() 
	{
		m_fireNodeTimer -= Time.deltaTime;
		if (m_fireNodeTimer <= 0) {
			m_fireNodeTimer += m_checkNodeFireTime;
			switch( m_type )
			{
				default:
				case Type.Standard:
				{
					FirePropagationManager.instance.FireUpNodes( bounds2D, Overlaps, m_dragon.data.tier, direction);
				}break;
				case Type.Mega:
				{
					FirePropagationManager.instance.FireUpNodes( bounds2D, Overlaps, DragonTier.COUNT - 1, direction);
				}break;
			}

		}
	}

	virtual protected void EndFury() 
	{
		switch (m_type) {
			case Type.Standard: {
				MegaFireUp();
				m_currentFury = 0;
				m_furyRushesCompleted++;

				if (m_breathSoundAO != null && m_breathSoundAO.IsPlaying() ){
					m_breathSoundAO.Stop();
					m_breathSoundAO = null;
				}
			} break;

			case Type.Mega: {
				// Set super fury counter to 0
				UsersManager.currentUser.superFuryProgression = 0;

				if (m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying()) {
					m_superBreathSoundAO.Stop();
					m_superBreathSoundAO = null;
				}
			} break;

		}

		RewardManager.currentFireRushMultiplier = 1;
		m_isFuryOn = false;
		m_currentFury = Mathf.Clamp(m_furyRushesCompleted * m_scoreToAddForNextFuryRushes, 0, m_maxScoreToAddForNextFuryRushes);

		if (m_healthBehaviour) m_healthBehaviour.enabled = true;
		if (m_attackBehaviour) m_attackBehaviour.enabled = true;

		Messenger.Broadcast<bool, Type>(GameEvents.FURY_RUSH_TOGGLED, false, m_type);
        m_type = Type.None;
	}

	private void MegaFireUp() {		
		if (UsersManager.currentUser.superFuryProgression < m_superFuryMax) {
			UsersManager.currentUser.superFuryProgression++;
		}
	}

	/// <summary>
	/// Add/remove fury to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of fury to be added/removed.</param>
	public void AddFury(float _offset) {
		if (!m_isFuryOn) {
			m_currentFury = Mathf.Clamp(m_currentFury + _offset, 0, m_furyMax);
		}
	}


	public float GetFuryProgression()
	{
		if ( m_isFuryOn && m_type == Type.Standard )
		{
			return m_currentRemainingFuryDuration / m_currentFuryDuration;
		}
		else
		{
			return m_currentFury / m_furyMax;
		}
	}

	public float GetSuperFuryProgression()
	{
		if ( m_isFuryOn && m_type == Type.Mega )
		{
			return m_currentRemainingFuryDuration / m_currentFuryDuration;
		}
		else
		{
			return UsersManager.currentUser.superFuryProgression/m_superFuryMax;
		}
	}


	public virtual void PauseFury()
	{
		m_isFuryPaused = true;
		m_animator.SetBool("breath", false);
		switch( m_type )
		{
			case Type.Standard: if ( m_breathSoundAO != null && m_breathSoundAO.IsPlaying() ) m_breathSoundAO.Pause();break;
			case Type.Mega: if ( m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying() ) m_superBreathSoundAO.Pause();break;
		}
	}

	public virtual void ResumeFury()
	{
		m_isFuryPaused = false;
		switch( m_type )
		{
			case Type.Standard: if ( m_breathSoundAO != null && m_breathSoundAO.IsPlaying() ) m_breathSoundAO.Unpause();break;
			case Type.Mega: if ( m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying() ) m_superBreathSoundAO.Unpause();break;
		}
	}


	public void AddPowerUpLengthMultiplier(float value)
    {
		m_lengthPowerUpMultiplier += value;
    }
}
