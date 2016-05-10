using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonTint : MonoBehaviour 
{
	DragonBreathBehaviour m_breath;

	DragonPlayer m_player;
	DragonHealthBehaviour m_health;

	Renderer[] m_dragonRenderers = null;
	List<Material> m_materials = new List<Material>();

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
	float m_shieldValue = 0;

	// Use this for initialization
	void Start () 
	{
		m_breath = GetComponent<DragonBreathBehaviour>();
		m_player = GetComponent<DragonPlayer>();
		m_health = GetComponent<DragonHealthBehaviour>();
		Transform t = transform.FindChild("view");
		if ( t != null )
		{
			m_dragonRenderers = t.GetComponentsInChildren<Renderer>();
			GetMaterials();
		}
	}

	void GetMaterials()
	{
		m_materials.Clear();
		if ( m_dragonRenderers != null )
		for( int i = 0; i<m_dragonRenderers.Length; i++ )
		{
			Material[] mats = m_dragonRenderers[i].materials;
			for( int j = 0;j<mats.Length; j++ )
			{
				string shaderName = mats[j].shader.name;
				if ( shaderName.Contains("Wings") || shaderName.Contains("Dragon") )
					m_materials.Add( mats[j] );
			}
		}
	}

	void OnEnable() 
	{
		Messenger.AddListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	void OnDisable() 
	{
		// Unsubscribe from external events
		Messenger.RemoveListener<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, OnDamageReceived);
	}

	private void OnDamageReceived(float _amount, Transform _source) 
	{
		m_damageTimer = m_damageTotalTime;
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		// Color multiply
		SetColorMultiply(m_caveColor);

		// Color add
		m_damageTimer -= Time.deltaTime;
		if ( m_damageTimer < 0 )
			m_damageTimer = 0;
		Color damageColor = m_damageColor * (m_damageTimer / m_damageTotalTime);


		// Other color
		Color otherColor = Color.black;
		if ( m_health.IsCursed() )
		{
			m_otherColorTimer += Time.deltaTime * 5;
			otherColor = m_curseColor * (Mathf.Sin( m_otherColorTimer) + 1) * 0.5f;
		}
		else if ( m_player.IsStarving() )
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

	}

	void SetColorMultiply( Color c )
	{
		for( int i = 0; i<m_materials.Count; i++ )	
			m_materials[i].SetColor("_ColorMultiply", c );
	}

	void SetColorAdd( Color c)
	{
		for( int i = 0; i<m_materials.Count; i++ )	
			m_materials[i].SetColor("_ColorAdd", c );
	}

	void SetInnerLightAdd( float innerValue )
	{
		for( int i = 0; i<m_materials.Count; i++ )	
			m_materials[i].SetFloat("_InnerLightAdd", innerValue );
	}

	public void SetCaveColor( Color c )
	{
		m_caveColor = c;
	}


}
