using UnityEngine;
using System.Collections;

public class EntityGroupBehaviour : MonoBehaviour 
{
	//---------------------------------------------------------------
	// Attributes
	//---------------------------------------------------------------
	[SerializeField] private Range m_groupAvoidRadiusRange;
	private float m_groupAvoidRadiusSqr;

	private EntityGroupController m_group;
	private float m_groupAvoidRadius;

	private PreyMotion m_motion;

	void Start () 
	{
		m_motion = GetComponent<PreyMotion>();
	}

	void OnEnable() 
	{
		m_groupAvoidRadius = m_groupAvoidRadiusRange.GetRandom();
		m_groupAvoidRadiusSqr = m_groupAvoidRadius * m_groupAvoidRadius;
	}

	void OnDisable() 
	{
		AttachGroup(null);
	}

	public void AttachGroup(EntityGroupController _group)
	{		
		if (m_group != null) {
			m_group.Remove(gameObject);
		}

		m_group = _group;

		if (m_group != null) {
			m_group.Add(gameObject);
		}
	}

	void Update()
	{
		if ( m_group != null )
		{
			Vector2 avoid = Vector2.zero;
			Vector2 direction = Vector2.zero;

			for (int i = 0; i < m_group.entities.Length; i++) 
			{
				GameObject entity = m_group.entities[i];

				if (entity != null && entity != gameObject) 
				{
					direction = (Vector2)m_motion.position - (Vector2)entity.transform.position;
					float distanceSqr = direction.sqrMagnitude;

					if (distanceSqr < m_groupAvoidRadiusSqr) {
						float distance = distanceSqr * m_groupAvoidRadius / m_groupAvoidRadiusSqr;
						avoid += direction.normalized * (m_groupAvoidRadius - distance);
					}
				}
			}
			m_motion.FlockSeparation(avoid);
		}
	}
}
