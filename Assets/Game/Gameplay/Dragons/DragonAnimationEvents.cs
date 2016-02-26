using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;
	private DragonBoostBehaviour m_bostBehaviour;
	public AudioSource m_wingsSound;
	public AudioSource m_eatSound;
	public AudioSource m_eatBigSound;

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
	}

	public void TurboLoopEnd()
	{
		m_bostBehaviour.DeactivateTrails();
	}

	public void WingsSound()
	{
		if (m_wingsSound != null)
		{
			m_wingsSound.Play();
		}
	}

	public void EatEvent()
	{
		Debug.Log("EatEvent");
		if ( m_eatSound != null )
		{
			m_eatSound.pitch = Random.Range( 0.75f, 1.25f);
			m_eatSound.Play();
		}
	}

	public void EatBigEvent()
	{
		Debug.Log("EatBigEvent");
		if ( m_eatBigSound != null )
		{
			m_eatBigSound.pitch = Random.Range( 0.75f, 1.25f);
			m_eatBigSound.Play();
		}
	}
}
