using UnityEngine;
using System.Collections;

public class DragonBreathBehaviour : MonoBehaviour, IBroadcastListener {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	private const float INITIAL_FURY_PERCENTAGE = 0.80f;

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
	protected bool m_modInfiniteFury = false;
	public bool modInfiniteFury
	{
		get { return m_modInfiniteFury; }
		set { m_modInfiniteFury = value; }
	}

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
    public float remainingFuryDuration{
        get{ return m_currentRemainingFuryDuration; }
    }

	public enum Type
    {
        Standard,
        Mega,
        None
    };
    
    public FireColorSetupManager.FireColorType[] m_colors = new FireColorSetupManager.FireColorType[] { FireColorSetupManager.FireColorType.RED, FireColorSetupManager.FireColorType.BLUE };
    protected FireColorSetupManager.FireColorType m_currentColor;
    public FireColorSetupManager.FireColorType currentColor{
        get{ return m_currentColor; }
    }

    public float m_prewarmDuration = 0.5f;
	protected float m_prewarmFuryTimer;
	protected bool m_isFuryPaused;
    public bool isFuryPaused { get { return m_isFuryPaused; } }
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

	private float m_lengthPowerUpPercentage = 0;
    protected float lengthPowerUpPercentage { get {           
            return Mathf.Clamp(m_lengthPowerUpPercentage, -90f, 100f);
        }
    }

    protected FuryRushToggled m_furyRushToggled = new FuryRushToggled();

	public enum State
	{
		NONE,
		NORMAL,
		PREWARM_BREATH,
		BREATHING
	};
	protected State m_state = State.NONE;

	protected int tournamentMegaFireValue = 0;
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
		m_animator = transform.Find("view").GetComponent<Animator>();
		m_bounds2D = new Rect();

		m_tier = m_dragon.data.tier;

        // Init content cache
        m_furyBase = m_dragon.data.furyMax;
		m_furyMax = m_furyBase;
		m_currentFury = 0;

		if (!UsersManager.currentUser.furyUsed) {
			m_currentFury = m_furyMax * INITIAL_FURY_PERCENTAGE;
		}

		// Get the level
		AddDurationBonus(0);
        m_fireRushMultiplier = m_dragon.data.furyScoreMultiplier;

		m_furyRushesCompleted = 0;
		m_scoreToAddForNextFuryRushes = (int)(AdditionalGoldRushCompletitionPercentageForConsecutiveRushes * (float)m_furyMax);
		m_maxScoreToAddForNextFuryRushes = (int)(MaxGoldRushCompletitionPercentageForConsecutiveRushes * (float)m_furyMax);

		ExtendedStart();

