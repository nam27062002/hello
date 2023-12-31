﻿using UnityEngine;
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

	public Color m_skyColor = Color.white;
	public Color m_horizonColor = Color.white;
	[Range(0,1)]
	public float m_horizonHeight;
	public Color m_groundColor = Color.white;

	[Range (0,2)]
	public float m_flaresIntensity = 1;

	public int m_rainIntensity;

	// public bool m_useLightDir = false;

	private bool m_isUsed = false;

	public delegate void OnEnter( AmbientNode node );
	public OnEnter m_onEnter;

	void OnDrawGizmos() 
	{
		if ( !m_isUsed )
			Gizmos.color = new Color(1.0f, 0, 1, 1);
		else
			Gizmos.color = new Color(1.0f, 1.0f, 0.12f, 1);
		Gizmos.DrawSphere(transform.position, 0.5f * transform.localScale.x);

		Gizmos.color = new Color(0.09f, 0.69f, 0.12f, 0.5f);
		Gizmos.DrawRay( transform.position, transform.forward);
	}

	public void SetIsUsed(bool isUsed)
	{
		m_isUsed = isUsed;
	}

	// Version 2 - WIP
	/*
	void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			// Tell Ambient Manager to use this one
			if (m_onEnter != null)
				m_onEnter(this);
		}
	}
	*/

}
