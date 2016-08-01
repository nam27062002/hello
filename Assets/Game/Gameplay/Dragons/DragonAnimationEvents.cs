using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;
	private DragonBoostBehaviour m_bostBehaviour;
	public AudioSource m_wingsSound;
	public AudioSource m_eatSound;
	public AudioSource m_eatBigSound;
	public AudioSource m_wingsWindSound;
	public AudioSource m_wingsStrongFlap;
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
			m_wingsWindSound.Play();
		}
	}

	public void TurboLoopEnd()
	{
		m_bostBehaviour.DeactivateTrails();
		if (m_wingsWindSound != null)
		{
			m_wingsWindSound.Pause();
		}
	}

	public void WingsSound()
	{
		if (m_wingsSound != null)
		{
			m_wingsSound.Play();
		}
	}

	public void StrongFlap()
	{
		if (m_wingsStrongFlap != null)
		{
			m_wingsStrongFlap.Play();
		}
	}

	public void EatStartEvent()
	{
		if ( m_eatSound != null )
		{
			m_eatSound.pitch = Random.Range( 0.75f, 1.25f);
			m_eatSound.Play();
		}
	}

	public void EatBigStartEvent()
	{
		if ( m_eatBigSound != null )
		{
			m_eatBigSound.pitch = Random.Range( 0.75f, 1.25f);
			m_eatBigSound.Play();
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


	public void OnInsideWater()
	{
		if (m_wingsWindSound != null)
			m_wingsWindSound.mute = true;
		if (m_wingsSound != null)
			m_wingsSound.mute = true;
		if (m_wingsStrongFlap != null)
			m_wingsStrongFlap.mute = true;
	}

	public void OnExitWater()
	{
		if (m_wingsWindSound != null)
			m_wingsWindSound.mute = false;
		if (m_wingsSound != null)
			m_wingsSound.mute = false;
		if (m_wingsStrongFlap != null)
			m_wingsStrongFlap.mute = false;
	}

	public void OnEnterOuterSpace()
	{
		if (m_wingsWindSound != null)
			m_wingsWindSound.mute = true;
		if (m_wingsSound != null)
			m_wingsSound.mute = true;
		if (m_wingsStrongFlap != null)
			m_wingsStrongFlap.mute = true;
	}

	public void OnExitOuterSpace()
	{
		if (m_wingsWindSound != null)
			m_wingsWindSound.mute = false;
		if (m_wingsSound != null)
			m_wingsSound.mute = false;
		if (m_wingsStrongFlap != null)
			m_wingsStrongFlap.mute = false;
	}
}
