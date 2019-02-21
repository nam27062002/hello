using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonIcePowers : MonoBehaviour {

    [Header("Frozen Area Setup")]
    public float m_frozenAreaRadius = 1;
    public float m_frozenAreaRadiusUnderwater = 2;
    public float m_frozenAreaRadiusPercentageUpgrade = 0.05f;
    
    
    protected float m_currentRadius = 0;
    FreezingObjectsRegistry.Registry m_frozenRegistry;
    
    DragonMotion m_motion;

    public void Start()
    {
        m_currentRadius = m_frozenAreaRadius;
        m_frozenRegistry = FreezingObjectsRegistry.instance.Register( transform, m_currentRadius);
        m_motion = InstanceManager.player.dragonMotion;
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
