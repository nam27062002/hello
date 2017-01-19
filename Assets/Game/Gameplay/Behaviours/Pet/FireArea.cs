using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleArea2D))]
public class FireArea : MonoBehaviour {

	private CircleArea2D m_circle;
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;
	private DragonTier m_tier;

	// Use this for initialization
	void Start () {
		m_circle = GetComponent<CircleArea2D>();
		m_tier = InstanceManager.player.data.tier;
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
					AI.Machine machine =  prey.GetComponent<AI.Machine>();
					if (machine != null) {
						machine.Burn(transform);
					}
				}
			}
		}
	}
}
