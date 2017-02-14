﻿using UnityEngine;
using System.Collections;

public class FogArea : MonoBehaviour
{
	public Color m_fogColor = Color.white;
	public float m_fogStart = 0;
	public float m_fogEnd = 100;
	public float m_fogRamp = 1;
	public float m_insideScale = 1.5f;
	FogManager m_fogManager;

	void Awake()
	{
		m_fogManager = FindObjectOfType<FogManager>();
	}

	void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			m_fogManager.RegisterFog( this );
			transform.localScale = Vector3.one * m_insideScale;
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			m_fogManager.UnregisterFog( this );
			transform.localScale = Vector3.one * 1;
		}
	}


	void OnDrawGizmosSelected()
	{
		Shader.SetGlobalFloat("_FogStart", m_fogStart);
		Shader.SetGlobalFloat("_FogEnd", m_fogEnd);
		Shader.SetGlobalColor("_FogColor", m_fogColor);
		Shader.SetGlobalFloat("_FogRampY", m_fogRamp);
	}
}
