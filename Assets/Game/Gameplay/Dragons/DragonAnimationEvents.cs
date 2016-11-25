using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;
	private DragonBoostBehaviour m_bostBehaviour;
	public string m_wingsIdleSound;
	private AudioObject m_wingsIdleSoundAO;

	public string m_wingsFlyingSound;
	private AudioObject m_wingsFlyingSoundAO;

	public string m_eatSound;

	public string m_wingsWindSound;
	private AudioObject m_wingsWindSoundAO;

	public string m_wingsStrongFlap;
	private AudioObject m_wingsStrongFlapAO;

	public string m_levelUpSound;

	public string m_starvingSound;
	private AudioObject m_starvingSoundAO;

	public string m_hitSound;

	protected Animator m_animator;


	public delegate void OnEatEvent();
	public OnEatEvent onEatEvent; 

	public delegate void OnEatEndEvent();
	public OnEatEndEvent onEatEndEvent; 

	private bool m_eventsRegistered = false;

	void Start() {
		m_attackBehaviour = transform.parent.GetComponent<DragonAttackBehaviour>();
		m_bostBehaviour = transform.parent.GetComponent<DragonBoostBehaviour>();
		m_animator = GetComponent<Animator>();
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
		Messenger.AddListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarving);
		m_eventsRegistered = true;
		// m_animator.SetBool( "starving", true);

	}


	void OnDestroy()
	{
		if (m_eventsRegistered)
		{
			Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnLevelUp);
			Messenger.RemoveListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarving);
		}
	}

	private void OnLevelUp(DragonData _data) 
	{
		if ( !string.IsNullOrEmpty( m_levelUpSound ) )
			AudioController.Play( m_levelUpSound );
		m_animator.SetTrigger("LevelUp");
	}

	private void OnStarving( bool starving)
	{
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
		m_bostBehaviour.ActivateTrails();
		if ( !string.IsNullOrEmpty( m_wingsWindSound))
		{
			m_wingsWindSoundAO = AudioController.Play( m_wingsWindSound, transform);
		}
	}

	public void TurboLoopEnd()
	{
		m_bostBehaviour.DeactivateTrails();
		if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
		{
			m_wingsWindSoundAO.Stop();
		}
	}

	public void WingsIdleSound()
	{
		if (!string.IsNullOrEmpty(m_wingsIdleSound))
		{
			m_wingsIdleSoundAO = AudioController.Play(m_wingsIdleSound, transform);
		}
	}

	public void WingsFlyingSound()
	{
		if (!string.IsNullOrEmpty(m_wingsFlyingSound))
		{
			m_wingsFlyingSoundAO = AudioController.Play(m_wingsFlyingSound, transform);
		}
	}

	public void StrongFlap()
	{
		if (!string.IsNullOrEmpty(m_wingsStrongFlap))
		{
			m_wingsStrongFlapAO = AudioController.Play(m_wingsStrongFlap, transform);
		}
	}

	public void EatStartEvent()
	{
		if (!string.IsNullOrEmpty(m_eatSound))
		{
			AudioController.Play(m_eatSound, transform);
		}
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
		if ( !string.IsNullOrEmpty( m_hitSound ) )
			AudioController.Play(m_hitSound);
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

	public void OnInsideWater()
	{
		MuteWindSounds();
	}

	public void OnExitWater()
	{
		UnmuteWindSounds();
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


}
