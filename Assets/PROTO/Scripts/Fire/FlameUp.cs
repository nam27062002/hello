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
	float m_posTimer;
	float m_duration;

	float m_startScale;
	float m_endScale;

	float m_distance;

	Vector3 m_startPosition;
	public Vector3 m_moveDir = Vector3.up;

	Transform m_referenceTransform;
	Vector3 m_referencePosition;

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
				float speedMultiplier = 1;
				float sqMagnitude = (m_referenceTransform.position - m_referencePosition).sqrMagnitude;
				if (  sqMagnitude > 4 )
				{
					speedMultiplier = 1 + (sqMagnitude - 4.0f);
				}

				m_timer -= Time.deltaTime * speedMultiplier;
				m_posTimer -= Time.deltaTime;
				if ( m_timer <= 0 )
				{
					state = State.INACTIVE;
					gameObject.SetActive(false);
				}
				else
				{
					// Scale
					float tt = (m_timer / m_duration);


					transform.localScale = Vector3.one * m_startScale * tt;

					// Alpha
					Color c = Color.white * tt;
					// c.a = tt;
					for( int i = 0; i <m_renderers.Length; i++ )
					{
						m_renderers[i].color = c;
					}

					// Pos
					float delta = 1.0f - (m_posTimer / m_duration);
					transform.position = m_startPosition + m_moveDir * m_distance * delta;

				}
			}break;
		}
	}

	public void Activate( float startScale, float endScale, float duration, float distance, Vector3 startPos, Transform referenceTransform)
	{
		m_startScale = startScale;
		m_endScale = endScale;

		m_timer = duration;
		m_posTimer = duration;

		m_duration = duration;

		m_distance = distance;

		m_startPosition = startPos;

		transform.Rotate(0,0, Random.Range(0.0f,360.0f));

		gameObject.SetActive(true);
		state = State.ACTIVE;

		m_referenceTransform = referenceTransform;
		m_referencePosition = m_referenceTransform.position;
	}
	
}
