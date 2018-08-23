using UnityEngine;
using System.Collections;

public class DOTMeleeWeapon : IMeleeWeapon {

	[SerializeField] private float m_duration = 5f;

	protected override void OnEnabled() { }
	protected override void OnDisabled() { }
	protected override void OnDealDamage() {
        Entity e = null;
        if (m_entity is Entity) { e = m_entity as Entity; }
        InstanceManager.player.dragonHealthBehaviour.ReceiveDamageOverTime(m_damage, m_duration, m_damageType, m_transform, true, m_entity.sku, e);
	}
}
