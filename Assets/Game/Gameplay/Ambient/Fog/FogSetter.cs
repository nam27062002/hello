using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FogSetter : MonoBehaviour 
{
	

	FogManager m_fogManager;

	float m_start;
	float m_end;
	Color m_color;

	bool m_firstTime;

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
			if ( Application.isPlaying )
				UpdateFog(Time.deltaTime);
			else
				UpdateFog(1);
		}
	}

	void Init()
	{
		m_fogManager = FindObjectOfType<FogManager>();
		m_firstTime = true;
	}
	
	void UpdateFog( float delta )
	{
		if (m_fogManager != null )
		{
			FogManager.FogResult res = m_fogManager.GetFog( transform.position);

			if ( m_firstTime )
			{
				m_firstTime = false;
				m_start = res.m_fogStart;
				m_end = res.m_fogEnd;
				m_color = res.m_fogColor;
			}
			else
			{
				m_start = Mathf.Lerp( m_start, res.m_fogStart, delta);
				m_end = Mathf.Lerp( m_end, res.m_fogEnd, delta);
				m_color = Color.Lerp( m_color, res.m_fogColor, delta);
			}

			Shader.SetGlobalFloat("_FogStart", m_start);
			Shader.SetGlobalFloat("_FogEnd", m_end);
			Shader.SetGlobalColor("_FogColor", m_color);
		}
	}
}
