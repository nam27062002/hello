using UnityEngine;
using System.Collections;

public class MeleeWeapon : IMeleeWeapon {

	[SerializeField] private float m_knockback = 0;

	[SeparatorAttribute("Trail effect")]
	[SerializeField] private Xft.XWeaponTrail m_trail;
	[SerializeField] private ParticleSystem[] m_trailParticles = new ParticleSystem[0];

	[SerializeField] private ViewParticleSpawner m_weaponParticle = null;

    protected override void OnAwake() { }

	protected override void OnEnabled() { 
		if (m_trail) m_trail.Activate();
		for (int i = 0; i < m_trailParticles.Length; i++) {
			if (m_trailParticles[i] != null) {
				m_trailParticles[i].Clear();
				ParticleSystem.EmissionModule em = m_trailParticles[i].emission;
				em.enabled = true;
				m_trailParticles[i].Play();
			}
		}
		if (m_weaponParticle != null) m_weaponParticle.Spawn();
	}

	protected override void OnDisabled() { 
		if (m_trail) m_trail.Deactivate();
		for (int i = 0; i < m_trailParticles.Length; i++) {
			if (m_trailParticles[i] != null) {
				ParticleSystem.EmissionModule em = m_trailParticles[i].emission;
				if (em.enabled && m_trailParticles[i].main.loop) {
					em.enabled = false;
					m_trailParticles[i].Stop();
				}
			}
		}
		if (m_weaponParticle != null) m_weaponParticle.Stop();
	}

	protected override void OnDealDamage() {
		if (m_knockback > 0) {
			DragonMotion dragonMotion = InstanceManager.player.dragonMotion;

			Vector3 knockBack = m_transform.position - m_lastPosition;
			knockBack.z = 0f;
			knockBack.Normalize();

			knockBack *= m_knockback;

			dragonMotion.AddForce(knockBack);
		}

        Entity e = null;
        if (m_entity is Entity) { e = m_entity as Entity; }
		InstanceManager.player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType, m_transform, true, m_entity.sku, e);
	}

}