using UnityEngine;
using System.Collections;

public class BurnParticle : MonoBehaviour 
{

	DeltaTimer m_timer;
	float m_size;

	SpriteRenderer m_sprite;

	void Awake()
	{
		m_timer = new DeltaTimer();
		m_sprite = GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		if ( m_timer.Finished() )
		{
			gameObject.SetActive(false);
		}
		else
		{
			transform.localScale = Vector3.one * m_timer.GetDelta(CustomEase.EaseType.circOut_01) * m_size;
			// transform.localScale = Vector3.one;
			Color c = Color.white;
			c.a = m_timer.GetDelta(CustomEase.EaseType.sinPi2Pi_10);
			m_sprite.color = c;

		}
	}
	
	public void Activate( float size, float time)
	{
		m_timer.Start( time );
		m_size = size;
		transform.localScale = Vector3.zero;
		gameObject.SetActive(true);
	}

}
