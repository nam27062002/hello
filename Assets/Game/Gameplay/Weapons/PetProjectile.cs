using UnityEngine;

public class PetProjectile : Projectile {
    [SeparatorAttribute]
    [SerializeField] private DragonTier m_tier = DragonTier.TIER_0;
    [SerializeField] [EnumMask] private IEntity.Tag m_entityTags = 0;
    [SerializeField] private LayerMask m_hitMask = 0;
    [SerializeField] private LayerMask m_groundMask = 0;


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
                entity.machine.BeginSwallowed(m_transform, true, IEntity.Type.PET);
                entity.machine.EndSwallowed(m_transform);
            }
        }
    }
}
