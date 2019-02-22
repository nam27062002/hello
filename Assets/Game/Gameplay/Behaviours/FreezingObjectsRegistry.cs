using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezingObjectsRegistry : Singleton<FreezingObjectsRegistry> 
{
	public class Registry
	{
		public Transform m_transform;
		public float m_distanceSqr;
        public bool m_killOnFrozen;
	};

	List<Registry> m_registry;
    List<AI.Machine> m_machines = new List<AI.Machine>();   // to froze machines
    List<AI.Machine> m_freezingMachines = new List<AI.Machine>();   // already freezing
    List<float> m_freezingLevels = new List<float>();

    List<AI.Machine> m_toFreeze = new List<AI.Machine>();

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


	public Registry Register( Transform tr, float distance )
	{
		Registry reg = new Registry();
		reg.m_transform = tr;
		reg.m_distanceSqr = distance * distance;
        reg.m_killOnFrozen = false;
		m_registry.Add( reg );
        return reg;
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
    
    public void Unregister( Registry reg )
    {
        m_registry.Remove( reg);
    }

	public Registry Overlaps( CircleAreaBounds bounds )
	{
        Registry ret = null;
		for( int i = 0; i<m_registry.Count && ret == null; i++ )
		{
			Vector2 v = (Vector2)(bounds.center - m_registry[i].m_transform.position);
            if (v.sqrMagnitude < m_registry[i].m_distanceSqr)
                ret = m_registry[i];
		}
		return ret;
	}
    
    
    public void CheckFreeze()
    {
        if ( m_registry.Count > 0 )
        {
            float freezingChange = Time.deltaTime * m_freezinSpeed;
            int max;
           
            max = m_machines.Count;
            m_toFreeze.Clear();
            
            for (int i = max-1; i >=0 ; i--)
            {
                Registry freezing = Overlaps((CircleAreaBounds)m_machines[i].entity.circleArea.bounds);
                if ( freezing != null )
                {
                    m_toFreeze.Add( m_machines[i] );
                    m_machines.RemoveAt( i );
                }   
            }
            
            max = m_freezingMachines.Count;
            for (int i = max-1; i >=0 ; i--)
            {
                Registry freezing = Overlaps((CircleAreaBounds)m_freezingMachines[i].entity.circleArea.bounds);
                if ( freezing == null)
                {
                    m_freezingLevels[i] -= freezingChange;
                    if ( m_freezingLevels[i] <= 0 )
                    {
                        m_freezingMachines[i].SetFreezingLevel(0);
                            // Add to non freezing
                        m_machines.Add( m_freezingMachines[i] );
                            // Remove from freezing
                        m_freezingMachines.RemoveAt(i);
                        m_freezingLevels.RemoveAt(i);
                            
                    }
                    else
                    {
                        m_freezingMachines[i].SetFreezingLevel(m_freezingLevels[i]);
                    }
                }
                else
                {
                    m_freezingLevels[i] += freezingChange;
                    if (m_freezingLevels[i] > 1.0f)
                    {
                        m_freezingLevels[i] = 1.0f;
                    }
                        
                    m_freezingMachines[i].SetFreezingLevel(m_freezingLevels[i]);
                    
                    if ( freezing.m_killOnFrozen && m_freezingLevels[i] >= 1.0f )
                    {
                        m_freezingMachines[i].Smash(IEntity.Type.PLAYER);
                    }
                }
            }
    
            max = m_toFreeze.Count;
            for (int i = 0; i < max; i++)
            {
                m_freezingLevels.Add( freezingChange );
                m_freezingMachines.Add( m_toFreeze[i] );
                m_toFreeze[i].SetFreezingLevel( freezingChange );
            }
        }
    }
    
    public void RegisterMachine(AI.Machine _machine)
    {
        if ( _machine.entity != null && _machine.entity.circleArea != null )
            m_machines.Add( _machine );
    }
    
    public void UnregisterMachine(AI.Machine _machine)
    {
        if (m_machines.Contains(_machine))
            m_machines.Remove( _machine );
        if ( m_freezingMachines.Contains( _machine ) )
        {
            _machine.SetFreezingLevel(0);
            m_freezingMachines.Remove( _machine );
        }
    }
    
    public bool IsFreezing(AI.IMachine _machine){
        bool ret = false;
        if ( _machine is AI.Machine )
        {
            ret = m_freezingMachines.Contains( _machine as AI.Machine );
        }
        return ret;
    }
    
    
}
