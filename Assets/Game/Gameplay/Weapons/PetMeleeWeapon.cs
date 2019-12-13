using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetMeleeWeapon : IMeleeWeapon {
    [SeparatorAttribute]
    [SerializeField] private DragonTier m_tier = DragonTier.TIER_0;
    [SerializeField] [EnumMask] private IEntity.Tag m_entityTags = 0;
    [SerializeField] private LayerMask m_hitMask = 0;
    [SerializeField] private bool m_killEverything = false;

    protected override void OnAwake() {}
    protected override void OnDealDamage() {}
    protected override void OnDisabled() {}
    protected override void OnEnabled() {}

    protected override void OnTriggerEnter(Collider _other) {
        int layer = (1 << _other.gameObject.layer);
        if ((layer & m_hitMask.value) > 0) {
            Entity e = _other.GetComponent<Entity>();
            if (e != null) {
                if (m_entity.machine.enemy == e.transform || m_killEverything) {
                    bool tagMatch = true;

                    if (m_entityTags > 0) {
                        tagMatch = e.HasTag(m_entityTags);
                    }

                    if (tagMatch) {
                        if (e.IsEdible(m_tier)) {
                            if (e.machine.CanBeBitten()) {
                                e.machine.Bite();
                                e.machine.BeginSwallowed(m_transform, true, IEntity.Type.PET, KillType.EATEN);
                                e.machine.EndSwallowed(m_transform);

                                OnEntityKilled(e);
                            }
                        }
                    }
                }
            }
        }
    }

    protected virtual void OnEntityKilled(Entity _e) {}
}
