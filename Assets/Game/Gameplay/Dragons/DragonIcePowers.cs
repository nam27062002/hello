using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonIcePowers : MonoBehaviour {

	[Header("Shield Drains")]
	public float[] m_shieldDrainByTier = new float[ (int)DragonTier.COUNT ]{ 1.1f, 1.1f, 1.1f, 1.1f, 1.1f, 1.1f};
    [Header("Frozen Area Setup")]
    public float m_frozenAreaRadius = 1;
    public float m_frozenAreaRadiusUnderwater = 2;
    [Header("Frozen Area Level 2 ")]
    public float m_frozenAreaRadiusPercentageUpgrade = 0.05f;

    [Header("Shield Level 1 Upgrade ")]
    public float m_increaseShield = 50;
    public float[] m_frozenKillProbabiblities = new float[(int)DragonTier.COUNT];
    
    protected float m_currentRadius = 0;
    FreezingObjectsRegistry.Registry m_frozenRegistry;
    
    DragonMotion m_motion;
    DragonBoostBehaviour m_boost;
    DragonBreathBehaviour m_breath;
    private int m_powerLevel = 0;
    private bool m_active = false;
    ToggleParam m_toggleParam = new ToggleParam();

    public void Start()
    {
        m_frozenRegistry = FreezingObjectsRegistry.instance.Register( transform, m_currentRadius);
        m_frozenRegistry.m_checkTier = true;
        
        FreezingObjectsRegistry.instance.RemoveRegister( m_frozenRegistry );    // Start deactivated
        
        
        
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.powerLevel;
        m_frozenRegistry.m_dragonTier = dataSpecial.tier;
        
        // Radius scale
        float scale = InstanceManager.player.data.scale;
        m_frozenAreaRadius = m_frozenAreaRadius * scale;
        m_frozenAreaRadiusUnderwater = m_frozenAreaRadiusUnderwater * scale;
        if (m_powerLevel >= 2)
        {
            m_frozenAreaRadius += m_frozenAreaRadius * m_frozenAreaRadiusPercentageUpgrade;
            m_frozenAreaRadiusUnderwater += m_frozenAreaRadiusUnderwater * m_frozenAreaRadiusPercentageUpgrade;    
        }
        m_currentRadius = m_frozenAreaRadius;
        
        // Increase shield
        if ( m_powerLevel >= 1 )
        {
            InstanceManager.player.dragonShieldBehaviour.m_maxShield += m_increaseShield;
        }
        InstanceManager.player.dragonShieldBehaviour.FullShield();

		InstanceManager.player.dragonShieldBehaviour.m_shieldDrain = m_shieldDrainByTier[ (int)dataSpecial.tier ];
        
        m_motion = InstanceManager.player.dragonMotion;
        m_boost = InstanceManager.player.dragonBoostBehaviour;
        m_breath = InstanceManager.player.breathBehaviour;
        
        FreezingObjectsRegistry.instance.m_killOnFrozen = m_powerLevel >= 3;
        FreezingObjectsRegistry.instance.m_killTiers = m_frozenKillProbabiblities;
    }

    void Update()
    {
        float radius = m_frozenAreaRadius;
        if ( m_motion.IsInsideWater() )
        {
            radius = m_frozenAreaRadiusUnderwater;    
        }
        
        m_currentRadius = Mathf.Lerp(m_currentRadius, radius, Time.deltaTime * 5.0f);
        m_frozenRegistry.m_distanceSqr = m_currentRadius * m_currentRadius;
        if ( !m_breath.IsFuryOn() )
        {
            m_frozenRegistry.m_checkType = FreezingObjectsRegistry.FreezingCheckType.EAT;
        }
        else
        {
            if ( m_breath.type != DragonBreathBehaviour.Type.Mega )
            {
                m_frozenRegistry.m_checkType = FreezingObjectsRegistry.FreezingCheckType.BURN;
            }
            else
            {
                m_frozenRegistry.m_checkType = FreezingObjectsRegistry.FreezingCheckType.NONE;
            }
        }

        m_frozenRegistry.m_checkTier = !m_breath.IsFuryOn();    // TODO Check it can be burned instead
        
        if ( m_boost.IsBoostActive() && !m_active )
        {
            FreezingObjectsRegistry.instance.AddRegister( m_frozenRegistry );
            m_active = true;
            
            m_toggleParam.value = m_active;
            Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
                
        }
        else if ( !m_boost.IsBoostActive() && m_active)
        {
            FreezingObjectsRegistry.instance.RemoveRegister( m_frozenRegistry );
            m_active = false;
            
            m_toggleParam.value = m_active;
            Broadcaster.Broadcast(BroadcastEventType.SPECIAL_POWER_TOGGLED, m_toggleParam);
        }
    }
    
    // Update is called once per frame
    void OnDestroy () {
        if ( FreezingObjectsRegistry.instance != null )
            FreezingObjectsRegistry.instance.RemoveRegister( m_frozenRegistry );
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawSphere( transform.position, m_currentRadius);
    }


}
