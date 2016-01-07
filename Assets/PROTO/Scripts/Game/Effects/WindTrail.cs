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
	Color m_color;
	bool m_needsRestart;

	void Awake()
	{
		m_needsRestart = true;
	}
	// Use this for initialization
	void CustomStart () 
	{
		m_trailRenderer = GetComponent<TrailRenderer>();
		m_trailRenderer.Clear();
		m_position = transform.position;
		m_initialY = m_position.y;
		m_moveSpeed = Random.Range( 2.0f, 4.0f);
		m_trailRenderer.time = m_moveSpeed;
		m_duration = Random.Range( 1.5f, 15.0f );
		m_time = 0;

		m_numBumps = Random.Range(0, 1.0f);
		m_bumpsSize = Random.Range( 0.01f, 0.5f);

		m_color = Color.white;
		UpdateAlpha(0);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( m_needsRestart )
		{
			CustomStart();
			m_needsRestart = false;
		}

		m_time += Time.deltaTime;
		float delta = m_time / m_duration;
		UpdateAlpha( delta );

		if ( m_time <= m_duration )
		{
			m_position.x += Time.deltaTime * m_moveSpeed;
			m_position.y = m_initialY + ( Mathf.Sin( m_time * Mathf.PI * m_numBumps ) * m_bumpsSize );
			transform.position = m_position;
		}
		else
		{
			m_needsRestart = true;
			gameObject.SetActive(false);
		}
	}

	void UpdateAlpha( float delta )
	{
		m_color.a = Mathf.Sin(delta * Mathf.PI);
		m_trailRenderer.material.SetColor("_TintColor", m_color);
	}
}
