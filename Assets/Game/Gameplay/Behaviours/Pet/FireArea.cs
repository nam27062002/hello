using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class FireArea : MonoBehaviour {

	private CircleArea2D m_circle;
	private Rect m_rect;
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;
	public DragonTier m_tier = DragonTier.TIER_4;

	private float m_checkNodeFireTime = 0.25f;
	private float m_fireNodeTimer = 0;

	// Use this for initialization
	void Start () {
		m_circle = GetComponent<CircleArea2D>();
		m_rect = new Rect();
	}
	
	// Update is called once per frame
	void Update () {

		if ( InstanceManager.player.breathBehaviour.IsFuryOn() )
		{
			m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2)m_circle.center, m_circle.radius, m_checkEntities);
			for (int i = 0; i < m_numCheckEntities; i++) 
			{
				Entity prey = m_checkEntities[i];
				if ( prey.IsBurnable() && (prey.IsBurnable(m_tier) || InstanceManager.player.breathBehaviour.type == DragonBreathBehaviour.Type.Super))
				{
					AI.MachineOld machine =  prey.GetComponent<AI.MachineOld>();
					if (machine != null) {
						machine.Burn(transform);
					}
				}
			}


			m_fireNodeTimer -= Time.deltaTime;
			if (m_fireNodeTimer <= 0) {
				
				m_fireNodeTimer += m_checkNodeFireTime;

				// Update rect
				m_rect.center = m_circle.center;
				m_rect.height = m_rect.width = m_circle.radius;

				FirePropagationManager.instance.FireUpNodes( m_rect, Overlaps, m_tier, Vector3.zero);
			}
		}
	}

	bool Overlaps( CircleAreaBounds _fireNodeBounds )
	{
		return m_circle.Overlaps( _fireNodeBounds.center, _fireNodeBounds.radius);
	}
}
