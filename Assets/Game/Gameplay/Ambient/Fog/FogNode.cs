using UnityEngine;
using System.Collections;

public class FogNode : MonoBehaviour 
{
	public Color m_fogColor = Color.white;
	public float m_fogStart = 0;
	public float m_fogEnd = 100;

	private bool m_used = false;
	public bool used
	{
		get{ return m_used; }
		set { m_used = value; }
	}


	public void CustomGuizmoDraw()
	{
		if ( m_used )
		{
			Gizmos.color = Color.white;
			Gizmos.DrawSphere( transform.position, 1.25f);	
		}
		Gizmos.color = m_fogColor;
		Gizmos.DrawSphere( transform.position, 1);
	}
}
