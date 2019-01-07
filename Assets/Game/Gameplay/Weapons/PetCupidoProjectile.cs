using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCupidoProjectile : PetProjectile {

    [SerializeField] private float m_inLoveDuration;
    private Entity[] m_checkEntities = new Entity[10];

	protected override void DealDamage() 
    {
        Entity entity = m_hitCollider.GetComponent<Entity>();
        if (entity != null && entity.machine != null) 
        {
            AI.Machine enittyMachine = entity.machine as AI.Machine;
            enittyMachine.InLove(m_inLoveDuration);    
        }
    }
    
    protected override void DealExplosiveDamage( bool _dealDamage )
    {
        int max = EntityManager.instance.GetEntitiesInRange2DNonAlloc(m_transform.position, m_radius, m_checkEntities);
        for (int i = 0; i < max; i++) 
        {
            if (m_checkEntities[i].machine != null)
            {
                AI.Machine enittyMachine = m_checkEntities[i].machine as AI.Machine;
                if ( enittyMachine != null)
                    enittyMachine.InLove(m_inLoveDuration);
            }
        }

        if (m_missHitSpawnsParticle || _dealDamage) 
        {               
            if ( _dealDamage && m_target != null )
            {
                m_onHitParticle.Spawn(m_target.position, Quaternion.Inverse( m_target.rotation ) );
            }
            else
            {
                m_onHitParticle.Spawn(m_position + m_onHitParticle.offset, Quaternion.Inverse(m_transform.rotation) );
            }
        }
            
    }
    
}
