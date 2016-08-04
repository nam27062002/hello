using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FogSetter : MonoBehaviour 
{
	Material[] m_materials;

	public bool m_onlyOnce = true;
	public bool m_onEnable = false;
	public float m_updateInterval = 1;
	float m_timer = 0;
	FogManager m_fogManager;
	// Use this for initialization
	void Start () 
	{
		Init();
	}

	void OnEnable()
	{
		if ( m_onEnable )
		{
			UpdateFog();
			if ( m_onlyOnce )
				enabled = false;
		}
	}

	void Update()
	{
		m_timer -= Time.deltaTime;
		if ( m_fogManager == null && !Application.isPlaying )
		{
			Init();
		}

		if ( m_timer <= 0 && m_fogManager != null && m_fogManager.IsReady())
		{
			UpdateFog();
			if ( Application.isPlaying )
			{
				if ( m_onlyOnce)
				{
					enabled = false;
				}
				else
				{
					m_timer = m_updateInterval;
				}
			}
		}
	}

	void Init()
	{
		Renderer rend = GetComponent<Renderer>();
		if ( Application.isPlaying )
			m_materials = rend.materials;
		else
			m_materials = rend.sharedMaterials;
		m_fogManager = FindObjectOfType<FogManager>();
	}
	
	void UpdateFog()
	{
		if (m_fogManager != null )
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
}
