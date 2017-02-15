using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FogSetter : MonoBehaviour 
{
	

	FogManager m_fogManager;

	float m_start;
	float m_end;
	Color m_color;
	float m_rampY;

	bool m_firstTime;

	FogManager.FogResult m_result;

	// Use this for initialization
	void Start () 
	{
		Init();
	}

	void Update()
	{
		if ( m_fogManager == null )
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
		m_result = new FogManager.FogResult();
	}
	
	void UpdateFog( float delta )
	{
		if (m_fogManager != null && m_fogManager.m_fogMode == FogManager.FogMode.FogNodes)
		{
			m_fogManager.GetFog( transform.position, ref m_result);

			if ( m_firstTime )
			{
				m_firstTime = false;
				m_start = m_result.m_fogStart;
				m_end = m_result.m_fogEnd;
				m_color = m_result.m_fogColor;
				m_rampY = m_result.m_fogRamp;
			}
			else
			{
				m_start = Mathf.Lerp( m_start, m_result.m_fogStart, delta);
				m_end = Mathf.Lerp( m_end, m_result.m_fogEnd, delta);
				m_color = Color.Lerp( m_color, m_result.m_fogColor, delta);
				m_rampY = Mathf.Lerp( m_rampY, m_result.m_fogRamp, delta);
			}

			Shader.SetGlobalFloat("_FogStart", m_start);
			Shader.SetGlobalFloat("_FogEnd", m_end);
			Shader.SetGlobalColor("_FogColor", m_color);
			Shader.SetGlobalFloat("_FogRampY", m_rampY);
		}
	}
}
