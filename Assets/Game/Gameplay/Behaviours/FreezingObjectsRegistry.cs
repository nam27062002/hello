using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezingObjectsRegistry : Singleton<FreezingObjectsRegistry> 
{
	public class Registry
	{
		public Transform m_transform;
		public float m_distanceSqr;
        public bool m_checkTier;
        public DragonTier m_dragonTier;
        public bool m_killOnFrozen;
        public float[] m_killTiers;
        
        public Registry()
        {
            m_checkTier = false;
            m_killOnFrozen = false;
            m_dragonTier = DragonTier.TIER_0;
        }
	};

	List<Registry> m_registry;
    List<AI.Machine> m_machines = new List<AI.Machine>();   // to froze machines
    List<AI.Machine> m_freezingMachines = new List<AI.Machine>();   // already freezing
    List<float> m_freezingLevels = new List<float>();
    List<bool> m_freezingKills = new List<bool>();

    List<AI.Machine> m_toFreeze = new List<AI.Machine>();
    List<bool> m_toKill = new List<bool>();

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
    
    public void AddRegister( Registry registry )
    {
        m_registry.Add( registry );
    }

	public void RemoveRegister( Registry registry )
	{
        m_registry.Remove(registry);
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
        float freezingChange = Time.deltaTime * m_freezinSpeed;
        int max;
       
        max = m_machines.Count;
        m_toFreeze.Clear();
        m_toKill.Clear();
        for (int i = max-1; i >=0 ; i--)
        {
            Registry freezing = Overlaps((CircleAreaBounds)m_machines[i].entity.circleArea.bounds);
            if ( freezing != null )
            {
                // Check if tier pass
                Entity entity = m_machines[i].entity as Entity;
                if (!freezing.m_checkTier || ( entity != null && (entity.IsEdible( freezing.m_dragonTier)|| entity.CanBeHolded( freezing.m_dragonTier ))))
                {
                    m_toFreeze.Add( m_machines[i] );
                    if ( freezing.m_killOnFrozen )
                    {
                        // Check random
                        if (m_machines[i].entity.edibleFromTier < DragonTier.COUNT)
                        {
                            m_toKill.Add( Random.Range(0, 100) < freezing.m_killTiers[ (int)m_machines[i].entity.edibleFromTier ] );
                        }else{
                            m_toKill.Add(false);
                        }
                    }
                    else
                    {
                        m_toKill.Add(false);
                    }
                    m_machines.RemoveAt( i );
                }


               
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
                    m_freezingKills.RemoveAt(i);
                        
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
                if ( m_freezingKills[i] && m_freezingLevels[i] >= 1.0f )
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
            m_freezingKills.Add( m_toKill[i] );
            
            m_toFreeze[i].SetFreezingLevel( freezingChange );
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
            int index = m_freezingMachines.IndexOf( _machine );
            
            m_freezingMachines.RemoveAt( index );
            m_freezingLevels.RemoveAt(index);
            m_freezingKills.RemoveAt(index);
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
