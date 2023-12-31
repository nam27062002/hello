﻿using UnityEngine;
using UnityEngine.Audio;

public class DragonAnimationEvents : MonoBehaviour, IBroadcastListener {

	private DragonAttackBehaviour m_attackBehaviour;
	protected DragonParticleController m_particleController;
	public string m_wingsIdleSound;
	private AudioObject m_wingsIdleSoundAO;

	public string m_wingsFlyingSound;
	private AudioObject m_wingsFlyingSoundAO;

	public string m_eatSound;
	public string m_eatHoldSound;
	private AudioObject m_eatHoldSoundAO;
	private static int EAT_HOLD_HASH = Animator.StringToHash("EatHold");

	public string m_wingsWindSound;
	private AudioObject m_wingsWindSoundAO;
	protected int m_turboValues = 0;

	public string m_wingsStrongFlap;
	private AudioObject m_wingsStrongFlapAO;

	public string m_levelUpSound;

	public string m_starvingSound;
	private AudioObject m_starvingSoundAO;

	public string m_hitSound;
	public string m_poisonHitSound;
    protected bool m_allowHitAnimation = true;
    public bool allowHitAnimation { 
        get{ return m_allowHitAnimation; }
        set{ m_allowHitAnimation = value; }
    }

	public string m_enterWaterSound;
	public string m_enterWaterWithSplashSound;
	private bool m_enterWaterSoundPlaying = false;

	public string m_exitWaterSound;
	public string m_exitWaterWithSplashSound;
	private bool m_exitWaterSoundPlaying = false;

	public string m_skimmingSound;
	private AudioObject m_skimmingSoundAO;

	protected Animator m_animator;


	public delegate void OnEatEvent();
	public OnEatEvent onEatEvent; 

	public delegate void OnEatEndEvent();
	public OnEatEndEvent onEatEndEvent; 

	private bool m_eventsRegistered = false;

	protected AudioMixerSnapshot m_insideWaterSnapshot;
	public float m_waterDeepToSnapshot;
	private bool m_checkWaterSnapshot = false;
	private float m_startWaterMovementY;


	// Drunk attributes
	public string m_hiccupSound;
	public delegate void OnHiccupEvent();
	public OnHiccupEvent onHiccupEvent;

	// Grunt
	public string m_gruntSound;
	[Range(0f, 100.0f)]
	public float m_gruntToEatProbability = 5;

	private DamageType m_lastDamageType = DamageType.NONE;
	public DamageType lastDamageType{get{return m_lastDamageType;} set{m_lastDamageType = value;}}

	private int m_damageAnimState;


	public string m_onDeadSound = "hd_dragon_dead";
	public string m_onCorpseSound = "hd_dragon_dead";
	public string m_onReviveSound = "hd_dragon_revive";
	public string m_onPreMegaFire = "";

    protected bool m_mutedWindSounds = false;

	protected Transform m_transform;

	void Awake()
	{
		m_transform = transform;
		m_insideWaterSnapshot = InstanceManager.masterMixer.FindSnapshot("Underwater");
	}

	protected virtual void Start() {
		m_attackBehaviour = transform.parent.GetComponent<DragonAttackBehaviour>();
		m_particleController = transform.parent.GetComponentInChildren<DragonParticleController>();
		m_animator = GetComponent<Animator>();
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
		Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
		Messenger.AddListener<DragonBreathBehaviour.Type, float>(MessengerEvents.PREWARM_FURY_RUSH, OnPrewarmFuryRush);
        Broadcaster.AddListener( BroadcastEventType.GAME_PAUSED, this);
		
		m_eventsRegistered = true;
		// m_animator.SetBool( "starving", true);

		StartEndStateMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndStateMachineBehaviour>();
		for( int i = 0; i<behaviours.Length; i++ ){
			behaviours[i].onStateEnter += onStateEnter;
			behaviours[i].onStateExit += onStateExit;
		}

		m_damageAnimState = Animator.StringToHash("BaseLayer.Damage");
	}

	void OnDisable() {
		if ( ApplicationManager.IsAlive )
		{
			if (!m_checkWaterSnapshot && m_insideWaterSnapshot != null)	{ // Means we registered snapshot
				if (InstanceManager.musicController != null) {
					InstanceManager.musicController.UnregisterSnapshot(m_insideWaterSnapshot);
				}
			}
		}
	}

