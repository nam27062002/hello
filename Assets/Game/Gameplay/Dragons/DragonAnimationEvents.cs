using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;
	private DragonBoostBehaviour m_bostBehaviour;
	public AudioSource m_wingsSound;
	public AudioSource m_eatSound;
	public AudioSource m_eatBigSound;
	public AudioSource m_wingsWindSound;
	public AudioSource m_wingsStrongFlap;

	void Start() {
		m_attackBehaviour = transform.parent.GetComponent<DragonAttackBehaviour>();
		m_bostBehaviour = transform.parent.GetComponent<DragonBoostBehaviour>();
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

	public void EatEvent()
	{
		if ( m_eatSound != null )
		{
			m_eatSound.pitch = Random.Range( 0.75f, 1.25f);
			m_eatSound.Play();
		}
	}

	public void EatBigEvent()
	{
		if ( m_eatBigSound != null )
		{
			m_eatBigSound.pitch = Random.Range( 0.75f, 1.25f);
			m_eatBigSound.Play();
		}
	}
}
