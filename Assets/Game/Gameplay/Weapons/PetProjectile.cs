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
    [SerializeField] [EnumMask] private IEntity.Tag m_entityTags = 0;
    [SerializeField] private LayerMask m_hitMask = 0;
    [SerializeField] private LayerMask m_groundMask = 0;
    [SerializeField] CircleArea2D m_explosionArea;
    [SerializeField] IEntity.Type m_firingEntityType = IEntity.Type.PET;
    private Rect m_rect;
    

    public override void OnTriggerEnter(Collider _other) {
        if (m_state == State.Shot) {
            if (m_machine == null || !m_machine.IsDying()) {
                m_hitCollider = _other;
                int layer = (1 << _other.gameObject.layer);
                if ((layer & m_hitMask.value) > 0) {
                    bool tagMatch = true;

                    if (m_entityTags > 0) {
                        Entity e = m_hitCollider.GetComponent<Entity>();
                        tagMatch = (e != null) && e.HasTag(m_entityTags);
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
                entity.machine.BeginSwallowed(m_transform, true, m_firingEntityType);
                entity.machine.EndSwallowed(m_transform);
            }
        }
    }
    
    protected override void DealExplosiveDamage( bool _dealDamage )
    {
        Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_explosionArea.center, m_explosionArea.radius);
        for (int i = 0; i < preys.Length; i++) {
            if (preys[i].IsBurnable(m_tier)) {
                bool burn = true;
                if (m_entityTags > 0) {
                    burn = preys[i].HasTag(m_entityTags);
                }
                
                AI.IMachine machine =  preys[i].machine;
                if (machine != null && burn) {
                    machine.Burn(transform, m_firingEntityType);
                }
            }
        }

        m_rect.center = m_explosionArea.center;
        m_rect.height = m_rect.width = m_explosionArea.radius;
        FirePropagationManager.instance.FireUpNodes(m_rect, Overlaps, m_tier, DragonBreathBehaviour.Type.None, Vector3.zero, m_firingEntityType);
    }
    
    bool Overlaps( CircleAreaBounds _fireNodeBounds )
    {
        return m_explosionArea.Overlaps( _fireNodeBounds.center, _fireNodeBounds.radius);
    }
}
