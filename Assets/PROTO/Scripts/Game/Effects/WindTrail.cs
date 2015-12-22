using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TrailRenderer))]
public class WindTrail : MonoBehaviour {

	TrailRenderer m_trailRenderer;
	Vector3 m_position;
	float m_initialY;
	float m_moveSpeed;
	float m_duration;
	float m_time;
	float m_numBumps;
	float m_bumpsSize;

	// Use this for initialization
	void Start () 
	{
		m_trailRenderer = GetComponent<TrailRenderer>();
		m_position = transform.position;
		m_initialY = m_position.y;
		m_moveSpeed = Random.Range( 2.0f, 4.0f);
		m_trailRenderer.time = m_moveSpeed;
		m_duration = Random.Range( 0.5f, 3.0f );
		m_time = 0;

		m_numBumps = Random.Range(0, 1.0f);
		m_bumpsSize = Random.Range( 0.01f, 0.5f);

	}
	
	// Update is called once per frame
	void Update () 
	{
		m_time += Time.deltaTime;
		float delta = m_time / m_duration;
		// if ( m_time > 0 )
		{
			m_position.x += Time.deltaTime * m_moveSpeed;
			m_position.y = m_initialY + ( Mathf.Sin( m_time * Mathf.PI * m_numBumps ) * m_bumpsSize );
			transform.position = m_position;
		}

	}
}
