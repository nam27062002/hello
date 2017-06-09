using UnityEngine;
using UnityEngine.Audio;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;
	private DragonParticleController m_particleController;
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

	public string m_wingsStrongFlap;
	private AudioObject m_wingsStrongFlapAO;

	public string m_levelUpSound;

	public string m_starvingSound;
	private AudioObject m_starvingSoundAO;

	public string m_hitSound;


	public string m_enterWaterSound;
	public string m_enterWaterWithSplashSound;
	public string m_exitWaterSound;
	public string m_exitWaterWithSplashSound;

	public string m_skimmingSound;
	private AudioObject m_skimmingSoundAO;

	protected Animator m_animator;


	public delegate void OnEatEvent();
	public OnEatEvent onEatEvent; 

	public delegate void OnEatEndEvent();
	public OnEatEndEvent onEatEndEvent; 

	private bool m_eventsRegistered = false;

	public AudioMixerSnapshot m_insideWaterSnapshot;
	public float m_waterDeepToSnapshot;
	private bool m_checkWaterSnapshot = false;
	private float m_startWaterMovementY;


	// Drunk attributes
	public string m_hiccupSound;
	public delegate void OnHiccupEvent();
	public OnHiccupEvent onHiccupEvent;

	void Start() {
		m_attackBehaviour = transform.parent.GetComponent<DragonAttackBehaviour>();
		m_particleController = transform.parent.GetComponentInChildren<DragonParticleController>();
		m_animator = GetComponent<Animator>();
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
		m_eventsRegistered = true;
		// m_animator.SetBool( "starving", true);

		StartEndStateMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndStateMachineBehaviour>();
		for( int i = 0; i<behaviours.Length; i++ ){
			behaviours[i].onStateEnter += onStateEnter;
			behaviours[i].onStateExit += onStateExit;
		}
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
			Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
			Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
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

	private void OnLevelUp(DragonData _data) 
	{
		PlaySound(m_levelUpSound);
		m_animator.SetTrigger("LevelUp");
	}

	private void OnHealthModifierChanged(DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier)
	{
		bool starving = (_newModifier != null && _newModifier.IsStarving());
		m_animator.SetBool( "starving", starving);
		if (!string.IsNullOrEmpty( m_starvingSound)){
			if ( starving ){
				m_starvingSoundAO = AudioController.Play(m_starvingSound, transform);
			}else{
				if ( m_starvingSoundAO.IsPlaying() )
					m_starvingSoundAO.Stop();
			}
		}
	}

	public void OnAttackEvent() {
		if ( m_attackBehaviour != null )
			m_attackBehaviour.OnAttack();
	}

	public void TurboLoopStart()
	{
		if ( m_particleController )
			m_particleController.ActivateTrails();
		if ( !string.IsNullOrEmpty( m_wingsWindSound))
		{
			m_wingsWindSoundAO = AudioController.Play( m_wingsWindSound, transform);
		}
	}

	public void TurboLoopEnd()
	{
		if ( m_particleController )
			m_particleController.DeactivateTrails();
		if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
		{
			m_wingsWindSoundAO.Stop();
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
		}
	}

	public void WingsFlyingSound()
	{
		// tell particle controller
		if ( m_particleController )
			m_particleController.WingsEvent();
		if (!string.IsNullOrEmpty(m_wingsFlyingSound))
		{
			m_wingsFlyingSoundAO = AudioController.Play(m_wingsFlyingSound, transform);
		}
	}

	public void StrongFlap()
	{
		// tell particle controller
		if ( m_particleController )
			m_particleController.WingsEvent();
		if (!string.IsNullOrEmpty(m_wingsStrongFlap))
		{
			m_wingsStrongFlapAO = AudioController.Play(m_wingsStrongFlap, transform);
		}
	}

	public void EatStartEvent()
	{
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

	public void HitEvent()
	{
		PlaySound( m_hitSound );
	}

	public void HiccupEvent()
	{
		PlaySound( m_hitSound );
		if ( onHiccupEvent != null)
			onHiccupEvent();
	}


	private void MuteWindSounds()
	{
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
		if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
			m_wingsWindSoundAO.volume = m_wingsWindSoundAO.volumeItem;
		if (m_wingsIdleSoundAO != null && m_wingsIdleSoundAO.IsPlaying())
			m_wingsIdleSoundAO.volume = m_wingsIdleSoundAO.volumeItem;
		if (m_wingsFlyingSoundAO != null && m_wingsFlyingSoundAO.IsPlaying())
			m_wingsFlyingSoundAO.volume = m_wingsFlyingSoundAO.volumeItem;
		if (m_wingsStrongFlapAO != null && m_wingsStrongFlapAO.IsPlaying())
			m_wingsStrongFlapAO.volume = m_wingsStrongFlapAO.volumeItem;
		if (m_starvingSoundAO != null && m_starvingSoundAO.IsPlaying())
			m_starvingSoundAO.volume = m_starvingSoundAO.volumeItem;
	}

	public void OnInsideWater( bool withSplash )
	{
		MuteWindSounds();
		m_checkWaterSnapshot = true;
		m_startWaterMovementY = transform.position.y;
		if ( withSplash )
		{
			PlaySound(m_enterWaterWithSplashSound);
		}
		else
		{
			PlaySound(m_enterWaterSound);
		}
	}

	public void OnExitWater( bool withSplash )
	{
		UnmuteWindSounds();
		if (!m_checkWaterSnapshot && m_insideWaterSnapshot != null)	// Means we registered snapshot
			InstanceManager.musicController.UnregisterSnapshot(m_insideWaterSnapshot);	
		m_checkWaterSnapshot = false;

		if (withSplash )
		{
			PlaySound(m_exitWaterWithSplashSound);
		}
		else
		{
			PlaySound(m_exitWaterSound);
		}

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
		}
	}

	private void PlaySound( string audioId )
	{
		if ( !string.IsNullOrEmpty(audioId) )
			AudioController.Play( audioId, transform );
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
		Messenger.Broadcast(GameEvents.GAME_COUNTDOWN_ENDED);
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
		}
	}

}