	void OnDestroy()
	{
		if (m_eventsRegistered)
		{
			Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, OnLevelUp);
			Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
			Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnKo);
			Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
			Messenger.RemoveListener<DragonBreathBehaviour.Type, float>(MessengerEvents.PREWARM_FURY_RUSH, OnPrewarmFuryRush);
			Broadcaster.RemoveListener( BroadcastEventType.GAME_PAUSED, this);
		}
	}

	void Update()
	{
		if ( m_checkWaterSnapshot )
		{
			if ( transform.position.y < m_startWaterMovementY - m_waterDeepToSnapshot)
			{
				if ( m_insideWaterSnapshot != null )
					InstanceManager.musicController.RegisterSnapshot(m_insideWaterSnapshot);
				m_checkWaterSnapshot = false;
			}
		}
	}

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
	{
		switch( eventType )
		{
			case BroadcastEventType.GAME_PAUSED:
			{
				OnGamePaused( ( broadcastEventInfo as ToggleParam ).value );
			}break;
		}
	}

	private void OnLevelUp(IDragonData _data) 
	{
		PlaySound(m_levelUpSound);
		// m_animator.SetTrigger("LevelUp");
	}

	private void OnHealthModifierChanged(DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier)
	{
		bool starving = (_newModifier != null && _newModifier.IsStarving());
		m_animator.SetBool( GameConstants.Animator.STARVING, starving);
		if (!string.IsNullOrEmpty( m_starvingSound)){
			if ( starving ){
				m_starvingSoundAO = AudioController.Play(m_starvingSound, transform);
                if ( m_mutedWindSounds )
                {
                    m_starvingSoundAO.volume = 0;
                }
			}else{
				if (m_starvingSoundAO != null && m_starvingSoundAO.IsPlaying() )
				{
					m_starvingSoundAO.Stop();
					m_starvingSoundAO = null;
				}
			}
		}
	}

	protected virtual void OnKo( DamageType type , Transform _source)
	{
		if (InstanceManager.player.CanSpawnCorpse(type))
		{
			PlaySound(m_onCorpseSound);
		}
		else
		{
			PlaySound(m_onDeadSound);
		}
	}

	protected virtual void OnRevive(DragonPlayer.ReviveReason reason)
	{
		PlaySound( m_onReviveSound );
	}

	void OnPrewarmFuryRush( DragonBreathBehaviour.Type _type, float duration )
	{
		switch( _type )
		{
			case DragonBreathBehaviour.Type.Mega:
			{
				PlaySound( m_onPreMegaFire );
			}break;
		}
	}

	public void OnAttackEvent() {
		if ( m_attackBehaviour != null )
			m_attackBehaviour.OnAttack();
	}

	public void TurboLoopStart()
	{
		if (m_turboValues == 0)
		{
			if ( m_particleController )
				m_particleController.ActivateTrails();
			if ( !string.IsNullOrEmpty(m_wingsWindSound))
			{
				m_wingsWindSoundAO = AudioController.Play( m_wingsWindSound, transform);
				if ( m_wingsWindSoundAO != null )
				{
					if (m_mutedWindSounds)
						m_wingsWindSoundAO.volume = 0;
					m_wingsWindSoundAO.completelyPlayedDelegate = OnWindsSoundCompleted;
				}
			}
		}
		m_turboValues++;
	}

	void OnWindsSoundCompleted( AudioObject ao )
	{
		ao.completelyPlayedDelegate = null;
		m_wingsWindSoundAO = null;
	}

	public void TurboLoopEnd()
	{
		m_turboValues--;
		if ( m_turboValues <= 0 )
		{
			if ( m_particleController )
				m_particleController.DeactivateTrails();
			if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
			{
				m_wingsWindSoundAO.Stop();
				m_wingsWindSoundAO = null;
			}
		}
		
	}

	public void IdleStart()
	{
		if (!string.IsNullOrEmpty(m_wingsIdleSound))
		{
			m_wingsIdleSoundAO = AudioController.Play(m_wingsIdleSound, transform);
			if ( m_wingsIdleSoundAO != null )
            {
                if (m_mutedWindSounds)
                    m_wingsIdleSoundAO.volume = 0;
				m_wingsIdleSoundAO.completelyPlayedDelegate = OnWigsIdleCompleted;
            }
		}
	}

	public void IdleEnd()
	{
		if (m_wingsIdleSoundAO != null && m_wingsIdleSoundAO.IsPlaying())
		{
			m_wingsIdleSoundAO.Stop();
			m_wingsIdleSoundAO = null;
		}
	}

	public void FlyStart()
	{
		if (!string.IsNullOrEmpty(m_wingsFlyingSound))
		{
			m_wingsFlyingSoundAO = AudioController.Play(m_wingsFlyingSound, transform);
			if ( m_wingsFlyingSoundAO != null )
            {
                if (m_mutedWindSounds)
                    m_wingsFlyingSoundAO.volume = 0;
				m_wingsFlyingSoundAO.completelyPlayedDelegate = OnWingsFlyingSoundCompleted;
            }
		}
	}

	public void FlyEnd()
	{
		if (m_wingsFlyingSoundAO != null && m_wingsFlyingSoundAO.IsPlaying())
		{
			m_wingsFlyingSoundAO.Stop();
			m_wingsFlyingSoundAO = null;
		}
	}

	public void WingsIdleSound()
	{
		// tell particle controller
		if ( m_particleController )
			m_particleController.WingsEvent();
		if (!string.IsNullOrEmpty(m_wingsIdleSound))
		{
			m_wingsIdleSoundAO = AudioController.Play(m_wingsIdleSound, transform);
			if ( m_wingsIdleSoundAO != null )
            {
                if (m_mutedWindSounds)
                    m_wingsIdleSoundAO.volume = 0;
				m_wingsIdleSoundAO.completelyPlayedDelegate = OnWigsIdleCompleted;
            }
		}
	}

	void OnWigsIdleCompleted( AudioObject ao )
	{
		ao.completelyPlayedDelegate = null;
		m_wingsIdleSoundAO = null;
	}

	public void WingsFlyingSound()
	{
		// tell particle controller
		if ( m_particleController )
			m_particleController.WingsEvent();
		if (!string.IsNullOrEmpty(m_wingsFlyingSound))
		{
			m_wingsFlyingSoundAO = AudioController.Play(m_wingsFlyingSound, transform);
			if ( m_wingsFlyingSoundAO != null )
            {
                if (m_mutedWindSounds)
                    m_wingsFlyingSoundAO.volume = 0;   
				m_wingsFlyingSoundAO.completelyPlayedDelegate = OnWingsFlyingSoundCompleted;
            }
		}
	}

	void OnWingsFlyingSoundCompleted( AudioObject ao )
	{
		ao.completelyPlayedDelegate = null;
		m_wingsFlyingSoundAO = null;
	}

	public void StrongFlap()
	{
		// tell particle controller
		if ( m_particleController )
			m_particleController.WingsEvent();
		if (!string.IsNullOrEmpty(m_wingsStrongFlap))
		{
			m_wingsStrongFlapAO = AudioController.Play(m_wingsStrongFlap, transform);
			if ( m_wingsStrongFlapAO != null )
            {
                if (m_mutedWindSounds)
                    m_wingsStrongFlapAO.volume = 0;
                m_wingsStrongFlapAO.completelyPlayedDelegate = OnWingsStrongFlapSoundCompleted;
            }
				
		}
	}

	void OnWingsStrongFlapSoundCompleted( AudioObject ao )
	{
		ao.completelyPlayedDelegate = null;
		m_wingsStrongFlapAO = null;
	}

	public void EatStartEvent()
	{
		if ( Random.Range(0.0f, 100.0f) < m_gruntToEatProbability )
		{
			PlaySound(m_gruntSound);
		}

		PlaySound(m_eatSound);
	}

	public void EatEvent()
	{
		if (onEatEvent != null)
			onEatEvent();
	}

	public void EatEndEvent()
	{
		if (onEatEndEvent != null)
			onEatEndEvent();
	}

	public void PlayHitAnimation( DamageType _type )
	{
        if (m_allowHitAnimation)
        {
    		AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
    		if (stateInfo.fullPathHash != m_damageAnimState) {
    			m_animator.SetTrigger( GameConstants.Animator.DAMAGE );// receive damage?
    			m_lastDamageType = _type;
    		}
        }
	}

	public void HitEvent()
	{
		switch(m_lastDamageType)
		{
			case DamageType.POISON:
			{
				PlaySound( m_poisonHitSound );
			}break;
			default:{
				PlaySound( m_hitSound );
			}break;
		}


	}

	public void HiccupEvent()
	{
		PlaySound( m_hiccupSound );
		if ( onHiccupEvent != null)
			onHiccupEvent();
	}


	private void MuteWindSounds()
	{
        m_mutedWindSounds = true;
		AudioItem item = AudioController.GetAudioItem( m_wingsWindSound );
		if ( item != null ) item.Volume = 0;
		if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
			m_wingsWindSoundAO.volume = 0;
		if (m_wingsIdleSoundAO != null && m_wingsIdleSoundAO.IsPlaying())
			m_wingsIdleSoundAO.volume = 0;
		if (m_wingsFlyingSoundAO != null && m_wingsFlyingSoundAO.IsPlaying())
			m_wingsFlyingSoundAO.volume = 0;
		if (m_wingsStrongFlapAO != null && m_wingsStrongFlapAO.IsPlaying())
			m_wingsStrongFlapAO.volume = 0;
		if (m_starvingSoundAO != null && m_starvingSoundAO.IsPlaying())
			m_starvingSoundAO.volume = 0;
	}

	private void UnmuteWindSounds()
	{
        m_mutedWindSounds = false;
		AudioItem item = AudioController.GetAudioItem( m_wingsWindSound );
		if ( item != null ) item.Volume = 1;
		if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
			m_wingsWindSoundAO.volume = m_wingsWindSoundAO.audioItem.Volume;
		if (m_wingsIdleSoundAO != null && m_wingsIdleSoundAO.IsPlaying())
			m_wingsIdleSoundAO.volume = m_wingsIdleSoundAO.audioItem.Volume;
		if (m_wingsFlyingSoundAO != null && m_wingsFlyingSoundAO.IsPlaying())
			m_wingsFlyingSoundAO.volume = m_wingsFlyingSoundAO.audioItem.Volume;
		if (m_wingsStrongFlapAO != null && m_wingsStrongFlapAO.IsPlaying())
			m_wingsStrongFlapAO.volume = m_wingsStrongFlapAO.audioItem.Volume;
		if (m_starvingSoundAO != null && m_starvingSoundAO.IsPlaying())
			m_starvingSoundAO.volume = m_starvingSoundAO.audioItem.Volume;
	}

	public void OnInsideWater( bool withSplash )
	{
		MuteWindSounds();
		m_checkWaterSnapshot = true;
		m_startWaterMovementY = transform.position.y;

		if (!m_enterWaterSoundPlaying)
		{
			string soundId = m_enterWaterSound;
			if ( withSplash )
			{
				soundId = m_enterWaterWithSplashSound;
			}
			if ( !string.IsNullOrEmpty(soundId) )
			{
				m_enterWaterSoundPlaying = true;
				AudioObject ao = AudioController.Play(soundId, m_transform);
				ao.completelyPlayedDelegate = OnEnterWaterSoundCompleted;
			}
		}
	}

	protected void OnEnterWaterSoundCompleted( AudioObject ao )
	{
		ao.completelyPlayedDelegate = null;
		m_enterWaterSoundPlaying = false;
	}

	public void OnExitWater( bool withSplash )
	{
		UnmuteWindSounds();
		if (!m_checkWaterSnapshot && m_insideWaterSnapshot != null)	// Means we registered snapshot
			InstanceManager.musicController.UnregisterSnapshot(m_insideWaterSnapshot);	
		m_checkWaterSnapshot = false;

		if ( !m_exitWaterSoundPlaying )
		{
			string soundId = m_exitWaterSound;
			if (withSplash )
			{
				soundId = m_exitWaterWithSplashSound;
			}
			if ( !string.IsNullOrEmpty(soundId) )
			{
				m_exitWaterSoundPlaying = true;
				AudioObject ao = AudioController.Play(soundId, m_transform);
				ao.completelyPlayedDelegate = OnExitWaterSoundCompleted;
			}

		}
	}

	void OnExitWaterSoundCompleted( AudioObject ao )
	{
		ao.completelyPlayedDelegate = null;
		m_exitWaterSoundPlaying = false;
	}

	public void StartedSkimming()
	{
		if ( !string.IsNullOrEmpty( m_skimmingSound))
		{
			m_skimmingSoundAO = AudioController.Play( m_skimmingSound, transform);
		}
	}

	public void EndedSkimming()
	{
		if (m_skimmingSoundAO != null && m_skimmingSoundAO.IsPlaying())
		{
			m_skimmingSoundAO.Stop();
			m_skimmingSoundAO = null;
		}
	}

	protected void PlaySound( string audioId )
	{
		if ( !string.IsNullOrEmpty(audioId) )
			AudioController.Play( audioId, m_transform );
	}

	public void OnEnterOuterSpace()
	{
		MuteWindSounds();
	}

	public void OnExitOuterSpace()
	{
		UnmuteWindSounds();
	}

	public void IntroDone()
	{
		Messenger.Broadcast(MessengerEvents.GAME_COUNTDOWN_ENDED);
	}

	public void OnNoAirBubbles()
	{
		if ( m_particleController != null )
			m_particleController.OnNoAirBubbles();
	}

	void onStateEnter( int stateNameHash )
	{
		if( stateNameHash == EAT_HOLD_HASH)
		{
			OnStartEatHold();
		}
	}

	void onStateExit( int stateNameHash )
	{
		if( stateNameHash == EAT_HOLD_HASH)
		{
			OnEndEatHold();
		}
	}

	void OnStartEatHold()
	{
		if (!string.IsNullOrEmpty( m_eatHoldSound ))
		{
			m_eatHoldSoundAO = AudioController.Play(m_eatHoldSound, transform);
		}
	}
	void OnEndEatHold()
	{
		if (m_eatHoldSoundAO != null && m_eatHoldSoundAO.IsPlaying())
		{
			m_eatHoldSoundAO.Stop();
			m_eatHoldSoundAO = null;
		}
	}
    
    protected virtual void OnGamePaused( bool _paused )
    {
        if ( _paused )
        {
            // Pause sound
            if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying() )
                m_wingsWindSoundAO.Pause();
        }
        else
        {
            // Resume sound
            if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPaused() )
                m_wingsWindSoundAO.Unpause();
        }
    }
    

}