		Messenger.AddListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEntityBurned);
		Messenger.AddListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
		Broadcaster.AddListener(BroadcastEventType.GAME_PAUSED, this);

		ChangeState(State.NORMAL);
        
        for (int i = 0; i < m_colors.Length; i++)
        {
            FireColorSetupManager.instance.LoadColor(m_colors[i]);
        }
	}

	


	public void SetFuryMax( float max )
	{
		m_furyMax = max;
	}

	public void SetDurationBonus( float furyBonus )
	{
        m_furyBaseDuration = m_dragon.data.furyBaseDuration;
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
		Messenger.RemoveListener<Transform, IEntity, Reward, KillType>(MessengerEvents.ENTITY_KILLED, OnEntityBurned);
		Messenger.RemoveListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_PAUSED, this);
	}

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
	{
		switch( eventType )
		{
			case BroadcastEventType.GAME_PAUSED:
			{
				OnGamePaused( (broadcastEventInfo as ToggleParam).value );
			}break;
		}
	}


	void OnDisable() {
		if ( ApplicationManager.IsAlive )
		{
			if (m_state == State.BREATHING)
			{
				EndFury( false );
			}
		}
	}

	public bool IsFuryOn() {

		return m_state == State.BREATHING;
	}

	protected virtual void Update() {


		#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.F)) {
				AddFury(m_furyMax);
			}
			else if (Input.GetKeyDown(KeyCode.G)) {
				SetMegaFireValue((int)m_superFuryMax);
			}
        #endif

        // Cheat for infinite fire
        bool infiniteFury = IsInfiniteFury();

		if (m_dragon.changingArea) return;

		switch( m_state )
		{
			case State.NORMAL:
			{
				if (infiniteFury)
				{
					if (DebugSettings.infiniteFire)
						AddFury(m_furyMax - m_currentFury);	// Set to max fury
					else if (DebugSettings.infiniteSuperFire){
						SetMegaFireValue((int)m_superFuryMax);
					}
				}

				if ( !m_dragon.dragonEatBehaviour.IsEating() && m_dragon.playable)
				{
					if (GetMegaFireValue() >= m_superFuryMax)
					{
						PrewarmFury(Type.Mega);
					}
					else if (m_currentFury >= m_furyMax)
					{
						BeginFury( Type.Standard );
					}
				}
			}break;
			case State.PREWARM_BREATH:
			{
				m_prewarmFuryTimer -= Time.unscaledDeltaTime;
				if ( m_prewarmFuryTimer <= 0)
					BeginFury( Type.Mega );
			}break;
			case State.BREATHING:
			{
				if ( !m_isFuryPaused )
				{
                    if (!infiniteFury)
                        AdvanceRemainingFire();

					switch( m_type )
					{
						case Type.Standard:	{
								m_currentFury = m_currentRemainingFuryDuration / m_currentFuryDuration * m_furyMax;
								if (GetMegaFireValue() + 1 == m_superFuryMax) {
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

					if (m_currentRemainingFuryDuration <= 0)
					{
						EndFury();
						m_animator.SetBool( GameConstants.Animator.BREATH, false);
					}
					else
					{
						Breath();
						m_animator.SetBool( GameConstants.Animator.BREATH, true);
					}
				}
			}break;
		}

	}
    
    public bool IsInfiniteFury()
    {
        return ((m_modInfiniteFury || DebugSettings.infiniteFire || DebugSettings.infiniteSuperFire));   
    }
    
    public void AdvanceRemainingFire()
    {
        // Don't decrease fury if cheating
        if(!m_dragon.changingArea)
        {
            m_currentRemainingFuryDuration -= Time.deltaTime;
        }
    }


	protected virtual void OnEntityBurned(Transform _t, IEntity _e, Reward _reward, KillType _type)
	{
        // Consider also electric rush, ice breath 
        if (_type == KillType.BURNT || _type == KillType.ELECTRIFIED || _type == KillType.FROZEN)
        {
            float healthReward = m_healthBehaviour.GetBoostedHp(_reward.origin, _reward.health);
            m_dragon.AddLife(healthReward, DamageType.NONE, _t);
            m_dragon.AddEnergy(_reward.energy);
            //AddFury(reward.fury);??
        }
	}

	protected virtual void OnRewardApplied( Reward _reward, Transform t)
	{
		AddFury( _reward.score );
        if ( _reward.fury > 0f )
            AddFury(m_furyMax * _reward.fury);
	}

	protected virtual void OnGamePaused( bool _paused )
	{
		if ( _paused && m_state == State.BREATHING)
		{
			// Pause sound
			if (m_breathSoundAO != null && m_breathSoundAO.IsPlaying() )
				m_breathSoundAO.Pause();
		}
		else if ( m_state == State.BREATHING )
		{
			// Resume sound
			if (m_breathSoundAO != null && m_breathSoundAO.IsPaused() )
				m_breathSoundAO.Unpause();
		}
	}

	virtual public bool IsInsideArea(Vector2 _point) { return false; }
	virtual public bool Overlaps( CircleAreaBounds _circle) { return false; }
	virtual protected void ExtendedStart() {}
	virtual protected void ExtendedUpdate() {}
	virtual public void RecalculateSize(){}

	virtual protected void PrewarmFury( Type _type )
	{
		m_type = _type;
		ChangeState(State.PREWARM_BREATH);
	}

	virtual protected void BeginFury( Type _type )
	{
        m_currentColor = m_colors[0];
        if (_type == Type.Mega)
            m_currentColor = m_colors[1];
		RecalculateSize();
		m_type = _type;

		ChangeState(State.BREATHING);
	}
	virtual protected void Breath()
	{
		m_fireNodeTimer -= Time.deltaTime;
		if (m_fireNodeTimer <= 0) {
			m_fireNodeTimer += m_checkNodeFireTime;
			FirePropagationManager.instance.FireUpNodes(bounds2D, Overlaps, m_dragon.data.tier, m_type, direction, IEntity.Type.PLAYER, m_currentColor);
		}
	}

	virtual protected void EndFury( bool increase_mega_fire = true )
	{
		if ( m_type == Type.Standard && increase_mega_fire )
			MegaFireUp();
		ChangeState( State.NORMAL );
	}

	private void MegaFireUp() {

		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
		{
			if (tournamentMegaFireValue < m_superFuryMax)
				tournamentMegaFireValue++;
		}
		else
		{
			if (UsersManager.currentUser.superFuryProgression < m_superFuryMax) {
				UsersManager.currentUser.superFuryProgression++;
			}
		}
	}

	private void SetMegaFireValue( int v)
	{
		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
		{
			tournamentMegaFireValue = v;
		}
		else
		{
			UsersManager.currentUser.superFuryProgression = v;
		}
	}

	public int GetMegaFireValue()
	{
		int ret = 0;
		if ( SceneController.mode == SceneController.Mode.TOURNAMENT )
		{
			ret = tournamentMegaFireValue;
		}
		else
		{
			ret = UsersManager.currentUser.superFuryProgression;
		}
		return ret;
	}

	/// <summary>
	/// Add/remove fury to the dragon.
	/// </summary>
	/// <param name="_offset">The amount of fury to be added/removed.</param>
	public void AddFury(float _offset) {
		if ( m_state != State.BREATHING && m_state != State.PREWARM_BREATH && m_dragon.form != DragonPlayer.Form.MUMMY) {
			m_currentFury = Mathf.Clamp(m_currentFury + _offset, 0, m_furyMax);
		}
	}
    
	public float GetFuryProgression()
	{
		if ( m_state == State.BREATHING && m_type == Type.Standard )
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
		if ( m_state == State.BREATHING && m_type == Type.Mega )
		{
			return m_currentRemainingFuryDuration / m_currentFuryDuration;
		}
		else
		{
			return GetMegaFireValue()/m_superFuryMax;
		}
	}


	public virtual void PauseFury()
	{
		m_isFuryPaused = true;
		m_animator.SetBool( GameConstants.Animator.BREATH, false);
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
			case Type.Standard: if ( m_breathSoundAO != null && !m_breathSoundAO.IsPlaying() ) m_breathSoundAO.Unpause();break;
			case Type.Mega: if ( m_superBreathSoundAO != null && !m_superBreathSoundAO.IsPlaying() ) m_superBreathSoundAO.Unpause();break;
		}
	}


	public void AddPowerUpLengthMultiplier(float value)
    {
		m_lengthPowerUpPercentage += value;
    }


    //
    void ChangeState( State _newState )
    {
    	if ( m_state == _newState ) return;

    	switch(m_state )
    	{
    		case State.NONE:
    		{
    		}break;
			case State.NORMAL:
    		{
    		}break;
			case State.PREWARM_BREATH:
    		{
	  	  	}break;
			case State.BREATHING:
    		{
				switch (m_type) {
					case Type.Standard: {
						m_currentFury = 0;
						m_furyRushesCompleted++;

						if (m_breathSoundAO != null && m_breathSoundAO.IsPlaying() ){
							m_breathSoundAO.Stop();
							m_breathSoundAO = null;
						}

					} break;

					case Type.Mega: {
						// Set super fury counter to 0
						SetMegaFireValue(0);

						if (m_superBreathSoundAO != null && m_superBreathSoundAO.IsPlaying()) {
							m_superBreathSoundAO.Stop();
							m_superBreathSoundAO = null;
						}
					} break;

				}

				RewardManager.currentFireRushMultiplier = 1;
				m_currentFury = Mathf.Clamp(m_furyRushesCompleted * m_scoreToAddForNextFuryRushes, 0, m_maxScoreToAddForNextFuryRushes);

				if (m_healthBehaviour) m_healthBehaviour.enabled = true;
				if (m_attackBehaviour) m_attackBehaviour.enabled = true;

                m_state = _newState;    // This is done so if in the event FURY_RUSH_TOGGLED someone checks if is fury on it says false. Check DragonPlayer CanIResumeEating
                m_furyRushToggled.activated = false;
                m_furyRushToggled.type = m_type;
                m_furyRushToggled.color = m_currentColor;
                Broadcaster.Broadcast(BroadcastEventType.FURY_RUSH_TOGGLED, m_furyRushToggled);
		        m_type = Type.None;
    		}break;
    	}
    	m_state = _newState;

		switch(m_state )
    	{
    		case State.NONE:
    		{
    		}break;
			case State.NORMAL:
    		{
    		}break;
			case State.PREWARM_BREATH:
    		{
				// With fury on boost is infinite
				m_dragon.AddEnergy(m_dragon.energyMax);

                // Reset megafire counter to 0 as soon as it starts
                SetMegaFireValue(0);

				m_prewarmFuryTimer = m_prewarmDuration;
				if (m_healthBehaviour) m_healthBehaviour.enabled = false;
				if (m_attackBehaviour) m_attackBehaviour.enabled = false;
				Messenger.Broadcast<Type, float>(MessengerEvents.PREWARM_FURY_RUSH, m_type, m_prewarmDuration);
	    	}break;
			case State.BREATHING:
    		{
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

				// With fury on boost is infinite
				m_dragon.AddEnergy(m_dragon.energyMax);

				if (m_healthBehaviour)
                {
                    m_healthBehaviour.CleanDotDamage(); // Remove all DOT Damage because we are invincible
                    m_healthBehaviour.enabled = false;
                }
				if (m_attackBehaviour) m_attackBehaviour.enabled = false;

                m_furyRushToggled.activated = true;
                m_furyRushToggled.type = m_type;
                m_furyRushToggled.color = m_currentColor;
                Broadcaster.Broadcast(BroadcastEventType.FURY_RUSH_TOGGLED, m_furyRushToggled);
    		}break;
    	}
    }
}
