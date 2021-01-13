using UnityEngine;

public class PetProjectile : Projectile
{
    [SeparatorAttribute]
    [SerializeField] private DragonTier m_tier = DragonTier.TIER_0;
    public DragonTier tier 
    { 
        get { return m_tier; }
        set { m_tier = value; } 
    }
    [SerializeField] [EnumMask] protected IEntity.Tag m_entityTags = 0;
    [SerializeField] [EnumMask] protected IEntity.Tag m_ignoreEntityTags = 0;
    [SerializeField] private LayerMask m_hitMask = 0;
    [SerializeField] private LayerMask m_groundMask = 0;
    [SerializeField] IEntity.Type m_firingEntityType = IEntity.Type.PET;
    private Rect m_rect;
    [SerializeField] bool m_explodeIfHomingtargetNull = false;
    public bool explodeIfHomingtargetNull
    {
        get{ return m_explodeIfHomingtargetNull; }
        set{ m_explodeIfHomingtargetNull = value; }
    }

    [SerializeField] bool m_breaksArmor = false;
    public bool breaksArmor
    {
        get{ return m_breaksArmor; }
    }
    

    protected override void Update()
    {
        base.Update();
        if ( m_explodeIfHomingtargetNull && m_state == State.Shot && m_motionType == MotionType.Homing)
        {
            if ( m_target == null || !m_target.gameObject.activeInHierarchy )
            {
                Explode(false);
            }
        }
    }

    public override void OnTriggerEnter(Collider _other) {
        if (m_state == State.Shot) {
            if (m_machine == null || !m_machine.IsDying()) {
                m_hitCollider = _other;
                int layer = (1 << _other.gameObject.layer);
                if ((layer & m_hitMask.value) > 0) {
                    bool tagMatch = true;

                    if (m_entityTags > 0 || m_ignoreEntityTags > 0) {
                        Entity e = m_hitCollider.GetComponent<Entity>();
                        tagMatch = (e != null) && e.HasTag(m_entityTags) && !e.HasTag(m_ignoreEntityTags);
                    }

                    if (tagMatch) {
                        Explode(true);
                    }
                } else if ((layer & m_groundMask.value) > 0) {
                    Explode(false);
                }
            }
        }
    }

    protected override void DealDamage() {
        Entity entity = m_hitCollider.GetComponent<Entity>();
        if (entity != null && entity.IsEdible(m_tier)) {
            if (entity.machine.CanBeBitten()) {
                entity.machine.Bite();
                entity.machine.BeginSwallowed(m_transform, true, m_firingEntityType, KillType.EATEN);
                entity.machine.EndSwallowed(m_transform);
            }
        }
    }
    
    protected override void DealExplosiveDamage( bool _dealDamage )
    {
        Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_transform.position, m_radius);
        for (int i = 0; i < preys.Length; i++) {
            if (preys[i].IsBurnable(m_tier)) {
                bool burn = true;
                if (m_entityTags > 0 || m_ignoreEntityTags > 0) {
                    burn = preys[i].HasTag(m_entityTags) && !preys[i].HasTag(m_ignoreEntityTags);
                }
                
                AI.IMachine machine =  preys[i].machine;
                if (machine != null && burn) {
                    machine.Burn(transform, m_firingEntityType);
                }
            }
        }

        m_rect.center = m_transform.position;
        m_rect.height = m_rect.width = m_radius;
        FirePropagationManager.instance.FireUpNodes(m_rect, Overlaps, m_tier, DragonBreathBehaviour.Type.None, Vector3.zero, m_firingEntityType);
        
        if (m_missHitSpawnsParticle || _dealDamage) {               
            m_onHitParticle.Spawn(m_position + m_onHitParticle.offset, Quaternion.Inverse(m_transform.rotation) );
        }
            
    }
    
    bool Overlaps( CircleAreaBounds _fireNodeBounds )
    {
        return MathTest.TestCircleVsCircle(m_transform.position, m_radius, _fireNodeBounds.center, _fireNodeBounds.radius);
    }
}
