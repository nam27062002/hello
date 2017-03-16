using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezingObjectsRegistry : Singleton<FreezingObjectsRegistry> 
{
	public class Registry
	{
		public Transform m_transform;
		public float m_distanceSqr;
	};

	List<Registry> m_registry;

	public static float m_freezinSpeed = 1;
	public static float m_defrostSpeed = 0.5f;
	public static float m_minFreezeSpeedMultiplier = 0.25f;


	public FreezingObjectsRegistry()
	{
		m_registry = new List<Registry>();

		DefinitionNode node = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.FREEZE_CONSTANTS, "freezeConstant");
		if ( node != null )
		{
			m_freezinSpeed = node.GetAsFloat( "freezingSpeed", 1.0f );
			m_defrostSpeed = node.GetAsFloat( "defrostSpeed", 0.5f );
			m_minFreezeSpeedMultiplier = node.GetAsFloat("minFreezeSpeedMultiplier", 0.25f);
		}

	}


	public void Register( Transform tr, float distance )
	{
		Registry reg = new Registry();
		reg.m_transform = tr;
		reg.m_distanceSqr = distance * distance;
		m_registry.Add( reg );
	}

	public void Unregister( Transform tr )
	{
		for( int i = m_registry.Count -1 ;i>=0; i-- )
		{
			if ( m_registry[i].m_transform == tr )
			{
				m_registry.RemoveAt( i );
			}
		}
	}

	public bool Overlaps( CircleAreaBounds bounds )
	{
		for( int i = 0; i<m_registry.Count; i++ )
		{
			Vector2 v = (Vector2)(bounds.center - m_registry[i].m_transform.position);
			if ( v.sqrMagnitude < m_registry[i].m_distanceSqr )
				return true;
		}
		return false;
	}
}
