using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollowTransform : MonoBehaviour {

	public Vector3 m_offset;
	public Transform m_follow;
	private Transform m_transform;
	public float m_speed = 5.0f;

	void Start()
	{
		m_transform = transform;
	}

	// Update is called once per frame
	void LateUpdate () 
	{
		if (m_follow != null)
		{
			Vector3 p = Vector3.Lerp( m_transform.position, m_follow.position + m_offset, Time.deltaTime * m_speed );
			m_transform.position = p;
		}
	}

	// Also called DampIIR (wiki search ...)
	float Damping(float src, float dst, float dt, float factor)
	{
	    return (((src * factor) + (dst * dt)) / (factor + dt));
	}

}
