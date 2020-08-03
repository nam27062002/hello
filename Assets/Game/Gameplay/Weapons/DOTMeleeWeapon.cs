using UnityEngine;
using System.Collections;

public class DOTMeleeWeapon : IMeleeWeapon {

	[SerializeField] private float m_duration = 5f;

    protected override void OnAwake() { }
	protected override void OnEnabled() { }
	protected override void OnDisabled() { }
	protected override void OnDealDamage() {
        string sku = "";
		Entity e = null;

        if (m_entity is Entity) {
			e = m_entity as Entity; 
			if (e != null) {
				sku = e.sku;
			}
		}

        InstanceManager.player.dragonHealthBehaviour.ReceiveDamageOverTime(m_damage, m_duration, m_damageType, m_transform, true, sku, e);
	}
}
