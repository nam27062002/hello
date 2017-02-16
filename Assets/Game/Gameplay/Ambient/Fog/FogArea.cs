﻿using UnityEngine;
using System.Collections;

public class FogArea : MonoBehaviour
{
	
	public float m_insideScale = 1.5f;
	FogManager m_fogManager;
	public FogManager.FogAttributes m_attributes;
	public bool m_drawInside = false;
	public Vector3 m_startScale;
	void Start()
	{
		m_fogManager = FindObjectOfType<FogManager>();
		m_startScale = transform.localScale;
		if ( UnityEngine.Debug.isDebugBuild )
		{
			m_fogManager.CheckTextureAvailability(m_attributes);
		}
	}

	void OnDestroy()
	{
		if ( m_attributes.texture != null )
			m_attributes.DestroyTexture();
	}

	void OnTriggerEnter( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			m_fogManager.ActivateArea( this );
			transform.localScale = m_startScale * m_insideScale;
		}
	}

	void OnTriggerExit( Collider other)
	{
		if ( other.CompareTag("Player") )	
		{
			m_fogManager.DeactivateArea( this );
			transform.localScale = m_startScale;
		}
	}

	void OnDrawGizmosSelected()
	{
		if ( m_attributes.texture == null )
			m_attributes.CreateTexture();
		m_attributes.RefreshTexture();

		Shader.SetGlobalFloat("_FogStart", m_attributes.m_fogStart);
		Shader.SetGlobalFloat("_FogEnd", m_attributes.m_fogEnd);
		Shader.SetGlobalTexture("_FogTexture", m_attributes.texture);

		if (m_drawInside)
		{
			Color c = m_attributes.m_fogGradient.Evaluate(1);
			c.a = c.a / 2.0f;
			Gizmos.color = c;
			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if ( meshFilter != null )
			{
				Gizmos.DrawMesh( meshFilter.sharedMesh, transform.position, transform.rotation, transform.localScale );	
			}
		}


		Gizmos.color = Color.white;
		Vector3 _from = transform.position;
		_from.z = m_attributes.m_fogStart;
		Vector3 _to = transform.position;
		_to.z = m_attributes.m_fogEnd;

		Gizmos.DrawLine( _from, _to);
		float crossSize = 2;
		Gizmos.DrawLine( _from + Vector3.up * crossSize, _from + Vector3.down * crossSize );
		Gizmos.DrawLine( _from + Vector3.left * crossSize, _from + Vector3.right * crossSize );

		Gizmos.DrawLine( _to + Vector3.up * crossSize, _to + Vector3.down * crossSize );
		Gizmos.DrawLine( _to + Vector3.left * crossSize, _to + Vector3.right * crossSize );
		// 
	}


}
