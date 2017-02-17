using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FogArea))]	// True to be used by heir classes as well
public class FogAreaEditor : Editor {

	FogArea m_target = null;

	private void OnEnable() {
		if ( !Application.isPlaying )
		{
			m_target = target as FogArea;
			if (m_target.m_attributes.texture == null)
				m_target.m_attributes.CreateTexture();
		}
	}

	private void OnDisable() {
		if ( !Application.isPlaying )
		{
			m_target.m_attributes.DestroyTexture(true);
			m_target = null;

			FogManager manager = FindObjectOfType<FogManager>();
			if ( manager != null )
			{
				Shader.SetGlobalFloat("_FogStart", manager.m_defaultAreaFog.m_fogStart);
				Shader.SetGlobalFloat("_FogEnd", manager.m_defaultAreaFog.m_fogEnd);
				if ( manager.m_defaultAreaFog.texture == null )
				{
					manager.m_defaultAreaFog.CreateTexture();
					manager.m_defaultAreaFog.RefreshTexture();
				}
				Shader.SetGlobalTexture("_FogTexture", manager.m_defaultAreaFog.texture);
			}
		}
	}

}
