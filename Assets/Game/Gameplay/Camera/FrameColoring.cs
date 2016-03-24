using UnityEngine;
using System.Collections;

public class FrameColoring : MonoBehaviour 
{
	
	public Color m_fireColor = Color.black;
	public Color m_startingColor = Color.black;

	private float m_value = 0.5f;
	private Color m_color;
	public Material m_material;

	private bool m_furyOn = false;
	private bool m_stargingOn = false;

	void Start()
	{
		m_value = 0;
		m_color = Color.black;
		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.AddListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarving);
	}

	private void OnDestroy() 
	{
		Messenger.RemoveListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.RemoveListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarving);
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
		if (m_furyOn)
		{
			m_value = Mathf.Lerp( m_value, 0.69f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_fireColor, Time.deltaTime * 10 );
		}
		else if ( m_stargingOn )
		{
			m_value = Mathf.Lerp( m_value, 0.7f + Mathf.Sin( Time.time * 5 ) * 0.2f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_startingColor, Time.deltaTime * 10);
		}
		else
		{
			m_value = Mathf.Lerp( m_value, 0, Time.deltaTime);
			m_color = Color.Lerp( m_color, Color.black, Time.deltaTime);
		}
		if ( m_value <= 0.1f )
		{
			Graphics.Blit (source, destination);
		}
		else
		{
	    	m_material.SetColor("_Color", m_color);
			m_material.SetFloat("_Intensity", m_value);
			Graphics.Blit (source, destination, m_material);
		}
    }

	private void OnFury(bool _enabled) {
		m_furyOn = _enabled;
	}

	private void OnStarving( bool _enabled )
	{
		m_stargingOn = _enabled;
	}
}
