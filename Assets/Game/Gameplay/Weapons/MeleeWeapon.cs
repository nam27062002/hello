﻿using UnityEngine;
using System.Collections;

public class MeleeWeapon : IMeleeWeapon {

	[SerializeField] private float m_knockback = 0;

	[SeparatorAttribute("Trail effect")]
	[SerializeField] private Xft.XWeaponTrail m_trail;
	[SerializeField] private ParticleSystem[] m_trailParticles = new ParticleSystem[0];

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
		InstanceManager.player.dragonHealthBehaviour.ReceiveDamage(m_damage, m_damageType, null, true, m_entity.sku, m_entity);
	}

}