using UnityEngine;
using System.Collections;

public class DragonTint : MonoBehaviour 
{
	DragonBreathBehaviour m_breath;

	DragonPlayer m_player;

	SkinnedMeshRenderer m_dragonRenderer;

	// Starving
	float m_starvingTimer = 0;

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
		m_dragonRenderer = transform.GetFirstComponentInChildren<SkinnedMeshRenderer>();
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
		m_dragonRenderer.material.SetColor("_ColorMultiply", m_caveColor );


		// Color add
		m_damageTimer -= Time.deltaTime;
		if ( m_damageTimer < 0 )
			m_damageTimer = 0;
		Color damageColor = m_damageColor * (m_damageTimer / m_damageTotalTime);

		Color starveColor = Color.black;
		if ( m_player.IsStarving() )
		{
			m_starvingTimer += Time.deltaTime * 5;
			starveColor = m_damageColor * (Mathf.Sin( m_starvingTimer) + 1) * 0.5f;
		}
		else
		{
			m_starvingTimer = 0;
		}

		m_dragonRenderer.material.SetColor("_ColorAdd",  damageColor + starveColor );


		// Inner light
		float innerValue = 0;
		if ( m_breath.IsFuryOn() )
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
		m_dragonRenderer.material.SetFloat("_InnerLightAdd", innerValue );

		// Shield
		if (m_player.HasMineShield())
		{
			m_shieldValue = Mathf.Lerp( m_shieldValue, 1, Time.deltaTime);
		}
		else
		{
			m_shieldValue = Mathf.Lerp( m_shieldValue, 0, Time.deltaTime);
		}
		m_dragonRenderer.material.SetFloat("_NoiseValue", m_shieldValue );

	}

	public void SetCaveColor( Color c )
	{
		m_caveColor = c;
	}


}
