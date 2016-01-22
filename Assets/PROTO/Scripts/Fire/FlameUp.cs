using UnityEngine;
using System.Collections;

public class FlameUp : MonoBehaviour 
{

	SpriteRenderer[] m_renderers;

	enum State{
		INACTIVE,
		ACTIVE
	}
	
	State state = State.INACTIVE;

	float m_timer;
	float m_duration;

	float m_startScale;
	float m_endScale;

	float m_distance;

	Vector3 m_startPosition;
	public Vector3 m_moveDir = Vector3.up;

	void Start()
	{
		m_renderers = GetComponentsInChildren<SpriteRenderer>();
	}

	void LateUpdate()
	{
		switch( state )
		{
			case State.ACTIVE:
			{
				m_timer -= Time.deltaTime;// * (1.0f + distance);
				if ( m_timer <= 0 )
				{
					state = State.INACTIVE;
					gameObject.SetActive(false);
				}
				else
				{
					// Scale
					float tt = (m_timer / m_duration);
					float delta = 1.0f - tt;

					// transform.localScale = Vector3.up * Mathf.Lerp( m_startScale, m_endScale, delta) + Vector3.right * m_startScale * tt;
					transform.localScale = Vector3.one * m_startScale * tt;

					// Alpha
					Color c = Color.white * tt;
					// c.a = tt;
					for( int i = 0; i <m_renderers.Length; i++ )
					{
						m_renderers[i].color = c;
					}

					// Pos
					transform.position = m_startPosition + m_moveDir * m_distance * delta;

				}
			}break;
		}
	}

	public void Activate( float startScale, float endScale, float duration, float distance, Vector3 startPos)
	{
		m_startScale = startScale;
		m_endScale = endScale;

		m_timer = duration;
		m_duration = duration;

		m_distance = distance;

		m_startPosition = startPos;

		transform.Rotate(0,0, Random.Range(0.0f,360.0f));

		gameObject.SetActive(true);
		state = State.ACTIVE;
	}
	
}
