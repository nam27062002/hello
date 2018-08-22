using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class ElectricBoostArea : MonoBehaviour {

	private CircleArea2D m_circle;
	private Rect m_rect;
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;
	public DragonTier m_tier = DragonTier.TIER_4;
	public IEntity.Type m_type = IEntity.Type.PLAYER;
	public float m_waterMultiplier = 2;
	float m_extraRadius;
	DragonBoostBehaviour m_boost;
	DragonBreathBehaviour m_breath;
	DragonMotion m_motion;
	private float m_originalRadius;
	private bool m_active = false;

	// Use this for initialization
	void Start () {
		m_circle = GetComponent<CircleArea2D>();
		m_originalRadius = m_circle.radius;
		m_rect = new Rect();
		m_boost = InstanceManager.player.dragonBoostBehaviour;
		m_breath = InstanceManager.player.breathBehaviour;
		m_motion = InstanceManager.player.dragonMotion;
		m_extraRadius = 1;
		m_tier = InstanceManager.player.data.tier;

	}
	
	// Update is called once per frame
	void Update () {

		if ( m_boost.IsBoostActive() || m_breath.IsFuryOn())
		{
			if (!m_active)
			{
				m_active = true;
			}
			m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2)m_circle.center, m_circle.radius, m_checkEntities);
			for (int i = 0; i < m_numCheckEntities; i++) 
			{
				Entity prey = m_checkEntities[i];
				if ( prey.IsBurnable() && (prey.IsBurnable(m_tier) || ( m_breath.IsFuryOn() && m_breath.type == DragonBreathBehaviour.Type.Mega )))
				{
					AI.IMachine machine =  prey.machine;
					if (machine != null) {
						machine.Burn(transform, m_type);
						// Launch Lightning!
					}
				}
			}
		}
		else
		{
			if (m_active)
			{
				m_active = false;
			}
		}

		if ( m_motion.IsInsideWater() ){
			m_extraRadius += Time.deltaTime;
		}else{
			m_extraRadius -= Time.deltaTime;
		}
		m_extraRadius = Mathf.Clamp(m_extraRadius, 1, m_waterMultiplier);
		m_circle.radius = m_originalRadius * m_extraRadius;
			
	}
}
