using UnityEngine;
using System.Collections;

public class FrameColoring : MonoBehaviour 
{
	
	public Color m_fireColor = Color.black;
	public Color m_superFireColor = Color.black;
	public Color m_starvingColor = Color.black;

	private float m_value = 0.5f;
	private Color m_color;
	public Material m_material = null;

	private bool m_furyOn = false;
	DragonBreathBehaviour.Type m_furyType = DragonBreathBehaviour.Type.None;
	private bool m_starvingOn = false;
	private bool m_criticalOn = false;
	private bool m_ko = false;

	void Awake()
	{
		m_material = new Material( m_material );
		m_value = 0;
		m_color = Color.black;
		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
		Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnRevive);
	}

	private void OnDestroy() 
	{
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFury);
		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
		Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnKo);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnRevive);
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
				case DragonBreathBehaviour.Type.Mega:
				{
					m_value = Mathf.Lerp( m_value, 0.69f, Time.deltaTime * 15);
					m_color = Color.Lerp( m_color, m_superFireColor, Time.deltaTime * 15 );
				}break;
			}

		}

		else if ( m_criticalOn )
		{
			m_value = Mathf.Lerp( m_value, 0.7f + Mathf.Sin( Time.time * 5 ) * 0.2f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_starvingColor, Time.deltaTime * 10);
		}
		else if ( m_starvingOn )
		{
			m_value = Mathf.Lerp( m_value, 0.15f + Mathf.Sin( Time.time * 2.5f ) * 0.1f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_starvingColor, Time.deltaTime * 5);
		}
		else if ( m_ko )
		{
			m_value = Mathf.Lerp( m_value, 0.9f + Mathf.Sin( Time.time * 2.5f ) * 0.05f, Time.deltaTime * 10);
			m_color = Color.Lerp( m_color, m_starvingColor, Time.deltaTime * 2.5f);
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

	private void OnHealthModifierChanged( DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier )
	{
		m_starvingOn = (_newModifier != null && _newModifier.IsStarving());
		m_criticalOn = (_newModifier != null && _newModifier.IsCritical());
	}

	private void OnKo(DamageType _type, Transform _source)
	{
		m_ko = true;
	}

	private void OnRevive( DragonPlayer.ReviveReason reason )
	{
		m_ko = false;	
	}
}
