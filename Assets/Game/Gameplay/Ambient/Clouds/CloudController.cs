using UnityEngine;
using System.Collections;

public class CloudController : MonoBehaviour 
{

	public Transform m_start;
	public Transform m_end;

	public float m_closeSpeed;
	public float m_closeSpeedZ = 0;

	public float m_farSpeed = 1;
	public float m_farSpeedZ = 100;

	Renderer[] m_renderers;

	// Use this for initialization
	void Start () 
	{
		m_renderers = GetComponentsInChildren<Renderer>();

	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		for( int i = 0; i<m_renderers.Length; i++ )
		{
			// Movement
			Renderer r = m_renderers[i];
			Vector3 pos = r.transform.position;

			float delta = ( pos.z - m_closeSpeedZ)/(m_farSpeedZ-m_closeSpeedZ);
			float speed = m_closeSpeed + ( m_farSpeed - m_closeSpeed ) * delta;

			pos.x -= speed * Time.deltaTime;

			if ( pos.x < m_end.position.x )
				pos.x = m_start.position.x;

			r.transform.position = pos;
			



		}
	}
}
