using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricBoostArea : MonoBehaviour {

	public List<CircleArea2D> m_circleAreas = new List<CircleArea2D>();
    private List<float> m_originalRadius = new List<float>();
    public DragonTier m_tier = DragonTier.TIER_4;
    public IEntity.Type m_type = IEntity.Type.PLAYER;
    public float m_waterMultiplier = 2;
    public float m_chainRadiusCheck = 2;
    public float m_blastRadius = 2;
    
    
	private Entity[] m_checkEntities = new Entity[50];
	private int m_numCheckEntities = 0;
	float m_extraRadius;
	DragonBoostBehaviour m_boost;
	DragonBreathBehaviour m_breath;
	DragonMotion m_motion;
	
	private bool m_active = false;
    private int m_powerLevel = 0;
    

	// Use this for initialization
	void Start () {
        m_originalRadius.Clear();
        for (int i = 0; i < m_circleAreas.Count; i++)
        {
            m_originalRadius.Add( m_circleAreas[i].radius );
        }
        
		m_boost = InstanceManager.player.dragonBoostBehaviour;
		m_breath = InstanceManager.player.breathBehaviour;
		m_motion = InstanceManager.player.dragonMotion;
		m_extraRadius = 1;
		m_tier = InstanceManager.player.data.tier;

	}
	
	// Update is called once per frame
	void Update () {

        int max = m_circleAreas.Count;

		if ( m_boost.IsBoostActive() || m_breath.IsFuryOn())
		{
			if (!m_active)
			{
				m_active = true;
			}

            bool done = false;
            
            for (int i = 0; i < max && !done ; i++)
            {
                Entity entity = GetFirstBurnableEntity((Vector2)m_circleAreas[i].center, m_circleAreas[i].radius);
                if ( entity != null )
                {
                    AI.IMachine startingMachine =  entity.machine;
                    if ( startingMachine != null)
                    {
                        startingMachine.Burn(transform, m_type);
                        
                        // BLAST!!
                        if ( m_powerLevel >= 2 )
                        {
                            m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities((Vector2) entity.circleArea.center , m_blastRadius, m_checkEntities);
                            for (int j = 0; j < m_numCheckEntities; j++)
                            {
                                Entity blastEntity = m_checkEntities[j];
                                if ( blastEntity.IsBurnable() && (blastEntity.IsBurnable(m_tier) || ( m_breath.IsFuryOn() && m_breath.type == DragonBreathBehaviour.Type.Mega )))
                                {
                                    AI.IMachine blastMachine =  blastEntity.machine;
                                    if (blastMachine != null)
                                    {
                                        // Launch Lightning!
                                        blastMachine.Burn(transform, m_type);
                                    }
                                }
                            }
                        }
                        
                        // Chain
                        if ( m_powerLevel >= 1 )
                        {
                            Entity chain1_entity = GetFirstBurnableEntity((Vector2) entity.circleArea.center, m_chainRadiusCheck);
                            if ( chain1_entity != null )
                            {
                                // Burn chain 1 entity
                                AI.IMachine chain1_machine =  chain1_entity.machine;
                                chain1_machine.Burn(transform, IEntity.Type.PLAYER);
                                 
                                // Chain Upgrade
                                if ( m_powerLevel >= 3 )
                                {
                                    Entity chain2_entity = GetFirstBurnableEntity((Vector2) chain1_entity.circleArea.center, m_chainRadiusCheck);
                                    if ( chain2_entity != null )
                                    {
                                        AI.IMachine chain2_machine =  chain2_entity.machine;
                                        chain2_machine.Burn(transform, IEntity.Type.PLAYER);        
                                    }
                                }
                            }
                        }
                           
                        done = true;
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
        m_extraRadius = Mathf.Clamp(m_extraRadius, 1, m_waterMultiplier);
        for (int i = 0; i < max; i++)
        {
            m_circleAreas[i].radius = m_originalRadius[i] * m_extraRadius;
        }
	}
    
    public Entity GetFirstBurnableEntity( Vector2 _center, float _radius )
    {
        Entity ret = null;
        m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(_center, _radius, m_checkEntities);
        for (int i = 0; i < m_numCheckEntities && ret == null; i++)
        {
            Entity prey = m_checkEntities[i];
            if (prey.IsBurnable() && (prey.IsBurnable(m_tier) || (m_breath.IsFuryOn() && m_breath.type == DragonBreathBehaviour.Type.Mega)))
            {
                ret = prey;
            }
        }
        return ret;
    }
}
