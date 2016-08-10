using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FogSetter : MonoBehaviour 
{
	

	FogManager m_fogManager;
	// Use this for initialization
	void Start () 
	{
		Init();
	}

	void Update()
	{
		if ( m_fogManager == null && !Application.isPlaying )
		{
			Init();
		}

		if ( m_fogManager != null && m_fogManager.IsReady())
		{
			UpdateFog();
		}
	}

	void Init()
	{
		m_fogManager = FindObjectOfType<FogManager>();
	}
	
	void UpdateFog()
	{
		if (m_fogManager != null )
		{
			FogManager.FogResult res = m_fogManager.GetFog( transform.position);

			Shader.SetGlobalFloat("_FogStart", res.m_fogStart);
			Shader.SetGlobalFloat("_FogEnd", res.m_fogEnd);
			Shader.SetGlobalColor("_FogColor", res.m_fogColor);
		}
	}
}
