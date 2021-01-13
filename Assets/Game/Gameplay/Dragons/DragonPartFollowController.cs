using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DragonPartFollowController : MonoBehaviour {

	DragonPartFollow[] m_parts;
	float[] m_originalSprings;
	DragonMotion m_playerMotion;
	const float TRANSFORM_SPEED = 0.75f;
	int m_size;

	// Use this for initialization
	void Start () {
		m_parts = GetComponentsInChildren<DragonPartFollow>();
		m_originalSprings = new float[ m_parts.Length ];
		m_size = m_parts.Length;
		for( int i = 0; i<m_size; i++ )
		{
			m_originalSprings[i] = m_parts[i].springSpeed;
		}
		m_playerMotion = InstanceManager.player.dragonMotion;
	}

	void Update()
	{
		if ( m_playerMotion.insideWater )
		{
			for( int i = 0; i<m_size; i++ )
			{
				m_parts[i].springSpeed = Mathf.Lerp( m_parts[i].springSpeed, m_originalSprings[i] * 0.5f, Time.deltaTime * TRANSFORM_SPEED) ;
			}
		}
		else
		{
			for( int i = 0; i<m_size; i++ )
			{
				m_parts[i].springSpeed = Mathf.Lerp( m_parts[i].springSpeed, m_originalSprings[i], Time.deltaTime * TRANSFORM_SPEED) ;
			}
		}
	}
}
