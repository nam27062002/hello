using UnityEngine;
using System.Collections;

public class AmbientNode : MonoBehaviour 
{
	public Color m_ambientColor = Color.white;

	[Range (0,8)]
	public float m_ambientIntensity;

	[Range (0,1)]
	public float m_sunSize;

	[Range (0,5)]
	public float m_atmosphereThickness;

	public Color m_skyTint = Color.white;
	public Color m_ground = Color.white;

	[Range (0,8)]
	public float m_exposure;

	public Color m_fogColor = Color.white;

	public float m_fogStart = 140;
	public float m_fogEnd = 750;

	[Range (0,2)]
	public float m_flaresIntensity = 1;

	public int m_rainIntensity;

	void OnDrawGizmos() 
	{
		Gizmos.color = new Color(0.09f, 0.69f, 0.12f, 0.5f);
		Gizmos.DrawSphere(transform.position, 0.5f * transform.localScale.x);

		Gizmos.color = new Color(0.09f, 0.69f, 0.12f, 0.5f);
		Gizmos.DrawRay( transform.position, transform.forward);
	}

}
