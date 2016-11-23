using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	private const float MaxGoldRushCompletitionPercentageForConsecutiveRushes = 0.5f; // max 50%.
	private const float AdditionalGoldRushCompletitionPercentageForConsecutiveRushes = 0.05f;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
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
	// private float m_furyModifier = 0;

	protected float m_furyDuration = 1f;	// Sandard fury Duraction


	protected float m_currentFuryDuration;		// If fury Active, total time it lasts
	protected float m_currentRemainingFuryDuration;	// If fury Active remaining time 


	public enum Type
    {
        Standard,
        Super,
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

	public string m_breathSound;
    private AudioObject m_breathSoundAO;
    public string m_superBreathSound;
    private AudioObject m_superBreathSoundAO;

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
	}

	void Start() 
	{

		m_dragon = GetComponent<DragonPlayer>();
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

		// m_furyModifier = 0;

		// Get the level
		m_furyDuration = m_dragon.data.def.GetAsFloat("furyBaseDuration");

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
		// SetFuryModifier( m_furyModifier );
	}

	public void SetFuryMax( float max )
	{
		m_furyMax = max;
	}
	/*
	public void SetFuryModifier( float value)
	{
		if (m_dragon != null)
		{
			m_furyBase = m_dragon.data.def.GetAsFloat("furyMax");
			m_furyModifier = value;
			m_furyMax = m_furyBase + ( m_furyModifier / 100.0f * m_furyBase );
		}
	}
	*/
	void OnDestroy()
	{
		Messenger.RemoveListener<Transform,Reward>(GameEvents.ENTITY_BURNED, OnEntityBurned);
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}
	
	void OnDisable() {

		if (m_isFuryOn) 
		{
			m_isFuryOn = false;
			m_animator.SetBool("breath", false);// Stop fury rush (if active)
			if (m_healthBehaviour) m_healthBehaviour.enabled = true;
			if (m_attackBehaviour) m_attackBehaviour.enabled = true;
			Messenger.Broadcast<bool, Type>(GameEvents.FURY_RUSH_TOGGLED, false, Type.None);
		}
	}

	public bool IsFuryOn() {
		
		return m_isFuryOn;
	}

	void Update() {
		// Cheat for infinite fire
		bool cheating = ((DebugSettings.infiniteFire || DebugSettings.infiniteSuperFire));


		if (m_isFuryOn) 
		{
			if ( !m_isFuryPaused )
			{
				// Don't decrease fury if cheating
				if(!cheating) 
				{
					m_currentRemainingFuryDuration -= Time.deltaTime;
				}

				switch( m_type )
				{
					case Type.Standard:
					{
						m_currentFury = m_currentRemainingFuryDuration / m_currentFuryDuration * m_furyMax;
					}break;
					case Type.Super:
					{
						
					}break;
				}
				
				// With fury on boost is infinite
				m_dragon.AddEnergy(m_dragon.energyMax);

				if ( m_currentRemainingFuryDuration <= 0) 
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

			if (UsersManager.currentUser.superFuryProgression >= m_superFuryMax)
			{
				BeginFury( Type.Super );

			}
			else if (m_currentFury >= m_furyMax)
			{
				BeginFury( Type.Standard );
			}
		}

		ExtendedUpdate();
	}


	protected virtual void OnEntityBurned(Transform t, Reward reward)
	{	
		m_dragon.AddLife( reward.health );
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

	virtual protected void BeginFury( Type _type ) 
	{
		m_type = _type;
		m_isFuryOn = true;

		UsersManager.currentUser.furyUsed = true;

		switch( m_type )
		{
			case Type.Super:
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


		if (m_healthBehaviour) m_healthBehaviour.enabled = false;
		if (m_attackBehaviour) m_attackBehaviour.enabled = false;

		Messenger.Broadcast<bool, Type>(GameEvents.FURY_RUSH_TOGGLED, true, m_type);
	}
	virtual protected void Breath() {}
	virtual protected void EndFury() 
	{
		switch( m_type )
		{
			case Type.Standard:
			{
				UsersManager.currentUser.superFuryProgression++;
				m_currentFury = 0;
				m_furyRushesCompleted++;

				if (m_breathSoundAO != null && m_breathSoundAO.IsPlaying() ){
					m_breathSoundAO.Stop();
					m_breathSoundAO = null;
				}
			}break;
			case Type.Super:
			{
				// Set super fury counter to 0
				UsersManager.currentUser.superFuryProgression = 0;

				if (m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying() )
				{
					m_superBreathSoundAO.Stop();
					m_superBreathSoundAO = null;
				}
			}break;

		}
		m_isFuryOn = false;
		m_currentFury = Mathf.Clamp(m_furyRushesCompleted * m_scoreToAddForNextFuryRushes, 0, m_maxScoreToAddForNextFuryRushes);

		if (m_healthBehaviour) m_healthBehaviour.enabled = true;
		if (m_attackBehaviour) m_attackBehaviour.enabled = true;

		Messenger.Broadcast<bool, Type>(GameEvents.FURY_RUSH_TOGGLED, false, m_type);
        m_type = Type.None;
	}

	/// <summary>
	/// Add/remove fury to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of fury to be added/removed.</param>
	public void AddFury(float _offset) {
		if ( !m_isFuryOn )
		{
			m_currentFury = Mathf.Clamp( m_currentFury + _offset, 0, m_furyMax);
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
		if ( m_isFuryOn && m_type == Type.Super )
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
			case Type.Super: if ( m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying() ) m_superBreathSoundAO.Pause();break;
		}
	}

	public virtual void ResumeFury()
	{
		m_isFuryPaused = false;
		switch( m_type )
		{
			case Type.Standard: if ( m_breathSoundAO != null && m_breathSoundAO.IsPlaying() ) m_breathSoundAO.Unpause();break;
			case Type.Super: if ( m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying() ) m_superBreathSoundAO.Unpause();break;
		}
	}

}
