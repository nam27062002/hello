using System.Collections;
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

    
    protected float m_currentRadius = 0;
    FreezingObjectsRegistry.Registry m_frozenRegistry;
    
    DragonMotion m_motion;
    DragonBoostBehaviour m_boost;
    private int m_powerLevel = 0;

    public void Awake()
    {
        m_frozenRegistry = FreezingObjectsRegistry.instance.Register( transform, m_currentRadius);
    }

    public void Start()
    {
        DragonDataSpecial dataSpecial = InstanceManager.player.data as DragonDataSpecial;
        m_powerLevel = dataSpecial.powerLevel;
        m_currentRadius = m_frozenAreaRadius;
        if ( m_powerLevel >= 1 )
        {
            InstanceManager.player.dragonShieldBehaviour.m_maxShield += m_increaseShield;
        }
        if (m_powerLevel >= 2)
        {
            m_currentRadius += m_currentRadius * m_frozenAreaRadiusPercentageUpgrade;
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
        if (m_powerLevel >= 2)
            radius += radius * m_frozenAreaRadiusPercentageUpgrade;
        m_currentRadius = Mathf.Lerp(m_currentRadius, radius, Time.deltaTime * 5.0f);
        m_frozenRegistry.m_distanceSqr = m_currentRadius * m_currentRadius;
        m_frozenRegistry.m_killOnFrozen = m_powerLevel >= 3 && m_boost.IsBoostActive();
    }
    
    // Update is called once per frame
    void OnDestroy () {
        if ( FreezingObjectsRegistry.isInstanceCreated )
            FreezingObjectsRegistry.instance.Unregister( m_frozenRegistry );
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0, 0, 1, 0.1f);
        Gizmos.DrawSphere( transform.position, m_currentRadius);
    }


}
