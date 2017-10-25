using UnityEngine;
using System.Collections;

public class DOTMeleeWeapon : IMeleeWeapon {

	[SerializeField] private float m_duration = 5f;

	protected override void OnEnabled() { }
	protected override void OnDisabled() { }
	protected override void OnDealDamage() {
		InstanceManager.player.dragonHealthBehaviour.ReceiveDamageOverTime(m_damage, m_duration, m_damageType, m_transform, true, m_entity.sku, m_entity);
	}
}
