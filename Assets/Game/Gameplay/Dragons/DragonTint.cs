using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonTint : MonoBehaviour 
{
	DragonBreathBehaviour m_breath;

	DragonPlayer m_player;
	DragonHealthBehaviour m_health;
	DragonMotion m_motion;

	Renderer[] m_renderers = null;
	List<Renderer> m_dragonRenderers = new List<Renderer>();

	int m_materialsCount = 0;
	List<Material> m_materials = new List<Material>();
    List<Shader> m_originalShaders = new List<Shader>();
	List<Color> m_fresnelColors = new List<Color>();
	List<Color> m_innerLightColors = new List<Color>();
	float m_innerLightColorValue = 1;

	float m_otherColorTimer = 0;

	// Cursed
	public Color m_curseColor = Color.green;

	// Cave
	Color m_caveColor = Color.white;

	// Damage
	public Color m_damageColor = Color.red;
	float m_damageTimer = 0;
	float m_damageTotalTime = 0.5f;

	// Fury Timer
	float m_furyTimer = 0;

	// Shield
	// float m_shieldValue = 0;

	float m_deathAlpha = 1;

	private bool m_starvingOn = false;
	private bool m_criticalOn = false;

	public bool m_reduceInnerColorInsideWater = false;

	// Use this for initialization
	IEnumerator Start () 
	{
		m_breath = GetComponent<DragonBreathBehaviour>();
		m_player = GetComponent<DragonPlayer>();
		m_health = GetComponent<DragonHealthBehaviour>();
		m_motion = m_player.dragonMotion;
		yield return null;
		Transform t = transform.FindChild("view");
		if ( t != null )
		{
			m_renderers = t.GetComponentsInChildren<Renderer>();
			GetMaterials();
		}
	}

	void GetMaterials()
	{
		m_materials.Clear();
		if ( m_renderers != null )
		for( int i = 0; i<m_renderers.Length; i++ )
		{
			Material[] mats = m_renderers[i].materials;
			bool hasDragonPart = false;
			for( int j = 0;j<mats.Length; j++ )
			{
				
				string shaderName = mats[j].shader.name;
                if (shaderName.Contains("Dragon/Wings") || shaderName.Contains("Dragon/Body"))
                {
                	hasDragonPart = true;
                    m_materials.Add(mats[j]);
                    m_fresnelColors.Add(mats[j].GetColor("_FresnelColor"));
					m_innerLightColors.Add(mats[j].GetColor("_InnerLightColor"));
                    m_originalShaders.Add(mats[j].shader);
//					if (shaderName.Contains("Body"))
//						m_bodyMaterials.Add( mats[j] );
//                  }
                }
			}
			if ( hasDragonPart )
			{
				m_dragonRenderers.Add( m_renderers[i] );
			}
				
		}
		m_materialsCount = m_materials.Count;
	}

	void OnEnable() 
	{
		Messenger.AddListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
		Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
	}

	void OnDisable() 
	{
		// Unsubscribe from external events
		Messenger.RemoveListener<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
		Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
	}

	private void OnDamageReceived(float _amount, DamageType _type, Transform _source) 
	{
		if ( _type != DamageType.LATCH && _type != DamageType.POISON)
			m_damageTimer = m_damageTotalTime;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		Color multiplyColor = m_caveColor;
		// Color multiply
		if ( !m_health.IsAlive() )
		{
			// To alpha
			m_deathAlpha -= Time.deltaTime * 1.0f/Time.timeScale * 0.5f;
		}
		else
		{
			// To opaque
			m_deathAlpha += Time.deltaTime;
		}
		m_deathAlpha = Mathf.Clamp01(m_deathAlpha);
		multiplyColor.a = m_deathAlpha;
		SetColorMultiply(multiplyColor);
		SetFresnelAlpha( m_deathAlpha );

		// Color add
		m_damageTimer -= Time.deltaTime;
		if ( m_damageTimer < 0 )
			m_damageTimer = 0;
		Color damageColor = m_damageColor * (m_damageTimer / m_damageTotalTime);


		// Other color
		Color otherColor = Color.black;
		if ( m_health.HasDOT() )
		{
			m_otherColorTimer += Time.deltaTime * 5;
			otherColor = m_curseColor * (Mathf.Sin( m_otherColorTimer) + 1) * 0.5f;
		}
		else if (m_starvingOn || m_criticalOn || m_player.BeingLatchedOn())
		{
			m_otherColorTimer += Time.deltaTime * 5;
			otherColor = m_damageColor * (Mathf.Sin( m_otherColorTimer) + 1) * 0.5f;
		}
		else
		{
			m_otherColorTimer = 0;
		}
		SetColorAdd(damageColor + otherColor);


		// Inner light
		float innerValue = 0;
		if ( m_breath.IsFuryOn())
		{
			// animate fury color and inner light
			m_furyTimer += Time.deltaTime;

			innerValue = (Mathf.Sin( m_furyTimer * 2 ) * 0.5f) + 0.5f;
			innerValue *= 4;
		}
		else
		{
			m_furyTimer = 0;
		}
		SetInnerLightAdd( innerValue );

		// Shield
		/*
		if (m_player.HasMineShield())
		{
			m_shieldValue = Mathf.Lerp( m_shieldValue, 1, Time.deltaTime);
		}
		else
		{
			m_shieldValue = Mathf.Lerp( m_shieldValue, 0, Time.deltaTime);
		}
		m_dragonRenderer.material.SetFloat("_NoiseValue", m_shieldValue );
		*/

		if (m_reduceInnerColorInsideWater)
		{
			float value = m_innerLightColorValue;
			if (m_motion.insideWater)
			{
				value -= Time.deltaTime;
				if ( value < 0.25f )
					value = 0.25f;
			}
			else
			{
				value += Time.deltaTime;
				if (value > 1 )
					value = 1;
			}
			if ( value != m_innerLightColorValue )
			{
				m_innerLightColorValue = value;
				SetInnerLightColorValue(m_innerLightColorValue);
			}
		}

	}

	void SetColorMultiply( Color c )
	{
		for( int i = 0; i<m_materialsCount; ++i )	
			m_materials[i].SetColor("_Tint", c );
	}

	void SetFresnelAlpha( float alpha )
	{
		for( int i = 0; i<m_materialsCount; ++i )	
		{
			Color c = m_fresnelColors[i];
			c.a = alpha;
			m_fresnelColors[i] = c;
			m_materials[i].SetColor("_FresnelColor", c );
		}
	}

	void SetColorAdd( Color c)
	{
		c.a = 0;
		for( int i = 0; i<m_materialsCount; ++i )	
			m_materials[i].SetColor("_ColorAdd", c );
	}

	void SetInnerLightAdd( float innerValue )
	{
		for( int i = 0; i<m_materialsCount; ++i )	
			m_materials[i].SetFloat("_InnerLightAdd", innerValue );
	}

	void SetInnerLightColorValue( float value )
	{
		for( int i = 0; i<m_materialsCount; ++i )	
		{
			Color c = m_innerLightColors[i] * value;
			m_materials[i].SetColor("_InnerLightColor", c );
		}
	}

	public void SetCaveColor( Color c )
	{
		m_caveColor = c;
	}

	private void OnPlayerKo( DamageType _type, Transform _source )
	{
        // Switch body material to wings
		for (int i = 0; i < m_materialsCount; ++i) 
            m_materials[i].shader = Shader.Find("Hungry Dragon/Dragon/Death");

     	if ( _type == DamageType.MINE )
     	{
     		// Shows corpse
     		m_deathAlpha = 0;
			for( int i = 0; i<m_dragonRenderers.Count; i++ )
			{
				m_dragonRenderers[i].enabled = false;
			}
     	}
    }

	private void OnPlayerRevive( DragonPlayer.ReviveReason reason )
	{
		// Switch back body materials
		for( int i = 0; i< m_materialsCount; ++i )
            m_materials[i].shader = m_originalShaders[i];

		m_deathAlpha = 1;
		for( int i = 0; i<m_dragonRenderers.Count; i++ )
		{
			m_dragonRenderers[i].enabled = true;
		}
    }

	private void OnHealthModifierChanged( DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier )
	{
		m_starvingOn = (_newModifier != null && _newModifier.IsStarving());
		m_criticalOn = (_newModifier != null && _newModifier.IsCritical());
	}
}
