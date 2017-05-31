using UnityEngine;
using System.Collections;

public class FireBallBreath : DragonBreathBehaviour {

	public GameObject m_fireBallPrefab;
	public float m_timeBetweenFires = 1.0f;
    public float m_speedBall = 20.0f;
	private float m_timer;

	private Transform m_mouthTransform;
	private DragonMotion m_dragonMotion;

	// Use this for initialization
	override protected void ExtendedStart() {

		//PoolManager.CreatePool(m_fireBallPrefab);

		m_dragonMotion = GetComponent<DragonMotion>();
		m_mouthTransform = m_dragonMotion.tongue;
		m_direction = Vector2.zero;
	}


	override protected void BeginFury( Type _type ) 
	{
		base.BeginFury(_type);
		m_timer = m_timeBetweenFires;
	}

	override protected void Breath()
	{
		m_direction = m_dragonMotion.direction;

		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			m_timer += m_timeBetweenFires;
			// Throw fire ball!!!
			GameObject go = null; //PoolManager.GetInstance (m_fireBallPrefab.name);
			if (go != null) {
				go.transform.position = m_mouthTransform.position;
				FireBall fb = go.GetComponent<FireBall> ();
				if (fb != null) {
					fb.SetBreath (this);
					fb.Shoot (m_direction);
					fb.m_speed = m_dragonMotion.speed + m_speedBall;
				}
			}
		}
	}

	override protected void EndFury() 
	{
		base.EndFury();
	}



	void OnTriggerEnter(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			if ( m_isFuryOn )
			{
				m_isFuryPaused = true;
				m_animator.SetBool("breath", false);
			}
		}
	}

	void OnTriggerExit(Collider _other)
	{
		if ( _other.CompareTag("Water") )
		{
			m_isFuryPaused = false;
		}
	}
}
