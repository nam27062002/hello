using UnityEngine;
using System.Collections;

public class FollowTransform : MonoBehaviour 
{
	public Vector3 m_offset;
	public Transform m_follow;
	private Transform m_transform;

	void Start()
	{
		m_transform = transform;
	}

	// Update is called once per frame
	void LateUpdate () 
	{
		if (m_follow != null)
			m_transform.position = m_follow.position + m_offset;
	}
}
