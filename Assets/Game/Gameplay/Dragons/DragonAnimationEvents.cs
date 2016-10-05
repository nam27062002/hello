using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;
	private DragonBoostBehaviour m_bostBehaviour;
	public string m_wingsIdleSound;
	private AudioObject m_wingsIdleSoundAO;

	public string m_wingsFlyingSound;
	private AudioObject m_wingsFlyingSoundAO;

	public string m_eatSound;
	public string m_eatBigSound;

	public string m_wingsWindSound;
	private AudioObject m_wingsWindSoundAO;

	public string m_wingsStrongFlap;
	private AudioObject m_wingsStrongFlapAO;

	protected Animator m_animator;


	public delegate void OnEatEvent();
	public OnEatEvent onEatEvent; 

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
		m_animator.SetTrigger("LevelUp");
	}

	private void OnStarving( bool starving)
	{
		m_animator.SetBool( "starving", starving);
	}

	public void OnAttackEvent() {
		m_attackBehaviour.OnAttack();
	}

	public void TurboLoopStart()
	{
		m_bostBehaviour.ActivateTrails();
		if (m_wingsWindSound != null)
		{
			m_wingsWindSoundAO = AudioController.Play( m_wingsWindSound, transform);
		}
	}

	public void TurboLoopEnd()
	{
		m_bostBehaviour.DeactivateTrails();
		if (m_wingsWindSoundAO != null && m_wingsWindSoundAO.IsPlaying())
		{
			m_wingsWindSoundAO.Pause();
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

	public void EatBigStartEvent()
	{
		if (!string.IsNullOrEmpty(m_eatBigSound))
		{
			AudioController.Play(m_eatBigSound, transform);
		}
	}

	public void EatEvent()
	{
		if (onEatEvent != null)
			onEatEvent();
	}

	// To remove when we delete all old dragons
	public void EatBigEvent()
	{

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
