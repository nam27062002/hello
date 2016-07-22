using UnityEngine;
using System.Collections;

public class FogNode : MonoBehaviour 
{
	public Color m_fogColor = Color.white;
	public float m_fogStart = 0;
	public float m_fogEnd = 100;


	public void CustomGuizmoDraw()
	{
		Gizmos.color = m_fogColor;
		Gizmos.DrawSphere( transform.position, 1);
	}
}
