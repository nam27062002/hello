using UnityEngine;
using System.Collections;

public class FrameColoring : MonoBehaviour 
{
	
	public Color m_fireColor = Color.black;
	public Color m_superFireColor = Color.black;
	public Color m_startingColor = Color.black;

	private float m_value = 0.5f;
	private Color m_color;
	public Material m_material;

	private bool m_furyOn = false;
	DragonBreathBehaviour.Type m_furyType = DragonBreathBehaviour.Type.None;
	private bool m_starvingOn = false;
	private bool m_criticaOn = false;

	void Start()
	{
		m_value = 0;
		m_color = Color.black;
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.AddListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarving);
		Messenger.AddListener<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, OnCritical);
	}

	private void OnDestroy() 
	{
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.RemoveListener<bool>(GameEvents.PLAYER_STARVING_TOGGLED, OnStarving);
		Messenger.RemoveListener<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, OnCritical);
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
    	if (m_furyOn)
		{
			switch( m_furyType )
			{
				case DragonBreathBehaviour.Type.Standard:
				{
					m_value = Mathf.Lerp( m_value, 0.69f, Time.deltaTime * 10);
					m_color = Color.Lerp( m_color, m_fireColor, Time.deltaTime * 10 );
				}break;
				case DragonBreathBehaviour.Type.Super:
				{
					m_value = Mathf.Lerp( m_value, 0.69f, Time.deltaTime * 15);
					m_color = Color.Lerp( m_color, m_superFireColor, Time.deltaTime * 15 );
				}break;
			}

		}
		else if ( m_criticaOn )
		{
			m_value = Mathf.Lerp( m_value, 0.7f + Mathf.Sin( Time.time * 5 ) * 0.2f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_startingColor, Time.deltaTime * 10);
		}
		else if ( m_starvingOn )
		{
			m_value = Mathf.Lerp( m_value, 0.15f + Mathf.Sin( Time.time * 2.5f ) * 0.1f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_startingColor, Time.deltaTime * 5);
		}
		else
		{
			m_value = Mathf.Lerp( m_value, 0, Time.deltaTime);
			m_color = Color.Lerp( m_color, Color.black, Time.deltaTime);
		}
		if ( m_value <= 0.04f )
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

	private void OnFury(bool _enabled, DragonBreathBehaviour.Type _type) {
		m_furyOn = _enabled;
		m_furyType = _type;
	}

	private void OnStarving( bool _enabled )
	{
		m_starvingOn = _enabled;
	}

	private void OnCritical( bool _isCritical ) 
	{
		m_criticaOn = _isCritical;
	}
}
