using UnityEngine;
using System.Collections;

public class FogSetter : MonoBehaviour 
{
	Material[] m_materials;

	public bool m_onlyOnce;
	public float m_updateInterval = 1;
	float m_timer = 0;
	FogManager m_fogManager;
	// Use this for initialization
	void Start () 
	{
		Renderer rend = GetComponent<Renderer>();
		m_materials = rend.materials;
		m_fogManager = FindObjectOfType<FogManager>();
	}

	void Update()
	{
		m_timer -= Time.deltaTime;
		if ( m_timer <= 0 && m_fogManager.IsReady())
		{
			UpdateFog();
			if ( m_onlyOnce )
			{
				enabled = false;
			}
			else
			{
				m_timer = m_updateInterval;
			}
		}
	}
	
	void UpdateFog()
	{
		FogManager.FogResult res = m_fogManager.GetFog( transform.position);

		for( int i = 0; i<m_materials.Length; i++ )
		{
			Material mat = m_materials[i];

			mat.SetColor("_FogColor", res.m_fogColor);
			mat.SetFloat("_FogStart", res.m_fogStart);
			mat.SetFloat("_FogEnd", res.m_fogEnd);
		}
	}
}
