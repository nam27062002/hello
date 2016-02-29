using UnityEngine;
using System.Collections;

public class FrameColoring : MonoBehaviour 
{
	
	public Color m_color = Color.black;
	private float m_value = 0.5f;
	public Material m_material;

	private bool m_furyOn = false;

	void Start()
	{
		m_value = 0;
		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
	}

	private void OnDestroy() 
	{
		Messenger.RemoveListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
		if (m_furyOn)
		{
			m_value = Mathf.Lerp( m_value, 0.69f, Time.deltaTime);
		}
		else
		{
			m_value = Mathf.Lerp( m_value, 0, Time.deltaTime);
		}
    	m_material.SetColor("_Color", m_color);
		m_material.SetFloat("_Intensity", m_value);
		Graphics.Blit (source, destination, m_material);
    }

	private void OnFury(bool _enabled) {
		m_furyOn = _enabled;
	}
}
