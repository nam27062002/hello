﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonIcePowers : MonoBehaviour {

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
    private int m_powerLevel = 0;
    private bool m_active = false;

    public void Awake()
    {
        m_frozenRegistry = FreezingObjectsRegistry.instance.Register( transform, m_currentRadius);
        FreezingObjectsRegistry.instance.RemoveRegister( m_frozenRegistry );    // Start deactivated
        m_frozenRegistry.m_killTiers = m_frozenKillProbabiblities;
    }

    public void Start()
    {
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.powerLevel;
        
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
        
        m_motion = InstanceManager.player.dragonMotion;
        m_boost = InstanceManager.player.dragonBoostBehaviour;
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
        m_frozenRegistry.m_killOnFrozen = m_powerLevel >= 3 && m_boost.IsBoostActive();
        
        if ( m_boost.IsBoostActive() && !m_active )
        {
            FreezingObjectsRegistry.instance.AddRegister( m_frozenRegistry );
            m_active = true;
        }
        else if ( !m_boost.IsBoostActive() && m_active)
        {
            FreezingObjectsRegistry.instance.RemoveRegister( m_frozenRegistry );
            m_active = false;
        }
    }
    
    // Update is called once per frame
    void OnDestroy () {
        if ( FreezingObjectsRegistry.isInstanceCreated )
            FreezingObjectsRegistry.instance.RemoveRegister( m_frozenRegistry );
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawSphere( transform.position, m_currentRadius);
    }


}
